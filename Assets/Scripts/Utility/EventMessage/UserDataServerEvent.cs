using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

/// <summary>
/// 用户处理服务器数据回答message
/// </summary>
public class UserDataServerEvent : CitrusGameEvent 
{
	public enum ShowUI
	{
		Error,
		Loading,
		Success,
		Ask,
		NoOne,
	}

	public ShowUI ShowUi;
	public UserDataServerEvent(ShowUI si) : base()
	{
		ShowUi = si;
	}
}
