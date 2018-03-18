using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using CodeStage.AdvancedFPSCounter;
using CitrusFramework;

public class GameScene : MonoBehaviour
{
	private static readonly string AFPSCounterPath = "Game/AFPSCounter";

	private static GameScene _instance;
	public static GameScene Instance
	{
		get { return _instance; }
	}

	public PuzzleMachine _puzzleMachine;
	public string _defaultMachine;
//	public AFPSCounter _fpsCounter;

	public PuzzleMachine PuzzleMachine { get { return _puzzleMachine; } }

	private void Awake()
	{
		_instance = this;

		GameManager.Instance.Init();
	}

	// Use this for initialization
	void Start()
	{
		string machineName = MapScene.CurrentMachineName;
		if(string.IsNullOrEmpty(machineName))
			machineName = _defaultMachine;

		#if DEBUG
		bool isContainName = ListUtility.IsContainElement(CoreDefine.AllMachineNames, machineName);
		if(!isContainName)
		{
			string s = string.Format("Please add {0} to CoreDefine.AllMachineNames", machineName);
			Debug.Assert(false, s);
		}
		#endif

		AddAFPSCounter ();
		_puzzleMachine.Init(machineName);
        DiceManager.Instance.LoadDiceUi();

		//		AudioManager.Instance.PlayMusic(AudioType.MachineBGM);

		//		var t = new CoreMachineTest();
		//		t.Run();
	}

	private void AddAFPSCounter(){
		#if DEBUG
		UGUIUtility.CreateObj(AFPSCounterPath);
		#endif
	}

	// Update is called once per frame
	void Update()
	{

	}

	void StartLevel()
	{
	}

	void OnDestroy(){
		_instance = null;
	}
}
