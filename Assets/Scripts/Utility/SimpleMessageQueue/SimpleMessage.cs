using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public interface ISimpleMessage
{
	void Run();
}

public class SimpleMessage : ISimpleMessage
{
	Callback _callback;

	public SimpleMessage(Callback c)
	{
		_callback = c;
	}

	public void Run()
	{
		_callback.Invoke();
	}
}

public class SimpleMessage<T> : ISimpleMessage
{
	Callback<T> _callback;
	T _arg;

	public SimpleMessage(Callback<T> c, T arg)
	{
		_callback = c;
		_arg = arg;
	}

	public void Run()
	{
		_callback.Invoke(_arg);
	}
}

public class SimpleMessage<T, U> : ISimpleMessage
{
	Callback<T, U> _callback;
	T _arg1;
	U _arg2;

	public SimpleMessage(Callback<T, U> c, T arg1, U arg2)
	{
		_callback = c;
		_arg1 = arg1;
		_arg2 = arg2;
	}

	public void Run()
	{
		_callback.Invoke(_arg1, _arg2);
	}
}


