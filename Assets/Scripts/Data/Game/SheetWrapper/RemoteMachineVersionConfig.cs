using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteMachineVersionConfig : SimpleSingleton<RemoteMachineVersionConfig>
{
	public static readonly string Name = "RemoteMachineVersion";

	private RemoteMachineVersionSheet _sheet;

	public RemoteMachineVersionConfig()
	{
		LoadData();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<RemoteMachineVersionSheet>(Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload RemoteMachineVersionConfig");
		RemoteMachineVersionConfig.Instance.LoadData();
	}

	public int GetVersion(string machineName)
	{
		int result = 1; //default is 1
		for(int i = 0; i < _sheet.dataArray.Length; i++)
		{
			RemoteMachineVersionData d = _sheet.dataArray[i];
			if(d.Machine == machineName)
			{
				result = d.Version;
				break;
			}
		}
		return result;
	}
}
