
public class HuffmanTree<T> {

	private HuffmanNode<T> _root;
	
	public HuffmanTree(T value, int weight)
	{
		_root = new HuffmanLeafNode<T>(value, weight);
	}
	
	public HuffmanTree(HuffmanInternalNode<T> root)
	{
		_root = root;
	}
	
	public HuffmanTree(HuffmanTree<T> left, HuffmanTree<T> right)
	{
		_root = new HuffmanInternalNode<T>(left.getRoot(), right.getRoot());
	}
	
	public HuffmanNode<T> getRoot()
	{
		return _root;
	}
	
	public int getWeight()
	{
		return _root.getWeight();
	}
}
