using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiggyBankCollectionEffect : MonoBehaviour
{
	public Animator EffectAnimator;
	public Animator PigAnimator;
	private void OnEnable()
	{
		PiggyBankSystem.Instance.PiggyBankAddCoinsAction += ShowEffect;
	}

	private void OnDisable()
	{
		if(PiggyBankSystem.Instance != null)
		{
			PiggyBankSystem.Instance.PiggyBankAddCoinsAction -= ShowEffect;
		}
	}

	private void ShowEffect()
	{
		if(EffectAnimator.gameObject.activeSelf == true)
		{
			return;
		}
		PigAnimator.SetTrigger("play");
		EffectAnimator.gameObject.SetActive(true);

		CitrusFramework.UnityTimer.Instance.StartTimer
					   (this, EffectAnimator.GetCurrentAnimatorStateInfo(0).length,
						() => { EffectAnimator.gameObject.SetActive(false); });
	}
}
