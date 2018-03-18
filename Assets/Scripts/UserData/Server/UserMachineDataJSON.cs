using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using System;

public static class UserMachineDataJSON
{
	public enum FieldName
	{
		MachineSeedDict,
		InitMachineSeedInfoDict,
		TotalSpinCount,
		CurrentMachine,
		MachineInfo,
	}

	public static string CreatDataDicJSON()
	{
		Dictionary<string, object> dic = new Dictionary<string, object>();

		dic.Add(FieldName.MachineSeedDict.ToString(), Json.Serialize(UserMachineData.Instance.MachineSeedDict));

		//this is egg pain
		//convert Dictionary<string, CustomClass> to Dictionary<string, string>
		Dictionary<string, string> infoStringDict = new Dictionary<string, string>();
		foreach(var pair in UserMachineData.Instance.InitMachineSeedInfoDict)
		{
			infoStringDict[pair.Key] = InitMachineSeedInfo.Serialize(pair.Value);
		}
		dic.Add(FieldName.InitMachineSeedInfoDict.ToString(), Json.Serialize(infoStringDict));

		dic.Add(FieldName.TotalSpinCount.ToString(), UserMachineData.Instance.TotalSpinCount);
		dic.Add(FieldName.CurrentMachine.ToString(), UserMachineData.Instance.CurrentMachine);

		infoStringDict.Clear ();
		foreach (var pair in UserMachineData.Instance.MachineInfoDict) {
			infoStringDict [pair.Key] = MachineInfo.Serialize (pair.Value);
		}
		dic.Add (FieldName.MachineInfo.ToString (), Json.Serialize (infoStringDict));

		return Json.Serialize(dic);
	}


	public static void UseJSONToData(JSONObject json)
	{
		//UserMachineData.Instance.MachineSeedDict 
		var mdic = new JSONObject(json.GetField(FieldName.MachineSeedDict.ToString()).str).ToDictionary();
		foreach(var item in mdic)
		{
			UserMachineData.Instance.MachineSeedDict[item.Key] = Convert.ToUInt32(item.Value);
		}

		//this is egg pain
		//convert Dictionary<string, string> back to Dictionary<string, CustomClass>
		var msstring = json.GetField(FieldName.InitMachineSeedInfoDict.ToString()).str;
		if(!string.IsNullOrEmpty(msstring))
		{
			var idic = new JSONObject(msstring).ToDictionary();

			foreach(var item in idic)
			{
				string infoString = item.Value as string;
				InitMachineSeedInfo info = InitMachineSeedInfo.Deserialize(infoString);
				if(info != null)
					UserMachineData.Instance.InitMachineSeedInfoDict[item.Key] = info;
			}
		}

		UserMachineData.Instance.TotalSpinCount = (int)json.GetField(FieldName.TotalSpinCount.ToString()).n;
		UserMachineData.Instance.CurrentMachine = json.GetField(FieldName.CurrentMachine.ToString()).str;
		if (json.HasField(FieldName.MachineInfo.ToString())) 
		{
			msstring = json.GetField (FieldName.MachineInfo.ToString ()).str;
			if (!string.IsNullOrEmpty (msstring)) {
				var idic = new JSONObject (msstring).ToDictionary ();

				foreach(var item in idic)
				{
					string infoString = item.Value as string;
					MachineInfo info = MachineInfo.Deserialize(infoString);
					if(info != null)
						UserMachineData.Instance.MachineInfoDict[item.Key] = info;
				}
			}
		}

		UserMachineData.Instance.Save();
	}
}
