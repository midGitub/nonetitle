using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleDebugPanel : MonoBehaviour
{
	public Text _totalSpinText;
	public Text _winRatioText;
	public Text _longLuckyText;
	public Text _shortLuckyText;
	public Text _luckyModeText;
	public Text _resultTypeText;
	public Text _resultIdText;

	public Text _reelFreeSpinTime;
	public Text _reelSpinTime;
	public Text _spinSpeed;
	public Text _startSpinOffsetFactor;
	public Text _startSpinTime;
	public Text _spinNeighorTime;
	public Text _slideReelInterval;
	public Text _endSpinOffsetFactors;
	public Text _endSpinTimes;
	public Text _fasterSpeedFactors;
	public Text _slowerSpeedFactors;


	// Use this for initialization
	void Start () {
		Refresh(0, 0f, CoreLuckyMode.Normal, SpinResultType.None, 0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Refresh(CoreSpinResult spinResult)
	{
		int resultId = CoreUtility.GetSpinResultRowId(spinResult);
		Refresh(UserMachineData.Instance.TotalSpinCount, spinResult.WinRatio, spinResult.LuckyMode, spinResult.Type, resultId);
	}

	public void RefreshSpinParam(PuzzleReelSpinConfig reelSpinConfig, PuzzleConfig config){
		_reelFreeSpinTime.text = StringUtility.ConstructString(config._reelFreeSpinTime, ",");
		_reelSpinTime.text = StringUtility.ConstructString(config._reelSpinTime, ",");
		_spinSpeed.text = reelSpinConfig._spinSpeed.ToString();
		_startSpinOffsetFactor.text = reelSpinConfig._startSpinOffsetFactor.ToString();
		_startSpinTime.text = reelSpinConfig._startSpinTime.ToString();
		_spinNeighorTime.text = reelSpinConfig._spinNeighorTime.ToString();
		_slideReelInterval.text = reelSpinConfig._slideReelInterval.ToString();
		_endSpinOffsetFactors.text = StringUtility.ConstructString(reelSpinConfig._endSpinOffsetFactors, ",");
		_endSpinTimes.text = StringUtility.ConstructString(reelSpinConfig._endSpinTimes, ",");
		_fasterSpeedFactors.text = StringUtility.ConstructString(reelSpinConfig._fasterSpeedFactors, ",");
		_slowerSpeedFactors.text = StringUtility.ConstructString(reelSpinConfig._slowerSpeedFactors, ",");
	}

	private void Refresh(int totalSpin, float winRatio, CoreLuckyMode luckyMode, SpinResultType resultType, int resultId)
	{
		_totalSpinText.text = "Total Spin: " + totalSpin.ToString();
		_winRatioText.text = "Win Ratio: " + winRatio.ToString();
		_longLuckyText.text = "Long Lucky: " + UserBasicData.Instance.LongLucky.ToString();
		_shortLuckyText.text = "Short Lucky: " + UserBasicData.Instance.ShortLucky.ToString();
		_luckyModeText.text = "Lucky Mode: " + luckyMode.ToString();
		_resultTypeText.text = "Result Type: " + resultType.ToString();
		_resultIdText.text = "Result Id: " + resultId.ToString();
	}

}
