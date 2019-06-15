import java.util.HashMap;
import java.util.LinkedList;
import java.util.Objects;
import java.util.Queue;

public class Vertex {
	private int _id;
	private static int _id_counter = 0;
	private final int resetValue = 0;
	private HashMap<Vertex, Integer> _edges = new HashMap<>();

	// cheater method for tracking path weight
	private int _path_weight = resetValue;

	// Stuff I added
	private int _load_factor = 1;
	private int _current_load = resetValue;
	private Queue<Vertex> _q = new LinkedList<Vertex>();

	protected Vertex() {
		_id_counter++;
		_id = _id_counter;
	}

	protected Vertex(int id) {
		if (id >= _id_counter) {
			_id_counter = id + 1;
		}
		_id = id;
	}

	protected Vertex(Vertex v) {
		_id = v.getId();
		_edges = v.getEdges();
		_path_weight = v.getPathWeight();
		_load_factor = v.getLoadFactor();
		_current_load = v.getPathWeight() * v.getLoadFactor();
		_q = v.getQueue();
	}

	protected void resetVertex() {
		_current_load = resetValue;
		_path_weight = resetValue;
		_q.clear();
	}

	protected void addVisit(Vertex v) {
		_q.add(v);
	}

	protected void inheritVisit(Vertex v) {
		_q = new LinkedList<Vertex>(v.getQueue());
	}

	protected Queue<Vertex> getQueue() {
		return _q;
	}

	protected int getCurrentLoad() {
		return _current_load;
	}

	protected void setCurrentLoad(int load) {
		_current_load = load;
	}

	protected int getLoadFactor() {
		return _load_factor;
	}

	protected void setLoadFactor(int load) {
		_load_factor = load;
	}

	protected int getPathWeight() {
		return _path_weight;
	}

	protected void setPathWeight(int weight) {
		_path_weight = weight;
	}

	protected int getId() {
		return _id;
	}

	protected void addEdge(Vertex vertex, int weight) {
		_edges.put(vertex, weight);
	}

	protected int getEdgeWeight(Vertex edge) {
		return _edges.get(edge);
	}

	protected HashMap<Vertex, Integer> getEdges() {
		return _edges;
	}

	protected void removeEdge(Vertex vertex) {
		_edges.remove(vertex);
	}

	// override the equals and hashCode function for hash map purposes
	// so that the Vertices are ONLY identified by their Ids
	// and only use Ids for hashing
	@Override
	public boolean equals(Object o) {

		if (o == this)
			return true;
		if (!(o instanceof Vertex)) {
			return false;
		}

		Vertex v = (Vertex) o;

		return this.getId() == v.getId();
	}

	@Override
	public int hashCode() {
		return Objects.hash(this.getId());
	}
}
