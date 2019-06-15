import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Queue;
import java.util.Scanner;
import java.util.Stack;
import java.util.stream.Stream;

public class PA3_main {

	// Global variables each algorithm can use
	private static List<Vertex> vertices = new ArrayList<>();
	private static List<String> inputs = new ArrayList<String>(); // 0 is file name, 1 is message

	private static Graph graph = new Graph();
	private static int starting = 0;
	private static int ending = 0;

	public static void main(String[] args) {

		// 1) Use as "test filename.txt" to have pre-set elements from bob's files
		// 2) Input just "filename.txt" and allow assigning of starting, ending, and
		// message
		// 3) Args can be empty but must assign a file

		// Change inputs.add here to a premade text file to test it quickly
		inputs.add("graph3.txt");

		if (!inputs.isEmpty()) {
			try {
				preAssignVariables(inputs.get(0));
				transmitMessage(inputs);
			} catch (Exception e) {
				e.printStackTrace();
				;
			}
		} else {
			try {
				if (args.length == 1) {
					inputs.add(args[0]);
					assignVariables();
					transmitMessage(inputs);
				} else if (args.length == 2) {
					if (args[0].equals("test")) {
						inputs.add(args[1]);
						preAssignVariables(args[1]);
						transmitMessage(inputs);
					}
				} else {
					assignVariables();
					transmitMessage(inputs);
				}
			} catch (Exception e) {
				e.printStackTrace();
			}
		}

	}

	private static void transmitMessage(List<String> inputs) {

		ArrayList<Node> list = new ArrayList<Node>();
		Queue<Node> startqueue = new LinkedList<Node>();

		// I use a stack because on graph2 @ time 172, yours send 'd' then 'e'
		//
		// A Queue would send mine as 'e' then 'd', a stack made it 'd' then 'e'
		Stack<Node> midstack = new Stack<Node>();

		for (int i = 0; i < inputs.get(1).length(); i++) {
			startqueue.add(new Node(inputs.get(1).charAt(i), vertices.get(starting - 1), vertices.get(ending - 1)));
		}

		// Variables necessary for this while loop
		int timer = 1;
		boolean transmitting = true;

		// Begin transmitting message
		while (transmitting) {

			// This part does initial sending in proper sequence
			if (!startqueue.isEmpty()) {
				partyBegins(startqueue.poll(), list, timer);
			}

			// Reduce currently traveling packets wait by 1 and determine if they need to
			// move to next node or arrived at final destination
			adjustTime(list, midstack, timer);

			// Send packets to next vertex/node if arrived
			while (!midstack.isEmpty()) {
				midTransmission(midstack.pop(), timer);
			}

			// Checks if a packet is still en route
			transmitting = keepGoing(list);
			timer++;
		}
		printEverything(list);
	}

	private static void partyBegins(Node currentNode, ArrayList<Node> list, int currentTime) {
		list.add(currentNode);
		nodeAdjustment(currentNode);
		currentNode.setArrival(currentNode.getWait() + currentTime);
		System.out.println("Sending packet " + currentNode.getItem() + " vertex " + currentNode.getDest().getId()
				+ " wait of " + currentNode.getWait() + " time " + currentTime);
	}

	private static void midTransmission(Node currentNode, int currentTime) {
		currentNode.downLoadFactor();
		currentNode.setCurrent(currentNode.getDest());
		nodeAdjustment(currentNode);
		currentNode.setArrival(currentNode.getArrival() + currentNode.getWait());
		System.out.println("Sending packet " + currentNode.getItem() + " vertex " + currentNode.getDest().getId()
				+ " wait of " + currentNode.getWait() + " time " + currentTime);
	}

	private static void nodeAdjustment(Node currentNode) {
		HashMap<Integer, Vertex> quickestPaths = graph.computeShortestPath(currentNode.getCurrent());
		currentNode.addVisit();
		currentNode.setDest(quickestPaths.get(ending).getQueue().poll());
		currentNode.setWait(currentNode.getDest().getCurrentLoad());
		currentNode.upLoadFactor();
	}

	private static void adjustTime(ArrayList<Node> list, Stack<Node> midstack, int currentTime) {
		list.forEach(n -> {
			if (n.getArrival() == currentTime) {
				if (!n.getDest().equals(n.getFinal())) {
					midstack.push(n);
				} else {
					nodeArrived(n);
				}
			}
		});
	}

