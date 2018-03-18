using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommentStarUI : MonoBehaviour {

    public Button starButton;
    public GameObject grayStar;
    public GameObject goldenStar;
    public GameObject ratingMachineHitEffect;

    public void Start()
    {
        starButton.onClick.AddListener(EffectStart);
    }

    public void EffectStart()
    {
        ratingMachineHitEffect.SetActive(false);
        ratingMachineHitEffect.SetActive(true);
    }

    public void LightStar()
    {
        goldenStar.SetActive(true);
    }

    public void LightOffStar()
    {
        goldenStar.SetActive(false);
    }
}
