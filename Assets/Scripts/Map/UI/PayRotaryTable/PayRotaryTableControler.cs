using System.Collections;
using CitrusFramework;
using UnityEngine;

public class PayRotaryTableControler : RotaryTableController
{
    private bool _rotating;
    public Vector3 _defaultRotation;
    [SerializeField]
    private Animator _pRtAnimator;

    public ChangeGameObjectButton LightBar;

    public void StartRotate(PayRotaryTableData data)
    {
        AudioManager.Instance.PlaySound(AudioType.WheelTick);

        _pRtAnimator.SetTrigger("rotate");

        StartCoroutine(ControllerAnimation(data));
    }

    private float CalculateRotateAngle(PayRotaryTableData data)
    {
		float angle = 0;
        int id = data.BonusID;

        for (int i = 0; i < id; i++)
		{
		    if (i == 0)
		    {
		        angle += PayRotaryTableConfig.Instance.ListSheet[i].Angle/2;
		    }
		    else
		    {
                angle += PayRotaryTableConfig.Instance.ListSheet[i].Angle;
            }
		}
        if (id > 0)
        {
            angle += PayRotaryTableConfig.Instance.ListSheet[id].Angle / 2;
        }

        switch (_rotaryType)
        {
            case RotaryType.Clockwise:
                angle = 360 - angle;
                break;
            case RotaryType.AntiClockwise:
                break;
        }

        LogUtility.Log("PayRotaryModule: Result data Id : " + id + "   ratate angle : " + angle);
        return angle;
    }

    private IEnumerator ControllerAnimation(PayRotaryTableData data)
    {
        _rotating = true;
        yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, StartSpeed, MaxSpeed, ToMaxSpeedAngle, _Rotatedir, MinSpeed, _rotaryType));
        yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, MaxSpeed, MaxSpeed, CalculateRotateAngle(data), _Rotatedir, MinSpeed, _rotaryType));
        yield return StartCoroutine(RotateUtility.ARotateT(RotaryTransform, MaxSpeed, 0, RotateDownAnagle, _Rotatedir, MinSpeed, _rotaryType));
        yield return new WaitForSeconds(StopWaitTime);
        _rotating = false;

        WheelResultEffect(data.Angle > 20);
        AudioManager.Instance.PlaySound(AudioType.WheelSpin);
        yield return new WaitForSeconds(1);
        StartCoroutine(OnRotateEnd(data));
    }

    private void WheelResultEffect(bool isOpen)
    {
        LightBar.OpenGameObject.SetActive(isOpen);
        LightBar.CloseGameObject.SetActive(!isOpen);
    }

    private IEnumerator OnRotateEnd(PayRotaryTableData data)
    {
        _pRtAnimator.SetTrigger("out");

        //call OnPayRotaryTableOver event during "out" anim(1 seconds later), not wait for it over
        yield return new WaitForSeconds(1);

        CitrusEventManager.instance.Raise(new OnPayRotaryTableOver(data));
        UIManager.Instance.ClosePopup<PayRotaryTableUiControler>(UIManager.PayRotaryTableUiPath);
    }

    public void ResetRotation()
    {
        LogUtility.Log("_defaultRotation is " + _defaultRotation);
        transform.localRotation = Quaternion.Euler(_defaultRotation);
        //transform.localRotation = Quaternion.identity;
    }
}
