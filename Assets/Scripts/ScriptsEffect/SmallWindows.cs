using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmallWindows : MonoBehaviour
{
	public Image SmallWindow;

	public void Start()
	{
		StartCoroutine(Show());
	}

	private IEnumerator Show()
	{
		while(true)
		{
			yield return StartCoroutine(ScreenShotMakeTexture.CreatScreenShotTexture((obj) =>
			{

				Sprite sp = Sprite.Create(obj, new Rect(0, 0, Screen.width, Screen.height), Vector2.one * 0.5f);
				SmallWindow.sprite = sp;
			}));
			yield return new WaitForEndOfFrame();
		}

	}

}