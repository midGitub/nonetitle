using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMessageQueue
{
	List<ISimpleMessage> _messages = new List<ISimpleMessage>();

	public void Add(ISimpleMessage m)
	{
		_messages.Add(m);
	}

	public void Remove(ISimpleMessage m)
	{
		_messages.Remove(m);
	}

	public void Clear()
	{
		_messages.Clear();
	}

	public void Run()
	{
		for(int i = 0; i < _messages.Count; i++)
			_messages[i].Run();
		
		Clear();
	}
}
