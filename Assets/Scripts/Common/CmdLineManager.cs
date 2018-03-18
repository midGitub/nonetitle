using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class CmdLineManager : Singleton<CmdLineManager> {

	public class cmdPair{// cmd and it's parameters
		public string name;
		public List<string> param = new List<string>();
	}
	public delegate void CommandCallback(cmdPair pair);
	private Dictionary<string, CommandCallback> m_CommandCache = new Dictionary<string, CommandCallback>();

	private string _cmdStr = "";// current cmd
	private List<string> _cmdList = new List<string>();
	private bool _showCmdLine = false;
	private const int CMD_SHOWMAX_LINE = 20;// show max line in screen

	#region open cmd // long press finger to open cmdline

	private float _touchesTickCount = 0.0f;
	private const float TOUCH_THRESHOLD = 4.0f;
	private bool _bTouchStart = false;

	#endregion

	#region forcewin// force to win big or mega or epic

	private const string CMD_FORCEWIN = "win";
	private const string CMD_FORCEWIN_BIG = "big";
	private const string CMD_FORCEWIN_MEGA = "mega";
	private const string CMD_FORCEWIN_EPIC = "epic";
	private const string CMD_FORCEWIN_HIGH = "high";
	private const string CMD_FORCEWIN_LOW = "low";
	private bool _enableForceWin = false;
	public bool EnableForceWin{ get{ return _enableForceWin; } }
	private WinType _winType = WinType.None;
	private NormalWinType _normalWinType = NormalWinType.Low;

	#endregion


	public void Init(){
	}

	// Use this for initialization
	void Start () {
		RegisterCommand (CMD_FORCEWIN, forceWinDelegate);
	}

	// Update is called once per frame
	void Update () {
		#if DEBUG
		#if UNITY_EDITOR
		if (Input.GetKeyDown (KeyCode.Escape))
		#else
		if (IsFingerLongTouch())
		#endif
		{
			_showCmdLine = !_showCmdLine;
		}
		#endif
	}

	void OnGUI(){
		#if DEBUG

		if (!_showCmdLine)
			return;
		
		if (GUI.Button(new Rect(Screen.width - 200, 200, 200, 100), "Click")){
			_showCmdLine = !_showCmdLine;
		}

		GUILayout.BeginArea (new Rect(100, 200, 1080, 800));

		GUILayout.BeginHorizontal ();
		GUILayoutOption[] opts = { 
			GUILayout.MaxWidth(400), GUILayout.MaxHeight(100) 
		};
		GUILayoutOption[] opts_button = { 
			GUILayout.MaxWidth(100), GUILayout.MaxHeight(100)
		};
//		_cmdStr = GUILayout.TextField (_cmdStr, GUILayout.MaxWidth (400));
		_cmdStr = GUILayout.TextField (_cmdStr, opts);
		if (GUILayout.Button ("Click", opts_button)){
			ProcessCMD ();
		}
		GUILayout.EndHorizontal ();

		GUIStyle style = new GUIStyle ();
		style.normal.textColor = Color.green;
		style.fontSize = 30;
		GUILayout.BeginVertical ();
		int start = Mathf.Clamp(_cmdList.Count - CMD_SHOWMAX_LINE, 0, _cmdList.Count);
		for (int i = _cmdList.Count - 1; i >= start; --i) {
			GUILayout.Label (_cmdList[i], style, GUILayout.MaxHeight(20));
		}
		GUILayout.EndVertical ();

		GUILayout.EndArea ();
		#endif
	}

	#region parse cmd

	private cmdPair ParseCmdLine(string cmd){
		if (string.IsNullOrEmpty (cmd))
			return null;

		string[] cmds = cmd.Split (' ');
		if (cmds.Length == 0)
			return null;

		cmdPair pair = new cmdPair ();
		pair.name = cmds [0].Trim().ToLower();
		for (int i = 1; i < cmds.Length; ++i) {
			pair.param.Add (cmds[i].Trim().ToLower());
		}

		return pair;
	}

	private void ProcessCMD(){
		cmdPair pair = ParseCmdLine (_cmdStr);
		if (pair == null)
			return;

		_cmdList.Add (_cmdStr);
		ProcessCMD (pair);
	}

	private void ProcessCMD(cmdPair pair){
		if (m_CommandCache.ContainsKey(pair.name)){
			m_CommandCache[pair.name](pair);
		}
		else{
			LogUtility.Log("cant process cmd , have not command name : " + pair.name, Color.yellow);
		}
	}

	private void RegisterCommand(string cmdName, CommandCallback callback){
		if (m_CommandCache.ContainsKey (cmdName)) {
			LogUtility.Log ("CommandCache contain repeat :" + cmdName, Color.yellow);
		}
		m_CommandCache.Add (cmdName, callback);
	}

	private void forceWinDelegate(cmdPair pair){
		if (pair.param.Count > 0 && !string.IsNullOrEmpty(pair.param[0])) {
			_enableForceWin = true;
			switch (pair.param [0]) {
			case CMD_FORCEWIN_BIG:
				_winType = WinType.Big;
				break;
			case CMD_FORCEWIN_EPIC:
				_winType = WinType.Epic;
				break;
			case CMD_FORCEWIN_HIGH:
				_winType = WinType.Normal;
				_normalWinType = NormalWinType.High;
				break;
			case CMD_FORCEWIN_LOW:
				_winType = WinType.Normal;
				_normalWinType = NormalWinType.Low;
				break;
			default:
				_winType = WinType.Normal;
				break;
			}
		} else {
			_enableForceWin = false;
			_winType = WinType.None;
		}
	}

	#endregion

	#region cmd public logic

	public int ForceWin(CoreGenerator gen, int index){
		#if DEBUG
		if (_enableForceWin) {
			int ret = gen.GetIndexByWinType (_winType, _normalWinType);
			return ret != -1 ? ret : index;
		} else {
			return index;
		}
		#else
		return index;
		#endif
	}

	public int ForceWinResultType(int type){
		#if DEBUG
		return _enableForceWin ? (int)SpinResultType.Win : type;
		#else
		return type;
		#endif
	}

	#endregion

	private bool IsFingerLongTouch(){
		if (Input.touchCount >= 3) {
			_bTouchStart = true;
		} else {
			_bTouchStart = false;
			_touchesTickCount = 0.0f;
			return false;
		}

		if (_bTouchStart) {
			_touchesTickCount += Time.deltaTime;
		}

		if (_touchesTickCount >= TOUCH_THRESHOLD) {
			return true;
		}
		return false;
	}
}
