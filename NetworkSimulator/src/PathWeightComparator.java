import java.util.Comparator;

public class PathWeightComparator implements Comparator<Vertex>{

	@Override
	public int compare(Vertex v1, Vertex v2) {
		return (v1.getCurrentLoad() - v2.getCurrentLoad()) <= 0 ? -1 : 1;
	} 
}
