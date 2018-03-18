using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using CitrusFramework;




public class NumberTickHandler : MonoBehaviour
{
	public Text _text;
//	public bool _playTickSound = true;

	private ulong _toNum;
	private Tweener _tweener;
	private Coroutine _tickSoundCoroutine;

	// zhousen
	public delegate void tweenOverCallback(NumberTickHandler num);
	public tweenOverCallback _completeHandler;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartTick(ulong fromNum, ulong toNum, float time)
	{
		_toNum = toNum;

		_tweener = DOTween.To(() => fromNum, x => fromNum = x, toNum, time).OnUpdate(() => {
			_text.text = StringUtility.FormatNumberString(fromNum, true, false);
		}).OnComplete(() => {
			if (_completeHandler != null) _completeHandler(this);
			_tweener = null;
		});

//		bool isPlayingSound = AudioManager.Instance.IsPlayingSound(AudioType.CreditsTick);
//		bool shouldPlaySound = _playTickSound && !isPlayingSound;
//		if(shouldPlaySound)
//		{
//			AudioManager.Instance.PlaySound(AudioType.CreditsTick, true);
//			_tickSoundCoroutine = UnityTimer.Start(this, time, () => {
//				AudioManager.Instance.StopSound(AudioType.CreditsTick);
//				AudioManager.Instance.PlaySound(AudioType.CreditsTickEnd);
//			});
//		}
	}

	public void StopTick()
	{
		if(_tweener != null)
		{
			_tweener.Kill(true);
			_text.text = StringUtility.FormatNumberString(_toNum, true, false);
		}

		//don't stop sound. let it fly for a while
//		if(_tickSoundCoroutine != null)
//		{
//			this.StopCoroutine(_tickSoundCoroutine);
//			_tickSoundCoroutine = null;
//
//			AudioManager.Instance.StopSound(AudioType.CreditsTick);
//			AudioManager.Instance.PlaySound(AudioType.CreditsTickEnd);
//		}
	}

}
