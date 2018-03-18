using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class GamePariculeCustom : MonoBehaviour
{
	private static ScaleSettingOnIpad _defaultScaleSetting = new ScaleSettingOnIpad(1.2f, 1.16f);// reel3 on ipad

	private List<ScaleData> scaleDatas = null;
	private void Awake()
	{
		scaleDatas = new List<ScaleData>();
		foreach(ParticleSystem p in transform.GetComponentsInChildren<ParticleSystem>(true))
		{
			scaleDatas.Add(new ScaleData() { transform = p.transform, beginScale = p.transform.localScale });
		}
		CitrusEventManager.instance.AddListener((PuzzleMachineInitFinishedEvent e) => { });
	}

	private void Start()
	{
		if (GameScene.Instance == null){
			StartChangeScaler(null);
		}else{
			StartChangeScaler(GameScene.Instance.PuzzleMachine);
		}
	}

	private void StartChangeScaler(PuzzleMachine pmm)
	{
		if(scaleDatas.Count == 0) { return; }

		ScaleSettingOnIpad newS = _defaultScaleSetting;
		if (pmm != null){
			newS = pmm.MachineConfig.BasicConfig.ReelCount == 3 ? pmm.PuzzleConfig.M3OnIpad :
									pmm.PuzzleConfig.M4OnIpad;
		}


		float designScale = DeviceUtility.GetDesignWidthHeightRatio();
		float scaleRate = DeviceUtility.GetScreenWidthHeightRatio();
		// Debug.Log("desing" + designScale + "scaleRate" + scaleRate);
		foreach(ScaleData scale in scaleDatas)
		{
			if(scale.transform != null)
			{
				if(scaleRate < designScale) {
					float scaleFactor = scaleRate / designScale;
					if(DeviceUtility.IsIPadResolution()) {
						scale.transform.localScale = scale.beginScale * scaleFactor * newS._reelFrameScaleOnIpad;
					} else {
						scale.transform.localScale = scale.beginScale * scaleFactor;
					}
				} else {
					scale.transform.localScale = scale.beginScale;
				}
			}
		}
		// Debug.Log(scaleDatas[0].transform.localScale);
	}
}
