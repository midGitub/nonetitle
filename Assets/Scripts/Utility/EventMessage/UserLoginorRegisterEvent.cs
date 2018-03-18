using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class UserLoginOrRegisterEvent : CitrusGameEvent
{
	public string SocialID = "";

	public UserLoginOrRegisterEvent(string socialid) : base()
	{
		SocialID = socialid;
	}
}
