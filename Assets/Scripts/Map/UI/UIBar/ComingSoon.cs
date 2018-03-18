using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.UI;

public class ComingSoon : Singleton<ComingSoon>
{
	public Image ComingImage;
	public GameObject ComingCanvas;

	public Image NoNewsImage;
	public GameObject NoNewCanvas;

	public float ShowTime;
	public float StayTime;

	public void ShowComingSoon()
	{
		ComingCanvas.SetActive(true);
		StartCoroutine(ScriptEffect.FadeInAndOut(this, (co) => { Color ca = (ComingImage.color);ca.a = co;ComingImage.color = ca;
		}, ShowTime,StayTime, 0f, 1,
												 () => { ComingCanvas.SetActive(false); }));
	}

	public void ShowNoNews()
	{
		NoNewCanvas.SetActive(true);
		StartCoroutine(ScriptEffect.FadeInAndOut(this, (co) =>
		{
			Color ca = (NoNewsImage.color); ca.a = co; NoNewsImage.color = ca;
		}, ShowTime, StayTime, 0f, 1,
												 () => { NoNewCanvas.SetActive(false); }));
	}
}
