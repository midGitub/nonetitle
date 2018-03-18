using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class SpinEndEvent : CitrusGameEvent {

	public ulong WinAmount;
	public float WinRatio;

	public SpinEndEvent(ulong winAmount, float winRatio)
	{
		WinAmount = winAmount;
		WinRatio = winRatio;
	}
}
