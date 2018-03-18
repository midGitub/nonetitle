using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class PuzzleTapBoxPlayBoard : MonoBehaviour
{
	public Text _betText;
	public Text _ratioText;
	public GameObject _ratioEffect;

	public void Init(ulong betAmount)
	{
		_betText.text = betAmount.ToString();

		_ratioText.text = "0";
		_ratioEffect.SetActive(false);
	}

	public void SetRatio(float ratio)
	{
		if(_ratioText.text != ratio.ToString())
		{
			UnityTimer.Start(this, 1.5f, () => {
				_ratioEffect.SetActive(true);
				_ratioEffect.GetComponent<ParticleSystem>().Play();
				UnityTimer.Start(this, 1.5f, () => {
					_ratioEffect.SetActive(false);
				});
			});

			UnityTimer.Start(this, 2.0f, () => {
				_ratioText.text = ratio.ToString();
			});
		}
	}
}
