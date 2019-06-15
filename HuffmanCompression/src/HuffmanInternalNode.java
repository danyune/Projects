
public class HuffmanInternalNode<T> extends HuffmanNode<T> {
	
	private HuffmanNode<T> _left_child = null;
	private HuffmanNode<T> _right_child = null;
	
	public HuffmanInternalNode(HuffmanNode<T> left, HuffmanNode<T> right)
	{
		if (left != null && right != null)
		{
			_weight = left.getWeight() + right.getWeight();
		}
		_left_child = left;
		_right_child = right;
	}
	
	public HuffmanNode<T> getLeftChild()
	{
		return _left_child;
	}
	
	public HuffmanNode<T> getRightChild()
	{
		return _right_child;
	}
	
	public void setLeftChild(HuffmanNode<T> node)
	{
		_left_child = node;
	}
	
	public void setRightChild(HuffmanNode<T> node)
	{
		_right_child = node;
	}
	
	@Override
	public boolean isLeaf() {
		// TODO Auto-generated method stub
		return false;
	}

	@Override
	public int getWeight() {
		// TODO Auto-generated method stub
		return _weight;
	}

}
