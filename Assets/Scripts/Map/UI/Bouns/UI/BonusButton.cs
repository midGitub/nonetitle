using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BonusButton : MonoBehaviour, IPointerClickHandler
{
	public UnityEvent OnClickEvent = new UnityEvent();

	[SerializeField]
	private GameObject _canCollectionButton;

	[SerializeField]
	private GameObject _officeLineButton;

	[SerializeField]
	private GameObject _nextTimeButton;

	[SerializeField]
	private GameObject CanCollectionCoinsEffect;

	[SerializeField]
	private Animator CollectioningCoinsEffectAnimator;

	public void OnPointerClick(PointerEventData eventData)
	{
		OnClickEvent.Invoke();
	}

	/// <summary>
	/// 如果传入true状态转变成Cancollections
	/// </summary>
	/// <param name="CanCollection">If set to <c>true</c> can collection.</param>
	public void ChangeButtonState(bool CanCollection)
	{
		_canCollectionButton.SetActive(CanCollection);
		_nextTimeButton.SetActive(!CanCollection);
		CanCollectionCoinsEffect.SetActive(CanCollection);
		_officeLineButton.SetActive(!NetworkTimeHelper.Instance.IsServerTimeGetted);
	}

	public void ShowOfficleLine(bool show)
	{
		_officeLineButton.SetActive(show);
	}

    public void ShowCollectioningCoinsEffect(Action callBack)
	{
		AudioManager.Instance.PlaySound(AudioType.HourlyBonusCreditsRollUp);
		ShowCoinsText.ChangeTextAnimationTime(CollectioningCoinsEffectAnimator.GetCurrentAnimatorStateInfo(0).length + 2);
		CollectioningCoinsEffectAnimator.gameObject.SetActive(true);
		CitrusFramework.UnityTimer.Instance.StartTimer(this, 
			CollectioningCoinsEffectAnimator.GetCurrentAnimatorStateInfo(0).length,
			() => {
				CollectioningCoinsEffectAnimator.gameObject.SetActive(false);
                callBack();
			});
	}
}
