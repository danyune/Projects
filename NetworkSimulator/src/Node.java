import java.util.ArrayList;

public class Node {

	private char _item;
	private int _wait = 0;
	private boolean _arrived = false;
	private int _arrivalTime = 0;
	private Vertex _current = null;
	private Vertex _dest = null;
	private Vertex _final = null;
	private ArrayList<Vertex> _vList = new ArrayList<Vertex>();

	protected Node() {

	}

	protected Node(char c, Vertex s, Vertex f) {
		_item = c;
		_current = s;
		_final = f;
	}
	
	protected void upLoadFactor() {
		_current.setLoadFactor(_current.getLoadFactor() + 1);
		_dest.setLoadFactor(_dest.getLoadFactor() + 1);
	}
	
	protected void downLoadFactor() {
		_current.setLoadFactor(_current.getLoadFactor() - 1);
		_dest.setLoadFactor(_dest.getLoadFactor() - 1);
	}

	protected int getArrival() {
		return _arrivalTime;
	}

	protected void setArrival(int i) {
		_arrivalTime = i;
	}

	protected boolean hasArrived() {
		return _arrived;
	}

	protected void setArrived(boolean b) {
		_arrived = b;
	}

	protected char getItem() {
		return _item;
	}

	protected int getWait() {
		return _wait;
	}

	protected void setWait(int i) {
		_wait = i;
	}

	protected ArrayList<Vertex> getVisits() {
		return _vList;
	}

	protected void addVisit() {
		_vList.add(_current);
	}

	protected Vertex getCurrent() {
		return _current;
	}

	protected void setCurrent(Vertex v) {
		_current = v;
	}

	protected Vertex getDest() {
		return _dest;
	}

	protected void setDest(Vertex v) {
		_dest = v;
	}

	protected Vertex getFinal() {
		return _final;
	}
}
