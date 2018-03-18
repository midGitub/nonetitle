using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class PuzzleTapBoxResultBoard : MonoBehaviour
{
	static string TextHintAnimation = "FX_ResultBoardRoot_Text";

	public Text _betText;
	public Text _ratioText;
	public Text _resultText;
	public GameObject _ratioEffect;

	public void Init()
	{
		gameObject.SetActive(false);
		_ratioEffect.SetActive(false);

		_betText.gameObject.SetActive(false);
		_ratioText.gameObject.SetActive(false);
		_resultText.gameObject.SetActive(false);
	}

	public IEnumerator ShowCoroutine(ulong bet, float ratio, ulong result)
	{
		gameObject.SetActive(true);

		_betText.text = bet.ToString();
		_ratioText.text = ratio.ToString();

		yield return new WaitForSeconds(0.5f);

		_betText.gameObject.SetActive(true);
		_betText.GetComponent<Animator>().Play(TextHintAnimation);
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlaySound(AudioType.DailyBonus);
		yield return new WaitForSeconds(0.8f);

		_ratioText.gameObject.SetActive(true);
		_ratioText.GetComponent<Animator>().Play(TextHintAnimation);
		_ratioEffect.SetActive(true);
		_ratioEffect.GetComponent<ParticleSystem>().Play(true);
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlaySound(AudioType.DailyBonus);
		yield return new WaitForSeconds(0.8f);

		AudioManager.Instance.PlaySound(AudioType.CreditsRollUp);
		_resultText.gameObject.SetActive(true);
		_resultText.gameObject.GetComponent<NumberTickHandler>().StartTick(0, result, 1.8f);

		yield return new WaitForSeconds(1.9f);

		_resultText.GetComponent<Animator>().Play(TextHintAnimation);
		yield return new WaitForSeconds(0.8f);

		_ratioEffect.SetActive(false);
	}
}
