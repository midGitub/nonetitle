using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using CitrusFramework;

public class WindowInfo
{
    public Canvas canvas;
    public Action Open;
    public Action<Action<bool>> Close;
    public Action ForceToCloseImmediately;

    public WindowInfo(Action Open, Action<Action<bool>> Close, Canvas canvas, Action ForceToCloseImmediately)
    {
        this.canvas = canvas;
        this.Open = Open;
        this.Close = Close;
        this.ForceToCloseImmediately = ForceToCloseImmediately;
    }
}

public class WindowManager : SimpleSingleton<WindowManager> {

    private WindowInfo _openingWindow = null;

    private List<WindowInfo> _pendingWindowList = new List<WindowInfo>();

	private List<WindowInfo> _openingWindowList = new List<WindowInfo>();//not include the front one

    public bool IsThereOpeningWindow 
    { 
        get 
        { 
            if (_openingWindow != null)
            {
                if (_openingWindow.canvas == null || !_openingWindow.canvas.gameObject.activeInHierarchy)
                {
					ClearAllWindows();
                } 
            }

            return _openingWindow != null; 
        } 
    }

	public void ApplyToOpen(WindowInfo windowInfo,bool overLap = false)
    {
        //直接显示
		if (_openingWindow == null)
        {
			#if DEBUG
			string str = "";
			if(windowInfo.canvas != null)
				str = windowInfo.canvas.gameObject.name;
			LogUtility.Log("WindowManger immediatelyOpen: " + str, Color.magenta);
			#endif

            _openingWindow = windowInfo;
            _openingWindow.Open();
        }
		else if(overLap == true)
		{
			_openingWindowList.Insert(0, _openingWindow);
			_openingWindow = windowInfo;
			_openingWindow.Open();
		}
        else
        {
            //如果正在打开的窗口被非正常的关闭，那么就直接显示新的窗口，并清空之前在排队的过期窗口
            if (_openingWindow.canvas == null || !_openingWindow.canvas.gameObject.activeInHierarchy)
            {
                LogUtility.Log("WindowManger AbnormalBlock, clear list and immediatelyOpen", Color.magenta);
				ClearAllWindows();
                _openingWindow = windowInfo;
                _openingWindow.Open();
            }
            //进入等待队列
            else
            {
                _pendingWindowList.Add(windowInfo);
                LogUtility.Log("WindowManger pendingList, count is " + _pendingWindowList.Count, Color.magenta); 
            }
        }
    }

    /// <summary>
    /// 窗口或是场景已经关闭退出后，由此接口通知此管理器
    /// </summary>
    /// <param name="windowInfo">Window info.</param>
    public void TellClosed(WindowInfo windowInfo)
    {
        if (windowInfo != _openingWindow)
        {
            Debug.LogError("The window applying to close is different from the window openning!!");
			Debug.Assert(false);

			//by nichos:
			//Shouldn't be here. But when it's here, it might cause soft-lock. I don't know the root cause,
			//but just add a patch to fix possible soft-lock
			if (_openingWindow != null && _openingWindow.canvas != null)
				ClearAllWindows();
        }
        else
        {
            _openingWindow = null;
			if (_openingWindowList.Count > 0)
			{
				_openingWindow = _openingWindowList [0];
				_openingWindowList.RemoveAt(0);

			}
            else if (_pendingWindowList != null)
			{
                LogUtility.Log("WindowManger afterClosed, pending count is " + _pendingWindowList.Count, Color.magenta);
                if (_pendingWindowList.Count > 0)
                {
                    _openingWindow = _pendingWindowList[0];
                    _pendingWindowList.RemoveAt(0);
                    _openingWindow.Open();
                }   
            }
        }
    }

    /// <summary>
    /// 留给之后点击手机Back键用
    /// </summary>
    /// <param name="callBack">Call back.</param>
    /// To do: ！！！注意防止玩家连续点击Back键而连续多次调用此方法!!!
    public void BackWindow(Action callBack)
    {
        if (_openingWindow.Close != null)
        {
            _openingWindow.Close((bool result) => {
                if (result)
                {
                    _openingWindow = null;
					if(_openingWindowList.Count > 0)
					{
						_openingWindow = _openingWindowList[0];
						_openingWindowList.RemoveAt(0);
					}
                    else if (_pendingWindowList.Count > 0)
                    {
                        _openingWindow = _pendingWindowList[0];
                        _pendingWindowList.RemoveAt(0);
                        _openingWindow.Open();
                    }  
                }
                callBack();
            });
        }
        else
        {
            callBack();
        }
    }

    public void ClearAllWindows()
    {
        if (_openingWindow != null)
        {
            if (_openingWindow.canvas != null)
            {
                _openingWindow.ForceToCloseImmediately();            

            }
        } 
		CleanWindowList(_openingWindowList);
		CleanWindowList(_pendingWindowList);
		_openingWindow = null;
    }

	void CleanWindowList(List<WindowInfo> windowlist)
	{
		if (windowlist != null)
		{
			foreach (WindowInfo info in windowlist)
			{
				if (info.canvas != null)
					info.ForceToCloseImmediately();
			}
			windowlist.Clear();
		}
	}
}
