using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MachineButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
	public MapMachineController _machineController;
	public bool IsComingSoonMachine;

	[HideInInspector]
	public MapScene _mapScene;

	public delegate void PointerDownHandler (PointerEventData eventData);
	public delegate void PointerUpHandler (PointerEventData eventData);
	public delegate void DragHandler (PointerEventData eventData);

	public PointerDownHandler _downHandler = null;
	public PointerUpHandler _upHandler = null;

	string _machineName;

	// Use this for initialization
	public void Init(MapScene mapScene)
	{
		_mapScene = mapScene;
		if(_machineController != null)
			_machineName = _machineController.MachineName;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_machineController == null)
			return;
		
		if (_machineController.IsUnlock && !_machineController._lockBehaviour.isActiveAndEnabled) {
			if(IsComingSoonMachine)
			{
				ComingSoon.Instance.ShowComingSoon();
			}
			else if(_machineController._machineDownloader != null && _machineController._machineDownloader.ShouldDownload())
			{
				if(!_machineController._machineDownloader.IsDownloading)
				{
					if(Application.internetReachability == NetworkReachability.NotReachable)
					{
						string warningText = LocalizationConfig.Instance.GetValue("no_internet_warning");
						WarningLayer.ShowWarningLayer(warningText);
					}
					else
					{
						_machineController._machineDownloader.StartDownload(
							(string machineName) => {
								_mapScene.MapButtonDown(_machineName);
							},
							null);
					}
				}
			}
			else
			{
				_mapScene.MapButtonDown (_machineName);
			}
		} else {
			#if false
			// 锁机台情况下
			_machineController.SwitchLockUI();
			#endif
		}
	}

	public void OnPointerDown (PointerEventData eventData){
		if (_downHandler != null) {
			_downHandler (eventData);
		}
	}

	public void OnPointerUp (PointerEventData eventData){
		if (_upHandler != null) {
			_upHandler (eventData);
		}
	}
}
