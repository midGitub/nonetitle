using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class MachineSeedManager : SimpleSingleton<MachineSeedManager>
{
	public uint GetRandomSeed(string machineName)
	{
		uint result = 0;
		List<uint> seeds = GetRandomSeeds(machineName);
		if(seeds.Count > 0)
		{
			System.Random r = new System.Random();
			int index = r.Next(seeds.Count);
			result = seeds[index];
		}
		else
		{
			Debug.Log("No machine seeds to load");
			System.Random r = new System.Random();
			result = (uint)r.Next();
		}

		return result;
	}

	public List<uint> GetRandomSeeds(string machineName)
	{
		List<uint> result = new List<uint>();

		string fileName = MachineSeedConfig.GetSeedFileName(machineName, false);
		string filePath = Path.Combine(MachineSeedConfig.SeedFileDir, fileName);
		TextAsset textAsset = AssetManager.Instance.LoadAsset<TextAsset>(filePath);
		if(textAsset != null)
		{
			string content = textAsset.text;
			if(!string.IsNullOrEmpty(content))
			{
				string[] seedArray = content.Split(MachineSeedConfig.SeedFileDelimitor);
				foreach(string s in seedArray)
				{
					if(!string.IsNullOrEmpty(s))
					{
						uint seed = 0;
						if(uint.TryParse(s, out seed))
							result.Add(seed);
						else
							Debug.LogError("Random seed parse error: " + s);
					}
				}
			}
		}
		else
		{
			Debug.Log("No seed file to load: " + filePath);
		}

		return result;
	}
}

