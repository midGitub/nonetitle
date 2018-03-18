using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class PuzzleTapChipController : MonoBehaviour
{
	static float _prologueEffectWaitTime = 3.0f;
	static float _playEffectWaitTime = 1.0f;
	static float _epilogueEffectTime = 3.0f;

	public PuzzleTapChipPlayBoard _playBoard;
	public PuzzleTapChipResultBoard _resultBoard;
	public GameObject _chipsParent;
	public GameObject _prologueEffect;
	public GameObject _prologueLightEffect;
	public GameObject _playEffect;
	public GameObject _endEffect;

	PuzzleMachine _machine;
	CoreTapBox _coreTapBox;
	Animator _curtainAnimator;
	List<PuzzleTapChipChip> _chipList = new List<PuzzleTapChipChip>();
	int _chipCount;
	List<PuzzleTapChipChip> _winChipList = new List<PuzzleTapChipChip>();
	Callback<bool> _endCallback;
	ulong _betAmount;
	bool _isAnyChipPlayed;
	bool _isEnded;

	public bool IsAnyChipPlayed
	{
		get { return _isAnyChipPlayed; }
		set { _isAnyChipPlayed = value; }
	}

	public bool IsEnded
	{
		get { return _isEnded; }
	}

	public PuzzleMachine Machine
	{
		get { return _machine; }
	}

	void Start()
	{
		for(int i = 0; i < _chipsParent.transform.childCount; i++)
		{
			Transform t = _chipsParent.transform.GetChild(i); 
			PuzzleTapChipChip chip = t.gameObject.GetComponent<PuzzleTapChipChip>();
			if(chip != null)
			{
				chip.Init(this);
				_chipList.Add(chip);
			}
		}
		_chipCount = _chipList.Count;

		_curtainAnimator = gameObject.GetComponent<Animator>();
		Debug.Assert(_curtainAnimator != null);
	}

	public void Init(PuzzleMachine machine)
	{
		_machine = machine;
		_coreTapBox = _machine.CoreMachine.TapBox;
	}

	public void Start(ulong betAmount, Callback<bool> endCallback)
	{
		CitrusEventManager.instance.Raise(new TournamentChangeSoundEvent(false));
		_betAmount = betAmount;
		_playBoard.Init(betAmount);
		_resultBoard.Init();

		_winChipList.Clear();
		_isEnded = false;
		_endCallback = endCallback;
		_coreTapBox.Reset();

		ListUtility.ForEach(_chipList, (PuzzleTapChipChip chip) => {
			chip.Reset();
		});

		gameObject.SetActive(true);
		_prologueEffect.SetActive(false);
		_prologueLightEffect.SetActive(false);
		_playEffect.SetActive(false);
		_endEffect.SetActive(false);

		UnityTimer.Start(this, 0.3f, () => {
			AudioManager.Instance.PlaySound(AudioType.M40_TapBoxPrologue);
		});

		StartCoroutine(PlayPrologueCoroutine());
	}

	IEnumerator PlayPrologueCoroutine()
	{
		yield return new WaitForSeconds(_prologueEffectWaitTime * 0.5f);

		_prologueLightEffect.SetActive(true);

		yield return new WaitForSeconds(_prologueEffectWaitTime * 0.5f);

		_prologueEffect.SetActive(true);

		yield return new WaitForSeconds(_playEffectWaitTime);

		_prologueEffect.SetActive(false);
		_playEffect.SetActive(true);
	}

	public float AddWinRatio(PuzzleTapChipChip chip)
	{
		float ratio = _coreTapBox.FetchWinRatio();

		if(ratio > 0.0f)
			_winChipList.Add(chip);
		
		_playBoard.SetRatio(_coreTapBox.TotalWinRatio, chip);

		return ratio;
	}

	public void CheckEnd(float ratio)
	{
		if(ratio == 0.0f || IsAllChipsTapped())
			StartCoroutine(HandleEndWinRatio());
	}

	bool IsAllChipsTapped()
	{
		bool result = ListUtility.IsAllElementsSatisfied(_chipList, (PuzzleTapChipChip chip) => {
			return chip.IsTapped;
		});
		return result;
	}

	IEnumerator HandleEndWinRatio()
	{
		_isEnded = true;
		ulong winAmount = _coreTapBox.GetWinAmout(_betAmount);

		yield return new WaitForSeconds(1.0f);

		_playEffect.SetActive(false);
		_endEffect.SetActive(true);

		//follow the original order
		for(int i = 0; i < _chipList.Count; i++)
		{
			PuzzleTapChipChip c = _chipList[i];
			if(_winChipList.Contains(c))
			{
				c.PlayEndEffect();
				yield return new WaitForSeconds(0.1f);
			}
		}

		yield return new WaitForSeconds(0.7f);

		yield return StartCoroutine(_resultBoard.ShowCoroutine(_betAmount, _coreTapBox.TotalWinRatio, winAmount));

		_machine.AddWinAmountForIndieGame(winAmount, SmallGameMomentType.Front);
		_coreTapBox.SubtractLucky(winAmount);

		string customData = _coreTapBox.GetRatioCustomData(",");
        AnalysisManager.Instance.SendTapBox(_machine, winAmount, customData, _machine._specialMode);

		_curtainAnimator.Play("tapbox_disapp");

		yield return new WaitForSeconds(_epilogueEffectTime * 0.5f);

		_resultBoard.gameObject.SetActive(false);
		_endEffect.SetActive(false);

		yield return new WaitForSeconds(_epilogueEffectTime * 0.5f);

		EndCallback(winAmount);
	}

	void EndCallback(ulong winAmount)
	{
		CitrusEventManager.instance.Raise(new TournamentChangeSoundEvent (true));
		gameObject.SetActive(false);

		if(_endCallback != null)
			_endCallback(false);
	}
}
