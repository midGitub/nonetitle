using System.Collections;
using UnityEngine;
using CitrusFramework;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public enum MapMachineRoom
{
    VIP = 0,
    CUSTOM = 1,
}

public class MapScene : MonoBehaviour
{
	private static readonly string AFPSCounterPath = "Game/AFPSCounter";

	public Vector3 _scrollContentPosition; 

	private static string _currentMachineName;
	public GameObject _effectParent;// 特效父节点
	//public static GameObject _reporter;

	public static string CurrentMachineName { get { return _currentMachineName; } }

    public GameObject CustomRoomScrollRect;
    public GameObject VipRoomScrollRect;
    public RectTransform ScrollContent;
    public RectTransform VipScrollContent;
    public GameObject VipMapBg;
    public GameObject MapBg;
	public GameObject TinyMapBg;
    public EnterRoom EnterRoomScript;

	private bool _buttonHasBeenDown = false;

    private string _generalMachineMapBg = "Images/Machines/BJ";
    private string _tinyMachineMapBg = "Images/Machines/MapBg";

	private MachineUnlockManager _machineUnlockManager;
    private Dictionary<MapMachineType, GameObject> _mapBgs;
    private Dictionary<MapMachineRoom, GameObject> _roomList = new Dictionary<MapMachineRoom, GameObject>();
    private Dictionary<MapMachineRoom, RectTransform> _contentDict;
    private MapMachineRoom _curStayRoom;

    void Awake()
	{
		GameManager.Instance.Init();
		_machineUnlockManager = new MachineUnlockManager();
	}

	// Use this for initialization
	void Start()
	{
		//assign IsNewGame here, the user is not new anymore
		if(UserDeviceLocalData.Instance.IsNewGame)
		{
			UserDeviceLocalData.Instance.IsNewGame = false;
			UserDeviceLocalData.Instance.Save();
		}

		AnalysisManager.Instance.EnterLobby();

		//CheckLoadReporter();
		AddAFPSCounter();
        InitBg();
		LoadEffects();
        InitRoom();

        MachineAssetDownloaderManager.Instance.Init(); //nothing but just for init the instance

		CitrusEventManager.instance.AddListener<MapSceneUpdateScrollPosEvent> (UpdateScrollPosition);
        CitrusEventManager.instance.AddListener<SwitchMachineTypeEvent>(ChangeMapBg);
        CitrusEventManager.instance.AddListener<AskSlideToMachinePosEvent>(TrySlideToMachinePos);
        CitrusEventManager.instance.AddListener<AskEnterMapRoomEvent>(AskEnterRoom);

        _currentMachineName = "None";

		//Test
		#if false
		GiftHelper.Instance.PrintTypeAndCode();
		UserBasicData.Instance.TestLuckyPeriod();
		#endif
	}

	// Update is called once per frame
	void Update()
	{
//		LogUtility.Log ("Scrollcontent position = "+ScrollContent.position, Color.red);
		_scrollContentPosition = ScrollContent.position;
	}

	//void CheckLoadReporter()
	//{
	//	#if DEBUG
	//	if(_reporter == null)
	//	{
	//		GameObject o = AssetManager.Instance.LoadAsset<GameObject>("Game/Reporter");
	//		_reporter = GameObject.Instantiate(o);
	//	}
	//	#endif
	//}

	void OnDestroy(){
		CitrusEventManager.instance.RemoveListener<MapSceneUpdateScrollPosEvent> (UpdateScrollPosition);
        CitrusEventManager.instance.RemoveListener<SwitchMachineTypeEvent>(ChangeMapBg);
        CitrusEventManager.instance.RemoveListener<AskSlideToMachinePosEvent>(TrySlideToMachinePos);
        CitrusEventManager.instance.RemoveListener<AskEnterMapRoomEvent>(AskEnterRoom);
    }

    public void EnterRoomAfterAllMachinesLoaded(Dictionary<string, MapMachineRoom> mapRoomMachinesDict)
    {
        //if has new custom machine unlocked, enter the room the machine belongs and locate at the position of the machine
        //else enter last time user stayed room
        string newUnlockCustomMachine = MachineUnlockManager.NewUnlockMachine;
        if (newUnlockCustomMachine != "" && mapRoomMachinesDict.ContainsKey(newUnlockCustomMachine))
            EnterRoom(mapRoomMachinesDict[newUnlockCustomMachine]);
        else
            EnterRoom(UserDeviceLocalData.Instance.LastStayMapRoom);
    }

    #region machineRooms
    void InitRoom()
    {
        _contentDict = new Dictionary<MapMachineRoom, RectTransform>
        {
            { MapMachineRoom.CUSTOM, ScrollContent},
            { MapMachineRoom.VIP, VipScrollContent}
        };
        AddNewRoom(MapMachineRoom.VIP, VipRoomScrollRect);
        AddNewRoom(MapMachineRoom.CUSTOM, CustomRoomScrollRect);
    }

