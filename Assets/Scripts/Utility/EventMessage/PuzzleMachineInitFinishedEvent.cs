using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class PuzzleMachineInitFinishedEvent : CitrusGameEvent
{
	public PuzzleMachine Machine;

	public PuzzleMachineInitFinishedEvent(PuzzleMachine pm) : base()
	{
		Machine = pm;
	}
}
