using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class BackButtonManager : Singleton<BackButtonManager> {

    private bool _isBacking = false;
	// Update is called once per frame
	void Update () {
        if (!_isBacking)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (WindowManager.Instance.IsThereOpeningWindow)
                {
                    BackWindow();
                }
                else
                {
                    BackScene();
                }
            }   
        }
	}

    private void BackWindow()
    {
        _isBacking = true;
        WindowManager.Instance.BackWindow(NormalCallBack);
    }

    private void BackScene()
    {
        LogUtility.Log("CurrSceneName: " + ScenesController.Instance.GetCurrSceneName() + " IsAsynLoading: " + ScenesController.Instance.IsAsyncLoading, Color.magenta);
        if (!ScenesController.Instance.IsAsyncLoading)
        {
            if (ScenesController.Instance.GetCurrSceneName() == ScenesController.GameSceneName)
            {
                if (GameScene.Instance.PuzzleMachine._state == MachineState.Idle)
                {
                    LogUtility.Log("Back Game", Color.magenta);
                    _isBacking = true;
		            AudioManager.Instance.StopAllSound();
                    ScenesController.Instance.EnterMainMapScene(NormalCallBack);
                }
            }
            else if (ScenesController.Instance.GetCurrSceneName() == ScenesController.MainMapSceneName)
            {
                UIManager.Instance.OpenQuitGameUI();
            }   
        }
    }

    private void NormalCallBack()
    {
        LogUtility.Log("Backing end", Color.magenta);
        _isBacking = false;
    }
}
