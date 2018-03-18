using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADSConfig : SimpleSingleton<ADSConfig> 
{
	public static readonly string Name = "ADS";

	public ADSSheet Sheet { get; private set; }

	public ADSConfig()
	{
		LoadRawData();
	}

	public ADSData FreePlayer
	{
		get
		{
			return Sheet.dataArray[0];
		}
	}

	public ADSData PayingPlayer
	{
		get
		{
			return Sheet.dataArray[1];
		}
	}

	public ADSData PayingNoADSPlayer
	{
		get
		{
			return Sheet.dataArray[2];
		}
	}

	void LoadRawData()
	{
		Sheet = GameConfig.Instance.LoadExcelAsset<ADSSheet>(Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload ADSConfig");
		ADSConfig.Instance.LoadRawData();
	}
}
