using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseRewardAdButton : MonoBehaviour
{
    [SerializeField] protected BaseRewardADController RewardAdController;
    [SerializeField] protected Button AdButton;
    [SerializeField] protected GameObject AdUiButton;
    public string AdTypeName;

    protected virtual void Init()
    {
    }

    protected void SetButtonClickListener()
    {
        AdButton.onClick.AddListener(OnButtonClick);
    }

    public abstract void OnButtonClick();

    public virtual void OnAdShow()
    {
        ShowAdButton(true);
    }

    public virtual void OnAdClose()
    {
        ShowAdButton(false);
    }

    public virtual void GetBonus(){}

    public virtual void ShowAdButton(bool show)
    {
        AdUiButton.SetActive(show);
    }
}
