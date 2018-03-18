using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System.Text.RegularExpressions;
using UnityEngine.Audio;

[System.Serializable]
internal class AudioPlayInfo
{
	public AudioType _type;
	public AudioSource _source;
	public Coroutine _callback;

	public AudioPlayInfo(AudioType type, AudioSource source, Coroutine callback)
	{
		_type = type;
		_source = source;
		_callback = callback;
	}
}

public class AudioManager : MonoBehaviour
{
	private static AudioManager _instance;
	public static AudioManager Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = FindObjectOfType<AudioManager>();
			}

			return _instance;
		}
	}

	public static string BackgroundMusicVolumeId = "BackgroundVolume";
	private static readonly string _musicPath = "Audio/Music/";
	private static readonly string _soundPath = "Audio/Sound/";
	public float _MaxMusicValue = 4;
	private static Dictionary<string, List<AudioType>> _prefixDict = new Dictionary<string, List<AudioType>>(); //for random play

	public AudioMixer MasterMixer;

	private int currBgMusic;
	private AudioSource[] backgroundMusic;
	public GameObject _object;
	private Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();
	private List<AudioPlayInfo> _playingSoundInfoList = new List<AudioPlayInfo>();
	public AudioType _playingMusic = AudioType.None;
	// 背景音乐切换用协程
	private Coroutine _bgmCoroutine = null;

	public bool IsMusicOn
	{
		get
		{
			return UserDeviceLocalData.Instance.IsMusicOn;
		}
		set
		{
			UserDeviceLocalData.Instance.IsMusicOn = value;
			if(!value) { MuteMusic(); }
			else { UnMuteMusic(); }

		}
	}

	public bool IsSoundOn
	{
		get
		{
			//todo
			return UserDeviceLocalData.Instance.IsSoundOn;
		}
		set
		{
			//todo
			UserDeviceLocalData.Instance.IsSoundOn = value;
			if(!value) { StopAllSound(); }

		}
	}

	static AudioManager()
	{
		//setup _prefixDict
		for(int i = 0; i < (int)AudioType.Count; i++)
		{
			AudioType type = (AudioType)i;
			string typeStr = type.ToString();
			Match match = Regex.Match(typeStr, @"\d+");
			if(match.Success)
			{
				string prefix = typeStr.Substring(0, match.Index);
				if(!_prefixDict.ContainsKey(prefix))
					_prefixDict[prefix] = new List<AudioType>();
				_prefixDict[prefix].Add(type);
			}
		}

		//		#if DEBUG
		//		Debug.Log("Start AudioManager prefixDict");
		//		foreach(var item in _prefixDict)
		//		{
		//			Debug.Log("key: " + item.Key);
		//			foreach(AudioType single in item.Value)
		//			{
		//				Debug.Log(single.ToString());
		//			}
		//		}
		//		Debug.Log("End AudioManager prefixDict");
		//		#endif
	}

	public void Awake()
	{
		_instance = this;
		currBgMusic = 0;
		backgroundMusic = GetComponentsInChildren<AudioSource>();
		_object = new GameObject("AudioSource_object");
		_object.transform.SetParent(this.transform);
		PreLoadAllSounds();
	}

	void Start()
	{
		if(IsMusicOn)
		{
			UnMuteMusic();
		}
		else
		{
			MuteMusic();
		}
	}

	#region Public methods

	public void ClearCache()
	{
		_cache.Clear();
	}

	public void ChangeBackgroundMusicSoundValue(float volume)
	{
		backgroundMusic[currBgMusic].volume = volume;
	}

	public void FadeBackGroundMusicVolumeToTarget(float targetVolume, float fadeTime)
	{
		StartCoroutine(CrossAudioVolume(backgroundMusic[currBgMusic], targetVolume, fadeTime));
	}

	private IEnumerator CrossAudioVolume(AudioSource aSource, float targetVolume, float fadeTime)
	{
		float startTime = Time.time;
		float startVolue = aSource.volume;
		while(Time.time < startTime + fadeTime)
		{
			float t = (Time.time - startTime) / fadeTime;
			aSource.volume = Mathf.Lerp(startVolue, targetVolume, t);
			yield return null;
		}

		aSource.volume = targetVolume;
		yield break;
	}

	public void FadeOutBackgroundMusic(float fadetime)
	{
		if(IsMusicOn)
		{
			FadeOut(backgroundMusic[currBgMusic], fadetime);
		}
	}

	public void FadeInBackgroundMusic(float fadeTime)
	{
		if(IsMusicOn)
		{
			FadeIn(backgroundMusic[currBgMusic], fadeTime);
		}
	}

	public void CrossFadeBgMusic(AudioClip newBgMusic, float fadeTime)
	{
		currBgMusic += 1;
		currBgMusic %= 2;

		Debug.Log("AudioManager" + "CrossFadeBgMusic" + "CurrBgMusic: " + currBgMusic.ToString() + " prev: " + ((currBgMusic + 1) % 2).ToString());

		backgroundMusic[currBgMusic].clip = newBgMusic;

		if(IsMusicOn)
		{
			backgroundMusic[currBgMusic].volume = 0;
			backgroundMusic[currBgMusic].Play();

			CrossFade(backgroundMusic[((currBgMusic + 1) % 2)], backgroundMusic[currBgMusic], fadeTime);
		}
		else
		{
			backgroundMusic[currBgMusic].volume = 1;
			backgroundMusic[currBgMusic].Stop();
		}
	}

	internal void CrossFade(AudioSource fadeOut, AudioSource fadeIn, float time)
	{
		StartCoroutine(crossFade(fadeOut, fadeIn, time));
	}

	internal void FadeIn(AudioSource fadeIn, float time)
	{
		StartCoroutine(crossFade(null, fadeIn, time));
	}

	internal void FadeOut(AudioSource fadeOut, float time)
	{
		StartCoroutine(crossFade(fadeOut, null, time));
	}

	private IEnumerator crossFade(AudioSource fadeOut, AudioSource fadeIn, float time)
	{
		float startTime = Time.time;

		if(fadeIn != null)
		{
			if(!fadeIn.isPlaying)
			{
				fadeIn.Play();
			}
		}

		while(Time.time < startTime + time)
		{
			float t = (Time.time - startTime) / time;

			if(fadeOut != null) fadeOut.volume = (1.0f - t);
			if(fadeIn != null) fadeIn.volume = (t);

			yield return null;
		}

		if(fadeIn != null)
			fadeIn.volume = 1;

		if(fadeOut != null)
			fadeOut.volume = 0;

		if(fadeOut != null) fadeOut.Stop();
	}

	//internal bool IsMusicMuted()
	//{
	//	float val = 0;
	//	if(MasterMixer.GetFloat(BackgroundMusicVolumeId, out val))
	//	{
	//		if(val <= -70)
	//			return true;
	//	}
	//	return false;
	//}

	internal void MuteMusic()
	{
		//MasterMixer.SetFloat(BackgroundMusicVolumeId, -100f);
		backgroundMusic[currBgMusic].Stop();
	}

	internal void UnMuteMusic()
	{
		// MasterMixer.SetFloat(BackgroundMusicVolumeId, _MaxMusicValue);
		backgroundMusic[currBgMusic].Play();
	}



	//public void PlayMusic(AudioType type)
	//{
	//	if(_playingMusic != type && type != AudioType.None)
	//	{
	//		string path = GetMusicPath(type.ToString());
	//		AudioClip clip = LoadAudio(path);
	//		if(clip != null)
	//		{
	//			_playingMusic = type;
	//			_source.clip = clip;
	//			_source.loop = true;
	//			if(IsMusicOn)
	//				_source.Play();
	//		}
	//	}
	//	else if(type == AudioType.None)
	//	{
	//		_playingMusic = type;
	//		_source.clip = null;
	//	}
	//}

	public void PlaySoundBGM(AudioType[] audios){
		if (audios.Length == 0)
			return;

		if (audios.Length == 1) {
			LogUtility.Log ("playsound bgm audios.length = 1", Color.yellow);
			PlaySound (audios [0], true);
		} else {
			PlaySound (audios [0]);
			int findIndex = ListUtility.Find(_playingSoundInfoList, (AudioPlayInfo info) =>
			{
				return info._type == audios [0];
			});
			if (findIndex >= 0) {
				float clipLength = _playingSoundInfoList [findIndex]._source.clip.length;
				LogUtility.Log ("bgm1 is " + _playingSoundInfoList [findIndex]._type + " length is " + clipLength, Color.yellow);

				if (_bgmCoroutine != null) {
					StopCoroutine (_bgmCoroutine);
				}
				_bgmCoroutine = UnityTimer.Start(this, clipLength, () =>
				{
					LogUtility.Log ("bgm2 is " + audios[1], Color.yellow);
					PlaySound (audios [1], true);
				});
			}
		}
	}

	public void StopSoundBGM(AudioType[] audios){
		if (audios.Length == 0)
			return;

		for (int i = 0; i < audios.Length; ++i) {
			StopSound (audios [i]);
		}

		if (_bgmCoroutine != null) {
			StopCoroutine (_bgmCoroutine);
			_bgmCoroutine = null;
		}
	}

	public void PlaySound(AudioType type, bool isLoop = false)
	{
		string path = GetSoundPath(type.ToString());
		if(IsSoundOn && !path.IsNullOrEmpty())
		{
			AudioClip clip = LoadAudio(path);
			if(clip != null)
			{
				AudioSource source = _object.AddComponent<AudioSource>();
				source.clip = clip;
				source.loop = isLoop;
				source.Play();

				Coroutine coroutine = null;
				if(!isLoop)
				{
					coroutine = UnityTimer.Start(this, clip.length, () =>
					{
						RemoveSourceBySource(source);
					});
				}

				AudioPlayInfo info = new AudioPlayInfo(type, source, coroutine);
				_playingSoundInfoList.Add(info);
			}
		}
	}

	public AudioType RollSound(string name)
	{
		//Debug.Log(_prefixDict.ContainsKey(name));
		List<AudioType> types = _prefixDict[name];
		int index = Random.Range(0, types.Count);
		return types[index];
	}

	public AudioType RollSound(string machine, int soundMax){
		int index = Random.Range (1, soundMax+1);
		string soundType = machine + "_SpinReel" + index;
		return TypeUtility.GetEnumFromString<AudioType> (soundType);
	}

	public void StopSound(AudioType type)
	{
		int findIndex = ListUtility.Find(_playingSoundInfoList, (AudioPlayInfo info) =>
		{
			return info._type == type;
		});
		if(findIndex >= 0)
		{
			AudioPlayInfo info = _playingSoundInfoList[findIndex];
			if(info._callback != null)
				this.StopCoroutine(info._callback);

			info._source.Stop();
			Destroy(info._source);
			_playingSoundInfoList.RemoveAt(findIndex);
		}
	}

	public bool IsPlayingSound(AudioType type)
	{
		int findIndex = _playingSoundInfoList.FindIndex((AudioPlayInfo obj) =>
		{
			return obj._type == type;
		});
		return findIndex >= 0;
	}

	public void StopAllSound()
	{
		foreach(var item in _playingSoundInfoList)
		{
			//will it be null? I'm not sure
			if(item._callback != null)
				this.StopCoroutine(item._callback);
			Destroy(item._source);
			//_playingSoundInfoList.Remove(item);
		}

		if (_bgmCoroutine != null) {
			StopCoroutine (_bgmCoroutine);
			_bgmCoroutine = null;
		}

		_playingSoundInfoList.Clear();
	}

	#endregion

	#region Private methods

	private void RemoveSourceBySource(AudioSource source)
	{
		int findIndex = ListUtility.Find(_playingSoundInfoList, (AudioPlayInfo info) =>
		{
			return info._source == source;
		});
		if(findIndex >= 0)
		{
			AudioPlayInfo info = _playingSoundInfoList[findIndex];
			Destroy(info._source);
			_playingSoundInfoList.RemoveAt(findIndex);
		}
	}

	private string GetMusicPath(string name)
	{
		return _musicPath + name;
	}

	private string GetSoundPath(string name)
	{
		return _soundPath + name;
	}

	private void PreLoadAllSounds()
	{
		string loadname = "";
		for(int i = 0; i < (int)AudioType.Count; i++)
		{
			AudioType type = (AudioType)i;
			if(type != AudioType.None)
			{
				string name = GetSoundPath(type.ToString());
				//todo: the audio might be music, cache it will fail
				//When resource manage mechanism is done, this concern is not needed
				CacheAudio(name);
			}
		}
	}

	private bool CacheAudio(string name)
	{
		bool result = false;
		AudioClip clip = AssetManager.Instance.LoadAsset<AudioClip>(name);
		if(clip != null)
		{
			_cache[name] = clip;
			result = true;
		}
		return result;
	}

	public AudioClip LoadAudio(AudioType at)
	{
		string path = GetMusicPath(at.ToString());
		return LoadAudio(path);
	}

	public AudioClip LoadAudio(string name)
	{
		AudioClip result = null;
		if(_cache.ContainsKey(name))
		{
			result = _cache[name];
		}
		else
		{
			AudioClip audio = AssetManager.Instance.LoadAsset<AudioClip>(name);
			if(audio != null)
			{
				_cache[name] = audio;
				result = audio;
			}
		}
		return result;
	}

	#endregion
}
