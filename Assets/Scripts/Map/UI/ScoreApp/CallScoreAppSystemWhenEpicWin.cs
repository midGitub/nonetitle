using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallScoreAppSystemWhenEpicWin : MonoBehaviour {

	public void CallScoreSystem()
	{
		ScoreAppSystem.Instance.EpicWinEvent();
	}

}
