using System;
using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.EventSystems;

public class VipCurtainUiController : MonoBehaviour
{
    public Animator AnimCtrl;
    public float ChangeBgDelayTime;
    public GameObject InputMask;

    private readonly string CurtainShowAnimName = "play";
	void Start ()
    {
        EnableInputMask(false);
		CitrusEventManager.instance.AddListener<VipCurtainAnimPlayEvent>(PlayCurtainAnim);
	}

    void OnDestroy()
    {
        CitrusEventManager.instance.RemoveListener<VipCurtainAnimPlayEvent>(PlayCurtainAnim);
    }

    void PlayCurtainAnim(VipCurtainAnimPlayEvent e)
    {
        AnimCtrl.SetTrigger(CurtainShowAnimName);
        EnableInputMask(true);
        StartCoroutine(WaitForAnimEnd(CurtainShowAnimName, () => EnableInputMask(false)));
        UnityTimer.Start(this, ChangeBgDelayTime, e.OnCurtainCoverScreen);
    }

    private IEnumerator WaitForAnimEnd(string animName, Action onAnimEnd)
    {
        AnimatorStateInfo stateInfo = AnimCtrl.GetCurrentAnimatorStateInfo(0);

        while (!stateInfo.IsName(animName))
        {
            stateInfo = AnimCtrl.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        yield return new WaitForSeconds(stateInfo.length);

        if (onAnimEnd != null)
        {
            onAnimEnd.Invoke();
        }
    }

    void EnableInputMask(bool enable)
    {
        InputMask.SetActive(enable);
    }
}
