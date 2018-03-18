//#define MAP_MACHINE_RENDER_OPTIMIZE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Note: not define MAP_MACHINE_RENDER_OPTIMIZE now
// This optimization is replaced by another better solution:
// Add a component RectMask2D in the parent node of MapMachines and set the size
// of RectMask2D GameObject the same as screen size

public class LoadingMapMachineController : MonoBehaviour
{
	static readonly string _dirPath = "Map/Machine/Prefab/MapMachine_";
    static readonly string _curtainsPath = "Map/Machine/Prefab/Curtains";
    static readonly string _tinyMapMachineRootPath = "Map/Machine/Prefab/TinyMapMachineRoot";

    public MapScene MapSce;

    private Transform _tinyMapMachineParent;
    private Transform _tinyMapMachineRoot;
    private Transform _curTainTrans;

	Dictionary<MapMachineType, List<GameObject>> _mapMachineObjDict = new Dictionary<MapMachineType, List<GameObject>>();
    Dictionary<string, MapMachineRoom> _mapRoomMachinesDict = new Dictionary<string, MapMachineRoom>();

    public void Start()
	{
	    LoadAllMapMachines();
        AfterAllMachineLoaded();
	}

    void LoadAllMapMachines()
    {
        if (MapSettingConfig.Instance.IsVipRoomEnable)
            LoadVipMapMachines();
        LoadNormalMapMachines();
        if (MapSettingConfig.Instance.IsTinyMachineRoomEnable)
            LoadTinyMapMachines();
    }

    void LoadVipMapMachines()
    {
        LoadMapMachine(MapSce.VipScrollContent, MapMachineType.Vip, MapMachineRoom.VIP);
    }

    void LoadNormalMapMachines()
    {
        LoadMapMachine(MapSce.ScrollContent, MapMachineType.Normal, MapMachineRoom.CUSTOM);
    }

    void LoadTinyMapMachines()
    {
        CreateCurtains(MapSce.ScrollContent.transform);
        CreateGroupLayout(MapSce.ScrollContent.transform);
        LoadMapMachine(_tinyMapMachineParent, MapMachineType.Tiny, MapMachineRoom.CUSTOM);
    }

    void LoadMapMachine(Transform parent, MapMachineType machineType, MapMachineRoom locateAtRoom)
    {
        _mapMachineObjDict[machineType] = new List<GameObject>();

        if (MachineUnlockSettingConfig.Instance.MapMachineDic.ContainsKey(machineType))
        {
            List<string> namelist = MachineUnlockSettingConfig.Instance.MapMachineDic[machineType];
            ListUtility.ForEach(namelist, x =>
            {
                string newdir = _dirPath + x;
                var m = Instantiate(AssetManager.Instance.LoadAsset<GameObject>(newdir));
                SetGameObjectDefault(m, parent);
                _mapMachineObjDict[machineType].Add(m);
                _mapRoomMachinesDict[x] = locateAtRoom;
            });
        }
    }

    void AfterAllMachineLoaded()
    {
        RegisterMachinePosInfo();
        MapSce.EnterRoomAfterAllMachinesLoaded(_mapRoomMachinesDict);
#if MAP_MACHINE_RENDER_OPTIMIZE
		InitScreenPoints();
#endif
    }

    void RegisterMachinePosInfo()
    {
        MapMachinePosManager.Instance.RegisterPosInfo(MapSce.VipScrollContent, MapSce.ScrollContent, _curTainTrans, _tinyMapMachineRoot, _tinyMapMachineParent);
    }

#region TinyMachineSetting
    void CreateCurtains(Transform parent)
    {
        _curTainTrans = Instantiate(AssetManager.Instance.LoadAsset<GameObject>(_curtainsPath)).transform;
        SetParentWithDefaultTrans(_curTainTrans.gameObject, parent);
    }

    void CreateGroupLayout(Transform parent)
    {
        _tinyMapMachineRoot = Instantiate(AssetManager.Instance.LoadAsset<GameObject>(_tinyMapMachineRootPath)).transform;
        SetParentWithDefaultTrans(_tinyMapMachineRoot.gameObject, parent);
        TinyMapMachineRoot root = _tinyMapMachineRoot.GetComponent<TinyMapMachineRoot>();
        _tinyMapMachineParent = root.TinyMachineParent;
        SetTinyMachineGroupLayout(root);
    }

