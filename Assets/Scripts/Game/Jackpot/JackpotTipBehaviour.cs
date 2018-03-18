using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public enum JackpotTipType{
	LeastBet,
	JackpotCanPlay,
	Max,
}

public class JackpotTipBehaviour : MonoBehaviour {
	public Text _minBet;
	public GameObject _leastBetTip;
	public GameObject _jackpotCanPlayTip;
	public GameObject _otherTips;
	public float _tipStayTime = 2.0f;// tip存在的时间

	private CoreMachine _machine;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Init(CoreMachine machine){
		_machine = machine;
		InitMinBet ();
	}

	private void InitMinBet(){
		ulong minbet = _machine.MachineConfig.BasicConfig.JackpotMinBet;
		_minBet.text = StringUtility.ConvertDigitalULongToString (minbet);
	}

	public void ShowTip(JackpotTipType type){
		if (type == JackpotTipType.LeastBet) {
			_jackpotCanPlayTip.SetActive (false);
			ShowAndHide (_leastBetTip);
		} else if (type == JackpotTipType.JackpotCanPlay) {
			_leastBetTip.SetActive (false);
			ShowAndHide (_jackpotCanPlayTip);
		}
	}

	private void ShowAndHide(GameObject obj){
		obj.SetActive (true);
		UnityTimer.Start (this, _tipStayTime, () => {
			obj.SetActive(false);
		});
	}
}
