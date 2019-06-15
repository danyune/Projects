public abstract class HuffmanNode<T> {
	protected int _weight;
	
	//gets weight of huffman node and children
	public abstract int getWeight();	
	
	//determines if node is a leaf or not
	public abstract boolean isLeaf();

}
