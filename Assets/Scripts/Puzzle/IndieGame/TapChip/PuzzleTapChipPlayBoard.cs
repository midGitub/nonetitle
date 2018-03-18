using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using DG.Tweening;

public class PuzzleTapChipPlayBoard : MonoBehaviour
{
	static string TextHintAnimation = "FX_ResultBoardRoot_Text";

	public Text _betText;
	public Text _ratioText;
	public GameObject _ratioEffect;
	public GameObject _flyEffect;

	public void Init(ulong betAmount)
	{
		_betText.text = betAmount.ToString();

		_ratioText.text = "0";
		_ratioEffect.SetActive(false);
		_flyEffect.SetActive(false);
		_flyEffect.transform.localPosition = Vector3.zero;
	}

	public void SetRatio(float ratio, PuzzleTapChipChip chip)
	{
		if(_ratioText.text != ratio.ToString())
		{
			Vector3 worldPos = chip.gameObject.transform.TransformPoint(Vector3.zero);
			Vector3 localPos = _ratioText.transform.InverseTransformPoint(worldPos);
			_flyEffect.SetActive(true);
			_flyEffect.transform.localPosition = localPos;

			_flyEffect.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutSine).OnComplete(() => {
				_flyEffect.SetActive(false);
				ShowRatio(ratio);
			});
		}
	}

	void ShowRatio(float ratio)
	{
		_ratioText.text = ratio.ToString();

		if(!_ratioEffect.activeSelf)
		{
			_ratioEffect.SetActive(true);
			_ratioEffect.GetComponent<ParticleSystem>().Play();
			UnityTimer.Start(this, 1.5f, () => {
				_ratioEffect.SetActive(false);
			});
		}
	}

//	public IEnumerator ShowResultCoroutine()
//	{
//		_betText.GetComponent<Animator>().Play(TextHintAnimation);
//		yield return new WaitForSeconds(0.2f);
//		AudioManager.Instance.PlaySound(AudioType.DailyBonus);
//		yield return new WaitForSeconds(0.8f);
//
//		_ratioText.GetComponent<Animator>().Play(TextHintAnimation);
//		yield return new WaitForSeconds(0.2f);
//		AudioManager.Instance.PlaySound(AudioType.DailyBonus);
//		yield return new WaitForSeconds(0.8f);
//	}
}
