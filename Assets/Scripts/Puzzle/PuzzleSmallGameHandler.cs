using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public enum PuzzleSmallGameStage
{
	None,
	Slide,
	FreeSpin,
	Rewind,
	FixWild,
	Collect,
	Wheel,
	TapBox,
	TapChip,
	SwitchSymbol,
	Count
}

public enum PuzzleSmallGameProcessState
{
	None,
	Ready,
	Done
}

public class PuzzleSmallGameHandler
{
	public delegate bool CheckDelegate(CoreSpinResult result, bool isSwitch);
	public delegate bool StepDelegate(CoreSpinResult result, Callback<bool> endCallback); //returns whether the action ends immediately

	PuzzleSmallGameStage _stage;
	PuzzleSmallGameProcessState _processState;
	CheckDelegate _checkFunc;
	StepDelegate _performFunc;

	public PuzzleSmallGameStage Stage { get { return _stage; } set { _stage = value; } }
	public PuzzleSmallGameProcessState ProcessState { get { return _processState; } set { _processState = value; } }
	public CheckDelegate CheckFunc { get { return _checkFunc; } set { _checkFunc = value; } }
	public StepDelegate PerformFunc { get { return _performFunc; } set { _performFunc = value; } }

	public PuzzleSmallGameHandler(PuzzleSmallGameStage stage, CheckDelegate checkFunc, StepDelegate performFunc)
	{
		_stage = stage;
		_processState = PuzzleSmallGameProcessState.None;
		_checkFunc = checkFunc;
		_performFunc = performFunc;
	}
}
