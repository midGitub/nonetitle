using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CitrusFramework;
using System.Text;
using UnityEngine.UI;

public class TLStoreController : Singleton<TLStoreController> {

    [SerializeField]
    private GeneralStoreItem _tLItem;
    [SerializeField]
    private CountTimeUI _countTimeUI;
    [SerializeField]
    private Button _exitButton;
    
    private bool _shouldShowTLStore = false;
    private bool _showTLStoreAfterBigWin = false;
    public bool ShowTLStoreAfterBigWin{
        get{ return _showTLStoreAfterBigWin;}
    }

    private WindowInfo _windowInfoReceipt = null;

    void Start()
    {
        CitrusEventManager.instance.AddListener<ManualCloseBigWinEvent>(TryShowAfterBigWin);
    }

    void OnDestroy()
    {
        CitrusEventManager.instance.RemoveListener<ManualCloseBigWinEvent>(TryShowAfterBigWin);
        base.OnDestroy();
    }

    public void TryOpenTLStore()
    {
        // Xhj 检查是否在大厅环境 -> 检查是否符合弹出限时商城条件
        if (ScenesController.Instance.GetCurrSceneName() == ScenesController.MainMapSceneName)
        {
            GetTLStoreState();
            //Debug.Log(_shouldShowTLStore + " " + !StoreController.Instance.IsOneStoreOpening(StoreType.Deal_TL));
            if (_shouldShowTLStore)// && !StoreController.Instance.IsAtLeastOneStoreOpening)
            {
                AutoPopupTLStore();
            }
        }
    }

	void Update()
    {
        TimeLimitedStoreHelper.IsInTLStorePeriod((bool arg1, TimeSpan arg2) => 
            {
                if (arg1)
                {
                    if (StoreController.Instance.IsStoreOpening(StoreType.Deal_TL))
                    {
                        _countTimeUI.SetValue(arg2);
                    }
                }
                else
                {
                    TLStoreEnd();
                }
            }); 
    }

    void GetTLStoreState()
    {
        NetworkTimeHelper.Instance.GetNowTime((bool arg1, DateTime arg2) =>
        {
            if(arg1)
            {
                _shouldShowTLStore = TimeLimitedStoreHelper.ShouldPopupTLStore();
            }
            else _shouldShowTLStore = false;
        });
    }

    // Xhj 自动弹出限时商城
    public void AutoPopupTLStore()
    {
        // Xhj 申请UI弹出
        Show(OpenPos.CollectHourlyBonus);
    }

    // 
    public void ManualOpenTLStore(OpenPos openpostion)
    { 
        // Xhj 申请UI弹出
        Show(openpostion);
    }

    public void BuySuccess()
    {
        UserBasicData.Instance.SetHasBoughtTLItem(true);

        TLStoreEnd();
    }

    private void TLStoreEnd()
    {
        if (StoreController.Instance.IsStoreOpening(StoreType.Deal_TL))
        {
            HideTLStore(); 
        }
        CitrusEventManager.instance.Raise(new TLStoreEndEvent());
    }

