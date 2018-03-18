using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class PuzzleTapBoxController : MonoBehaviour
{
	public PuzzleTapBoxPlayBoard _playBoard;
	public PuzzleTapBoxResultBoard _resultBoard;
	public List<PuzzleTapBoxChar> _charList;
	public Text _desText;

	PuzzleMachine _machine;
	CoreTapBox _coreTapBox;
	Callback<bool> _endCallback;
	ulong _betAmount;
	bool _isAnyCharPlayed;
	bool _isEnded;

	public bool IsAnyCharPlayed
	{
		get { return _isAnyCharPlayed; }
		set { _isAnyCharPlayed = value; }
	}

	public bool IsEnded
	{
		get { return _isEnded; }
	}

	void Start()
	{
		ListUtility.ForEach(_charList, (PuzzleTapBoxChar character) => {
			character.Init(this);
		});
	}

	public void Init(PuzzleMachine machine)
	{
		_machine = machine;
		_coreTapBox = _machine.CoreMachine.TapBox;

		InitDesText();
	}

	void InitDesText()
	{
		string des = _desText.text;
		float[] winRatios = _machine.MachineConfig.BasicConfig.TapBoxWinRatios;

		Debug.Assert(des.Contains("{0}"), "TapBox: DesText text is wrong");

		des = string.Format(des, winRatios[winRatios.Length - 1]);
		_desText.text = des;
	}

	public void Start(ulong betAmount, Callback<bool> endCallback)
	{
		CitrusEventManager.instance.Raise(new TournamentChangeSoundEvent(false));
		_betAmount = betAmount;
		_playBoard.Init(betAmount);
		_resultBoard.Init();

		_isEnded = false;
		_endCallback = endCallback;
		_coreTapBox.Reset();

		gameObject.SetActive(true);

		UnityTimer.Start(this, 0.3f, () => {
			AudioManager.Instance.PlaySound(AudioType.M40_TapBoxPrologue);
		});
	}

	public float AddWinRatio()
	{
		float ratio = _coreTapBox.FetchWinRatio();

		_playBoard.SetRatio(_coreTapBox.TotalWinRatio);

		if(ratio == 0.0f)
			StartCoroutine(HandleEndWinRatio());

		return ratio;
	}

	IEnumerator HandleEndWinRatio()
	{
		_isEnded = true;
		ulong winAmount = _coreTapBox.GetWinAmout(_betAmount);

		yield return new WaitForSeconds(2.5f);

		yield return StartCoroutine(_resultBoard.ShowCoroutine(_betAmount, _coreTapBox.TotalWinRatio, winAmount));

		yield return new WaitForSeconds(1.0f);

		gameObject.GetComponent<Animator>().SetTrigger("Disable");

		yield return new WaitForSeconds(1.7f);

		_resultBoard.gameObject.SetActive(false);

		yield return new WaitForSeconds(1.7f);

		EndCallback(winAmount);
	}

	void EndCallback(ulong winAmount)
	{
		CitrusEventManager.instance.Raise(new TournamentChangeSoundEvent(true));
		_machine.AddWinAmountForIndieGame(winAmount, SmallGameMomentType.Behind);
		_coreTapBox.SubtractLucky(winAmount);

        string customData = _coreTapBox.GetRatioCustomData(",");
		AnalysisManager.Instance.SendTapBox(_machine, winAmount, customData, _machine._specialMode);

		gameObject.SetActive(false);

		if(_endCallback != null)
			_endCallback(false);
	}
}
