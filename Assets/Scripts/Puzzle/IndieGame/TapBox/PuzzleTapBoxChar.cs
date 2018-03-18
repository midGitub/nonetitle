using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using DG.Tweening;

internal enum TapBoxCharState
{
	Idle,
	Success,
	Fail
}

public class PuzzleTapBoxChar : MonoBehaviour
{
	public Text _ratioText;
	public Animator _animator;
	public GameObject _candyHitEffect;

	PuzzleTapBoxController _controller;
	Button _button;
	Vector3 _ratioTextInitPos;

	void Start()
	{
		_button = gameObject.GetComponent<Button>();
		_button.onClick.AddListener(ButtonDown);
		_ratioText.gameObject.SetActive(false);
		_candyHitEffect.SetActive(false);
		_ratioTextInitPos = _ratioText.gameObject.transform.localPosition;
	}

	public void Init(PuzzleTapBoxController c)
	{
		_controller = c;
	}

	public void SetEnabled(bool flag)
	{
		_button.interactable = flag;
	}

	void ButtonDown()
	{
		if(_controller.IsAnyCharPlayed || _controller.IsEnded)
			return;
		
		// play fx

		_controller.IsAnyCharPlayed = true;

		UnityTimer.Start(this, 0.1f, HandleWinRatio);
	}

	void HandleWinRatio()
	{
		float ratio = _controller.AddWinRatio();

		if(ratio > 0.0f)
		{
			SetState(TapBoxCharState.Success);

			AudioManager.Instance.PlaySound(AudioType.M40_TapBoxCollectSuccess);

			UnityTimer.Start(this, 0.75f, ShowCandyHitEffect);

			UnityTimer.Start(this, 1.3f, () => {
				_ratioText.text = "X" + ratio.ToString();
				_ratioText.gameObject.SetActive(true);
				_ratioText.GetComponent<Animator>().Play("tapbox_text");
			});

			UnityTimer.Start(this, 1.8f, () => {
				_controller.IsAnyCharPlayed = false;
			});

			UnityTimer.Start(this, 2.6f, () => {
				_ratioText.gameObject.SetActive(false);
			});
		}
		else
		{
			SetState(TapBoxCharState.Fail);

			AudioManager.Instance.PlaySound(AudioType.M40_TapBoxCollectFail);

			_controller.IsAnyCharPlayed = false;
		}
	}

	void SetState(TapBoxCharState s)
	{
		_animator.SetTrigger(s.ToString());
	}

	void ShowCandyHitEffect()
	{
		_candyHitEffect.SetActive(false);
		_candyHitEffect.SetActive(true);
	}
}
