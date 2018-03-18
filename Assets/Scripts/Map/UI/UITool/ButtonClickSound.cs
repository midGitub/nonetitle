using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickSound : MonoBehaviour 
{
	public static AudioType SoundType = AudioType.Click;

	public void PlayClickSound()
	{
		AudioManager.Instance.PlaySound(SoundType);
	}
}
