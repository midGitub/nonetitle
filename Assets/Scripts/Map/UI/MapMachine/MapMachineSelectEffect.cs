using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapMachineSelectEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MapMachineType MapMachineType;

    private static readonly string _tinyMapMachineSelectEffectPath = "Effect/Prefab/FX_MapMachineSmallGlow";
    private static readonly string _machineSelectEffectPath = "Effect/Prefab/FX_MapMachineGlow";
	private Coroutine currCor;
	private GameObject currEffect;

	public void OnPointerEnter(PointerEventData eventData)
	{
	    if (currEffect == null)
	    {
	        currEffect = LoadEffect();
            InitEffect();
	    }

		PlayEffect();
	}

    GameObject LoadEffect()
    {
        string effectPath = GetEffectPath(MapMachineType);
        currEffect = UGUIUtility.OpenUIOrCreatOPen(currEffect, effectPath);
        return currEffect;
    }

    void InitEffect()
    {
        currEffect.SetActive(false);
        currEffect.transform.SetParent(this.transform);
        currEffect.transform.localPosition = Vector3.zero;
        currEffect.transform.localScale = Vector3.one;
    }

    void PlayEffect()
    {
        CloseEffect();
        currEffect.SetActive(true);
        currCor = StartCoroutine(Timer(1.5f, () => { currEffect.SetActive(false); }));
    }

    public void OnPointerExit(PointerEventData eventData)
	{
		//CloseEffect();
	}

    void CloseEffect()
	{
		if(currCor != null)
		{
            currEffect.SetActive(false);
            StopCoroutine(currCor);
		}
		//if(currEffect != null)
		//{
		//	currEffect.SetActive(false);
		//}
	}

    IEnumerator Timer(float time, Action ac)
	{
		yield return new WaitForSeconds(time);
		ac();
	}

    string GetEffectPath(MapMachineType type)
    {
        string result = "";
        switch (type)
        {
            case MapMachineType.Normal:
                result = _machineSelectEffectPath;
                break;
            case MapMachineType.Tiny:
                result = _tinyMapMachineSelectEffectPath;
                break;
        }

        return result;
    }
}
