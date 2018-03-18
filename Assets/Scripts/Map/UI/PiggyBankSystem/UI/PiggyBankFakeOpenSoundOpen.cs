using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiggyBankFakeOpenSoundOpen : MonoBehaviour 
{
	public void PlayPiggyBankFakeOpenSound()
	{
		AudioManager.Instance.PlaySound(AudioType.PiggyBankFakeOpen);
	}
}