    public void TryShowAfterBigWin(ManualCloseBigWinEvent e)
    {
        // Xhj 因为这里判断了当前是否有窗口，所以如果能显示就会直接显示，不需要将条件判断留到Open里再做，否则还要向WindowManager申请关闭
        GetTLStoreState();
        _showTLStoreAfterBigWin = TimeLimitedStoreHelper.IsFirstBigWinToday() &&
                                  !WindowManager.Instance.IsThereOpeningWindow &&
                                  _windowInfoReceipt == null &&
                                  _shouldShowTLStore;
        if (_showTLStoreAfterBigWin)
        {
            if (_shouldShowTLStore)
            {
                _windowInfoReceipt = new WindowInfo(FirstBigWinFirstPopOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
                WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
            }
            else if (TimeLimitedStoreHelper.InPeriodButNotShouldPop())
            {
                _windowInfoReceipt = new WindowInfo(FirstBigWinInPeriodOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
                WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
            }
        }
    }

    public void Show(OpenPos openPosition)
    {
        if (_windowInfoReceipt == null)
        {
            if (openPosition == OpenPos.CollectHourlyBonus)
                _windowInfoReceipt = new WindowInfo(CollectHourBonusOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
            else if (openPosition == OpenPos.Lobby)
                _windowInfoReceipt = new WindowInfo(LobbyOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
            else if (openPosition == OpenPos.GameUp)
                _windowInfoReceipt = new WindowInfo(GameUpOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);

            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt); 
        }
    }

    public void HideTLStore()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    private void FirstTimePopOpen()
    {
        UserBasicData.Instance.SetHasBoughtTLItem(false);
        //fill item before show time limited store
        string productID = TimeLimitedStoreHelper.GetProductID(VIPSystem.Instance.GetCurrVIPInforData);
		if (_tLItem != null) {
			_tLItem.ProductID = productID;
			_tLItem.UpdateVal();
		}

        UserBasicData.Instance.SetLastTimeLimitedStore(NetworkTimeHelper.Instance.GetNowTime());
        UserBasicData.Instance.SetLimitedStoreItemID(productID);

        CitrusEventManager.instance.Raise(new TLStoreShowEvent(IAPCatalogConfig.Instance.FindIAPItemByID(productID)));  
    }

    public void CollectHourBonusOpen()
    {
        FirstTimePopOpen();
        StoreController.Instance.OpenStoreUI(OpenPos.CollectHourlyBonus.ToString(), StoreType.Deal_TL); 
    }

    private void NotFirstTImeOpen()
    {
        string productID = UserBasicData.Instance.LimitedStoreItemID;
        _tLItem.ProductID = productID;
        _tLItem.UpdateVal();
    }

    public void LobbyOpen()
    {
        NotFirstTImeOpen();
        StoreController.Instance.OpenStoreUI(OpenPos.Lobby.ToString(), StoreType.Deal_TL); 
    }

    public void GameUpOpen()
    {
        NotFirstTImeOpen();
        StoreController.Instance.OpenStoreUI(OpenPos.GameUp.ToString(), StoreType.Deal_TL);
    }

    public void FirstBigWinFirstPopOpen()
    {
        FirstTimePopOpen();
        StoreController.Instance.OpenStoreUI(OpenPos.HugeWin.ToString(), StoreType.Deal_TL);
        ResetAutoSpin();
    }

    public void FirstBigWinInPeriodOpen()
    {
        NotFirstTImeOpen();
        StoreController.Instance.OpenStoreUI(OpenPos.HugeWin.ToString(), StoreType.Deal_TL);
        ResetAutoSpin();
    }

    private void SelfClose(Action callBack)
    {
        StoreController.Instance.CloseAllStoreUI(callBack);
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(StoreController.Instance.StoreCanvas, _exitButton.gameObject))
        {
            StoreController.Instance.CloseAllStoreUI(() => {
                _windowInfoReceipt = null;
                callBack(true);     
            }); 
        }
        else
        {
            callBack(false);
        }
    }

    public void ForceToCloseImmediately()
    {
        StoreController.Instance.CloseAllStoreUI(() =>
            {
            }, true);
        _windowInfoReceipt = null;
    }

    private void ResetAutoSpin(){
        //停止自动
        if(GameScene.Instance != null)
        {
            GameScene.Instance.PuzzleMachine.SetSpinMode(SpinMode.Normal);
        }
    }

     //Xhj 测试用，不用记得注释！！
//    void OnGUI()
//    {
//#if DEBUG
//        if (GUI.Button(new Rect(10, 100, 100, 50), "-29m50s"))
//        {
//            DateTime tempDT = UserBasicData.Instance.LastTimeLimitedStore;
//            UserBasicData.Instance.SetLastTimeLimitedStore(tempDT.AddMinutes(-29).AddSeconds(-50));
//        }
//
//        if (GUI.Button(new Rect(120, 100, 100, 50), "+1day"))
//        {
//            DateTime tempDT = UserBasicData.Instance.LastTimeLimitedStore;
//            UserBasicData.Instance.SetLastTimeLimitedStore(tempDT.AddDays(-1));
//            tempDT = UserDeviceLocalData.Instance.LastBigWinDay;
//            UserDeviceLocalData.Instance.LastBigWinDay = tempDT.AddDays(-1);
//        }
//
//        if (GUI.Button(new Rect(230, 100, 100, 50), "ResetHourBonus"))
//        {
//            DateTime tempDT = UserBasicData.Instance.LastHourBonusDateTime;
//            UserBasicData.Instance.SetLastGetHourBonusDate(tempDT.AddHours(-1));
//            CitrusEventManager.instance.Raise(new UserDataLoadEvent());
//        }
//
////        if (GUI.Button(new Rect(340, 100, 100, 50), "ResetFirstBW"))
////        {
////            DateTime tempDT = UserDeviceLocalData.Instance.LastBigWinDay;
////            UserDeviceLocalData.Instance.LastBigWinDay = tempDT.AddDays(-1);
////        }
//
//        if (GUI.Button(new Rect(340, 100, 100, 50), "ResetDayBonus"))
//        {
//            DateTime tempDT = UserBasicData.Instance.LastDayBonusDateTime;
//            UserBasicData.Instance.SetLastGetDayBonusDate(tempDT.AddDays(-1));
//        }
//
//
////        if (GUI.Button(new Rect(450, 100, 100, 50), "M1Comment"))
////        {
////            UserDeviceLocalData.Instance.MachineCommentTimesToday = 0;
////            UserDeviceLocalData.Instance.LastRefuseMachineCommentTime = NetworkTimeHelper.Instance.GetNowTime().AddDays(-3);
////            UserDeviceLocalData.Instance.LastMachineCommentTime = NetworkTimeHelper.Instance.GetNowTime().AddDays(-3);
////            UserMachineData.Instance.SetFirstTimeFlag("M1", true);
////            UserDeviceLocalData.Instance.FirstEnterGameTime = NetworkTimeHelper.Instance.GetNowTime().AddDays(-3);
////            UserDeviceLocalData.Instance.LastPopFaceBookLogin = NetworkTimeHelper.Instance.GetNowTime().AddDays(-1);
////        }
////       
////        if (GUI.Button(new Rect(560, 100, 100, 50), "ShowInfo"))
////        {
////            LogUtility.Log("Times today: " + UserDeviceLocalData.Instance.MachineCommentTimesToday, Color.magenta);
////            LogUtility.Log("LastRefuseMachineCommentTime: " + UserDeviceLocalData.Instance.LastRefuseMachineCommentTime, Color.magenta);
////            LogUtility.Log("LastMachineCommentTime: " + UserDeviceLocalData.Instance.LastMachineCommentTime, Color.magenta);
////            LogUtility.Log("M1 FirstTimeFlag: " + UserMachineData.Instance.GetIsFirstTimeFlag("M1"), Color.magenta);
////            LogUtility.Log("FirstEnterGameTime: " + UserDeviceLocalData.Instance.FirstEnterGameTime, Color.magenta);
////            LogUtility.Log("Now Time: " + NetworkTimeHelper.Instance.GetNowTime(), Color.magenta);
////            LogUtility.Log("DailyBonus canvas is active?: " + DayBonus.Instance.Canvas.gameObject.activeInHierarchy, Color.magenta);
////            LogUtility.Log("LastPopFaceBook: " + UserDeviceLocalData.Instance.LastPopFaceBookLogin, Color.magenta);
////            StringBuilder sb = new StringBuilder();
////            LogUtility.Log("MachineInfoDict Keys Count: " + UserMachineData.Instance.MachineInfoDict.Keys.Count);
////            if (UserMachineData.Instance.MachineInfoDict != null)
////            {
////                foreach (var item in UserMachineData.Instance.MachineInfoDict.Keys)
////                {
////                    sb.Append(item + " ");
////                }
////                LogUtility.Log(sb.ToString(), Color.magenta); 
////            }
////        }
//#endif
//    }
}
