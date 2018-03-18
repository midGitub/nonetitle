using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using CitrusFramework;

public class SpriteStoreManager : Singleton<SpriteStoreManager>
{
	private static string _saveDir = "Sprite";

	private Dictionary<string, Sprite> _spriteDict = new Dictionary<string, Sprite>();

	// Use this for initialization
	void Start()
	{
		CitrusEventManager.instance.AddListener<EnterGameSceneEvent>(EnterGameSceneDelegate);
		CitrusEventManager.instance.AddListener<EnterMainMapSceneEvent>(EnterMainMapSceneDelegate);
	}

	void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<EnterGameSceneEvent>(EnterGameSceneDelegate);
		CitrusEventManager.instance.RemoveListener<EnterMainMapSceneEvent>(EnterMainMapSceneDelegate);
	}

	#region Public

	public Sprite GetSprite(string key)
	{
		Sprite result = null;
		if(key == "")
		{
		}
		else if(_spriteDict.ContainsKey(key))
		{
			result = _spriteDict[key];
		}
		else
		{
			result = LoadSprite(key);
			if(result != null)
			{
				_spriteDict[key] = result;
			}
		}
		return result;
	}

	public void DownloadSprite(string key, string url, Callback callback = null)
	{
		if(key == "" || url == "" || _spriteDict.ContainsKey(key))
		{
			return;
		}

		//Debug.Log(url + "MD5:");
		StartCoroutine(NetWorkHelper.GetDownloadingPicture(this, url, (obj) =>
			{
				SetSprite(key, obj);
				if(callback != null)
				{
					callback();
				}
			}));
	}

	public void Purge()
	{
		Debug.Log("Purge called");
		_spriteDict.Clear();
	}

	#endregion

	#region Private

	void SaveSprite(string key, Sprite sprite)
	{
		string dir = GetDir();
		if(!Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		string path = GetPath(key);
		TextureUtility.SaveTexture(path, sprite.texture);
	}

	Sprite LoadSprite(string key)
	{
		string path = GetPath(key);
		Texture2D tex = TextureUtility.LoadTexture(path);
		Sprite sprite = null;
		if(tex != null)
			sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
		return sprite;
	}

	string GetDir()
	{
		return Path.Combine(Application.persistentDataPath, _saveDir);
	}

	string GetPath(string key)
	{
		string dir = GetDir();
		string path = Path.Combine(dir, key);
		path += ".png";
		return path;
	}

	void EnterGameSceneDelegate(CitrusGameEvent e)
	{
		Purge();
	}

	void EnterMainMapSceneDelegate(CitrusGameEvent e)
	{
		Purge();
	}

	void SetSprite(string key, Sprite sprite)
	{
		_spriteDict[key] = sprite;
		SaveSprite(key, sprite);
	}

	#endregion
}

