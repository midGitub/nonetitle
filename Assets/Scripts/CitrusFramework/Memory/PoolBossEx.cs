using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBossEx : PoolBoss {

	[SerializeField]
	public new string tag = "";

	private readonly Dictionary<string, PoolItemInstanceList> PoolItemsByName = new Dictionary<string, PoolItemInstanceList>();
	private Transform _trans;
	private PoolBossEx _instance;
	private bool _isReady;
	private const string SpawnedMessageName = "OnSpawned";
	private const string DespawnedMessageName = "OnDespawned";
	private const string NotInitError = "Pool Boss has not initialized (does so in Awake event) and is not ready to be used yet.";

	public string GetTag()
	{
		return tag;
	}

	// ReSharper disable once UnusedMember.Local
	public new void InitOnAwake() {
		_isReady = false;
		PoolItemsByName.Clear();

		for (var p = 0; p < poolItems.Count; p++) {
			var item = poolItems[p];

			if (item.instancesToPreload <= 0) {
				continue;
			}

			if (item.prefabTransform == null) {
				Debug.LogError("You have an item in Pool Boss with no prefab assigned at position: " + (p + 1));
				continue;
			}

			var itemName = item.prefabTransform.name;
			if (PoolItemsByName.ContainsKey(itemName)) {
				Debug.LogError("You have more than one instance of '" + itemName + "' in Pool Boss. Skipping the second instance.");
				continue;
			}

			var itemClones = new List<Transform>();

			for (var i = 0; i < item.instancesToPreload; i++) {
				var createdObjTransform = InstantiateForPool(item.prefabTransform, i + 1);
				itemClones.Add(createdObjTransform);
			}

			var instanceList = new PoolItemInstanceList(itemClones) {
				LogMessages = item.logMessages,
				AllowInstantiateMore = item.allowInstantiateMore,
				SourceTrans = item.prefabTransform,
				ItemHardLimit = item.itemHardLimit
			};

			PoolItemsByName.Add(itemName, instanceList);
			PoolManager.SetObjectBelonging (itemName, tag);
		}

		_isReady = true;
	}

	public new bool InitReady ()
	{
		return _isReady;
	}

	private Transform InstantiateForPool(Transform prefabTrans, int cloneNumber) {
		var createdObjTransform = Instantiate(prefabTrans, Trans.position, prefabTrans.rotation) as Transform;
		// ReSharper disable once PossibleNullReferenceException
		createdObjTransform.name = prefabTrans.name + " (Clone " + cloneNumber + ")"; // don't want the "(Clone)" suffix.

		SetParent(createdObjTransform, Trans);

		SetActive(createdObjTransform.gameObject, false);

		return createdObjTransform;
	}
		
	public new Transform SpawnInPool(string itemName, Vector3 position, Quaternion rotation) {
		return Spawn (itemName, position, rotation, Trans);
	}

	public new Transform Spawn(string itemName, Vector3 position, Quaternion rotation,
		Transform parentTransform) {

		PoolItemInstanceList itemSettings = null;
		if (PoolItemsByName.ContainsKey(itemName))
		{
			itemSettings = PoolItemsByName[itemName];
		}
		else
		{
			Debug.LogError("[ " + itemName + " ]" + "Not Found !");
			return null;
		}

		if (itemSettings.DespawnedClones.Count == 0) {
			if (!itemSettings.AllowInstantiateMore) {
				Debug.LogError("The Transform '" + itemName + "' has no available clones left to Spawn in Pool Boss. Please increase your Preload Qty, turn on Allow Instantiate More or turn on Recycle Oldest (Recycle is only for non-essential things like decals).");
				return null;
			}

			// Instantiate a new one
			var curCount = NumberOfClones(itemSettings);
			if (curCount >= itemSettings.ItemHardLimit) {
				Debug.LogError("The Transform '" + itemName + "' has reached its item limit in Pool Boss. Please increase your Preload Qty or Item Limit.");
				return null;
			}

			var createdObjTransform = InstantiateForPool(itemSettings.SourceTrans, curCount + 1);
			itemSettings.DespawnedClones.Add(createdObjTransform);

			if (logMessages || itemSettings.LogMessages) {
				Debug.LogWarning("Pool Boss Instantiated an extra '" + itemName + "' at " + Time.time + " because there were none left in the Pool.");
			}
		}

		var randomIndex = Random.Range(0, itemSettings.DespawnedClones.Count);
		var cloneToSpawn = itemSettings.DespawnedClones[randomIndex];

		if (cloneToSpawn == null) {
			Debug.LogError("One or more of the prefab '" + itemName + "' in Pool Boss has been destroyed. You should never destroy objects in the Pool. Despawn instead. Not spawning anything for this call.");
			return null;
		}

		cloneToSpawn.position = position;
		cloneToSpawn.rotation = rotation;

		SetActive(cloneToSpawn.gameObject, true);

		if (logMessages || itemSettings.LogMessages) {
			Debug.Log("Pool Boss spawned '" + cloneToSpawn.name + "' at " + Time.time);
		}

		SetParent(cloneToSpawn, parentTransform);

		cloneToSpawn.BroadcastMessage(SpawnedMessageName, SendMessageOptions.DontRequireReceiver);

		itemSettings.DespawnedClones.Remove(cloneToSpawn);
		itemSettings.SpawnedClones.Add(cloneToSpawn);

		return cloneToSpawn;
	}

	public new void Despawn(Transform transToDespawn) {
		if (!_isReady) {
			Debug.LogError(NotInitError);
			return;
		}

		if (transToDespawn == null) {
			Debug.LogError("No Transform passed to Despawn method.");
			return;
		}

		//        if (!IsActive(transToDespawn.gameObject)) {
		//            return; // already sent to despawn
		//        }

		var itemName = PoolManager.GetPrefabName(transToDespawn);

		if (!PoolItemsByName.ContainsKey(itemName)) {
//			if (Instance.autoAddMissingPoolItems) {
//				CreateMissingPoolItem(transToDespawn, itemName, false);
//			} else {
				Debug.LogError("The Transform '" + itemName + "' passed to Despawn is not in the Pool Boss. Not despawning.");
				return;
//			}
		}

		transToDespawn.BroadcastMessage(DespawnedMessageName, SendMessageOptions.DontRequireReceiver);

		var cloneList = PoolItemsByName[itemName];

		SetParent(transToDespawn, Trans);

		SetActive(transToDespawn.gameObject, false);

		if (logMessages || cloneList.LogMessages) {
			Debug.Log("PoolBoss despawned '" + transToDespawn.name + "' at " + Time.time);
		}

		cloneList.SpawnedClones.Remove(transToDespawn);
		cloneList.DespawnedClones.Add(transToDespawn);
	}

	public new void DespawnAllOfPrefab(Transform transToDespawn) {

		if (transToDespawn == null) {
			Debug.LogError("No Transform passed to DespawnAllOfPrefab method.");
			return;
		}

		var itemName = PoolManager.GetPrefabName(transToDespawn);

		if (!PoolItemsByName.ContainsKey(itemName)) {
			Debug.LogError("The Transform '" + itemName + "' passed to DespawnAllOfPrefab is not in the Pool Boss. Not despawning.");
			return;
		}

		var spawned = PoolItemsByName[itemName].SpawnedClones;

		var max = spawned.Count;
		while (spawned.Count > 0 && max > 0) {
			Despawn(spawned[0]);
			max--;
		}
	}

	public new void DespawnAllPrefabs() {

		var items = PoolItemsByName.Values.GetEnumerator();
		while (items.MoveNext()) {
			// ReSharper disable once PossibleNullReferenceException
			DespawnAllOfPrefab(items.Current.SourceTrans);
		}
	}

	private void SetParent(Transform trns, Transform parentTrans) {
		#if UNITY_4_6 || UNITY_5
		var rectTrans = trns.GetComponent<RectTransform>();
		if (rectTrans != null) {
			rectTrans.SetParent(parentTrans);
		} else {
			trns.parent = parentTrans;
		}
		#else
		trns.parent = parentTrans;
		#endif
	}

	private int NumberOfClones(PoolItemInstanceList instList) {
		return instList.DespawnedClones.Count + instList.SpawnedClones.Count;
	}

	public new Transform Trans {
		get {
			if (_trans != null) {
				return _trans;
			}
			_trans = GetComponent<Transform>();

			return _trans;
		}
	}

	public new bool PrefabIsInPool(Transform trans) {
		if (_isReady) {
			return PrefabIsInPool(PoolManager.GetPrefabName(trans));
		}

		Debug.LogError(NotInitError);
		return false;
	}

	private bool PrefabIsInPool(string transName) {
		if (_isReady) {
			return PoolItemsByName.ContainsKey(transName);
		}

		Debug.LogError(NotInitError);
		return false;
	}
}
