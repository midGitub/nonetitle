using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using CitrusFramework;
using MiniJSON;

public class UserMachineNetworkHelper : Singleton<UserMachineNetworkHelper>
{
	private delegate void NetworkCompleteDelegate(WWW www, string path);

	private static string _getSeedPath = "get_random_seed_array";

	#region Private

	private IEnumerator FetchUserMachineSeedsCoroutine(IList<string> machineNames, NetworkCompleteDelegate onSuccess, NetworkCompleteDelegate onFail)
	{
		Dictionary<string, object> dict = new Dictionary<string, object>();
		dict.Add("ProjectName", BuildUtility.GetProjectName());

		//give a default udid if it's empty
		string udid = string.IsNullOrEmpty(UserBasicData.Instance.UDID) ? "000000" : UserBasicData.Instance.UDID;
		//string udid = UserBasicData.Instance.UDID;

		dict.Add("UDID", udid);
		dict.Add("NameArray", machineNames);
		string jsonStr = Json.Serialize(dict);

		string url = Path.Combine(ServerConfig.GameServerUrl, _getSeedPath);
		WWW www = new WWW(url, Encoding.UTF8.GetBytes(jsonStr));
		yield return www;

		if(!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError("Network error:" + www.error);
			if(onFail != null)
				onFail(www, _getSeedPath);
			yield break;
		}
		else if(string.IsNullOrEmpty(www.text))
		{
			Debug.LogError("Network error: response is empty:");
			if(onFail != null)
				onFail(www, _getSeedPath);
			yield break;
		}

		onSuccess(www, _getSeedPath);
	}

	private IEnumerator FetchUserMachineSeedsCoroutineRepeated(IList<string> machineNames, 
		NetworkCompleteDelegate onSuccess, NetworkCompleteDelegate onFail, int tryCount)
	{
		yield return StartCoroutine(FetchUserMachineSeedsCoroutine(machineNames, onSuccess, (WWW www, string p) => {
			if(--tryCount > 0)
				FetchUserMachineSeedsCoroutine(machineNames, onSuccess, onFail);
			else
				onFail(www, p);
		}));
	}

	private void FetchSuccessCallback(WWW www, string path)
	{
		Debug.Log("FetchUserMachineSeeds Success");

		Dictionary<string, object> dict = Json.Deserialize(www.text) as Dictionary<string, object>;
		if(dict.ContainsKey("error"))
		{
			int errorCode = Convert.ToInt32(dict["error"]);
			if(errorCode == 0)
			{
				if(dict.ContainsKey("Seeds"))
				{
					Dictionary<string, object> objDict = dict["Seeds"] as Dictionary<string, object>;
					Dictionary<string, uint> seedDict = new Dictionary<string, uint>();
					foreach(var pair in objDict)
					{
						string name = pair.Key;
						uint seed = Convert.ToUInt32(pair.Value);

						#if UNITY_EDITOR
						Debug.Assert(seed != 0, "This can't be real");
						#endif

						seedDict.Add(name, seed);
					}

					UserMachineData.Instance.SaveFetchedInitMachineSeedDict(seedDict);
				}
			}
		}
		else
		{
			Debug.Assert(false);
		}
	}

	private void FetchFailCallback(WWW www, string path)
	{
		Debug.Log("FetchUserMachineSeeds Fail");
	}

	private List<string> GetAllFetchMachineNames()
	{
		List<string> result = new List<string>();
		Dictionary<string, InitMachineSeedInfo> infoDict = UserMachineData.Instance.InitMachineSeedInfoDict;
		string[] allMachines = CoreDefine.AllMachineNames;

		for(int i = 0; i < allMachines.Length; i++)
		{
			bool shouldFetch = false;
			string name = allMachines[i];
			if(infoDict.ContainsKey(name))
				shouldFetch = !infoDict[name]._isFromServer;
			else
				shouldFetch = true;

			if(shouldFetch)
				result.Add(name);
		}
		return result;
	}

	#endregion

	#region Public

	[Obsolete("Useless for now, but for possible future use")]
	private void TryFetchAllUserMachineSeeds()
	{
		List<string> fetchNames = GetAllFetchMachineNames();
		if(fetchNames.Count > 0)
		{
			StartCoroutine(FetchUserMachineSeedsCoroutineRepeated(fetchNames, 
				FetchSuccessCallback, FetchFailCallback, 3));
		}
		else
		{
			Debug.Log("No need to fetch user machine seeds");
		}
	}

	[Obsolete("Useless for now, but for possible future use")]
	private void FetchSingleUserMachineSeed(string machineName)
	{
		List<string> fetchNames = new List<string>(){ machineName };
		if(fetchNames.Count > 0)
		{
			StartCoroutine(FetchUserMachineSeedsCoroutineRepeated(fetchNames, 
				FetchSuccessCallback, FetchFailCallback, 3));
		}
	}

	#endregion
}
