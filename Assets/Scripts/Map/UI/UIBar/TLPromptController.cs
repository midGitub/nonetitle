using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using CitrusFramework;

public class TLPromptController : MonoBehaviour {

    [SerializeField]
    private CountTimeUI _countTimeUI;	

    [SerializeField]
    private Text _discount;

    private bool _needToShowCountdown = false;

    public void Active(IAPCatalogData item)
    {
       // _discount.text = Mathf.Round((1f - (item.Price / item.OldPrice)) * 100f) + "%";
		_discount.text = item.Preferential;
        FillCountDownTime();
        
        _needToShowCountdown = true;
//        UnityTimer.Instance.WaitForFrame(0, () =>
//            {
                gameObject.SetActive(true);
//            });
    }

    public void Hide()
    {
        _needToShowCountdown = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        FillCountDownTime();
    }

    private void FillCountDownTime()
    {
        if (_needToShowCountdown)
        {
            TimeLimitedStoreHelper.IsInTLStorePeriod((bool arg1, TimeSpan arg2) => 
                {
                    if (arg1)
                    {
                        _countTimeUI.SetValue(arg2);
                    }
                }); 
        } 
    }

}
