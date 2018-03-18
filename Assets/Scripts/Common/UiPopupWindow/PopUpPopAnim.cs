using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PopUpPopAnim : MonoBehaviour
{
    public float LargenDuringTime = 0.1f;
    public float ResetDuringTime = 0.05f;
    public float InitScaleFactor = 0.1f;
    public float LargeScaleFactor = 1.05f;
    public Transform TargetTransform;

    private Vector3 _origScale;
    private Coroutine _popupAnimCoroutine;

    void Awake()
    {
        _origScale = TargetTransform.localScale;
    }

    void OnEnable()
    {
        if (TargetTransform != null)
        {
            _popupAnimCoroutine = StartCoroutine(PopUpAnim());
        }
    }

    void OnDisable()
    {
        if (_popupAnimCoroutine != null)
        {
            StopCoroutine(_popupAnimCoroutine);
        }
    }

    private IEnumerator PopUpAnim()
    {
        TargetTransform.localScale = InitScaleFactor * _origScale;
        TargetTransform.DOScale(LargeScaleFactor, LargenDuringTime);

        yield return new WaitForSeconds(LargenDuringTime);
        TargetTransform.DOScale(_origScale, ResetDuringTime);
    }
}
