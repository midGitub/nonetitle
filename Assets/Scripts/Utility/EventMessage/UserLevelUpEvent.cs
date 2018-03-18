using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class UserLevelUpEvent : CitrusGameEvent
{
	public LevelConfigData CurrLevelConfigData;

	public UserLevelUpEvent(LevelConfigData lcd) : base()
	{
		CurrLevelConfigData = lcd;
	}
}
