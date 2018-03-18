using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AnnunciationUiController : MonoBehaviour
{
    public Canvas CanvasComponent;
    public Text AnnunciationText;
    public Image AnnunciationUiBg;
    public RectTransform InputField;
    public float ScrollDuringTime;
    public float AnnunciateIntervalTime;
    public float UiFadeDuringTime;

    private Queue<string> _annunciationQueue = new Queue<string>();
    private bool _isAnnuncaiting;
    private WaitForSeconds _waitForAnnunciateOverTime;
    private Coroutine _labelMoveAnimCoroutine;

    private void Start()
    {
        CanvasComponent.worldCamera = Camera.main;
        _waitForAnnunciateOverTime = new WaitForSeconds(ScrollDuringTime + AnnunciateIntervalTime);
        CitrusEventManager.instance.AddListener<AnnunciationShowEvent>(UpdateAnnunciationQueue);
    }

    private void OnDestroy()
    {
        CitrusEventManager.instance.RemoveListener<AnnunciationShowEvent>(UpdateAnnunciationQueue);
    }

    private void UpdateAnnunciationQueue(AnnunciationShowEvent e)
    {
        List<string> content = e.AnnunciationList;
        if (content != null && content.Count > 0)
        {
            ListUtility.ForEach(content, s => _annunciationQueue.Enqueue(s));

            if (!_isAnnuncaiting)
            {
                AnnunciationUiBg.DOFade(1, UiFadeDuringTime);
                AnnunciationText.DOFade(1, UiFadeDuringTime);
                StartCoroutine(HandleAnnunciateQueue());
                _isAnnuncaiting = true;
            }
        }
    }

    private IEnumerator HandleAnnunciateQueue()
    {
        while (_annunciationQueue.Count > 0)
        {
            PlayAnnunciation(_annunciationQueue.Dequeue());
            yield return _waitForAnnunciateOverTime;
        }

        OnAllAnnunciationsHandled();
    }

    private void PlayAnnunciation(string text)
    {
        AnnunciationText.text = text;
        if (_labelMoveAnimCoroutine != null)
        {
            StopCoroutine(_labelMoveAnimCoroutine);
        }
        _labelMoveAnimCoroutine = StartCoroutine(MoveLabelAnim(ScrollDuringTime));
    }

    private IEnumerator MoveLabelAnim(float duringTime)
    {
        AnnunciationText.rectTransform.anchoredPosition3D = Vector3.zero;
        float speedX = (AnnunciationText.rectTransform.sizeDelta.x + InputField.sizeDelta.x)/ScrollDuringTime;
        Vector3 speedPerFrame = new Vector3(-speedX, 0, 0);
        while (duringTime > 0)
        {
            AnnunciationText.rectTransform.anchoredPosition3D += speedPerFrame * Time.deltaTime;
            duringTime -= Time.deltaTime;
            yield return null;
        }
    }

    private void OnAllAnnunciationsHandled()
    {
        _isAnnuncaiting = false;
        AnnunciationUiBg.DOFade(0, UiFadeDuringTime);
        AnnunciationText.DOFade(0, UiFadeDuringTime);
    }
}
