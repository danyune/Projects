import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileWriter;
import java.io.IOException;
import java.io.Writer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Scanner;
import java.util.Set;
import java.util.TreeMap;

public class PA2 {

	public static int findSmallestTree(List<HuffmanTree<Character>> forest) {
		return findSmallestTree(forest, -1); // find the real smallest
	}

	public static int findSmallestTree(List<HuffmanTree<Character>> forest, int index_to_ignore) {

		// This means the forest has only one tree, so it will be the smallest and next
		// smallest.
		if (forest.size() == 1) {
			return -1;
		}

		// Initializing variables to an index impossible in the forest
		int nextSmallest = -1;
		int nextSmallestIndex = -1;
		for (int i = 0; i < forest.size(); i++) {

			if (forest.get(i).getWeight() < nextSmallest || nextSmallest == -1) {
				if (i != index_to_ignore) {
					nextSmallest = forest.get(i).getWeight();
					nextSmallestIndex = i;
				}
			}
		}

		// Should have found the next smallest index from index_to_ignore
		return nextSmallestIndex;
	}

	public static HuffmanTree<Character> huffmanTreeFromText(List<String> data) {

		// Initialize a HashMap
		Map<Character, Integer> result = new HashMap<Character, Integer>();

		// Initialize a blank string to begin concatenating
		String str = "";

		int counter = 0;
		for (String s : data) {
			str += s;
			// Add \r\n for the end of each line with the exception of the last one
			if (++counter != data.size()) {
				str += "\r\n";
			}
		}

		// Start adding the characters and frequency to a hash map, making a new entry
		// if it doesn't exist, otherwise add 1 to the value if it already exists.
		for (int i = 0; i < str.length(); i++) {
			char c = str.charAt(i);
			if (result.containsKey(c)) {
				int frequency = result.get(c);
				result.replace(c, ++frequency);
			} else {
				result.put(c, 1);
			}
		}

		// Array of HuffmanTrees to store each individual tree and frequency
		List<HuffmanTree<Character>> treeList = new ArrayList<>();

		// Store trees into the list using lambda function
		result.forEach((key, weight) -> treeList.add(new HuffmanTree<Character>(key, weight)));

		// Calls findSmallestTree twice to find smallest and second smallest, and merge
		// them into a single tree. Continue to do this until there's only one massive
		// tree leftover.

		while (treeList.size() > 1) {
			int smallest = findSmallestTree(treeList);
			int secondSmallest = findSmallestTree(treeList, smallest);

			HuffmanTree<Character> merged_tree = new HuffmanTree<Character>(treeList.get(smallest),
					treeList.get(secondSmallest));

			treeList.set(smallest, merged_tree);
			treeList.remove(secondSmallest);
		}

		// Return the only tree leftover, a massive tree
		return treeList.get(0);
	}

	public static HuffmanTree<Character> huffmanTreeFromMap(Map<Character, String> huffmanMap) {

		// Create a tree with only an internal node root
		HuffmanInternalNode<Character> node = new HuffmanInternalNode<Character>(null, null);
		HuffmanTree<Character> tree = new HuffmanTree<Character>(node);

		for (Map.Entry<Character, String> temp : huffmanMap.entrySet()) {
			char c = temp.getKey();
			String s = temp.getValue();
			huffmanTreeFromMapHelper(tree, node, c, s);
		}

		// Once all nodes are put together, return that same tree
		return tree;
	}

	protected static HuffmanTree<Character> huffmanTreeFromMapHelper(HuffmanTree<Character> tree,
			HuffmanNode<Character> node, char c, String s) {

		// Just a check to signify that we're still traversing through Internal Nodes
		if (s.length() > 1) {

			// Check the next # in the binary sequence
			char digit = s.charAt(0);

			// Once we recorded the binary number, cut off that number and slowly shorten
			// the code until it's one digit left
			s = s.substring(1);

			// Means make an internal node to the left since code length is > 1
			if (digit == '0') {
				HuffmanInternalNode<Character> internalNode = (HuffmanInternalNode<Character>) node;
				HuffmanInternalNode<Character> dummyNode = null;

				// Checks if a left child already exists or not, that way we don't replace it
				// with a new one if there is already one with or without children
				if (internalNode.getLeftChild() == null) {
					dummyNode = new HuffmanInternalNode<Character>(null, null);
					internalNode.setLeftChild(dummyNode);
				} else {
					dummyNode = (HuffmanInternalNode<Character>) internalNode.getLeftChild();
				}

				// Recursion call with left child as new root
				huffmanTreeFromMapHelper(tree, dummyNode, c, s);

				// Means right
			} else {
				HuffmanInternalNode<Character> internalNode = (HuffmanInternalNode<Character>) node;
				HuffmanInternalNode<Character> dummyNode = null;

				// Checks if a right child already exists or not, that way we don't replace it
				// with a new one if there is already one with or without children
				if (internalNode.getRightChild() == null) {
					dummyNode = new HuffmanInternalNode<Character>(null, null);
					internalNode.setRightChild(dummyNode);
				} else {
					dummyNode = (HuffmanInternalNode<Character>) internalNode.getRightChild();
				}

				// Recursion call with right child as new root
				huffmanTreeFromMapHelper(tree, dummyNode, c, s);
			}
			// Means s.length() == 1, so this is the last code digit, meaning whichever
			// direction we go will be a leafNode
		} else {

			// Get that digit to decide whether the leaf node will be on the left or right
			char digit = s.charAt(0);

			// Leaf node goes on the left
			if (digit == '0') {
				HuffmanLeafNode<Character> newNode = new HuffmanLeafNode<Character>(c, -1);
				HuffmanInternalNode<Character> dummyNode = (HuffmanInternalNode<Character>) node;
				dummyNode.setLeftChild(newNode);

				// Leaf node goes on the right
			} else {
				HuffmanLeafNode<Character> newNode = new HuffmanLeafNode<Character>(c, -1);
				HuffmanInternalNode<Character> dummyNode = (HuffmanInternalNode<Character>) node;
				dummyNode.setRightChild(newNode);
			}
		}

		// Return the tree
		return tree;
	}

