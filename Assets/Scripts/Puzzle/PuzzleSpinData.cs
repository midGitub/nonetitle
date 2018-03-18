using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSpinData
{
	private float _numberTickTime;
	private float _symbolBlinkTime;
	private bool _isUserTriggeredNextSpin;
	private bool _isUserForcedHalt;
	private AudioType _spinSound;
	private Coroutine _endPostSpinCoroutine;

	public float NumberTickTime { get { return _numberTickTime; } }
	public float SymbolBlinkTime { get { return _symbolBlinkTime; } }
    public bool IsUserTriggeredNextSpin { get { return _isUserTriggeredNextSpin; } set { _isUserTriggeredNextSpin = value; } }
	public bool IsUserForcedHalt { get { return _isUserForcedHalt; } set { _isUserForcedHalt = value; } }
	public AudioType SpinSound { get { return _spinSound; } set { _spinSound = value; } }
	public Coroutine EndPostSpinCoroutine { get { return _endPostSpinCoroutine; } set { _endPostSpinCoroutine = value; } }

	public void Clear()
	{
		_numberTickTime = 0.0f;
		_symbolBlinkTime = 0.0f;
        _isUserTriggeredNextSpin = false;
		_isUserForcedHalt = false;
		_spinSound = AudioType.None;
		_endPostSpinCoroutine = null;
	}

	public void SetNumberTickTime(float value)
	{
		if(value > _numberTickTime)
			_numberTickTime = value;
	}

	public void SetSymbolBlinkTime(float value)
	{
		if(value > _symbolBlinkTime)
			_symbolBlinkTime = value;
	}

    public float GetPostSpinWaitTime()
	{
		return Mathf.Max(_numberTickTime, _symbolBlinkTime);
	}
}
