using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class GetCredits : Singleton<GetCredits> 
{
	private string GainCreditsResPath = "GainCredits/GainCredits";
	private float _effectContinueTime = 2.2f;
	private GameObject _effectObj;

    public float EffectContinueTime {
        get { return _effectContinueTime; }
    }

    public void Init(){}

	public void PlayGetCreditEffect()
	{
		if(_effectObj == null)
		{
			_effectObj = Instantiate(AssetManager.Instance.LoadAsset<GameObject>(GainCreditsResPath)) as GameObject;
		}

		SetChildActive(true);

		AudioManager.Instance.PlaySound(AudioType.HourlyBonusCreditsRollUp);
		ShowCoinsText.ChangeTextAnimationTime(_effectContinueTime);
		UnityTimer.Instance.StartTimer(this, _effectContinueTime, () => {SetChildActive(false);});
	}

	private void SetChildActive(bool active)
	{
		if(_effectObj == null)
		{
			return;
		}

		for(int i = 0; i < _effectObj.transform.childCount; i++)
		{
			_effectObj.transform.GetChild(i).gameObject.SetActive(active);
		}
	}

	void OnDestory()
	{
		_effectObj = null;
	}
}
