using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapCurtainsUiController : MonoBehaviour
{
    public float StopScrollOffset;
    public float ResumeTimeWhenStopScroll;
    public float ShowBoardOffset;
    public float AutoMoveOutWidth;
    public float AutoMoveInDuringTime;
    public float AutoMoveOutDuringTime;
    public float MinBoardUpdatePos;
    public float AutoMoveInWaitTime;
    public MapCurtainsAnimController AnimCtrl;
    public MapMoreSlotsBoardController LeftBoard;
    public MapMoreSlotsBoardController RightBoard;
    public RectTransform Content;

    private EventSystem _eventSystem;
    private ScrollRect _scrollRect;
    private MapMoreSlotsBoardController _curBoardCtrl;
    private RectTransform _curBoardTrans;
    private RectTransform _selfRectTransform;
    private RectTransform _tinyMachinesRoot;
    private GridLayoutGroup _tinyGridLayoutGroup;
    private bool _curtainIsShowingFlag;
    private bool _alreadySwtiched;
    private Vector3 _templeBoardPos = Vector3.one;
    private readonly float _boardPosArg = 0.5f;
    private float _curWaitTime;
    private int _gridLeftPadding;

    private Vector3 _lastTimeMouseOrFingerPos;
    private Vector3 _curMounseOrFingerPos;
    private bool _isNotFirstFrameDrag;
    private readonly float _minDragDistance = 20;

    public bool AlreadySwitched
    {
        get { return _alreadySwtiched; }
        set { _alreadySwtiched = value; }
    }

    public RectTransform CurBoardTrans
    {
        get { return _curBoardTrans; }
    }

    public float LengthOfCurtainVisiblePart
    {
        get
        {
            float relativeDis = Content.anchoredPosition3D.x + _selfRectTransform.anchoredPosition3D.x;
            float distance = _alreadySwtiched ? relativeDis : DeviceUtility.DesignWidth - relativeDis;
            return distance;
        }
    }

    void Awake()
    {
        CitrusEventManager.instance.AddListener<OnRecordMachinePosEvent>(FixMachineRecordPos);
        CitrusEventManager.instance.AddListener<EnterMapRoomEvent>(InitCurtainsPos);
    }

    void OnDisable()
    {
        //after disable custom room, reset some args
        _curtainIsShowingFlag = false;
        if (_scrollRect != null)
            _scrollRect.enabled = true;
    }

    void OnDestroy()
    {
        CitrusEventManager.instance.RemoveListener<OnRecordMachinePosEvent>(FixMachineRecordPos);
        CitrusEventManager.instance.RemoveListener<EnterMapRoomEvent>(InitCurtainsPos);
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        CheckAutoMoveInState();
        OnDragCurtain();
    }

    void Init()
    {
        _eventSystem = EventSystem.current;
        Content = transform.parent.GetComponent<RectTransform>();
        _scrollRect = Content.parent.parent.GetComponent<ScrollRect>();
        _selfRectTransform = gameObject.GetComponent<RectTransform>();
        _tinyGridLayoutGroup = Content.transform.GetComponentInChildren<GridLayoutGroup>();
        _tinyMachinesRoot = _tinyGridLayoutGroup.GetComponent<RectTransform>();

        AssetIfComponentIsNull(new List<Component> { _eventSystem , Content, _scrollRect, _selfRectTransform, _tinyGridLayoutGroup, _tinyMachinesRoot});

        _gridLeftPadding = _tinyGridLayoutGroup.padding.left;
       _scrollRect.onValueChanged.AddListener(OnScrollVauleChanged);
        RegisterSelfToRoomManager();
    }

    void AssetIfComponentIsNull(List<Component> componentList)
    {
        ListUtility.ForEach(componentList, 
            component => Debug.Assert(component != null, "MapCurtainsModule : component is missing!"));
    }

    void InitCurtainsPos(EnterMapRoomEvent e)
    {
        if (e.Room == MapMachineRoom.CUSTOM)
        {
            AnimCtrl.SetCurtainPos(UserDeviceLocalData.Instance.IsInTinyMachineRoom);
        }  
    }

    void RegisterSelfToRoomManager()
    {
        MapMachinePosManager.Instance.MapCurtainsUiCtrl = this;
    }

    void FixMachineRecordPos(OnRecordMachinePosEvent e)
    {
        if (e.CurStayRoom == MapMachineRoom.CUSTOM)
        {
            SetIsInTinyMachinesRoom(_alreadySwtiched);

            float recordXpos = UserDeviceLocalData.Instance.GetMapRoomLastStayPos(MapMachineRoom.CUSTOM);
            _selfRectTransform = _selfRectTransform ?? transform.GetComponent<RectTransform>();
            float selfXpos = _selfRectTransform.anchoredPosition3D.x;

            if (UserDeviceLocalData.Instance.IsInTinyMachineRoom)
            {
                if (Mathf.Abs(recordXpos) < selfXpos)
                {
                    UserDeviceLocalData.Instance.SetMapRoomLastStayPos(MapMachineRoom.CUSTOM, -selfXpos);
                }
            }
            else
            {
                if (Mathf.Abs(recordXpos) + DeviceUtility.DesignWidth > selfXpos)
                {
                    UserDeviceLocalData.Instance.SetMapRoomLastStayPos(MapMachineRoom.CUSTOM, -(selfXpos - DeviceUtility.DesignWidth));
                }
            }
        }
    }

    void SetIsInTinyMachinesRoom(bool isInTinyMachinesRoom)
    {
        UserDeviceLocalData.Instance.IsInTinyMachineRoom = isInTinyMachinesRoom;
    }

    void OnScrollVauleChanged(Vector2 value)
    {
        CleanWaitTime();
        NotifyCurtains();
    }

    void RefreshBoardPos()
    {
        float relativePos = LengthOfCurtainVisiblePart * _boardPosArg;
        _templeBoardPos = _curBoardTrans.anchoredPosition3D;
        bool needResetPos = _alreadySwtiched ? relativePos > -MinBoardUpdatePos : relativePos < MinBoardUpdatePos;

        if (needResetPos)
        {
            _templeBoardPos.x = _alreadySwtiched ? -MinBoardUpdatePos : MinBoardUpdatePos;
        }
        else
        {
            _templeBoardPos.x = relativePos;
        }
        _curBoardTrans.anchoredPosition3D = _templeBoardPos;
    }

    void NotifyCurtains()
    {
        if (NeedShowCurtain() && !_curtainIsShowingFlag)
        {
            AutoMoveOut();
        }
    }

    void OnDragCurtain()
    {
        if (_curtainIsShowingFlag)
        {
            float moveDistanceX;
            bool isDraging = false;
            Vector3 mouseOrFingerDragPoint = Vector3.one;

#if UNITY_EDITOR
            isDraging = Input.GetMouseButton(0);
            mouseOrFingerDragPoint = Input.mousePosition;
#else
            isDraging = Input.touches.Length > 0;
            mouseOrFingerDragPoint = isDraging ? Input.touches[0].position : Vector2.zero;
#endif

            if (isDraging)
            {
                if (_isNotFirstFrameDrag)
                {
                    _curMounseOrFingerPos = mouseOrFingerDragPoint;
                    moveDistanceX = _curMounseOrFingerPos.x - _lastTimeMouseOrFingerPos.x;
                    _lastTimeMouseOrFingerPos = mouseOrFingerDragPoint;

                    if (Mathf.Abs(moveDistanceX) > _minDragDistance)
                    {
                        bool slideToLeft = moveDistanceX < 0;
                        bool needSwitch = (slideToLeft && !_alreadySwtiched) || (!slideToLeft && _alreadySwtiched);
                        if (needSwitch)
                        {
                            MapCurtainsAnim animType = slideToLeft
                           ? MapCurtainsAnim.MoveInFromRight
                           : MapCurtainsAnim.MoveInFromLeft;
                            PlayMoreSlotsAnim(animType);
                        }
                        else
                        {
                            AutoMoveIn();
                            _curtainIsShowingFlag = false;
                        }
                    }
                }
                else
                {
                    _curMounseOrFingerPos = mouseOrFingerDragPoint;
                    _lastTimeMouseOrFingerPos = mouseOrFingerDragPoint;
                    _isNotFirstFrameDrag = true;
                }
            }
            else
            {
                _isNotFirstFrameDrag = false;
            }
        }
    }

    bool NeedShowCurtain()
    {
        float relativeXdis = Content.anchoredPosition3D.x + _selfRectTransform.anchoredPosition3D.x;
        bool result = _alreadySwtiched ? relativeXdis > StopScrollOffset : relativeXdis < DeviceUtility.DesignWidth - StopScrollOffset;

        return result;
    }

    void CleanWaitTime()
    {
        _curWaitTime = 0;
    }

    void CheckAutoMoveInState()
    {
        if (_curtainIsShowingFlag)
        {
            _curWaitTime += Time.deltaTime;
            if (_curWaitTime > AutoMoveInWaitTime)
            {
                _curtainIsShowingFlag = false;
                AutoMoveIn();
                CleanWaitTime();
            }
        }
        else
        {
            _curWaitTime = 0;
        }
    }

    public void PlayMoreSlotsAnim(MapCurtainsAnim animType, bool needHideBoard = true, UnityAction callback = null)
    {
        _curtainIsShowingFlag = false;
        BlockScrollEvent(true);
        StartCoroutine(AnimCtrl.PlayAnim(animType, () =>
        {
            OnCurtainsShowEnd();
            _scrollRect.enabled = true;
            if (callback != null)
            {
                callback.Invoke();
            }
        }));

        if (_curBoardCtrl != null && needHideBoard)
        {
            HideMoreSlotsBoard();
        }
        if (!_alreadySwtiched)
        {
            ResetLeftPadding();
        }
    }

    void ShowMoreSlotsBoard()
    {
        _curBoardCtrl = _alreadySwtiched ? RightBoard : LeftBoard;
        _curBoardTrans = _curBoardCtrl.GetComponent<RectTransform>();
        _curBoardCtrl.BoardButton.enabled = true;
        RefreshBoardPos();
        _curBoardCtrl.PlayBoardAnim(MoreSlotsAnim.ShowBoard);
    }

    void HideMoreSlotsBoard()
    {
        _curBoardCtrl = _alreadySwtiched ? RightBoard : LeftBoard;
        _curBoardTrans = _curBoardCtrl.GetComponent<RectTransform>();
        _curBoardCtrl.BoardButton.enabled = false;
        _curBoardCtrl.PlayBoardAnim(MoreSlotsAnim.HideBoard);
    }

    void AutoMoveIn()
    {
        float distance = _alreadySwtiched ? - LengthOfCurtainVisiblePart : LengthOfCurtainVisiblePart;
        StartCoroutine(AutoMove(AutoMoveInDuringTime, distance, () =>
        {
            if (_curBoardCtrl != null)
            {
                _curBoardCtrl.AnimCtrl.SetTrigger("IdleOutside");
            }
        }));

        if (AlreadySwitched)
        {
            StartCoroutine(FixTinyMachineRootPos(AutoMoveInDuringTime, true));
        }
    }

    void AutoMoveOut()
    {
        float distance = _alreadySwtiched ? AutoMoveOutWidth - LengthOfCurtainVisiblePart : LengthOfCurtainVisiblePart - AutoMoveOutWidth;
        StartCoroutine(AutoMove(AutoMoveOutDuringTime, distance, () =>
        {
            ShowMoreSlotsBoard();
            _scrollRect.enabled = false;
            _curtainIsShowingFlag = true;
        }));

        if (AlreadySwitched)
        {
            StartCoroutine(FixTinyMachineRootPos(AutoMoveOutDuringTime, false));
        }
    }

    void BlockScrollEvent(bool block)
    {
        _scrollRect.enabled = !block;
        _eventSystem.enabled = !block;
    }

    void ResetLeftPadding()
    {
        _tinyMachinesRoot.anchoredPosition3D = Vector3.zero;
    }

    IEnumerator FixTinyMachineRootPos(float time, bool moveIn)
    {
        Vector3 initPos = _tinyMachinesRoot.anchoredPosition3D;
        float targetXpos = moveIn ? 0 : -_gridLeftPadding;
        float distance = targetXpos - _tinyMachinesRoot.anchoredPosition3D.x;
        float speedPerSec = distance / time;
        Vector3 templeVec = Vector3.zero;
        while (time > 0)
        {
            templeVec.x = speedPerSec * Time.deltaTime;
            _tinyMachinesRoot.anchoredPosition3D += templeVec;
            time -= Time.deltaTime;
            yield return null;
        }

        _tinyMachinesRoot.anchoredPosition3D = initPos + new Vector3(distance, 0, 0);
    }

    IEnumerator AutoMove(float duringTime, float distance, Action callback = null)
    {
        BlockScrollEvent(true);
        Vector3 initPos = Content.anchoredPosition3D;
        float speedPerSec = distance/duringTime;
        Vector3 templeVec = Vector3.zero;

        while (duringTime > 0)
        {
            templeVec.x = speedPerSec*Time.deltaTime;
            Content.anchoredPosition3D += templeVec;
            duringTime -= Time.deltaTime;
            yield return null;
        }

        Content.anchoredPosition3D = initPos + new Vector3(distance, 0, 0);

        BlockScrollEvent(false);
        if (callback != null)
        {
            callback.Invoke();
        }
    }

    void OnCurtainsShowEnd()
    {
        BlockScrollEvent(false);
        _alreadySwtiched = !_alreadySwtiched;
    }
}
