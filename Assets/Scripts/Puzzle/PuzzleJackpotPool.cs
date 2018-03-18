using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleJackpotPool : MonoBehaviour {
	[SerializeField]
	private Text[] _poolScoreArray;

	[SerializeField]
	private JackpotType _type;

	private CoreMachine _machine;

	public string _name = "";

	// Use this for initialization
	void Start () {
		if (GameScene.Instance != null && GameScene.Instance.PuzzleMachine != null)
			_machine = GameScene.Instance.PuzzleMachine.CoreMachine;

		StartCoroutine(ScoreUpdate());
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void SetPoolScore(JackpotPoolType type , ulong score){
		if (_type == JackpotType.Single) {
			_poolScoreArray [0].text = StringUtility.ConvertDigitalULongToString((ulong)score);
		} else {
			_poolScoreArray [(int)type].text = StringUtility.ConvertDigitalULongToString((ulong)score);
		}
	}

	private IEnumerator ScoreUpdate(){
		while(true){
			if (_machine == null && GameScene.Instance != null && GameScene.Instance.PuzzleMachine != null) {
				_machine = GameScene.Instance.PuzzleMachine.CoreMachine;
			}

			if (PuzzleJackpotManager.Instance != null) {
				string name;
				if (_machine != null) {
					name = _machine.Name;
				} else {
					name = _name;
				}
				if (PuzzleJackpotManager.Instance.GetJackpotType(name) == JackpotType.Single) {
					JackpotBonusPool pool = PuzzleJackpotManager.Instance.GetJackpotBonusPool (JackpotType.Single, JackpotPoolType.Single, name);
					if (pool != null) {
						SetPoolScore(JackpotPoolType.Single, pool.CurrentBonus);
					}
				} else if (PuzzleJackpotManager.Instance.GetJackpotType(name) == JackpotType.FourJackpot){
					for (int i = (int)JackpotPoolType.Colossal; i <= (int)JackpotPoolType.Big; ++i) {
						JackpotBonusPool pool = PuzzleJackpotManager.Instance.GetJackpotBonusPool (JackpotType.FourJackpot, (JackpotPoolType)i, name);
						if (pool != null) {
							SetPoolScore((JackpotPoolType)i,  pool.CurrentBonus);
						}
					}
				}
			}

			yield return new WaitForSeconds(2.5f);
		}
	}

}
