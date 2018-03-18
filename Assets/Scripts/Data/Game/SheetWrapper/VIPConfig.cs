using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VIPConfig : SimpleSingleton<VIPConfig>
{
	public static readonly string Name = "VIP";
	static readonly string _imageDirPath = "Images/UI/VIP/";

	private VIPSheet _sheet;
	public List<VIPData> ListSheet { private set; get; }

	public VIPConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<VIPSheet>(Name);
		ListSheet = _sheet.dataArray.ToList();
	}

	public static void Reload()
	{
		Debug.Log("Reload VIPConfig");
		VIPConfig.Instance.LoadData();
	}

	public VIPData FindVIPDataByLevel(int level)
	{
		var max = ListSheet.Last();
		if(max.VIPLeveL <= level)
			return max;

		var item = ListSheet.FirstOrDefault((VIPData obj) => {
			return obj.VIPLeveL == level;
		});

		#if UNITY_EDITOR
		if(item == null)
			Debug.LogError("没有VIPLevel数据" + level);
		#endif

		return item;
	}

	public int GetPointAboutVIPLevel(int currPoint)
	{
		var max = ListSheet.Last();
		if(max.VIPLevelNeedPoint <= currPoint)
			return max.VIPLeveL;

		var nextLevel = ListSheet.FirstOrDefault((VIPData arg) => {
			return currPoint < arg.VIPLevelNeedPoint;
		});

		return nextLevel.VIPLeveL - 1;
	}

	public Sprite GetDiamondImageByLevelName(string name)
	{
		return AssetManager.Instance.LoadAsset<Sprite>(_imageDirPath + name);
	}

	public Sprite GetDiamondLineByLevelName(string name)
	{
		return AssetManager.Instance.LoadAsset<Sprite>(_imageDirPath + name + "Line");
	}

}
