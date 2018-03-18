using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class ADSButton : MonoBehaviour
{
	public GameObject ButtonGameObject;

	[SerializeField]
	private Animator CollectioningCoinsEffectAnimator;

	private bool _isShowingADSEffect = false;
	private WaitForSeconds _waitFor1Second = new WaitForSeconds(1);
	private WaitForSeconds _waitFor10Seconds = new WaitForSeconds(10);

	private Coroutine _collectEffectCoroutine = null;

	// Use this for initialization
	private void Start()
	{
		StartCoroutine(TryUpdateShow());
	}

	private IEnumerator TryUpdateShow()
	{
		while(true)
		{
			//#if UNITY_EDITOR
			//yield return new WaitForSeconds(10);
			//#endif

			yield return _waitFor1Second;
			if(_isShowingADSEffect)
			{
				Debug.Log("ISShowADSEffect");
				yield return _waitFor10Seconds;
				continue;
			}

			if(ShowADSController.Instance.TryGetRewardADSVedio())
			{
				ButtonGameObject.SetActive(true);
			}
			else
			{
				ButtonGameObject.SetActive(false);
			}
		}
	}

	public void ShowRewardAD()
	{
		// ButtonGameObject.SetActive(false);
#if UNITY_EDITOR
		AddBonus(); return;
#endif
        if (ShowADSController.Instance.TryGetRewardADSVedio())
		{
        	ADSManager.Instance.SetAnalysisData(RewardAdType.LobbyButton);
            ADSManager.Instance.RewardBasedVideoClosed = null;
            ADSManager.Instance.ADFinishedGetBonus = AddBonus;
			ShowADSController.Instance.ShowRewardADSVedio();
			// 测试之后放在广告播放之后
			// UserBasicData.Instance.AddCredits((ulong)data.RewardCredit,true);
		}
	}

	public void AddBonus()
	{
		var data = ADSConfig.Instance.Sheet.dataArray[(int)UserBasicData.Instance.PlayerPayState];
		UserBasicData.Instance.AddCredits((ulong)data.RewardCredit, FreeCreditsSource.WatchBonusAdBonus,  true);
		UserDeviceLocalData.Instance.LastGetGetRewardADSVedioTime = NetworkTimeHelper.Instance.GetNowTime();
		UserDeviceLocalData.Instance.RewardADSVedioPlayTime++;
		ADSManager.Instance.ADFinishedGetBonus -= AddBonus;
		ShowCollectioningCoinsEffect();
	}

	private void ShowCollectioningCoinsEffect()
	{
		_isShowingADSEffect = true;
		ButtonGameObject.SetActive(true);
		CollectioningCoinsEffectAnimator.gameObject.SetActive(true);
		AudioManager.Instance.PlaySound(AudioType.CreditsRollUp);
		ShowCoinsText.ChangeTextAnimationTime(CollectioningCoinsEffectAnimator.GetCurrentAnimatorStateInfo(0).length + 2);
		float time = CollectioningCoinsEffectAnimator.GetCurrentAnimatorStateInfo(0).length;
		_collectEffectCoroutine = UnityTimer.Start(this, time, () => {
			//null check for safe
			if(CollectioningCoinsEffectAnimator != null && CollectioningCoinsEffectAnimator.gameObject != null)
				CollectioningCoinsEffectAnimator.gameObject.SetActive(false);
			if(ButtonGameObject != null)
				ButtonGameObject.SetActive(false);
			_isShowingADSEffect = false;

			_collectEffectCoroutine = null;
		});
	}

	private void OnDestroy()
	{
		if(ADSManager.Instance != null)
		{
			ADSManager.Instance.ADFinishedGetBonus -= AddBonus;
		}

		if(_collectEffectCoroutine != null)
		{
			this.StopCoroutine(_collectEffectCoroutine);
			_collectEffectCoroutine = null;
		}
	}
}
