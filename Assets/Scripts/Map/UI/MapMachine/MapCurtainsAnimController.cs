using System;
using System.Collections;
using CitrusFramework;
using UnityEngine;

public enum MapCurtainsAnim
{
    MoveInFromLeft,
    MoveInFromRight,
}
public class MapCurtainsAnimController : MonoBehaviour {

    public Animator AnimCtrl;
    public RectTransform Root;
    public float FixDistance;
    public float FixPosDuringTime;
    public float FixPosDelayTime;
    public float PlayMoveInDelayTime;
    public MapCurtainsUiController CurtainsUiCtrl;

    private bool _needShowNormalBg;

    public void SetCurtainPos(bool isInTinyMachinesRoom)
    {
        float xOffset = DeviceUtility.DesignWidth + FixDistance;
        Vector3 offset = isInTinyMachinesRoom ? new Vector3(xOffset, 0, 0) : Vector3.zero;
        string animName = isInTinyMachinesRoom ? "IdleInLeft" : "IdleInRight";
        CurtainsUiCtrl.AlreadySwitched = isInTinyMachinesRoom;

        Root.anchoredPosition3D = new Vector3(offset.x, Root.anchoredPosition3D.y, Root.anchoredPosition3D.z);
        AnimCtrl.SetTrigger(animName);
    }

    public IEnumerator PlayAnim(MapCurtainsAnim anim, Action callback)
    {
        yield return new WaitForSeconds(PlayMoveInDelayTime);

        string animName = anim.ToString();
        AnimCtrl.SetTrigger(animName);

        AnimatorStateInfo stateInfo = AnimCtrl.GetCurrentAnimatorStateInfo(0);
        while (!stateInfo.IsName(animName))
        {
            stateInfo = AnimCtrl.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        StartCoroutine(FixCurtainsPos(anim, FixPosDuringTime, OnFixPosEnd));
        yield return new WaitForSeconds(stateInfo.length);

        if (callback != null)
        {
            callback();
        }
    }

    IEnumerator FixCurtainsPos(MapCurtainsAnim anim, float duringTime, Action onfixEnd)
    {
        yield return new WaitForSeconds(FixPosDelayTime);
        Vector3 initRootPos = Root.anchoredPosition3D;
        float relativeDis = Root.parent.localPosition.x + CurtainsUiCtrl.Content.anchoredPosition3D.x;
        float distance = 0;
        float moveScrollDis = 0;

        switch (anim)
        {
            case MapCurtainsAnim.MoveInFromLeft:
                distance = -(relativeDis + FixDistance);
                moveScrollDis = DeviceUtility.DesignWidth - relativeDis;
                _needShowNormalBg = true;
                break;
            case MapCurtainsAnim.MoveInFromRight:
                distance = DeviceUtility.DesignWidth - relativeDis + FixDistance;
                moveScrollDis = -relativeDis;
                _needShowNormalBg = false;
                break;
        }

        float xSpeed = distance / duringTime;
        Vector3 xAxisVector3 = new Vector3(xSpeed, 0, 0);
        if (Mathf.Abs(xSpeed) > 0)
        {
            while (duringTime > 0)
            {
                Root.anchoredPosition3D += xAxisVector3 * Time.deltaTime;
                duringTime -= Time.deltaTime;
                yield return null;
            }

            CurtainsUiCtrl.Content.anchoredPosition3D += new Vector3(moveScrollDis, 0, 0);
            if (CurtainsUiCtrl.CurBoardTrans != null)
            {
                CurtainsUiCtrl.CurBoardTrans.anchoredPosition3D += new Vector3(-moveScrollDis, 0, 0);
            }
            Root.anchoredPosition3D = initRootPos + new Vector3(distance - moveScrollDis, 0, 0);
   
            if (onfixEnd != null)
            {
                onfixEnd();
            }
        }
    }

    void OnFixPosEnd()
    {
        MapMachineType type = _needShowNormalBg ? MapMachineType.Normal : MapMachineType.Tiny;
        CitrusEventManager.instance.Raise(new SwitchMachineTypeEvent(type));
    }
}