	private static void nodeArrived(Node n) {
		n.downLoadFactor();
		n.setCurrent(n.getFinal());
		n.addVisit();
		n.setArrived(true);
	}

	private static boolean keepGoing(ArrayList<Node> node) {
		for (Node n : node) {
			if (!n.hasArrived())
				return true;
		}
		return false;
	}

	private static void makeGraph() {

		try (Stream<String> stream = Files.lines(Paths.get(inputs.get(0)))) {
			stream.forEach(s -> {
				String[] split = s.split(" ");
				if (split.length == 1) {
					vertices.add(new Vertex());
				} else {
					vertices.get(Integer.parseInt(split[0]) - 1).addEdge(vertices.get(Integer.parseInt(split[1]) - 1),
							Integer.parseInt(split[2]));
				}
			});

		} catch (IOException e) {
			e.printStackTrace();
		}

		vertices.forEach(v -> graph.addVertex(v));
	}

	private static void preAssignVariables(String input) {
		starting = 1;
		inputs.add("Hello, World!");
		if (input.equals("graph2.txt"))
			ending = 10;
		else if (input.equals("graph3.txt"))
			ending = 23;
		else if (input.equals("graph4.txt"))
			ending = 3;
		else if (input.equals("test5.txt")) {
			ending = 2;
			inputs.set(1, "Hello!");
		} else if (input.equals("dan.txt"))
			ending = 4;

		getPrompts(1);
		System.out.println(input);
		makeGraph();
		getPrompts(2);
		System.out.println(starting);
		getPrompts(3);
		System.out.println(ending);
		getPrompts(4);
		System.out.println(inputs.get(1));

	}

	private static void assignVariables() {
		Scanner sc = new Scanner(System.in);
		if (inputs.isEmpty()) {
			File file;
			String filename;
			do {
				getPrompts(0);
				filename = sc.nextLine();
				file = new File(filename);
			} while (!file.exists());
			inputs.add(filename);
		} else {
			getPrompts(1);
			System.out.println(inputs.get(0));
		}
		makeGraph();
		do {
			getPrompts(2);
			starting = sc.nextInt();
		} while (starting < vertices.get(0).getId() || starting > vertices.get(vertices.size() - 1).getId());

		do {
			getPrompts(3);
			ending = sc.nextInt();
			// Eat the '/r' since next prompt is a full line
			// Need it here incase user puts a bad input multiple times
			sc.nextLine();
		} while (ending < vertices.get(0).getId() || ending > vertices.get(vertices.size() - 1).getId());

		getPrompts(4);
		inputs.add(sc.nextLine());

		sc.close();
	}

	private static void getPrompts(int i) {
		List<String> prompts = Arrays.asList("Enter graph file: ", "Graph file: ", "Enter a starting vertex: ",
				"Enter a destination vertex: ", "Enter a message to transmit: ");
		if (!vertices.isEmpty()) {
			int first = vertices.get(0).getId();
			int second = vertices.get(vertices.size() - 1).getId();
			String range = " between " + first + " and " + second + ": ";
			prompts.set(2, prompts.get(2).substring(0, prompts.get(2).length() - 2) + range);
			prompts.set(3, prompts.get(3).substring(0, prompts.get(3).length() - 2) + range);
		}
		System.out.print(prompts.get(i));
	}

	private static void printEverything(ArrayList<Node> node) {
		System.out.println(String.format("%-10s", "Packet") + String.format("%-20s", "Arrival Time") + "Route");
		for (Node n : node) {
			System.out.print(String.format("%-10c", n.getItem()));
			System.out.print(String.format("%-20d", n.getArrival()));
			Integer maxlength = (vertices.get(vertices.size() - 1).getId()) < 10 ? 10
					: vertices.get(vertices.size() - 1).getId();
			// If node has 3 digits, then append 2 zeroes in front of single digit, etc.
			// Default 2-digit even if max node length is single digit
			String format = "%0" + maxlength.toString().length() + "d, ";
			n.getVisits().forEach(i -> System.out.print(String.format(format, i.getId())));
			System.out.println();
		}
		System.out.println("\nFile tested: " + inputs.get(0) + " from vertex " + starting + " to vertex " + ending
				+ " with message: \n" + "\"" + inputs.get(1) + "\"");
	}

}