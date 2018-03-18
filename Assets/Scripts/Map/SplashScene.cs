using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScene : MonoBehaviour
{
	// Use this for initialization
	void Start () {
		StartCoroutine(StartNextScene());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator StartNextScene()
	{
		yield return new WaitForEndOfFrame();
		string sceneName = "StartLoading";
		Debug.Log("SplashScene: loading " + sceneName);
		SceneManager.LoadScene(sceneName);
	}
}
