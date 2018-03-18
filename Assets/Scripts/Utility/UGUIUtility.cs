using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;

public class UGUIUtility
{
	public static GameObject CreateObj(string path, GameObject parent = null, 
		bool resetLocalPos = true, bool resetLocalRot = true, bool resetLocalScale = true)
	{
		GameObject obj = null;
		GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>(path);
		if (prefab != null) {
			obj = CreateObj (prefab, parent, resetLocalPos, resetLocalRot, resetLocalScale);
		} else {
			LogUtility.Log ("CreateObj failed path = "+path, Color.red);
		}

		return obj;
	}
	
	public static GameObject CreateMachineAsset(string path, string machineName, GameObject parent = null,
		bool resetLocalPos = true, bool resetLocalRot = true, bool resetLocalScale = true)
	{
		GameObject obj = null;
		GameObject prefab = AssetManager.Instance.LoadMachineAsset<GameObject>(path, machineName);
		if (prefab != null) {
			obj = CreateObj (prefab, parent, resetLocalPos, resetLocalRot, resetLocalScale);
		} else {
			LogUtility.Log ("CreateMachineAsset failed path = "+path+" machine = "+machineName, Color.red);
		}

		return obj;
	}

	public static GameObject CreateObj(GameObject objPrefab, GameObject parent, 
		bool resetLocalPos = true, bool resetLocalRot = true, bool resetLocalScale = true)
	{
		GameObject obj;
		if(parent != null)
		{
			obj = GameObject.Instantiate(objPrefab, parent.transform);
		}
		else
		{
			obj = GameObject.Instantiate(objPrefab);
		}
		if (resetLocalPos)
			obj.transform.localPosition = Vector3.zero;
		if (resetLocalRot)
			obj.transform.localRotation = Quaternion.identity;
		if (resetLocalScale)
			obj.transform.localScale = Vector3.one;

		return obj;
	}

	public static GameObject FindOrInstantiateUI<T>(GameObject parent, string assetPath) where T : MonoBehaviour
	{
		GameObject obj = null;
		if (parent == null)
			return null;
		
		T behaviour = parent.GetComponentInChildren<T>(true);
		if(behaviour == null)
		{
			string path = assetPath;
			GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>(path);
			obj = CreateObj(prefab, parent);
		}
		else
		{
			obj = behaviour.gameObject;
		}

		//if(obj != null) obj.SetActive(true);

		return obj;
	}

	public static GameObject OpenUIOrCreatOPen(GameObject newgo,string assetPath)
	{
		if(newgo == null)
		{
			string path = assetPath;
			GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>(path);
			newgo = CreateObj(prefab, null);
		}

		newgo.SetActive(true);
		return newgo;
	}

    public static GameObject InstantiateUI(GameObject newgo, string assetPath)
    {
        if (newgo == null)
        {
            string path = assetPath;
            GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>(path);
            newgo = CreateObj(prefab, null);
        }

        return newgo;
    }

    public static Vector2 GetScreenPos(Canvas canvas, GameObject obj)
    {
        Vector3 screenPos;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            screenPos = obj.transform.position;
        else
            screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);

        return new Vector2(screenPos.x, screenPos.y);
    }

    public static bool CanObjectBeClickedNow(Canvas canvas, GameObject obj)
    {
        PointerEventData exit = new PointerEventData(EventSystem.current);                            // This section prepares a list for all objects hit with the raycast
        exit.position = GetScreenPos(canvas, obj);
        List<RaycastResult> objectsHit = new List<RaycastResult> ();
        EventSystem.current.RaycastAll(exit, objectsHit);

        StringBuilder sb = new StringBuilder("ObjectsHit count: ");
        sb.Append(objectsHit.Count);
        int i;
        for (i = 0; i < objectsHit.Count; i++)
        {
            sb.Append(" ");
            sb.Append(objectsHit[i].gameObject.name);
        }
        Debug.Log(sb.ToString());

        if (objectsHit.Count > 0 && objectsHit[0].gameObject == obj)
            return true;
        else
            return false;
    }

	public static GameObject InstantiateUI(string assetPath)
	{
		GameObject newgo; 
		string path = assetPath;
		GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>(path);
		newgo = CreateObj(prefab, null);
		return newgo;
	}
}
