using UnityEngine;
using System.Collections;

namespace CitrusFramework{


	public class Pool {

		static bool mInited = false;
		static public void Init(GameObject prefeb)
		{
            PoolBoss pool = GameObject.FindObjectOfType(typeof(PoolBoss)) as PoolBoss;
			if(!mInited)
			{
                if(pool == null)
				{
					GameObject instance = GameObject.Instantiate<GameObject>(prefeb);
                    pool = instance.GetComponent<PoolBoss>();
                    if (pool != null)
                    {
                        GameObject.DontDestroyOnLoad(pool);
                    }
                    else
                    {
                        Debug.LogError("Init Pool Boss Error !");
                    }
				}
                pool.InitOnAwake();
				mInited = true;
			}
       
		}

        static public bool LoadedFinished (GameObject prefeb)
        {
            PoolBoss pool = GameObject.FindObjectOfType(typeof(PoolBoss)) as PoolBoss;

            if (mInited)
            {
                if(pool == null)
                {
                    GameObject instance = GameObject.Instantiate<GameObject>(prefeb);
                    pool = instance.GetComponent<PoolBoss>();
                }

                if (pool != null)
                {
                    return pool.InitReady();
                }
            }

            return false;
        }

		static public Transform Spawn(string path , Vector3 pos , Quaternion rot , Transform  parentTrans)
		{
			return PoolBoss.Spawn(path , pos , rot , parentTrans);
		}
		/// <summary>
		/// Spawn the specified path, pos and rot. it's a child of poolboss prfab
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="pos">Position.</param>
		/// <param name="rot">Rot.</param>
		static public Transform Spawn(string path, Vector3 pos, Quaternion rot)
		{
			return PoolBoss.SpawnInPool(path, pos, rot);
		}

		/// <summary>
		/// Spawns the outside.
		/// </summary>
		/// <returns>The outside.</returns>
		static public Transform SpawnOutside(string path , Vector3 pos , Quaternion rot)
		{
			return PoolBoss.SpawnOutsidePool(path , pos , rot);
		}

		static public bool Despawn(Transform transform)
		{
			if(PoolBoss.PrefabIsInPool(transform))
			{
				PoolBoss.Despawn(transform);
				return true;
			}

			return false;
		}

		static public void DespwanAll()
		{
			PoolBoss.DespawnAllPrefabs();
		}
	}

}