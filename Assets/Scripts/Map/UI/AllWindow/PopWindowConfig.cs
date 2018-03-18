using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.Events;
using System;

//Map the time row in uiWindows sheet
public enum ShowWindowTime
{
    FirstDayEnterMap = 1,
    Backrupt = 2,
    HugeWin = 3,
    FirstHourBonusToday = 4,
}

//Map the ID row in uiWindow sheet, need add new enum type when add new id in this sheet
public enum UiWindow
{
    FacebookLigin = 1,
    BecomeVip = 2,
    DailyBonus = 3,
    SmallBuy = 4,
    Piggybank = 5,
    DealTL = 6,
    Dice = 7,
    BackFlow = 8,
    DoubleLvUpNotify = 9,
}

public class PopWindowConfig : SimpleSingleton<PopWindowConfig> {

    private Dictionary<UiWindow, Action> _showWindowActionDict;

    public void Init()
    {
        CitrusEventManager.instance.AddListener<LoadSceneFinishedEvent>(OnLoadSceneFinished);  
        CitrusEventManager.instance.AddListener<OutOfCreditsEvent>(OnOutOfCredits);

        InitWindowActionDict();
    }

    public void Clean()
    {
        CitrusEventManager.instance.RemoveListener<LoadSceneFinishedEvent>(OnLoadSceneFinished);
        CitrusEventManager.instance.RemoveListener<OutOfCreditsEvent>(OnOutOfCredits);
    }

    private void InitWindowActionDict()
    {
        //This should not be written like this, custom way is to use common interface to open ui window, but ui framework in our project is in a mess
        _showWindowActionDict = new Dictionary<UiWindow, Action>
        {
            {UiWindow.FacebookLigin, () => CitrusEventManager.instance.Raise(new FaceBookLoginPopEvent())},
            {UiWindow.BecomeVip, () => SpecialOfferHelper.Instance.TryShowSpecialWindow()},
            {UiWindow.DailyBonus, () => DayBonus.Instance.TryShow()},
            {UiWindow.SmallBuy, null},
            {UiWindow.Piggybank, null},
            {UiWindow.DealTL, null},
            {UiWindow.Dice, null},
            {UiWindow.BackFlow, () => BackFlowReward.TryShow()},
            {UiWindow.DoubleLvUpNotify, () => DoubleLevelUpNotify.TryShow()},
        };
    }

    public void OnLoadSceneFinished(LoadSceneFinishedEvent e)
    {
        // Xhj 每次转变场景会清除之前失效的窗口
        WindowManager.Instance.ClearAllWindows();

        // Xhj 清除失效窗口后依设置弹出窗口
        if (e.SceneName == ScenesController.MainMapSceneName)
        {
            if (UserDeviceLocalData.Instance.IsFirstLoginToday)
            {
                ShowWindowsAtTime(ShowWindowTime.FirstDayEnterMap);
            }
            else
            {
                if (MapScene.CurrentMachineName != null// 需要是从机台到大厅
                   && UserMachineData.Instance.GetIsFirstTimeFlag(MapScene.CurrentMachineName)// 是否已经评价过该机台
                   && !MachineCommentHelper.IsInNewNoCommentPeriod()// 是否在新玩家免评价Ï的时间段内
                   && !MachineCommentHelper.IsInDayInterval()//是否在几天内不用评价
                   && !MachineCommentHelper.IsInMinInterval()//是否在几分钟内不用评价
                   && !MachineCommentHelper.DoesExceedTimesLimitBerDay()) //是否超过了一天的评价次数上限
                {
                    LogUtility.Log("PopConfig OnMachineComment", Color.magenta);
                    UIManager.Instance.OpenMachineCommentUI();
                }
                else
                {
                    LogUtility.Log("PopConfig OnEnterMapScene", Color.magenta);
                    CheckToShowWindows();
                    CitrusEventManager.instance.Raise(new FaceBookLoginPopEvent());
                    StoreController.Instance.threeStoreController.TryShow(OpenPos.EnterLobby);
                }
            }
            RegisterMaxWinActivity.Instance.TryShowActivityPopup();
            DoubleHourBonusActivity.Instance.TryStartActivity();
            CheckBillBoard();
            
            MapScene.ResetCurrentMachineName();// 放在这里重置，这样不会影响上面的机台评价
        }
        if (e.SceneName == ScenesController.MainMapSceneName || e.SceneName == ScenesController.GameSceneName)
		{
			SpecialOfferHelper.Instance.TryShowIcon();
			DoubleLevelUpHelper.Instance.TryShowIcon();
		}
    }
     
//    private void OnEnterLobby()
//    {
//        LogUtility.Log("PopConfig OnEnterMapScene", Color.magenta);
//		CheckToShowWindows ();
//		CheckBillBoard();
//        CitrusEventManager.instance.Raise(new FaceBookLoginPopEvent());
//        StoreController.Instance.threeStoreController.TryShow(OpenPos.EnterLobby); 
//    }

    public void OnOutOfCredits(OutOfCreditsEvent e)
    {
        PiggyBankUIController.Instance.RecordCriditsWhenBckrupt(e.UserCreditsNumWhenBackrupt);
        StoreController.Instance.twoStoreController.Show();
        PiggyBankUIController.Instance.Show(OpenPos.Auto);
    }

	private void CheckToShowWindows()
	{
		BackFlowReward.TryShow ();
		bool result = DoubleLevelUpNotify.TryShow();
		DayBonus.Instance.TryShow ();
		if(!result)
			SpecialOfferHelper.Instance.TryShowSpecialWindow ();
	}
		
	private void CheckBillBoard()
	{
		DoubleLevelUpHelper.Instance.Tryshow();

		List<int> advance = PromotionHelper.Instance.PromotionAdvanceDayList;
		List<int> last = PromotionHelper.Instance.PromotionLastDayList;
		List<string> name = PromotionHelper.Instance.PromotionNameList;
		for(int i=0;i<PromotionHelper.Instance.PromotionLen;i++)
		{
			PromotionHelper.Instance.Tryshow(name[i],last[i],advance[i]);
		}

		NewMachineHelper.Instance.AddNewMachineBoard();
        NoFunctionBillboard.Instance.AddBillboards();
	}

    private void ShowWindowsAtTime(ShowWindowTime time)
    {
        List<UiWindow> windowsFilterByGroup = UiWindowsConfig.Instance.GetWindowsByShowTime(time, true);

        //special condition for firstDayEnterMap, if backflow ui should be open, then only dailyBonus and doubleLvUpNotify ui can show, other uis should be blocked
        if (time == ShowWindowTime.FirstDayEnterMap && BackFlowReward.ShouldShow())
            windowsFilterByGroup = new List<UiWindow>{ UiWindow.BackFlow, UiWindow.DailyBonus, UiWindow.DoubleLvUpNotify};

        ListUtility.ForEach(windowsFilterByGroup, id =>
        {
            UiWindow uiWindow = id;
            if (_showWindowActionDict.ContainsKey(uiWindow) && _showWindowActionDict[uiWindow] != null)
                _showWindowActionDict[uiWindow].Invoke();
        });

    }
}
