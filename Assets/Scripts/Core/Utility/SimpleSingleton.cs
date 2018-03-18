using System.Collections;
using System.Collections.Generic;

public class SimpleSingleton<T> where T : class, new()
{
	static private T _instance = null;
	static public T Instance
	{
		get
		{
			if(_instance == null)
				_instance = new T();
			return _instance;
		}
	}

	static public bool HasInstance()
	{
		return _instance != null;
	}

	protected SimpleSingleton()
	{
	}

	public void Destroy()
	{
		_instance = null;
	}
}
