using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


class ScaleData
{
	public Transform transform;
	public Vector3 beginScale = Vector3.one;
}
public class ParticleCustom : MonoBehaviour
{
	private List<ScaleData> scaleDatas = null;
	void Awake()
	{
		scaleDatas = new List<ScaleData>();
		foreach(ParticleSystem p in transform.GetComponentsInChildren<ParticleSystem>(true))
		{
			scaleDatas.Add(new ScaleData() { transform = p.transform, beginScale = p.transform.localScale });
		}
	}

	void Start()
	{
		if(scaleDatas.Count == 0) { return; }

		float designScale = DeviceUtility.GetDesignWidthHeightRatio();
		float scaleRate = DeviceUtility.GetScreenWidthHeightRatio();
		// Debug.Log("desing" + designScale + "scaleRate" + scaleRate);
		foreach(ScaleData scale in scaleDatas)
		{
			if(scale.transform != null)
			{
				if(scaleRate < designScale)
				{
					float scaleFactor = scaleRate / designScale;
					scale.transform.localScale = scale.beginScale * scaleFactor;
				}
				else {
					scale.transform.localScale = scale.beginScale;
				}
			}
		}
	}

#if UNITY_EDITOR
	void Update()
	{
		Start(); //Editor下修改屏幕的大小实时预览缩放效果
	}
#endif


}
