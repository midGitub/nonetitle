using CitrusFramework;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class VipMachineUnlockNotifyUiController : PopUpControler
{
    public Text UnlockCountText;
    public Text VipLevel;
    public Button ConfirmButton;
    public Button ExitButton;
    public Image VipIcon;

    private List<string> _unlockList;
    private bool _isWaitingForOpen; //user may promote vip lv many times when this popup is waiting for show,  we need handle this situation to avoid opening mutiple times at once

    public override void Init()
    {
        base.Init();

        ConfirmButton.onClick.AddListener(TryUnlockVipMachine);
        ExitButton.onClick.AddListener(TryUnlockVipMachineInVipRoom);
        RegisterCloseButton(ConfirmButton);
        RegisterCloseButton(ExitButton);
    }

    public void HandleVipLvUp(LevelData data)
    {
        _unlockList = MachineUnlockHelper.NewUnlockVipMachineList((int)data.Level);
        if (_unlockList.Count > 0)
        {
            UnlockCountText.text = _unlockList.Count.ToString();
            VIPData currLevelInfor = VIPSystem.Instance.GetCurrVIPInforData;
            VipIcon.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(currLevelInfor.VIPLevelName);
            VipLevel.text = currLevelInfor.VIPLevelName.ToUpper() + " VIP";
            Open();
        }
    }

    public override void Open()
    {
        if (!_isWaitingForOpen)
        {
            _isWaitingForOpen = true;
            base.Open();
        }
    }

    public override void Close()
    {
        _isWaitingForOpen = false;
        base.Close();
    }

    void TryUnlockVipMachine()
    {
        if (_unlockList != null && _unlockList.Count > 0)
        {
            string curSceneName = ScenesController.Instance.GetCurrSceneName();
            if (curSceneName == ScenesController.GameSceneName)
            {
                ScenesController.Instance.EnterMainMapScene(() =>
                {
                    CitrusEventManager.instance.Raise(new AskSlideToMachinePosEvent(ListUtility.Last(_unlockList), null));
                });
            }
            else if (curSceneName == ScenesController.MainMapSceneName)
            {
                UnityAction askUnlockMachine =
                    () => CitrusEventManager.instance.Raise(new AskUnlockMachineEvent(_unlockList));

                CitrusEventManager.instance.Raise(new AskSlideToMachinePosEvent(ListUtility.Last(_unlockList), askUnlockMachine));
            }
        }
    }

    void TryUnlockVipMachineInVipRoom()
    {
        if (ScenesController.Instance.GetCurrSceneName() == ScenesController.MainMapSceneName)
        {
            UnityAction askUnlockMachine =
                   () => CitrusEventManager.instance.Raise(new AskUnlockMachineEvent(_unlockList));

            CitrusEventManager.instance.Raise(new AskSlideToMachinePosEvent(ListUtility.Last(_unlockList), askUnlockMachine, true));
        }
    }
}
