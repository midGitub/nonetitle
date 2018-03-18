using UnityEngine;
using System.Collections;
using CitrusFramework;

public class LoadSceneFinishedEvent : CitrusGameEvent
{
	private string _sceneName;
	public string SceneName { get { return _sceneName; } }

	public LoadSceneFinishedEvent(string sceneName)
	{
		_sceneName = sceneName;
	}
}

