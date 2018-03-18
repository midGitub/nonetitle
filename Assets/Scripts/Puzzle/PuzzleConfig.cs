using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

[System.Serializable]
public class PostSpinTimeConfig
{
	public enum TimeType
	{
		NumberTick,
		SymbolBlink
	}

	public int _winRatio;
	public float _numberTickTime;
	public float _symbolBlinkTime;

	public float GetTime(TimeType type)
	{
		switch(type)
		{
			case TimeType.NumberTick:
				return _numberTickTime;
				break;
			case TimeType.SymbolBlink:
				return _symbolBlinkTime;
				break;
		}
		Debug.Assert(false);
		return 0.0f;
	}
}

[System.Serializable]
public class ScaleSettingOnIpad
{
	public float _leaderboardScaleOnIpad;
	public float _reelFrameScaleOnIpad;

	public ScaleSettingOnIpad(){
		_leaderboardScaleOnIpad = 1.1f;
		_reelFrameScaleOnIpad = 1.1f;
	}

	public ScaleSettingOnIpad(float value1, float value2){
		_leaderboardScaleOnIpad = value1;
		_reelFrameScaleOnIpad = value2;
	}
}

public class PuzzleConfig : MonoBehaviour
{
	public float[] _reelFreeSpinTime = new float[CoreDefine.MaxReelCount];
	public float[] _reelSpinTime = new float[CoreDefine.MaxReelCount];
	public float _topLightHighlightTime;
	public float _hypeTimeForReel34Machine;
	public float _hypeTimeForReel5Machine;
	public float _pressAutoSpinTime;
	public float _postSpinWaitTimeWhenRespin;

	public float _respinWinTickTime;
	public float _lowWinTickTime;
	public float _highWinTickTime;
	public float _bigWinTickTime;
	public float _epicWinTickTime;

//	public List<PostSpinTimeConfig> _winTimeList = new List<PostSpinTimeConfig>();
//	public List<PostSpinTimeConfig> _freeSpinTriggerTimeList = new List<PostSpinTimeConfig>();
//	public List<PostSpinTimeConfig> _freeSpinningTimeList = new List<PostSpinTimeConfig>();

	public ScaleSettingOnIpad M3OnIpad = new ScaleSettingOnIpad();
	public ScaleSettingOnIpad M4OnIpad = new ScaleSettingOnIpad();

	// adjust play audio delay
	[SerializeField]
	private float _rewindSwitchAudioDelay = 1.0f;
	[SerializeField]
	private float _rewindSpinReelAudioDelay = 0.7f;
	//	[SerializeField]
	private float _freeSpinReelAudioDelay = 0.4f;
	//	[SerializeField]
	private float _spinReelAudioDelay = 0.4f;
	//	[SerializeField]
	private float _tryStartRoundDelay = 0.4f;

	public bool UserGoldFinger = false;// 是否使用金手指
	public float[] _reelFreeSpinTimeGoldFinger = new float[CoreDefine.MaxReelCount];// 金手指freespin速度
	public float[] _reelSpinTimeGoldFinger = new float[CoreDefine.MaxReelCount];// 金手指普通spin速度

	public float rewindSwitchAudioDelay { get { return _rewindSwitchAudioDelay; } }
	public float rewindSpinReelAudioDelay { get { return _rewindSpinReelAudioDelay; } }
	public float freeSpinReelAudioDelay { get { return _freeSpinReelAudioDelay; } }
	public float spinReelAudioDelay { get { return _spinReelAudioDelay; } }
	public float tryStartRoundDelay { get { return _tryStartRoundDelay; } }

	public float DebugSymbolScaleFactor = 1.0f;

	void Start(){
		_reelFreeSpinTimeGoldFinger = _reelFreeSpinTime;
		_reelSpinTimeGoldFinger = _reelSpinTime;
	}

