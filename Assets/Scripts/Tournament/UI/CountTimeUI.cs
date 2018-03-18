using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class CountTimeUI : MonoBehaviour
{
	public Text M1;
	public Text M2;
	public Text S1;
	public Text S2;

	public void SerValue(int lastscond)
	{
		TimeSpan ts = new TimeSpan(0, 0, lastscond);
		//float sc = lastscond % 60;
		//float mi = (lastscond - sc) / 60;

        FillValue(ts);
	}

	public void SetTimerColor(Color color)
	{
		M1.color = color;
		M2.color = color;
		S1.color = color;
		S2.color = color;
	}

    public void SetValue(TimeSpan lastSpan)
    {
        FillValue(lastSpan);
    }

    private void FillValue(TimeSpan ts)
    {
        float sc = ts.Seconds;
        float mi = ts.Minutes;

        var allString = mi.ToString("00") + sc.ToString("00");
        M1.text = allString[0].ToString();
        M2.text = allString[1].ToString();
        S1.text = allString[2].ToString();
        S2.text = allString[3].ToString(); 
    }
		
	public IEnumerator TimerFlickerEffect(MonoBehaviour mono, int animPlayTimes, float duringTime, float singleLoopTime, Vector3 targetScale, Vector3 smallScale)
	{
		var list = new List<Text>{ M1, M2, S1, S2 };
		while(animPlayTimes > 0)
		{
			for(int i = 0; i < list.Count; i++)
			{
                mono.StartCoroutine(PingPongAnim(list[i], duringTime, singleLoopTime, targetScale, smallScale));
			}
			animPlayTimes -= 1;
			yield return new WaitForSeconds(1);
		}

	}

	private IEnumerator PingPongAnim(Text text, float duringTime, float singleLoopTime, Vector3 largeScale ,Vector3 smallScale)
	{
		var origScale = transform.localScale;
		bool reversed = false;

		while (duringTime > 0) 
		{
			if (!reversed)
			{
				duringTime -= singleLoopTime;
				reversed = !reversed;
				text.gameObject.transform.DOScale(largeScale, singleLoopTime);
				text.DOFade(1, singleLoopTime);
				yield return new WaitForSeconds (singleLoopTime);
			} 
			else 
			{
				duringTime -= singleLoopTime;
				reversed = !reversed;
				text.gameObject.transform.DOScale (smallScale, singleLoopTime);
				text.DOFade(0.5f, singleLoopTime);
				yield return new WaitForSeconds (singleLoopTime);
			}
		}
			
		text.gameObject.transform.localScale = origScale;
		text.DOFade(1, Time.deltaTime);
	}
}