	public static Map<Character, String> huffmanEncodingMapFromTree(HuffmanTree<Character> tree) {

		// Make a new HashMap to return
		Map<Character, String> result = new HashMap<>();

		// All the work done in this algorithm
		huffmanEncodingMapFromTreeHelper(tree.getRoot(), result, "");

		// Return the HashMap that the Helper filled out
		return result;
	}

	protected static Map<Character, String> huffmanEncodingMapFromTreeHelper(HuffmanNode<Character> node,
			Map<Character, String> map, String binaryString) {

		// First check if the node brought in is an Internal node or Leaf node
		if (node.isLeaf()) {
			HuffmanLeafNode<Character> leafNode = (HuffmanLeafNode<Character>) node;
			char c = leafNode.getValue();
			map.put(c, binaryString);
		} else {
			HuffmanInternalNode<Character> internalNode = (HuffmanInternalNode<Character>) node;

			if (internalNode.getLeftChild() != null) {
				huffmanEncodingMapFromTreeHelper(internalNode.getLeftChild(), map, binaryString + "0");
			}

			if (internalNode.getRightChild() != null) {
				huffmanEncodingMapFromTreeHelper(internalNode.getRightChild(), map, binaryString + "1");
			}
		}
		// Return the filled out map
		return map;
	}

	public static void writeEncodingMapToFile(Map<Character, String> huffmanMap, String file_name) {

		// I did this algorithm early on so I'm not sure why I used a treeMap aside from
		// maybe wanting to have it in order
		Map<Character, String> orderedMap = new TreeMap<Character, String>(huffmanMap);
		Set<Entry<Character, String>> orderedSet = orderedMap.entrySet();
		Iterator<Entry<Character, String>> orderedIterator = orderedSet.iterator();

		try {
			Writer w = new FileWriter(file_name);

			while (orderedIterator.hasNext()) {
				Map.Entry<Character, String> codeMap = (Map.Entry<Character, String>) orderedIterator.next();
				if (codeMap.getKey() == '\n') {
					w.write("\\n");
				} else if (codeMap.getKey() == '\r') {
					w.write("\\r");
				} else {
					w.write(codeMap.getKey());
				}
				w.write("||");
				w.write(codeMap.getValue());
				if (orderedIterator.hasNext()) {
					w.write("\r\n");
				}
			}
			w.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	public static Map<Character, String> readEncodingMapFromFile(String file_name) {
		// Don't think this one can take in anything that won't work

		// Initialize a HashMap
		Map<Character, String> result = new HashMap<>();

		// Start reading from the file, nothing too fancy
		try {
			File inFile = new File(file_name);
			Scanner sc = new Scanner(inFile);
			while (sc.hasNext()) {
				String splitCode[] = sc.next().split("\\|\\|");

				char c;
				// In one of the files, there's a space as the key, needed to consider it as
				// empty, and empty will be considered as a space
				if (splitCode[0].isEmpty()) {
					c = ' ';
				} else if (splitCode[0].equals("\\r")) {
					c = '\r';
				} else if (splitCode[0].equals("\\n")) {
					c = '\n';
				} else {
					c = splitCode[0].charAt(0);
				}
				result.put(c, splitCode[1]);
				if (sc.hasNextLine()) {
					sc.nextLine();
				}
			}
			sc.close();
		} catch (FileNotFoundException e) {
			e.printStackTrace();
		}

		return result;
	}

	public static String decodeBits(List<Boolean> bits, Map<Character, String> huffmanMap) {

		// Initialize some variables
		HuffmanTree<Character> tree = huffmanTreeFromMap(huffmanMap);
		HuffmanNode<Character> node = tree.getRoot();

		// Use a StringBuilder to append results.
		StringBuilder result = new StringBuilder();

		// Only if root is leaf, may only happen with 1 character map
		if (node.isLeaf()) {
			HuffmanLeafNode<Character> leafNode = (HuffmanLeafNode<Character>) node;
			result.append(leafNode.getValue());
			node = tree.getRoot();

		} else {
			// Traverses the tree depending on bool value
			for (boolean bool : bits) {
				HuffmanInternalNode<Character> internalNode = (HuffmanInternalNode<Character>) node;
				node = bool ? internalNode.getRightChild() : internalNode.getLeftChild();

				if (node.isLeaf()) {
					HuffmanLeafNode<Character> leafNode = (HuffmanLeafNode<Character>) node;
					result.append(leafNode.getValue());
					node = tree.getRoot();
				}
			}
		}
		return result.toString();
	}

	public static List<Boolean> toBinary(List<String> text, Map<Character, String> huffmanMap) {

		// Initialize a boolean ArrayList, basically a big list of True or False
		List<Boolean> result = new ArrayList<>();

		// I'm keeping this so I can write in the \r\n. Lambda does not allow changes
		String str = "";
		int counter = 0;
		for (String s : text) {
			str += s;
			if (++counter != text.size()) {
				str += "\r\n";
			}
		}

		// Lets re-use the array brought in
		text.clear();
		str.chars().forEachOrdered(i -> text.add(huffmanMap.get((char) i)));
		
		text.forEach(s -> s.chars().forEachOrdered(i -> result.add(i == '1' ? true : false)));

		return result;
	}
}
