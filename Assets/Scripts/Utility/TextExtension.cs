using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public static class TextExtension
{
	public static Tweener TickNumber(this Text text, ulong fromNum, ulong toNum, float time)
	{
		Tweener t = DOTween.To(() => fromNum, x => fromNum = x, toNum, time).OnUpdate(() =>
		{
			text.text = StringUtility.FormatNumberString(fromNum, true, false);
		});

		return t;
	}

	public static IEnumerator IETickNumber(ulong fromNum, ulong toNum, float time, Action<ulong> getTextAction)
	{
		float startTime = Time.time;
		float text;
		while (Time.time - startTime <= time)
		{
			text = Mathf.Lerp(fromNum, toNum, (Time.time - startTime) / time);
			getTextAction((ulong)text);
			yield return new WaitForEndOfFrame();
		}

		getTextAction((ulong)toNum);

		yield return null;
	}
}
