
public class HuffmanLeafNode<T> extends HuffmanNode<T> {

	private T _value;
	
	public HuffmanLeafNode(T value, int frequency)
	{
		_value = value;
		_weight = frequency;
	}
	
	public T getValue()
	{
		return _value;
	}
	
	@Override
	public int getWeight() {
		// TODO Auto-generated method stub
		return _weight;
	}

	@Override
	public boolean isLeaf() {
		// TODO Auto-generated method stub
		return true;
	}

}
