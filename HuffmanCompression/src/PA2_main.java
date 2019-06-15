import java.util.List;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Map;
import java.io.*;

public class PA2_main {

	public static void main(String[] args) {
		// TODO Auto-generated method stub
		PA2_main main_test = new PA2_main();
		
		try
		{
			//calling for unit tests?
			if (args.length == 1)
			{
				if (args[0].equals("test"))
				{
					main_test.pa2Test();
					main_test.pa2Test2();
				}
				
				main_test.outputUsage();
			}
			else if(args.length == 2)
			{
				if (args[0].equals("compress"))
				{
					String to_compress = args[1];
					System.out.println("Compressing file " + to_compress + "...");
					List<String> file_contents = main_test.readFile(to_compress);
					
					//PA #2: build tree
					HuffmanTree<Character> coding_tree = PA2.huffmanTreeFromText(file_contents);
					
					//PA #2: generate endoing map
					Map<Character, String> encoder = PA2.huffmanEncodingMapFromTree(coding_tree);
					
					//PROVIDED: convert file into list of bits
					List<Boolean> raw_stream = PA2.toBinary(file_contents, encoder);
					
					//PROVIDED: write list of bits to separate file
					String[] pieces = to_compress.split("\\.");
					String file_name = pieces[0];
					String extension = "";
					if (pieces.length > 1)
					{
						extension = pieces[1];
					}
					String output_file_name = file_name + ".pa2c";
					BinaryFile.writeToFile(raw_stream, output_file_name);
					
					//PA #2: write map to file
					String map_file = file_name + ".pa2m";
					PA2.writeEncodingMapToFile(encoder, map_file);				
				}
				else
				{
					main_test.outputUsage();
				}
			}
			else if (args.length == 3)
			{
				if (args[0].equals("decompress"))
				{
					System.out.println("Decompressing " + args[1] + "...");
					
					String to_decompress = args[1];
					String extract_location = args[2];
					
					//Separate extension from name in to_decompress
					String[] pieces = to_decompress.split("\\.");
					String file_name = pieces[0];
					String extension = "";
					if (pieces.length > 1)
					{
						extension = pieces[1];
					}
					
					//PA #2: get bits back from file
					List<Boolean> bits_from_file = BinaryFile.readFromFile(to_decompress);
					
					//PA #2: read map from file
					Map<Character, String> encoder_from_file = PA2.readEncodingMapFromFile(file_name + ".pa2m");
					
					//PA #2: convert file bits back into text
					String text = PA2.decodeBits(bits_from_file, encoder_from_file);
					
					//PA #2: write decompressed back to file
					FileOutputStream output_file = new FileOutputStream(new File(extract_location));
					output_file.write(text.getBytes());
					
					output_file.close();
				}
			}
			else
			{
				main_test.outputUsage();
			}
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}
	
	public static List<String> readFile(String input_file_name)
	{
		List<String> result = new ArrayList<>();
		try
		{
			BufferedReader br = new BufferedReader(new FileReader(input_file_name));
			String line = "";
			while( (line = br.readLine()) != null)
			{
				result.add(line);
			}
			br.close();
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}		

		return result;
}
	
	//Test function for PA2
	public void pa2Test()
	{
		String[] file_array = {"test.txt", "savio.txt", "kennedy.txt"};
		List<String> files = new ArrayList<>(Arrays.asList(file_array));
		
		try
		{
			for(String file: files)
			{
				System.out.println("Analyzing file: " + file + "...");
				
				//PROVIDED: read contents of file into vector of strings
				List<String> file_contents = readFile(file);
				
				//PA #2: build tree
				HuffmanTree<Character> coding_tree = PA2.huffmanTreeFromText(file_contents);
				
				//PA #2: generate encoding map
				Map<Character, String> encoder = PA2.huffmanEncodingMapFromTree(coding_tree);
				
				//PROVIDED: convert file into vector of bits
				List<Boolean> raw_stream = PA2.toBinary(file_contents, encoder);
				
				//PROVIDED: write list of bits to separate file
				String[] pieces = file.split("\\.");
				String file_name = pieces[0];
				String extension = "";
				if(pieces.length > 1)
				{
					extension = pieces[1];
				}
				
				String output_file_name = file_name + ".pa2c";
				BinaryFile.writeToFile(raw_stream, output_file_name);
				
				//PA #2: write map to file
				String map_file = file_name + ".pa2m";
				PA2.writeEncodingMapToFile(encoder, map_file);
				
				//PA #2: get bits back from file
				List<Boolean> bits_from_file = BinaryFile.readFromFile(output_file_name);
				
				//PA #2: read map from file
				Map<Character, String> encoder_from_file = PA2.readEncodingMapFromFile(map_file);
				
				//PA #2: convert file bits back into text
				String text = PA2.decodeBits(bits_from_file, encoder_from_file);
				
				//PA #2: write decompressed back to file
				output_file_name = file_name + "_1." + extension;
				FileOutputStream output_file = new FileOutputStream(new File(output_file_name));
				output_file.write(text.getBytes());
				
				output_file.close();				
			}
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}

	public void pa2Test2()
	{
		//TEST #1: smallest tree function
		List<HuffmanTree<Character>> forest = new ArrayList<>();
		forest.add(new HuffmanTree<Character>('a', 10));
		forest.add(new HuffmanTree<Character>('b', 9));
		forest.add(new HuffmanTree<Character>('c', 8));
		forest.add(new HuffmanTree<Character>('d', 7));
		forest.add(new HuffmanTree<Character>('e', 6));
		forest.add(new HuffmanTree<Character>('f', 5));
		
		System.out.println("findSmallestTree test");
		int smallest = PA2.findSmallestTree(forest);
		int second_smallest = PA2.findSmallestTree(forest, smallest);
		
		System.out.println("smallest: " + smallest + " (expected: 5)");
		System.out.println("second smallest: " + second_smallest + "(expected 4)");
		
		//TEST #2: huffman from tree test
		//PROVIDED: read contents of file into list of Strings
		String file_name = "pa2test.txt";
		List<String> file_contents = readFile(file_name);
		
		//PA #2: build tree
		HuffmanTree<Character> coding_tree = PA2.huffmanTreeFromText(file_contents);
		
		//PA #2: generate encoding map
		Map<Character, String> encoder = PA2.huffmanEncodingMapFromTree(coding_tree);
		
		//test encoding map
		System.out.println("Encoding map from tree test");
		for (Character key: encoder.keySet())
		{
			if (key == '\n') {
				System.out.println("char: " + "\\n" + " val: " + encoder.get(key));
				continue;
			}
			if (key == '\r') {
				System.out.println("char: " + "\\r" + " val: " + encoder.get(key));
				continue;
			}
			System.out.println("char: " + key + " val: " + encoder.get(key));
		}
		
		//TEST #3: write map to file
		List<Boolean> raw_stream = PA2.toBinary(file_contents, encoder);
		String map_file = file_name + ".pa2m";
		PA2.writeEncodingMapToFile(encoder, map_file);
		String output_file_name = file_name + ".pa2c";
		BinaryFile.writeToFile(raw_stream, output_file_name);
		
		//TEST #4: get bits back from file
		List<Boolean> bits_from_file = BinaryFile.readFromFile(output_file_name);
		Map<Character, String> encoder_from_file = PA2.readEncodingMapFromFile(map_file);
		String text = PA2.decodeBits(bits_from_file, encoder_from_file);
		System.out.println("decoded bits:");
		System.out.println(text);
	}
	
	//output PA2 usage
	public void outputUsage()
	{
		System.out.println("Usage: parameters list: <Action> <File1> <File2>");
		System.out.println("Actions (must be lower case):");
		System.out.println("test - runs built-in unit test functions");
		System.out.println("compress - compresses <File1>");
		System.out.println("decompress - decompresses <File1> into <File2>");
	}
}
