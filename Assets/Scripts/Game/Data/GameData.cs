using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;
using System;

public class GameData
{
	public delegate void NumChangeDelegate<T>(T prevNum, T curNum);

    private static readonly float _initBetAmountFactor = 0.05f;

	PuzzleMachine _machine;
	ulong[] _betOptions;
	int _betIndex;
    int _lastBetIndex;
	ObscuredULong _betAmount;
	ObscuredULong _winAmount;
	WinType _winType;

	public ulong Credits { get { return UserBasicData.Instance.Credits; } }
	public ulong MinBetAmount { get { return _betOptions[0]; } }
	public int BetIndex { get { return _betIndex; } }

    public int LastBetIndex { get { return _lastBetIndex; } }
    public ulong BetAmount { get { return _betAmount; } }
	public ulong WinAmount { get { return _winAmount; } }
	public WinType WinType { get { return _winType; } }
	public ulong[] BetOptions { get { return _betOptions; } }
	public bool IsBetLowest { get { return IsBetIndexLowest (); } }
	public bool IsBetHighest{ get { return IsBetIndexHighest (); } }


    public event NumChangeDelegate<ulong> BetAmountChangeEventHandler = delegate { };
	public event NumChangeDelegate<ulong> WinAmountChangeEventHandler = delegate { };
	public event NumChangeDelegate<ulong> CreditsChangeEventHandler = delegate { };

	//By nichos:
	//Since _normalWinType is calculated in every spin, it is abandoned here and moved to CoreSpinResult.
	//But still keeps the code and just make it private
	NormalWinType _normalWinType;
	private NormalWinType NormalWinType { get { return _normalWinType; } }

	public GameData(PuzzleMachine machine)
	{
		_machine = machine;
		Init();
	}

	void Init()
	{
		InitBetAmount(UserBasicData.Instance.Credits);
		_winAmount = 0;
	}

	void InitBetAmount(ulong credits)
	{
		_betOptions = BetOptionConfig.Instance.GetMachineBetOptions(_machine.MachineName);

		ulong value = (ulong)(credits * _initBetAmountFactor);
		int locateIndex = MathUtility.GetLocateIndex(_betOptions, value);
		SetBetIndex(locateIndex);
	}

	#region Private

	public bool SetBetIndex(int betIndex)
	{
		bool result = false;
		ulong betMax = (ulong)BetUnlockSettingConfig.Instance.GetMaxBet(_machine.MachineName);

		if (betIndex < 0 || betIndex >= _betOptions.Length)
			return result;

		bool overflowBet = _betOptions [betIndex] > betMax;
		if (betIndex >= 0 && betIndex < _betOptions.Length
			&& !overflowBet) {
			ulong prevBetAmount = _betAmount;
			_betIndex = betIndex;
			_betAmount = _betOptions [_betIndex];
			BetAmountChangeEventHandler (prevBetAmount, _betAmount);
			result = true;
		} else if (overflowBet) {
			ulong prevBetAmount = _betAmount;
			// betindex等于当前bet最大值的index
			int index = GetBetIndex (betMax);
			if (index != -1) {
				_betIndex = index;
				_betAmount = _betOptions [_betIndex];
				BetAmountChangeEventHandler (prevBetAmount, _betAmount);
			} else {
				CoreDebugUtility.LogError ("no suitable betAmount betMax = " + betMax + " betAmount = " + _betOptions [betIndex]);
			}
		}
		return result;
	}

	public float GetWinBetRatio()
	{
		return (float)_winAmount / (float)_betAmount;
	}

	void RefreshWinType()
	{
		float ratio = GetWinBetRatio();
		_winType = PuzzleUtility.GetWinType(ratio);
		if (_machine.CoreMachine.SpinResult.IsJackpotWin)
			_winType = WinType.Jackpot;

		if(_winType == WinType.Normal)
			_normalWinType = PuzzleUtility.GetNormalWinType(ratio);
		else
			_normalWinType = NormalWinType.None;
	}

	#endregion

	#region Public

	public bool AddBet()
	{
		bool result = SetBetIndex(_betIndex + 1);
		return result;
	}

	public bool SubtractBet()
	{
		bool result = SetBetIndex(_betIndex - 1);
		return result;
	}

	public void MaxBet()
	{
		SetBetIndex(_betOptions.Length - 1);
	}

    public void RecordLastBet()
    {
        _lastBetIndex = _betIndex;
    }

    public void ResetToLastBet()
    {
        SetBetIndex(_lastBetIndex);
    }

    public void SetWinAmount(ulong amount)
	{
		ulong prevAmount = _winAmount;
		_winAmount = amount;
		RefreshWinType();
		WinAmountChangeEventHandler(prevAmount, _winAmount);
	}

	public void AddWinAmount(ulong amount)
	{
		ulong prevAmount = _winAmount;
		_winAmount += amount;
		RefreshWinType();
		WinAmountChangeEventHandler(prevAmount, _winAmount);

		AddCredits(amount);
	}

	public void AddCredits(ulong amount)
	{
		ulong prevAmount = UserBasicData.Instance.Credits;
		UserBasicData.Instance.AddCredits(amount, FreeCreditsSource.NotFree, true);
		CreditsChangeEventHandler(prevAmount, UserBasicData.Instance.Credits);
	}

	public void SubtractCreditsFromBet()
	{
		if(HasEnoughCredits())
		{
			if(DoubleLevelUpHelper.Instance.IsInDouble())
				UserLevelSystem.Instance.AddLevelXP(Convert.ToInt32(_betAmount)*2, false);
			else
				UserLevelSystem.Instance.AddLevelXP(Convert.ToInt32(_betAmount), false);
			PiggyBankSystem.Instance.AddCoinsInPiggyBank(Convert.ToInt32(_betAmount));
			ulong prevAmount = UserBasicData.Instance.Credits;

			// don't save for better performance
			UserBasicData.Instance.SubtractCredits(_betAmount, false);

			CreditsChangeEventHandler(prevAmount, UserBasicData.Instance.Credits);
		}
		else
		{
			Debug.LogError("Credits subtracted to < 0");
		}
	}

	public bool HasEnoughCredits()
	{
		return UserBasicData.Instance.Credits >= _betAmount;
	}

	private bool IsBetIndexLowest(){
		return _betIndex == 0;
	}

	private bool IsBetIndexHighest(){
		ulong betMax = BetUnlockSettingConfig.Instance.GetMaxBet(_machine.MachineName);
		Debug.Assert (_betOptions [_betIndex] <= betMax, "IsBetIndexHighest failed betindex = " + _betIndex + " betMax = " + betMax);
		return _betOptions [_betIndex] == betMax;
	}

	public int GetBetIndex(ulong betAmount){
		for (int i = _betOptions.Length - 1; i >= 0; --i){
			if (betAmount >= _betOptions[i]){
				return i;
			}
		}
		return -1;
	}

	#endregion
}
