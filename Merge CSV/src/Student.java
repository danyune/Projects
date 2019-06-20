import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Set;

public class Student {

	// New LinkedHashMap and some variables.
	private Map<String, ArrayList<Grade>> grades = new LinkedHashMap<String, ArrayList<Grade>>();
	public String studentId, studentName;

	// Few constructors so we can make a blank one and a new one with ID and name.
	public Student() {

	}

	public Student(String studentId, String studentName) {

		this.studentId = studentId;
		this.studentName = studentName;
	}

	// Adding grade for a given student, separated by category (homework, quiz,
	// etc), then by assignment number (quiz1, quiz2, etc), followed by the grade
	public void AddGrade(String category, String assignment, int grade) {
		ArrayList<Grade> categoryGrade = grades.get(category);

		// If we can't find the category, make a new ArrayList of it
		if (categoryGrade == null) {
			categoryGrade = new ArrayList<Grade>();
			grades.put(category, categoryGrade);
		}

		// Start searching for the assignment assuming another student has a grade
		// already for it
		for (Grade searchGrade : categoryGrade) {
			if (searchGrade.name.equals(assignment)) {
				searchGrade.grade = grade;
				// Debug line if we find the assignment already made
				System.out.println(categoryGrade.size() + " " + grade + " - " + studentName + " - " + category);
				return;
			}
		}

		// Create the assignment if this is the first student to have the assignment
		categoryGrade.add(new Grade(assignment, grade));

		// Debug line
		System.out.println(categoryGrade.size() + " " + grade + " - " + studentName + " - " + category);
	}

	// Just shows what categories we have stored if called upon
	public Set<String> getCategories() {
		return grades.keySet();
	}

	// Shows the grades in each category
	public ArrayList<Grade> getGradesValue(String category) {
		return grades.get(category);
	}

}