using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class PuzzleSpinManager
{
	private PuzzleMachine _machine;
	private MachineConfig _machineConfig;

	private float[] _spinTimes;
	private bool[] _shouldHypes;
	private bool _shouldAnyHype;
	private int _firstHypeIndex = -1;
	private int _lastHypeIndex = -1;
	private int[] _hypeReelIndexes;
	private AudioType _hypeAudio = AudioType.HypeSpin;

	public PuzzleSpinManager(PuzzleMachine machine)
	{
		_machine = machine;
		_machineConfig = _machine.MachineConfig;
		_spinTimes = new float[_machineConfig.BasicConfig.ReelCount];
		_shouldHypes = new bool[_machineConfig.BasicConfig.ReelCount];
		_hypeReelIndexes = _machineConfig.BasicConfig.HypeReelIndexes;
	}

	public void StartSpinReels(CoreSpinResult spinResult)
	{
		RefreshShouldHypes(spinResult);
		RefreshSpinTimes(spinResult);
		SpinReels(spinResult);
	}

	private void RefreshShouldHypes(CoreSpinResult spinResult)
	{
		_firstHypeIndex = -1;
		_lastHypeIndex = -1;
		ListUtility.FillElements(_shouldHypes, false);
		_shouldAnyHype = false;

		if(PuzzleUtility.CanSmallGameStateHype(_machine.CoreMachine.SmallGameState))
		{
			ListUtility.ForEach(_hypeReelIndexes, (int reelIndex) => {
				_shouldHypes[reelIndex] = _machine.CoreMachine.BaseChecker.ShouldHype(spinResult, reelIndex);
			});

			_shouldAnyHype = ListUtility.IsAnyElementSatisfied(_shouldHypes, (bool b) => {
				return b;
			});

			for(int i = 0; i < _shouldHypes.Length; i++)
			{
				if(_shouldHypes[i])
				{
					_firstHypeIndex = i;
					break;
				}
			}

			for(int i = _shouldHypes.Length - 1; i >= 0; i--)
			{
				if(_shouldHypes[i])
				{
					_lastHypeIndex = i;
					break;
				}
			}
		}
	}

	private void RefreshSpinTimes(CoreSpinResult spinResult)
	{
		float[] configSpinTimes;
		if (_machineConfig.BasicConfig.ReelSpinTimes.Length > 0){
			configSpinTimes = _machine.PuzzleConfig.GetReelSpinTime(_machine, true, _machineConfig.BasicConfig.ReelSpinTimes);
		}else{
			configSpinTimes = _machine.PuzzleConfig.GetReelSpinTime(_machine, false, null);
		}

		float[] deltaTimes = new float[configSpinTimes.Length];
		configSpinTimes.CopyTo(deltaTimes, 0);

		ListUtility.ForEach(_hypeReelIndexes, (int reelIndex) => {
			if(_shouldHypes[reelIndex])
			{
				if(_machineConfig.BasicConfig.IsFiveReel)
					deltaTimes[reelIndex] = _machine.PuzzleConfig._hypeTimeForReel5Machine;
				else
					deltaTimes[reelIndex] = _machine.PuzzleConfig._hypeTimeForReel34Machine;
			}
		});

		float time = 0.0f;
		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			time += deltaTimes[i];
			_spinTimes[i] = time;
		}
	}

	private void SpinReels(CoreSpinResult spinResult)
	{
		SpinDirection spinDir = (spinResult.IsReversedSpin ? SpinDirection.Up : SpinDirection.Down);

		List<int> stopIndexes = spinResult.StopIndexes;
		for(int i = 0; i < stopIndexes.Count; i++)
//		for(int i = 0; i < 1; i++)
		{
			int stopIndex = stopIndexes[i];
			PuzzleReel reel = _machine.ReelList[i];

			//don't spin when fixed
			if(spinResult.IsFixedList[i])
			{
				reel.FixReel();
				continue;
			}

			reel.Spin(stopIndex, spinDir, _spinTimes[i], CheckHype);
		}
	}

	private void CheckHype(PuzzleReel reel)
	{
		if(_shouldAnyHype)
		{
			int reelIndex = reel.ReelIndex;
			int nextIndex = reelIndex + 1;
			int reelCount = _machineConfig.BasicConfig.ReelCount;

			if(nextIndex == _firstHypeIndex && !_machine.SpinData.IsUserForcedHalt){
				StartFirstHype(reelCount - nextIndex);
			} else if(reelIndex == _lastHypeIndex) {
				EndLastHype();
			}
			
			if(nextIndex < _machineConfig.BasicConfig.ReelCount && _shouldHypes[nextIndex]){
				StartHype(nextIndex);
			}
		}
	}

	private void StartHype(int reelIndex)
	{
		for(int i = 0; i < _machine.ReelList.Count; i++)
		{
			PuzzleReel r = _machine.ReelList[i];
			if(i < reelIndex)
			{
				r.StartHypeDarkEffect();
				r.EndHypeHighlightEffect();
			}
			else
			{
				r.StartHypeHighlightEffect();
			}
		}
	}

	private void StartFirstHype(int hypeReelCount)
	{
		_machine._effect.StartHypeEffect();
		PlayHypeSound(hypeReelCount);
	}

	private void PlayHypeSound(int hypeReelCount){
		AudioType[] hypeAudios = _machineConfig.BasicConfig.HypeAudios;
		int hypeSoundMax = hypeAudios.Length;
		bool useSpecialAudio = hypeSoundMax > 0;

		if (useSpecialAudio){
			for(int i = 0; i < hypeSoundMax; ++i){
				if (i == hypeReelCount - 1){
					_hypeAudio = hypeAudios[i];
					break;
				}
			}
		}
		// LogUtility.Log("PlayHypeSound " + _hypeAudio.ToString(), Color.green);
		AudioManager.Instance.PlaySound(_hypeAudio);
	}

	private void EndLastHype()
	{
		for(int i = 0; i < _machineConfig.BasicConfig.ReelCount; i++)
		{
			PuzzleReel r = _machine.ReelList[i];
			r.EndHypeDarkEffect();
			r.EndHypeHighlightEffect();
		}

		_machine._effect.EndHypeEffect();
		
		// LogUtility.Log("EndHypeSound " + _hypeAudio.ToString(), Color.green);
		AudioManager.Instance.StopSound(_hypeAudio);
		AudioManager.Instance.PlaySound(AudioType.HypeEnd);
	}
}
