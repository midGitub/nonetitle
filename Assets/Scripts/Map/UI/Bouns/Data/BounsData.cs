using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ChangeSpriteButton
{
	public Image ButtonImage;
	public Sprite OpenSprite;
	public Sprite CloseSprite;

	public void UpdateButtonState(bool isOpen)
	{
		ButtonImage.sprite = isOpen ? OpenSprite : CloseSprite;
	}
}

[Serializable]
public class ChangeGameObjectButton
{
	public GameObject OpenGameObject;
	public GameObject CloseGameObject;

	public void UpdateButtonState(bool isOpen)
	{
		OpenGameObject.SetActive(isOpen);
		CloseGameObject.SetActive(!isOpen);
	}
}

[Serializable]
public class DayBonusTypeInfor
{
	public ChangeGameObjectButton DayType;

	public int MagnificationNum;
}