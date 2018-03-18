using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class MapEffect : MonoBehaviour {

    [SerializeField]
    private GameObject CanCollectionCoinsEffect;

    [SerializeField]
    private Animator CollectioningCoinsEffectAnimator;

    void OnEnable()
    {
        CitrusEventManager.instance.AddListener<CollectingCoinsEffectEvent>(OnEffectEvent);
    }

    void OnDisable()
    {
        CitrusEventManager.instance.RemoveListener<CollectingCoinsEffectEvent>(OnEffectEvent);
    }

    private void OnEffectEvent(CollectingCoinsEffectEvent e)
    {
        ShowCollectioningCoinsEffect();
    }

    public void ShowCollectioningCoinsEffect()
    {
        AudioManager.Instance.PlaySound(AudioType.HourlyBonusCreditsRollUp);
        ShowCoinsText.ChangeTextAnimationTime(CollectioningCoinsEffectAnimator.GetCurrentAnimatorStateInfo(0).length + 2);
        CollectioningCoinsEffectAnimator.gameObject.SetActive(true);
        CitrusFramework.UnityTimer.Instance.StartTimer(this, 
            CollectioningCoinsEffectAnimator.GetCurrentAnimatorStateInfo(0).length,
            () => {
            CollectioningCoinsEffectAnimator.gameObject.SetActive(false);
        });
    }
}
