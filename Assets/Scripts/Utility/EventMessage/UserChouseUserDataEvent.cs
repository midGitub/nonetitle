using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class UserChouseUserDataEvent : CitrusGameEvent
{
	public UserChouseDataType type;

	public UserChouseUserDataEvent(UserChouseDataType tp) : base()
	{
		type = tp;
	}
}
