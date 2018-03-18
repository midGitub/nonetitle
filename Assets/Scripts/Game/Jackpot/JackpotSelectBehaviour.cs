using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JackpotSelectBehaviour : MonoBehaviour {
	public Text _jackpotScore;
	public Button _defaultBetButton;
	public Button _jackpotMinBetButton;
	public Text _defaultBetText;
	public Text _jackpotMinBetText;

	private CoreMachine _machine;
	private PuzzleMachine _puzzleMachine;
	private JackpotBonusPoolManager _jackpotManager;
	private JackpotBonusPool _pool;

	// Use this for initialization
	void Start () {
		_defaultBetButton.onClick.AddListener (ApplyDefaultBet);
		_jackpotMinBetButton.onClick.AddListener (ApplyJackpotMinBet);

		Canvas canvas = gameObject.GetComponent<Canvas> ();
		canvas.worldCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		if (_pool != null) 
			UpdateScore(_pool.CurrentBonus);
	}

	public void UpdateScore(ulong score){
		if (_jackpotScore != null) {
			_jackpotScore.text = StringUtility.ConvertDigitalULongToString (score);
		}
	}

	public void Init(CoreMachine machine, PuzzleMachine puzzleMachine){
		_machine = machine;
		_puzzleMachine = puzzleMachine;
		UpdateBetText (_machine);
		_jackpotManager = PuzzleJackpotManager.Instance.GetManager (machine.Name);
		if (_jackpotManager != null) {
			if (_jackpotManager.JackpotType == JackpotType.FourJackpot) {
				_pool = _jackpotManager.GetJackpotBonusPool (JackpotType.FourJackpot, JackpotPoolType.Colossal);
			} else {
				_pool = _jackpotManager.GetJackpotBonusPool (JackpotType.Single);
			}
			if (_pool != null) 
				UpdateScore(_pool.CurrentBonus);
		}
	}

	private void UpdateBetText(CoreMachine machine){
		if (_defaultBetText != null){
			ulong bet = _puzzleMachine.GameData.BetAmount;
			// _leastBetText.text = "BET " + StringUtility.ConvertDigitalULongToString( machine.MachineConfig.BasicConfig.LeastBet );
			_defaultBetText.text = "BET " + StringUtility.ConvertDigitalULongToString( bet );
		}
			
		if (_jackpotMinBetText != null)
			_jackpotMinBetText.text = "BET " + StringUtility.ConvertDigitalULongToString (machine.MachineConfig.BasicConfig.JackpotMinBet);
	}

	private void SetBet(ulong bet){
		// TODO: 
		MachineBar bar = _puzzleMachine._machineBar;
		bar.SetBet (bet);
	}

	private void ApplyDefaultBet(){
		ulong bet = _puzzleMachine.GameData.BetAmount;
		// SetBet (_machine.MachineConfig.BasicConfig.LeastBet);
		SetBet (bet);
		gameObject.SetActive (false);
	}

	private void ApplyJackpotMinBet(){
		SetBet (_machine.MachineConfig.BasicConfig.JackpotMinBet);
		gameObject.SetActive (false);
	}
}