    void SetTinyMachineGroupLayout(TinyMapMachineRoot root)
    {
        GridLayoutGroup gridLayout = root.GridLayoutGroupComp;
        LayoutGroupAdapterBehaviour ipadAdapter = root.GetComponent<LayoutGroupAdapterBehaviour>();

        //because we need to set grouplayout size before ipadAdapter script work, so just adapt for ipad at here
        if (ipadAdapter != null && DeviceUtility.IsIPadResolution())
        {
            gridLayout.cellSize = new Vector2(ipadAdapter._scaleIpad.x * gridLayout.cellSize.x, ipadAdapter._scaleIpad.y * gridLayout.cellSize.y);
            ipadAdapter.enabled = false;
        }

        Dictionary<MapMachineType, List<string>> unlockDic = MachineUnlockSettingConfig.Instance.MapMachineDic;
        int tinyMachineCount = unlockDic.ContainsKey(MapMachineType.Tiny) ? unlockDic[MapMachineType.Tiny].Count : 0;
        int rowCount = gridLayout.constraintCount;
        int logicTinyMachineCount = tinyMachineCount % rowCount == 0 ? tinyMachineCount / rowCount : (tinyMachineCount + 1) / rowCount;
        float layoutWidth = gridLayout.padding.left + gridLayout.padding.right + logicTinyMachineCount * (gridLayout.cellSize.x + gridLayout.spacing.x);
        float layoutHeight = gridLayout.padding.top + gridLayout.padding.bottom + rowCount * (gridLayout.cellSize.y + gridLayout.spacing.y);
        _tinyMapMachineRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(layoutWidth, layoutHeight);
    }

#endregion

    void SetParentWithDefaultTrans(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
    }

    private void SetGameObjectDefault(GameObject go, Transform parent)
	{
        SetParentWithDefaultTrans(go, parent);
		go.GetComponentInChildren<MachineButton>().Init(MapSce);
		go.SetActive(true);
	}

	#if MAP_MACHINE_RENDER_OPTIMIZE

	static readonly float _normalMachineWidth = 8.0f;
	static readonly float _tinyMachineWidth = 5.0f;
	static readonly int _totalDelayFrameCount = 3;

	Vector3 _screenLeftPos;
	Vector3 _screenRightPos;
	int _curDelayFrameCount = 0;

	void InitScreenPoints()
	{
		_screenLeftPos = Camera.main.ScreenToWorldPoint(Vector3.zero);
		_screenRightPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, 0.0f));
	}

	void Update()
	{
		// Critical fix: if optimize from the first frame, all MapMachines will be invisible for the first one
		// or several frames. Maybe it's because the positions of MapMachines are not ready yet in the beginning.
		// So don't do the optimizations for the first several frames
		if(_curDelayFrameCount < _totalDelayFrameCount)
		{
			++_curDelayFrameCount;
			return;
		}
		
		// Normal machine
		foreach(GameObject obj in _mapMachineObjDict[MapMachineType.Normal])
		{
			Transform child = obj.transform.GetChild(0);
			GameObject childObj = child.gameObject;

			#if DEBUG
			Debug.Assert(!childObj.name.Contains("FX_"));
			#endif
			
			if(obj.transform.position.x <= _screenLeftPos.x - _normalMachineWidth || obj.transform.position.x >= _screenRightPos.x + _normalMachineWidth)
				child.gameObject.SetActive(false);
			else
				child.gameObject.SetActive(true);
		}

		//Tiny machine
		foreach(GameObject obj in _mapMachineObjDict[MapMachineType.Tiny])
		{
			Transform child = obj.transform.GetChild(0);
			GameObject childObj = child.gameObject;

			#if DEBUG
			Debug.Assert(!childObj.name.Contains("FX_"));
			#endif
			
			if(obj.transform.position.x <= _screenLeftPos.x - _tinyMachineWidth || obj.transform.position.x >= _screenRightPos.x + _tinyMachineWidth)
				child.gameObject.SetActive(false);
			else
				child.gameObject.SetActive(true);
		}
	}

	#endif
}
