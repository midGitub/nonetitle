using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CitrusFramework;
using CodeStage.AntiCheat.ObscuredTypes;

public class HourChest : MonoBehaviour
{
	public GameObject _tipText;
	public Button _chest;
	public CollecEffect[] _collecEffect;
	[SerializeField]
	private Text _timeText;

	[SerializeField]
	private GameObject _canCollectionCoinsEffect;

	[SerializeField]
	private Button _chestButton;

	[SerializeField]
	private Animator _collectioningCoinsEffectAnimator;

	[System.Serializable]
	public class CollecEffect{
		public GameObject _collecEffect;
		public int _coinsNum;
	};
		

	private ObscuredULong _bonusCoinsNum;
	private DateTime _lastGetChestDate;
	private int   _bonusSecond;
	private bool _hasInit = false;
	private static TimeSpan _lastGetChestSpan= new TimeSpan(0,0,-1);
	public void Start()
	{
		_chestButton.onClick.AddListener (TryGetBonus);
		_chest.onClick.AddListener (TryGetBonus);
		_hasInit = InitState ();
	}

	public void Update()
	{
		if(!_hasInit) 
			_hasInit = InitState ();
	}
		
	private bool InitState()
	{
		_lastGetChestDate = UserBasicData.Instance.LastGetChestDateTime;
		_bonusSecond = ChestConfig.Instance.GetCDTime (UserBasicData.Instance.TotalPayAmount);
		#if DEBUG
		_bonusSecond=10;
		#endif
		//_remainTime = new TimeSpan (0, 0, _bonusSecond);
		if(_lastGetChestSpan.TotalSeconds==-1)
			_lastGetChestSpan = GameTimer.Instance.gameTimeSpan;
		if (Application.internetReachability != NetworkReachability.NotReachable) 
		{
			SetCanCollectChest (false);
			ShowRemainTime ();
			StartCoroutine (TimeFuntion ());
			return true;
		}
		else
			return false;
	}
		
	private void OnDestroy()
	{
		_chestButton.onClick.RemoveListener (TryGetBonus);
		_chest.onClick.RemoveListener (TryGetBonus);
	}
		
	private void TryGetBonus()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
			return;
		AudioManager.Instance.PlaySound(AudioType.Click);
		ShowCollectioningCoinsEffect();
		int[] Reward = ChestConfig.Instance.GetReward(UserBasicData.Instance.TotalPayAmount);
		int[] Weight = ChestConfig.Instance.GetWeight (UserBasicData.Instance.TotalPayAmount);
		_bonusCoinsNum = (ulong)GetRandonReward(Reward,Weight);
		int lucky = ChestConfig.Instance.GetLTLucky(UserBasicData.Instance.TotalPayAmount);
		ProcessHourBonusData(_lastGetChestDate, _bonusCoinsNum,lucky);
		AnalysisManager.Instance.Get5MinuteBonus ((ulong)_bonusCoinsNum,lucky);
		ShowCollecEffect (_bonusCoinsNum);
		SetCanCollectChest (false);
		_lastGetChestSpan = GameTimer.Instance.gameTimeSpan;
		StartCoroutine (TimeFuntion());
	}

	private void ShowCollecEffect(ulong coin)
	{
		for (int i = 0; i < _collecEffect.Length; i++) 
		{
			if ((coin == (ulong)_collecEffect [i]._coinsNum)&&(_collecEffect [i]._collecEffect.GetComponent <ParticleSystem> ())) 
				StartCoroutine (PlayCollecEffect (_collecEffect [i]._collecEffect));
		}
	}

	private IEnumerator PlayCollecEffect(GameObject effect)
	{
		effect.SetActive (true);
		float second = effect.GetComponent <ParticleSystem> ().main.duration;
		yield return new WaitForSeconds (second);
		effect.SetActive (false);
	}

	private int GetRandonReward(int[] Reward,int[] Weight)
	{
		int weightSum = 0;
		for (int i = 0; i < Weight.Length; i++)	
			weightSum += Weight [i];
		for (int i = 0,index = UnityEngine.Random.Range (1, weightSum); i < Weight.Length;i++)
		{
			index -= Weight [i];
			if (index <= 0)
				return Reward [i];
		}
		return 0;
	}
			
	private IEnumerator TimeFuntion()
	{
		WaitForSeconds delay = new WaitForSeconds (1.0f);
		while ((GameTimer.Instance.gameTimeSpan - _lastGetChestSpan).TotalSeconds < _bonusSecond) 
		{
			ShowRemainTime ();
			yield return delay;
		}
		SetCanCollectChest ();
		yield break;
	}

	public void ShowCollectioningCoinsEffect()
	{
		AudioManager.Instance.PlaySound(AudioType.HourlyBonusCreditsRollUp);
		ShowCoinsText.ChangeTextAnimationTime(_collectioningCoinsEffectAnimator.GetCurrentAnimatorStateInfo(0).length + 2);
		_collectioningCoinsEffectAnimator.gameObject.SetActive(true);
		CitrusFramework.UnityTimer.Instance.StartTimer(this, 
			_collectioningCoinsEffectAnimator.GetCurrentAnimatorStateInfo(0).length,
			() => {
				if(_collectioningCoinsEffectAnimator!=null&&_collectioningCoinsEffectAnimator.gameObject!=null)
					_collectioningCoinsEffectAnimator.gameObject.SetActive(false);
			});
	}
	public  void ProcessHourBonusData(DateTime lastGetChestDate, ulong coins,int lucky)
	{
		UserBasicData.Instance.SetLastGetChestDateTime(lastGetChestDate);
		UserBasicData.Instance.AddCredits(coins, FreeCreditsSource.HourlyBonus, false);
		UserBasicData.Instance.AddLongLucky(lucky, false);
		UserBasicData.Instance.Save();
	}
		
	private void SetCanCollectChest(bool flag=true)
	{
		_timeText.enabled = !flag;
		_chestButton.interactable = flag;
		_chest.interactable = flag;
		_canCollectionCoinsEffect.SetActive (flag);
		_tipText.SetActive (flag);
	}

	private void ShowRemainTime()
	{
		TimeSpan remainTime=new TimeSpan(0,0,_bonusSecond)-(GameTimer.Instance.gameTimeSpan-_lastGetChestSpan);
		if (remainTime.TotalSeconds >= 0) 
		{
			if(!_timeText.enabled)
				_timeText.enabled = true;
			if (remainTime.Hours > 0) {
				_timeText.text = remainTime.Hours.ToString ("00") + ":" + remainTime.Minutes.ToString ("00") + ":" + remainTime.Seconds.ToString ("00");
			} else {
				_timeText.text = remainTime.Minutes.ToString ("00") + ":" + remainTime.Seconds.ToString ("00");
			}
		}
	}
}
