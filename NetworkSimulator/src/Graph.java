import java.util.HashMap;
import java.util.PriorityQueue;

public class Graph {
	private HashMap<Integer, Vertex> _vertices = new HashMap<>();

	protected void addVertex(Vertex vertex) {
		_vertices.put(vertex.getId(), vertex);
	}

	protected HashMap<Integer, Vertex> computeShortestPath(Vertex start) {

		// holds known distances
		HashMap<Integer, Vertex> discovered = new HashMap<>();

		// underlying heap
		PriorityQueue<Vertex> dijkstra_queue = new PriorityQueue<>(new PathWeightComparator());

		// Vertices is stored as <Integer, Vertex>
		_vertices.forEach((v, i) -> i.resetVertex());

		// Added this check from your MA8 solution
		if (_vertices.get(start.getId()) != null) {

			dijkstra_queue.add(start);

			while (!dijkstra_queue.isEmpty()) {
				Vertex test = dijkstra_queue.poll();
				test.getEdges().forEach((v, i) -> {
					v.setPathWeight(i);
					if (!discovered.containsKey(v.getId())) {
						if (test.getCurrentLoad() + (i * v.getLoadFactor()) < v.getCurrentLoad()
								|| v.getCurrentLoad() == 0) {
							v.setCurrentLoad((v.getPathWeight() * v.getLoadFactor()) + test.getCurrentLoad());
							v.inheritVisit(test);
							v.addVisit(v);
							dijkstra_queue.add(v);
						}
					}
				});
				discovered.put(test.getId(), test);
			}
		}
		return discovered;
	}
}