    void AddNewRoom(MapMachineRoom room, GameObject roomRoot)
    {
        _roomList.Add(room, roomRoot);
    }

    public void AskEnterRoom(AskEnterMapRoomEvent e)
    {
        if (e.Room != _curStayRoom)
        {
            CitrusEventManager.instance.Raise(new VipCurtainAnimPlayEvent(() =>
            {
                EnterRoom(e.Room, e.LocateAtMachine);
                if (e.Callback != null)
                    e.Callback.Invoke();
            }));
        }
    }

    void EnterRoom(MapMachineRoom roomType, string locateAtMachine = null)
    {
        MapMachineType type = MapMachineType.Normal;
        //if vip room is not open to user at now, show custom room as default
        if (!MapSettingConfig.Instance.IsVipRoomEnable)
        {
            roomType = MapMachineRoom.CUSTOM;
        }
        switch (roomType)
        {
            case MapMachineRoom.CUSTOM:
                type = UserDeviceLocalData.Instance.IsInTinyMachineRoom
                     ? MapMachineType.Tiny
                     : MapMachineType.Normal;
                break;
            case MapMachineRoom.VIP:
                type = MapMachineType.Vip;
                break;
        }

        ShowMapBg(type);
        foreach (var room in _roomList)
        {
            room.Value.SetActive(room.Key == roomType);
        }

        EnterRoomScript.gameObject.SetActive(MapSettingConfig.Instance.IsVipRoomEnable);
        EnterRoomScript.ShowRoomButton(roomType);

        _curStayRoom = roomType;
        UpdateMachindPos(roomType);
        HandleLocateMachinePos(roomType, locateAtMachine);
        CitrusEventManager.instance.Raise(new EnterMapRoomEvent(roomType));

        //load select machine position after all enter room events handled
        LoadSelectMachinePosition();
    }

    void UpdateMachindPos(MapMachineRoom roomType)
    {
        Canvas.ForceUpdateCanvases();
        MapMachinePosManager.Instance.UpdateMachinePos(roomType);
    }

    private void HandleLocateMachinePos(MapMachineRoom roomType, string machineName)
    {
        if (machineName != null)
        {
            MapMachinePosManager.Instance.RecordMachinePos(roomType, machineName);
        }
        else
        {
            if (MachineUnlockManager.NewUnlockMachine != "")
            {
                MapMachinePosManager.Instance.RecordMachinePos(roomType, MachineUnlockManager.NewUnlockMachine);
                MachineUnlockManager.NewUnlockMachine = "";
            }
        }
    }

    #endregion machineRooms

    public void MapButtonDown(string machineName)
	{
		// 激励视频开始加载的时候不能点击进入机台
		if (ADSManager.Instance.StartPrepareWatch)
			return;

		// 不允许本方法多次运行。如果没有这个防护则在Android和iOS平台上，当退出机台到大厅loading时，多次点击机台出现位置，会
		// 造成连续调用此方法的情况。
		if (_buttonHasBeenDown)
			return;
		else
			_buttonHasBeenDown = true;

		_currentMachineName = machineName;
		AudioManager.Instance.FadeOutBackgroundMusic(1);
		Debug.Assert(!string.IsNullOrEmpty(_currentMachineName), "MapMachineButton has empty machine name");
		SaveSelectMachinePosition();
		ScenesController.Instance.EnterGameScene(null);
		SendProfileHelper.ForceSendProfile();

		// zhosuen Test UDID
		Debug.Log("MapButtonDown");
		Debug.Log("MapButtonDown udid: " + DeviceUtility.GetDeviceId());
	}

    private void TrySlideToMachinePos(AskSlideToMachinePosEvent e)
    {
        if (MapMachinePosManager.Instance.MachinePosDic.ContainsKey(e.MachineName))
        {
            MachinePosInfo posInfo = MapMachinePosManager.Instance.MachinePosDic[e.MachineName];
            bool needChangeRoom = _curStayRoom != posInfo.RoomType;
            bool abandonSlide = needChangeRoom && e.AbandonIfNeedChangeMapRoom;

            if (!abandonSlide)
            {
                UnityAction slideCallback = () => StartCoroutine(SlideToMachine(posInfo, e.Callback));
                if (needChangeRoom)
                {
                    string locateAtMachineName = e.CancleSlideWhenEnterRoom ? e.MachineName : null;
                    UnityAction callback = locateAtMachineName == null
                                         ? slideCallback
                                         : e.Callback;
                    CitrusEventManager.instance.Raise(new AskEnterMapRoomEvent(posInfo.RoomType, callback, locateAtMachineName));
                }
                else
                {          
                    //play map curtain anim when chang between tiny and normal machine
                    MapCurtainsUiController mapCurtainsUi = MapMachinePosManager.Instance.MapCurtainsUiCtrl;
                    bool needPlayCurtainAnim = _curStayRoom == MapMachineRoom.CUSTOM 
                                             && mapCurtainsUi != null 
                                             && MachineUnlockSettingConfig.Instance.IsTinyMachine(e.MachineName) != mapCurtainsUi.AlreadySwitched;
                    if (needPlayCurtainAnim)
                        MapMachinePosManager.Instance.RequstSwitchTinyAndNormalRoom(slideCallback);
                    else
                        slideCallback.Invoke();
                }
            }
        }
    }

