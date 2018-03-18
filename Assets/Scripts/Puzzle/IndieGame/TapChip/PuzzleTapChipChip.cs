//#define ADD_TAP_SCALE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using DG.Tweening;

internal enum TapChipChipState
{
	Back,
	Front
}

public class PuzzleTapChipChip : MonoBehaviour
{
//	static readonly string _backImage = "Images/Machines/M20/TapChip/TapChip_Chip_Back";
//	static readonly string _frontImagePrefix = "Images/Machines/M20/TapChip/TapChip_Chip_Front_";

	static readonly Dictionary<float, Vector2> _texureOffsetDict = new Dictionary<float, Vector2>() {
		{3.0f, new Vector2(0.0f, 0.5f)},
		{5.0f, new Vector2(0.25f, 0.5f)},
		{10.0f, new Vector2(0.5f, 0.5f)},
		{15.0f, new Vector2(0.75f, 0.5f)},
		{20.0f, new Vector2(0.0f, 0.0f)},
		{50.0f, new Vector2(0.25f, 0.0f)},
		{100.0f, new Vector2(0.5f, 0.0f)},
		{0.0f, new Vector2(0.75f, 0.0f)}
	};

	//public Image _image;
	public Animator _animator;
	public GameObject _frontObject;
	public GameObject _flashEffect;

	PuzzleTapChipController _controller;
	Button _button;
	Material _frontMaterial;

	TapChipChipState _state;

	public bool IsTapped { get { return _state == TapChipChipState.Front; } }

	public void Init(PuzzleTapChipController c)
	{
		_controller = c;

		_button = gameObject.GetComponent<Button>();
		EventTriggerListener.Get(_button.gameObject).onDown += ButtonDown;
		EventTriggerListener.Get(_button.gameObject).onClick += ButtonClicked;
		EventTriggerListener.Get(_button.gameObject).onUp += ButtonUp;

		gameObject.transform.localScale = Vector3.one;

		_frontMaterial = _frontObject.GetComponent<MeshRenderer>().material;
		Debug.Assert(_frontMaterial != null);

		Reset();
	}

	void OnDestroy()
	{
		EventTriggerListener.Get(_button.gameObject).onDown -= ButtonDown;
		EventTriggerListener.Get(_button.gameObject).onClick -= ButtonClicked;
		EventTriggerListener.Get(_button.gameObject).onUp -= ButtonUp;
	}

	public void Reset()
	{
		SetState(TapChipChipState.Back);

		_flashEffect.SetActive(false);
	}

	bool CanClick()
	{
		bool result = _state == TapChipChipState.Back && !_controller.IsAnyChipPlayed && !_controller.IsEnded;
		return result;
	}

	void ButtonDown(GameObject obj)
	{
		#if ADD_TAP_SCALE
		if(CanClick())
		{
			gameObject.transform.DOScale(1.2f, 0.1f);
		}
		#endif
	}

	void ButtonUp(GameObject obj)
	{
		#if ADD_TAP_SCALE
		if(CanClick())
		{
			gameObject.transform.DOScale(1.0f, 0.1f);
		}
		#endif
	}

	void ButtonClicked(GameObject obj)
	{
		if(CanClick())
		{
			_controller.IsAnyChipPlayed = true;

			#if ADD_TAP_SCALE
			gameObject.transform.DOScale(1.0f, 0.1f).OnComplete(HandleWinRatio);
			#else
			HandleWinRatio();
			#endif
		}
	}

	void HandleWinRatio()
	{
		float ratio = _controller.AddWinRatio(this);
		SetState(TapChipChipState.Front, ratio);
		_controller.CheckEnd(ratio);

		UnityTimer.Start(this, 0.4f, () => {
			_controller.IsAnyChipPlayed = false;
		});

		if(ratio > 0.0f)
			AudioManager.Instance.PlaySound(AudioType.M20_TapChipSuccess);
		else
			AudioManager.Instance.PlaySound(AudioType.M20_TapChipFail);
	}

	void SetState(TapChipChipState state, float ratio = 0.0f)
	{
		_state = state;
		if(_state == TapChipChipState.Back)
		{
			_button.interactable = true;
			_animator.Play("Idle");

//			_image.sprite = AssetManager.Instance.LoadMachineAsset<Sprite>(_backImage, _controller.Machine.MachineName);
//			Debug.Assert(_image.sprite != null);
		}
		else if(_state == TapChipChipState.Front)
		{
			_button.interactable = false;

			if(ratio == 0.0f)
				_animator.Play("Fail");
			else
				_animator.Play("Success");

			Debug.Assert(_texureOffsetDict.ContainsKey(ratio));
			Vector2 offset = _texureOffsetDict[ratio];
			_frontMaterial.SetTextureOffset("_MainTex", offset);
			
//			int intRatio = (int)ratio;
//			string name = _frontImagePrefix + intRatio.ToString();
//			_image.sprite = AssetManager.Instance.LoadMachineAsset<Sprite>(name, _controller.Machine.MachineName);
//			Debug.Assert(_image.sprite != null);
		}
		else
		{
			Debug.Assert(false);
		}
	}

	public void PlayEndEffect()
	{
		_flashEffect.SetActive(true);

		UnityTimer.Start(this, 1.5f, () => {
			_flashEffect.SetActive(false);
		});
	}
}
