using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.UI;

public class PuzzleJackpotManager : Singleton<PuzzleJackpotManager> {
	private JackpotManager _jackpotManager;

	private static readonly string _jackpotMachineDefaultStr = "Images/UI/Jackpot/jackpot_winner_machine_";
	private List<string> _machineImageNames = new List<string>();
	private Dictionary<string, Sprite> _machineImageDict = new Dictionary<string, Sprite> ();

	public GameObject _notifyParent;// 通知界面父节点

	public void Init(){
		_jackpotManager = JackpotManager.Instance;
		_jackpotManager.Init (TriggerJackpotNotifyAction);

		StartRunRefresh ();
		StartRunSaveExitTimeAndJackpotData ();

		_machineImageNames.AddRange(ListUtility.CreateList (CoreDefine.singleJackpotMachines, CoreDefine.singleJackpotMachines.Length));
		_machineImageNames.AddRange(ListUtility.CreateList (CoreDefine.fourJackpotMachines, CoreDefine.fourJackpotMachines.Length));

		LoadMachineImage ();

		#if false
		UnityTimer.Start (this, 1.0f, () => {
			StartCoroutine( RunJackpotNotifyTest() );
		});
		#endif
	}

	#region jackpot notify
	private IEnumerator RunJackpotNotifyTest(){
		yield return new WaitForSeconds(10.0f);
		TriggerJackpotNotifyAction ("M9", 100000000);
		yield return new WaitForSeconds(10.0f);
		TriggerJackpotNotifyAction ("M16", 200000000);
		yield return new WaitForSeconds(10.0f);
		TriggerJackpotNotifyAction ("M9", 300000000);
		yield return new WaitForSeconds(10.0f);
		TriggerJackpotNotifyAction ("M16", 400000000);
	}
	#endregion

	public void SetNotifyParent(GameObject obj){
		_notifyParent = obj;
	}

	public JackpotBonusPoolManager GetManager(string name){
		if (_jackpotManager != null) {
			return _jackpotManager.GetManager (name);
		}
		return null;
	}

	private void LoadMachineImage(){
		ListUtility.ForEach (_machineImageNames, (string s) => {
			Sprite spr = AssetManager.Instance.LoadAsset<Sprite>(_jackpotMachineDefaultStr + s);
			_machineImageDict[s] = spr;
		});
	}

	void OnApplicationQuit(){
		if (_jackpotManager != null) {
			_jackpotManager.SaveExitTimeAndJackpotData ();
		}
	}

	public JackpotType GetJackpotType(string name){
		if (_jackpotManager != null) {
			return _jackpotManager.GetJackpotType (name);
		}
		return JackpotType.FourJackpot;
	}

	public JackpotBonusPool GetJackpotBonusPool(JackpotType type, JackpotPoolType poolType, string name){
		if (_jackpotManager != null) {
			return _jackpotManager.GetJackpotBonusPool (type, poolType, name);
		}
		return null;
	}

	private IEnumerator RunRefresh(){
		while (true) {
			if (_jackpotManager != null) {
				_jackpotManager.RefreshJackpotPools ();
			}
			yield return new WaitForSeconds (1);
		}
	}

	private void StartRunRefresh(){
		StartCoroutine (RunRefresh());
	}

	private IEnumerator RunSaveExitTimeAndJackpotData(){
		while (true) {
			if (_jackpotManager != null) {
				_jackpotManager.SaveExitTimeAndJackpotData ();
			}
			yield return new WaitForSeconds (2 * 60);
		}
	}

	private void StartRunSaveExitTimeAndJackpotData(){
		StartCoroutine (RunSaveExitTimeAndJackpotData ());
	}

	private void TriggerJackpotNotifyAction(string name, ulong bonus){
		string guest = "Guest";
		IRandomGenerator generator = RandomUtility.CreateRandomGenerator ();
		int guestID = RandomUtility.RollInt (generator, 100000, 999999);
		guest += guestID.ToString ();

		GameObject notifyObj = UIManager.Instance.OpenJackpotNotifyUI(_notifyParent);
		if (notifyObj != null) {
			JackpotNotifyBehaviour behaviour = notifyObj.GetComponent<JackpotNotifyBehaviour> ();
			if (behaviour != null) {
				behaviour.SetPlayerName (guest);
				behaviour.SetJackpotBonus (bonus);
				if (_machineImageDict.ContainsKey (name)) {
					behaviour.SetMachineSprite (_machineImageDict[name]);
				}
			}
		}
	}
}
