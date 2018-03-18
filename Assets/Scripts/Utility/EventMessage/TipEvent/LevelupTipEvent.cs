using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class LevelupTipEvent : CitrusGameEvent {
	private int _oldLevel;
	private int _newLevel;
	public int OldLevel{ get { return _oldLevel; } }
	public int NewLevel { get { return _newLevel; } }

	public LevelupTipEvent(int oldLv, int newLv) : base(){
		_oldLevel = oldLv;
		_newLevel = newLv;
	}
}