	public float GetNumberTickTime(PuzzleMachine machine)
	{
		float result = 0.0f;
		bool shouldRespin = machine.CoreMachine.ShouldRespin();
		if(shouldRespin)
		{
			if(machine.CoreMachine.SpinResult.WinRatio > 0.0f)
				result = _respinWinTickTime;
		}
		else
		{
			if(machine.CoreMachine.SpinResult.IsJackpotWin)
			{
				result = _epicWinTickTime;
			}
			else
			{
				switch(machine.GameData.WinType)
				{
					case WinType.Big:
						result = _bigWinTickTime;
						break;
					case WinType.Epic:
						result = _epicWinTickTime;
						break;
					case WinType.Normal:
						{
							NormalWinType normalWinType = machine.CoreMachine.SpinResult.NormalWinType;
							if(normalWinType == NormalWinType.Low)
								result = _lowWinTickTime;
							else if(normalWinType == NormalWinType.High)
								result = _highWinTickTime;
							else
								result = 0.0f;
						}
						break;
				}
			}
		}

		return result;
	}

//	public float GetSymbolBlinkTime(PuzzleMachine machine, int winRatio)
//	{
//		List<PostSpinTimeConfig> list = GetPostSpinTimeConfigList(machine);
//		float time = GetPostSpinTime(list, PostSpinTimeConfig.TimeType.SymbolBlink, winRatio);
//		float intTime = (int)Mathf.Floor(time); //the blink time should be an integer since the effect prefab has 1.0s period
//		return (float)intTime;
//	}

	public float[] GetReelSpinTime(PuzzleMachine machine, bool useDefault, float[] defaultValues)
	{
		if (UserGoldFinger)
		{
			if(machine.CoreMachine.SmallGameState == SmallGameState.FreeSpin)
			{
				return _reelFreeSpinTimeGoldFinger;
			}
			
			return _reelSpinTimeGoldFinger;
		}

		if(machine.CoreMachine.SmallGameState == SmallGameState.FreeSpin)
		{
			return _reelFreeSpinTime;
		}

		if (useDefault){
			return defaultValues;
		}
		
		return _reelSpinTime;
	}

	public void SetFreespinTimesGoldFinger(string str){
		UserGoldFinger = true;
		StringUtility.SetValueArray(ref _reelFreeSpinTimeGoldFinger, str, false);
	}

	public void SetSpinTimesGoldFinger(string str){
		UserGoldFinger = true;
		StringUtility.SetValueArray(ref _reelSpinTimeGoldFinger, str, false);
	}

	public void EnableGoldFinger(int state){
		UserGoldFinger = state == 1;
	}

//	private List<PostSpinTimeConfig> GetPostSpinTimeConfigList(PuzzleMachine puzzleMachine)
//	{
//		List<PostSpinTimeConfig> result = null;
//		CoreMachine coreMachine = puzzleMachine.CoreMachine;
//		#if false // zhousen
//		if(coreMachine.IsFreeSpinTriggered() || coreMachine.IsRewindTriggered())
//		#else
//		if (coreMachine.IsTriggerSmallGameState()
//			&& (coreMachine.SmallGameState == SmallGameState.FreeSpin
//				|| coreMachine.SmallGameState == SmallGameState.Rewind))
//		#endif
//		{
//			result = _freeSpinTriggerTimeList;
//		}
//		else if(coreMachine.SmallGameState == SmallGameState.FreeSpin
//			|| coreMachine.SmallGameState == SmallGameState.Rewind)
//		{
//			result = _freeSpinningTimeList;
//		}
//		else
//		{
//			result = _winTimeList;
//		}
//		return result;
//	}
//
//	private float GetPostSpinTime(List<PostSpinTimeConfig> configList, PostSpinTimeConfig.TimeType timeType, int winRatio)
//	{
//		//Debug.Assert(winRatio >= 1);
//
//		float result = 0.0f;
//		Debug.Assert(configList.Count > 0);
//
//		PostSpinTimeConfig maxConfig = configList[configList.Count - 1];
//		if(winRatio >= maxConfig._winRatio)
//		{
//			result = maxConfig.GetTime(timeType);
//		}
//		else
//		{
//			int fallIndex = 0;
//			for(int i = configList.Count - 2; i >= 0; i--)
//			{
//				if(winRatio >= configList[i]._winRatio)
//				{
//					fallIndex = i;
//					break;
//				}
//			}
//
//			PostSpinTimeConfig lowConfig = configList[fallIndex];
//			PostSpinTimeConfig upConfig = configList[fallIndex + 1];
//			float factor = (float)(winRatio - lowConfig._winRatio) / (float)(upConfig._winRatio - lowConfig._winRatio);
//			result = Mathf.Lerp(lowConfig.GetTime(timeType), upConfig.GetTime(timeType), factor);
//		}
//
//		return result;
//	}
}
