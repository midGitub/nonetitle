using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CitrusFramework;
using System;

public class MainMapBar : MonoBehaviour
{
	public GameObject _jackpotNotifyParent;

	private static bool _gameIsPlaying = false;

    [SerializeField]
    public TLPromptController _TLController;

	public static bool GameIsPlaying
	{
		get { return _gameIsPlaying; }
	}

	private void Awake()
	{
		_gameIsPlaying = true;
	}

    private void Start()
    {
        CitrusEventManager.instance.AddListener<TLStoreShowEvent>(ActiveTLStorePrompt);
        CitrusEventManager.instance.AddListener<TLStoreEndEvent>(HideTLStorePrompt);
        NetworkTimeHelper.Instance.ServerTimeHasGottenEvent += CheckTLStorePromptState;
        CheckTLStorePromptState();

		// 传递jackpot 通知parent
		PuzzleJackpotManager.Instance.SetNotifyParent(_jackpotNotifyParent);
    }

    private void OnDestroy()
    {
        CitrusEventManager.instance.RemoveListener<TLStoreShowEvent>(ActiveTLStorePrompt);
        CitrusEventManager.instance.RemoveListener<TLStoreEndEvent>(HideTLStorePrompt);
        if (NetworkTimeHelper.Instance != null)
        {
            NetworkTimeHelper.Instance.ServerTimeHasGottenEvent -= CheckTLStorePromptState;
        }
    }

    private void CheckTLStorePromptState()
    {
        if (TimeLimitedStoreHelper.InPeriodButNotShouldPop())
        {
            string productID = UserBasicData.Instance.LimitedStoreItemID;
            IAPCatalogData item = IAPCatalogConfig.Instance.FindIAPItemByID(productID);
            _TLController.Active(item);
        } 
    }

    private void ActiveTLStorePrompt(TLStoreShowEvent e)
    {
        _TLController.Active(e.item);
    }

    private void HideTLStorePrompt(TLStoreEndEvent e)
    {
        _TLController.Hide();
    }

	private void ClickSound()
	{
		AudioManager.Instance.PlaySound(AudioType.Click);
	}

#if DEBUG
	public void TestCleanCoins()
	{
		UserBasicData.Instance.CleanCredits();
	}
#endif

	public void OnVIPButtonDown()
	{
		ClickSound();
        VIPUIController.Instance.Show();
	}

	public void OnBuyButtonDown()
	{
		ClickSound();
        OpenPos names = OpenPos.Lobby;
        if(ScenesController.Instance.GetCurrSceneName() == ScenesController.GameSceneName)
        {
            names = OpenPos.GameUp;
        }
        StoreController.Instance.bigStoreController.Show(names);
	}

    public void OnBuyCredditsDown()
    {
        ClickSound();
        StoreController.Instance.bigStoreController.Show(OpenPos.GameBelow);
    }

	public void OnDealButtonDown()
	{
		ClickSound();

        OpenPos names = OpenPos.Lobby;
		if(ScenesController.Instance.GetCurrSceneName() == ScenesController.GameSceneName)
		{
            names = OpenPos.GameUp;
		}

        // xhj 必须要自动弹出过限时商城，并仍在之后的30分钟内才能通过点击Deal按钮打开限时商城
        if (TimeLimitedStoreHelper.IsInTLStorePeriod() && !TimeLimitedStoreHelper.ShouldPopupTLStore()){
			TLStoreController.Instance.ManualOpenTLStore(names);
		} else {
            StoreController.Instance.threeStoreController.TryShow(names);
		}
	}

	public void OnSettingButtonDown()
	{
		ClickSound();
        SettingController.Instance.Show();
	}

	public void ShowComingSoon()
	{
		ClickSound();
		ComingSoon.Instance.ShowComingSoon();
	}

	public void BackButtonDown()
	{
		AudioManager.Instance.StopAllSound();
		AudioManager.Instance.PlaySound(AudioType.Click);

		ScenesController.Instance.EnterMainMapScene(null);
	}

	public void NewsButtonDown()
	{
		ComingSoon.Instance.ShowNoNews();
		// ComingSoon.Instance.ShowMillionComing();
		// NewsButton.Instance.ShowMillionComing();
	}

	public void OpenPiggyBank()
	{
        OpenPos names = OpenPos.Lobby;
		if(ScenesController.Instance.GetCurrSceneName() == ScenesController.GameSceneName)
		{
            names = OpenPos.GameUp;
		}
        PiggyBankUIController.Instance.Show(names);
		
	}

	public void OpenMail()
	{
		ClickSound();
		UIManager.Instance.OpenMailUI();
	}

}
