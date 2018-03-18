using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public static class ScriptEffect
{
	public static IEnumerator FadeImage(Action<float> changeColorAction, float allTime, float startV, float EndV)
	{
		float startTime = Time.time;
		while(Time.time - startTime <= allTime)
		{
			float cr;
			cr = Mathf.Lerp(startV, EndV, (Time.time - startTime) / allTime);
			changeColorAction(cr);
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	}

	/// <summary>
	/// Fades the in and out.
	/// </summary>
	/// <returns>The in and out.</returns>
	/// <param name="go">Go.</param>
	/// <param name="image">Image.</param>
	/// <param name="allTime">All time.</param>
	/// <param name="startV">Start v. 0-1</param>
	/// <param name="EndV">End v.0-1</param>
	/// <param name="callback">Callback.</param>
	public static IEnumerator FadeInAndOut(MonoBehaviour go, Action<float> changeColorAAction, float fadeTime, float stayTime, float startV, float EndV,
										   UnityAction callback)
	{
		yield return go.StartCoroutine(FadeImage((co) => { changeColorAAction(co);}, fadeTime / 2, startV, EndV));
		yield return new WaitForSeconds(stayTime);
		yield return go.StartCoroutine(FadeImage(changeColorAAction, fadeTime / 2, EndV, startV));
		if(callback != null)
		{
			callback();
		}
		yield return null;
	}
}
