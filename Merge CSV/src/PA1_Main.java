import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.Map;

public class PA1_Main {

	public static void main(String[] args) {

		Map<String, Student> ht = new LinkedHashMap<String, Student>();

		// Loop thru each file to read
		for (int i = 0; i < args.length; i++) {
			PA1_Main.readerHashTable(ht, args[i]);
		}

		PA1_Main.writerHashTable(ht);

	}

	public static void readerHashTable(Map<String, Student> ht, String fileName) {

		// Initialize some variables
		String category = null;
		String entry = null;

		try {
			FileReader fr = new FileReader(fileName);

			BufferedReader bufferedReader = new BufferedReader(fr);
			int line = 0;
			String[] columns = null;

			while ((entry = bufferedReader.readLine()) != null) {
				// Regular expression to determine splits. Priority shown to keep quotations for
				// the student name
				String[] splitString = entry.split(",(?=([^\"]*\"[^\"]*\")*[^\"]*$)");

				// Just want to make sure we do this once, create a copy of splitString array to
				// use for assignments
				if (line == 0) {
					columns = splitString;
					line++;
					continue;
				}

				// Means there are no grades
				if (splitString.length < 3) {
					System.out.println("Bad line: " + entry);
					continue;
				}

				// Gets the file name to import, assigning category to the filename.
				// Should still be fine even if there is no /, such as being in root folder of
				// program. Will return index -1, +1 makes it 0, so substring will be string.
				int fileIndex = fileName.lastIndexOf("/");
				String fileOnly = fileName.substring(fileIndex + 1);
				String[] fileSplit = fileOnly.split("_");
				category = fileSplit[0];

				// Assign ID to the first substring, name to the second
				String id = splitString[0];
				String name = splitString[1];
				Student currentStudent = ht.get(id);

				// If the ht.get(id) didn't find a Student with that id, create a new one
				if (currentStudent == null) {
					currentStudent = new Student(id, name);
					ht.put(id, currentStudent);
				}

				// Assigning each category and how many of that category, as well as a grade
				for (int i = 3; i < splitString.length; i++) {
					String gradeString = splitString[i];
					String assignment = columns[i];
					int gradeInt = Integer.parseInt(gradeString);
					currentStudent.AddGrade(category, assignment, gradeInt);
				}
			}
			bufferedReader.close();
		} catch (FileNotFoundException ex) {
			System.out.println("Bad file: " + fileName);
		} catch (IOException ex) {
			System.out.println("Bad lines: " + fileName);
		}
	}

	public static void writerHashTable(Map<String, Student> ht) {

		// Initialize string variable for catch exception to add fileName.
		String fileName = "";

		// Details writing
		try {
			fileName = "output - details.csv";
			FileWriter fw = new FileWriter(fileName);

			BufferedWriter bw = new BufferedWriter(fw);

			// Print headers
			Student id = ht.get("");
			String headerLine = "\"#ID\",\"Name\"";

			// Just getting categories of all files read and writing them out as header line
			for (String category : id.getCategories()) {
				ArrayList<Grade> grades = id.getGradesValue(category);
				for (Grade grade : grades) {
					headerLine += ",\"" + grade.name + "\"";
				}
			}

			bw.write(headerLine);

			// Print student stuff
			Iterator<Map.Entry<String, Student>> it = ht.entrySet().iterator();

			while (it.hasNext()) {
				// Bob's file has 6 decimal places for some reason
				DecimalFormat df = new DecimalFormat("#.000000");

				Map.Entry<String, Student> entry = it.next();
				String line = "";
				Student student = entry.getValue();
				
				// Just concatenating the output string
				line += "\"" + student.studentId + "\"";
				line += "," + student.studentName;
				for (String category : student.getCategories()) {
					ArrayList<Grade> grades = student.getGradesValue(category);
					for (Grade grade : grades) {
						// Bob's file has no decimals for the overall line
						if (student.studentId.equals("")) {
							line += "," + "\"" + grade.grade + "\"";
						} else {
							line += "," + "\"" + df.format(grade.grade) + "\"";
						}
					}
				}
				bw.newLine();
				bw.write(line);
			}

			bw.close();
		} catch (

		IOException ex) {
			System.out.println("Error writing to file: " + fileName);
		}

		// Now for Summary write
		try {
			fileName = "output - summary.csv";
			FileWriter fw = new FileWriter(fileName);

			BufferedWriter bw = new BufferedWriter(fw);

			// Print headers
			Student id = ht.get("");
			String headerLine = "\"#ID\",\"Name\",\"Final Grade\"";

			// Bob's output file has the first letter capitalized while filenames did not.
			for (String category : id.getCategories()) {
				String inputString = category.substring(0, 1).toUpperCase();
				String firstCapitalized = inputString + category.substring(1);
				headerLine += ",\"" + firstCapitalized + "\"";
			}

			bw.write(headerLine);

			// Print student stuff
			Iterator<Map.Entry<String, Student>> it = ht.entrySet().iterator();

			double totalGrade = 0;
			while (it.hasNext()) {
				// Bob's file has 6 decimal places for some reason, but I only want precision to
				// two places for this summary file
				DecimalFormat df = new DecimalFormat("#.00");
				df.setMinimumFractionDigits(6);

				Map.Entry<String, Student> entry = it.next();
				String line = "";
				Student student = entry.getValue();
				
				// Concatenating the output string
				line += "\"" + student.studentId + "\"";
				// Bob's input has OVERALL, but output Overall
				if (student.studentName.equals("OVERALL")) {
					student.studentName = "Overall";
					line += "," + "\"" + student.studentName + "\"";
				} else {
					// Omit the quotes since they're already saved in student names
					line += "," + student.studentName;
				}

				// Grade stuff, should be calculating total grades in each category
				int order = 0;
				double calcGrade = 0;
				int eachGrade[] = new int[student.getCategories().size()];
				for (String category : student.getCategories()) {
					ArrayList<Grade> grades = student.getGradesValue(category);
					for (Grade grade : grades) {
						calcGrade += grade.grade;
					}
					eachGrade[order] += calcGrade;
					calcGrade = 0;
					order++;
				}

				// Combining the total for each category
				for (int i = 0; i < eachGrade.length; i++) {
					calcGrade += eachGrade[i];
				}
				if (student.studentName.equals("Overall")) {
					// Can be used to show 100.0 as final grade, but bobs file doesn't show that.
					// Will keep here.
					totalGrade = calcGrade;
					line += "," + "\"" + "\"" + "," + "\"" + eachGrade[0] + "\"" + "," + "\"" + eachGrade[1] + "\""
							+ "," + "\"" + eachGrade[2] + "\"";
				} else {
					// Rounds the final grade to 2 decimal places and shows 6 decimal places, like
					// Bob's summary, not sure why but I don't want to get docked points
					double finalGrade = Math.round((calcGrade / totalGrade) * 10000);
					line += "," + "\"" + df.format(finalGrade / 100) + "\"" + "," + "\"" + df.format(eachGrade[0])
							+ "\"" + "," + "\"" + df.format(eachGrade[1]) + "\"" + "," + "\"" + df.format(eachGrade[2])
							+ "\"";
				}

				// Next line and write the concatenated string variable
				bw.newLine();
				bw.write(line);
			}

			bw.close();
		} catch (

		IOException ex) {
			System.out.println("Error writing to file: " + fileName);
		}
	}
}
