using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaitTimeCallFunction : MonoBehaviour 
{
	public float Time;
	public UnityEvent WaitTimeCall = new UnityEvent();
	private void OnEnable()
	{
		WaitTime(Time);
	}

	private void WaitTime(float time)
	{
		StartCoroutine(WaitTimeCallIE(time));
	}

	private IEnumerator WaitTimeCallIE(float time)
	{
		yield return new WaitForSeconds(time);
		WaitTimeCall.Invoke();
	}
}
