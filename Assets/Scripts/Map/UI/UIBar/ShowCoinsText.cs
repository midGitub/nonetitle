using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowCoinsText : MonoBehaviour
{
	[SerializeField]
	private Text _creditsText;
	public float TextAnimationTime = 1;

	private static float _currTextAnimationTime;

	private ulong _lastCoins;

	public PuzzleMachine _PuzzleMachine;

	public static void ChangeTextAnimationTime(float newTime)
	{
		_currTextAnimationTime = newTime;
	}

	private void Start()
	{
		_currTextAnimationTime = TextAnimationTime;
		_lastCoins = UserBasicData.Instance.Credits;
		_creditsText.text = StringUtility.FormatNumberString(_lastCoins, true, true);
		StartCoroutine(UpdateCoinsText());
	}


	private void GetAnimationTime(ulong prevAmount, ulong winAmount)
	{
		_currTextAnimationTime = _PuzzleMachine._puzzleConfig.GetNumberTickTime(_PuzzleMachine);
	}

	private IEnumerator UpdateCoinsText()
	{
		yield return new WaitForEndOfFrame();
		if(SceneManager.GetActiveScene().name == "Game" && _PuzzleMachine != null && _PuzzleMachine.GameData != null)
		{
			_PuzzleMachine.GameData.WinAmountChangeEventHandler += GetAnimationTime;
		}

		while (true)
		{
			ulong newNum = UserBasicData.Instance.Credits;

			if (newNum != _lastCoins)
			{
				if (newNum > _lastCoins)
				{
					yield return StartCoroutine(TextExtension.IETickNumber(_lastCoins, newNum, _currTextAnimationTime, (num) =>
				{
					_creditsText.text = StringUtility.FormatNumberString(num, true, true);

				}));
				}

				_lastCoins = newNum;
				_creditsText.text = StringUtility.FormatNumberString(_lastCoins, true, true);
				_currTextAnimationTime = TextAnimationTime;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private void OnDisable()
	{
		if (_PuzzleMachine != null && _PuzzleMachine.GameData != null)
		{
			_PuzzleMachine.GameData.WinAmountChangeEventHandler -= GetAnimationTime;
		}
	}
}
