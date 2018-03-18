using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class CallbackCount
{
	private Callback _callback;
	private int _count = 0;
	private bool _isExecuted = false;

	public CallbackCount(Callback callback)
	{
		_callback = callback;
	}

	public void IncreaseCount()
	{
		++_count;
	}

	public void DecreaseCount()
	{
		--_count;
		if(_count == 0 && !_isExecuted)
		{
			_callback();
			_isExecuted = true;
		}
	}
}
