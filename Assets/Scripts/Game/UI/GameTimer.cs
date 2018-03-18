using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System;

public class GameTimer : Singleton<GameTimer> {
	public TimeSpan gameTimeSpan = new TimeSpan (0, 0, 0);
	// Use this for initialization
	void Start () {
		StartCoroutine (TimeFuntion());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private IEnumerator TimeFuntion()
	{
		while (true) 
		{
			yield return  new WaitForSeconds (1.0f);
			gameTimeSpan = gameTimeSpan.Add (new TimeSpan (0, 0, 1));
		}
	}
}
