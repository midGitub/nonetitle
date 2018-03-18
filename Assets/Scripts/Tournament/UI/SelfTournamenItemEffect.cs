using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfTournamenItemEffect : MonoBehaviour
{
	public Animator UpEffect;
	public Animator DownEffect;

	private Coroutine UpEffectCor;
	private Coroutine DownEffectCor;

	public void ShowUpEffect()
	{
		if(UpEffectCor != null)
		{
			StopCoroutine(UpEffectCor);
		}
		UpEffectCor = NetworkTimeHelper.Instance.StartCoroutine(UpEffectIE());
	}

	public void ShowDownEffect()
	{
		if(DownEffectCor != null)
		{
			StopCoroutine(DownEffectCor);
		}
		DownEffectCor = NetworkTimeHelper.Instance.StartCoroutine(DownEffectIE());
	}

	private IEnumerator UpEffectIE()
	{
		UpEffect.gameObject.SetActive(true);
		yield return new WaitForSeconds(UpEffect.GetCurrentAnimatorStateInfo(0).length);
		UpEffect.gameObject.SetActive(false);
	}

	private IEnumerator DownEffectIE()
	{
		DownEffect.gameObject.SetActive(true);
		yield return new WaitForSeconds(DownEffect.GetCurrentAnimatorStateInfo(0).length);
		DownEffect.gameObject.SetActive(false);
	}
}
