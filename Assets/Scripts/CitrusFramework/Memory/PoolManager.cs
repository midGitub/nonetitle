using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PoolManager {

	static Dictionary<string, PoolBossEx> dictionaryOfPool = new Dictionary<string, PoolBossEx>();
	static Dictionary<string, string> dictionaryOfObjectBelonging = new Dictionary<string, string>();

	public static bool HavePoolBossWithTag(string tag)
	{
		return dictionaryOfPool.ContainsKey (tag);
	}

	public static PoolBoss GetPoolBossWithTag(string tag)
	{
		if (dictionaryOfPool.ContainsKey (tag))
			return dictionaryOfPool [tag];
		else
			return null;
	}

	public static void Init(GameObject prefab)
	{
		PoolBossEx pool = prefab.GetComponent<PoolBossEx> ();
		string tag = pool.GetTag ();
		if (!dictionaryOfPool.ContainsKey (tag)) 
		{
			GameObject instance = GameObject.Instantiate<GameObject>(prefab);
			PoolBossEx poolInstance = instance.GetComponent<PoolBossEx>();
			if (poolInstance != null)
			{
				GameObject.DontDestroyOnLoad(poolInstance);
			}
			else
			{
				Debug.LogError("Init Pool Boss Error !");
			}
			dictionaryOfPool.Add (tag, poolInstance);
			poolInstance.InitOnAwake ();
		}
	}

	public static Transform Spawn(string path, Vector3 pos, Quaternion rot)
	{
		string tag = GetObjectBelonging (path);
		if (string.IsNullOrEmpty (tag)) 
		{
			Debug.LogError("The GameObject with name "+ path + " is not in the Poolboss");
			return null;
		}
		if (!dictionaryOfPool.ContainsKey (tag)) {
			Debug.LogError("The PoolBoss with tag "+ tag + " is not loaded yet");
			return null;
		}
		PoolBossEx pool = dictionaryOfPool [tag];
		return pool.SpawnInPool (path, pos, rot);
	}

	public static bool Despawn(Transform transform)
	{
		string name = GetPrefabName (transform);
		if (string.IsNullOrEmpty (name)) 
		{
			Debug.LogError("Fail to find prefab name");
			return false;
		}

		string tag = GetObjectBelonging (name);
		if (string.IsNullOrEmpty (tag)) 
		{
			Debug.LogError("Fail to find poolboss tag");
			return false;
		}

		if (!dictionaryOfPool.ContainsKey (tag)) {
			Debug.LogError("The PoolBoss with tag "+ tag + " is not loaded yet");
			return false;
		}

		PoolBossEx pool = dictionaryOfPool [tag];
		if(pool.PrefabIsInPool(transform))
		{
			pool.Despawn(transform);
			return true;
		}

		return false;
	}

	public static void DespawnAll()
	{
		foreach (var pair in dictionaryOfPool) 
		{
			pair.Value.DespawnAllPrefabs ();
		}
	}

	public static void SetObjectBelonging(string objectName, string poolBossTag)
	{
		if (!dictionaryOfObjectBelonging.ContainsKey (objectName)) 
		{
			dictionaryOfObjectBelonging.Add (objectName, poolBossTag);
		}
	}

	public static string GetObjectBelonging(string objectName)
	{
		if (dictionaryOfObjectBelonging.ContainsKey (objectName)) {
			return dictionaryOfObjectBelonging [objectName];
		} else {
			Debug.LogError("The GameObject with name "+ objectName + " is not in the Poolboss");
			return null;
		}
	}

	public static string GetPrefabName(Transform trans) {
		if (trans == null) {
			return null;
		}

		var itemName = trans.name;
		var iParen = itemName.IndexOf(" (", StringComparison.Ordinal);
		if (iParen > -1) {
			itemName = itemName.Substring(0, iParen);
		}

		return itemName;
	}
}
