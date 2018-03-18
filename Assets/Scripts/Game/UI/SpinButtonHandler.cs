using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SpinButtonState
{
	Normal,
	Auto
}

public class SpinButtonHandler : MonoBehaviour
{
	private static readonly string _spriteDir = "Images/Machines/";

	private SpinButtonState _state;
	private Button _button;

	public SpinButtonState State { get { return _state; } set { _state = value; } }

	// Use this for initialization
	void Start () {
		_button = this.gameObject.GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetState(SpinButtonState s)
	{
		_state = s;

		string normalName = "";
		string pressedName = "";
		if(_state == SpinButtonState.Normal)
		{
			normalName = "spinButton";
			pressedName = "spinButtonDown";
		}
		else if(_state == SpinButtonState.Auto)
		{
			normalName = "spinButtonAuto";
			pressedName = "spinButtonAutoDown";
		}

		Sprite normalSprite = Resources.Load<Sprite>(_spriteDir + normalName);
		Image image = this.GetComponent<Image>();
		image.sprite = normalSprite;

		Sprite pressedSprite = Resources.Load<Sprite>(_spriteDir + pressedName);
		SpriteState ss = new SpriteState();
		ss.disabledSprite = normalSprite;
		ss.highlightedSprite = normalSprite;
		ss.pressedSprite = pressedSprite;
		_button.spriteState = ss;
	}
}