    private IEnumerator SlideToMachine(MachinePosInfo posInfo, UnityAction callback)
    {
        ScrollRect scroll = _roomList[posInfo.RoomType].GetComponent<ScrollRect>();
        scroll.enabled = false;
        float targetPos = -MapMachinePosManager.Instance.CalculatePreferMachinePos(posInfo.RoomType, posInfo.MachineName);
        Vector3 templeVec = scroll.content.anchoredPosition3D;
        while (Mathf.Abs(targetPos - scroll.content.anchoredPosition3D.x) > 1 
                    && scroll.horizontalNormalizedPosition > -0.02f 
                    && scroll.horizontalNormalizedPosition < 1.02)
        {
            templeVec.x = Mathf.Lerp(scroll.content.anchoredPosition3D.x, targetPos, 0.15f);
            scroll.content.anchoredPosition3D = templeVec;
            yield return null;
        }
        if (callback != null)
            callback.Invoke();

        scroll.enabled = true;
    }

    public void LoadSelectMachinePosition()
    {
        float x = UserDeviceLocalData.Instance.GetMapRoomLastStayPos(_curStayRoom);
        RectTransform content = _contentDict[_curStayRoom];

        Vector3 newPostion = content.anchoredPosition3D;
        newPostion.x = x;
        content.anchoredPosition3D = newPostion;
    }

    void SaveSelectMachinePosition()
	{
		UserDeviceLocalData.Instance.SetMapRoomLastStayPos(_curStayRoom, _contentDict[_curStayRoom].anchoredPosition3D.x);
	    UserDeviceLocalData.Instance.LastStayMapRoom = _curStayRoom;
        CitrusEventManager.instance.Raise(new OnRecordMachinePosEvent(_curStayRoom));
	}

    public void AddCreditButton(GameObject obj)
	{
		//#if DEBUG
		UserBasicData.Instance.AddCredits(10000, FreeCreditsSource.NotFree, true);
		//#endif
	}

	public void ResetCreditButton(GameObject obj)
	{
		//#if DEBUG
		ulong sub = UserBasicData.Instance.Credits - 10000;
		UserBasicData.Instance.SubtractCredits(sub, true);
		//#endif
	}

	private void UpdateScrollPosition(MapSceneUpdateScrollPosEvent Event){
		LoadSelectMachinePosition ();
	}

	private void AddAFPSCounter(){
		#if DEBUG
		UGUIUtility.CreateObj(AFPSCounterPath);
		#endif
	}

    void InitBg()
    {
        _mapBgs = new Dictionary<MapMachineType, GameObject>
        {
            {MapMachineType.Vip, VipMapBg},
            {MapMachineType.Normal, MapBg},
            {MapMachineType.Tiny, TinyMapBg}
        };
    }
//	void OnGUI()
//	{
//		#if DEBUG
//		if(GUI.Button(new Rect(10, 100, 100, 50), "Crash1"))
//		{
//			MapScene s = null;
//			s.AddCreditButton(null);
//		}
//		if(GUI.Button(new Rect(110, 100, 100, 50), "Crash2"))
//		{
//			Debug.Assert(false);
//			GameObject s = null;
//			Debug.Log("crash log2:" + s.activeSelf.ToString());
//		}
//		if(GUI.Button(new Rect(210, 100, 100, 50), "Crash3"))
//		{
//			//Fabric.Crashlytics.Crashlytics.Crash();
//			GameObject o = null;
//			Debug.Assert(o != null);
//			Debug.Log("crash log3");
//			throw new Exception("my exception");
//		}
//		#endif
//	}

    void ChangeMapBg(SwitchMachineTypeEvent msg)
    {
        ShowMapBg(msg.MachineType);
    }

    private void ShowMapBg(MapMachineType type)
    {
        foreach (var item in _mapBgs)
        {
            if (item.Key == type)
                item.Value.SetActive(true);
            else
                item.Value.SetActive(false);
        }
    }

	// 大厅特效加载
	private void LoadEffects(){
		string path = MapSettingConfig.Instance.Read<string>("MapEffect", "");

		if (!path.IsNullOrEmpty()){
			GameObject obj = AssetManager.Instance.LoadAsset<GameObject>(path);
			Debug.Assert(obj != null, "Load MapScene Effect Failed");
			if (obj != null){
				UGUIUtility.CreateObj(obj, _effectParent);
			}
		}
	}
    
    public static void ResetCurrentMachineName(){
        _currentMachineName = "None";
    }
}
