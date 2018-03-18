using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class RotaryAnimatorContoller : MonoBehaviour
{
	public Animator RotartAnimator;
	public Animator BounsAnimator;
	public Animator TextBoxAnimator;
    public Animator BackgroundAnimator;
	public RotaryTableController RotaryTableCont;
	public DayBonus DayBon;
	public Text CoinsText;
	public float TextAnimatorTime = 2;
	public float CloseDailyBounsWaitTime = 2;

	public ChangeGameObjectButton LightBar;

	private AnimatorStateEvent _animatorStateEvnet;

	private void Start()
	{
		RotaryTableCont.RotateFinisedEvent.AddListener(PlayUpAnimation);
        CitrusEventManager.instance.AddListener<OnPayRotaryTableOver>(StringAnimationPlay);
	}

	private void OnDestroy()
	{
		RotaryTableCont.RotateFinisedEvent.RemoveListener(PlayUpAnimation);
        CitrusEventManager.instance.RemoveListener<OnPayRotaryTableOver>(StringAnimationPlay);
    }

	void OnEnable(){
		_animatorStateEvnet = RotartAnimator.GetBehaviour<AnimatorStateEvent>();
		_animatorStateEvnet.OnStateExitEvent.AddListener(OnRotateAnimEnd);
	}

	void OnDisable(){
		_animatorStateEvnet.OnStateExitEvent.RemoveListener(OnRotateAnimEnd);
	}

	public void PlayUpAnimation()
	{
		AudioManager.Instance.StopSound(AudioType.WheelTick);
		AudioManager.Instance.PlaySound(AudioType.WheelSpin);
		LightBar.UpdateButtonState(DayBonus.Instance.NowDailyBonusData.Angle > 20);
		RotartAnimator.SetBool("Play", true);
	    StartCoroutine(PlayHideTitleAnim());
	}

    private IEnumerator PlayHideTitleAnim()
    {
        //play anim 1.4 seconds later
        yield return new WaitForSeconds(1.4f);
        BackgroundAnimator.SetTrigger("out");
    }

    private void OnRotateAnimEnd()
    {
        CitrusEventManager.instance.Raise(new OnDailyRotaryAnimEnd());
    }

    public void StringAnimationPlay(OnPayRotaryTableOver result)
	{
        SetPayRotaryTabelData(result);

	    if (result != null)
	    {
            BackgroundAnimator.SetTrigger("in");
        }
		StartCoroutine(BonusAnimation());
	}

    private void SetPayRotaryTabelData(OnPayRotaryTableOver result)
    {
        DayBon.PayRotaryTableData = result != null ? result.Result : null;
        DayBon.GetBonus();
    }

    private IEnumerator BonusAnimation()
	{
		DayBonusTypeInfor di = null;
		DayBonus.Instance.UpdateDaysTypeState((a) => { di = a; });
		AudioManager.Instance.PlaySound(AudioType.Calendar);
		var dtAn = di.DayType.OpenGameObject.GetComponent<Animator>();
		var clipinfor = dtAn.GetCurrentAnimatorStateInfo(0);
		yield return new WaitForSeconds(clipinfor.length);
		BounsAnimator.SetBool("wait", true);
		while(true)
		{
			if(BounsAnimator.GetCurrentAnimatorStateInfo(0).IsName("bonus"))
			{
				break;
			}
			yield return new WaitForEndOfFrame();
		}

        //4 means four ui texts need play sound
	    for (int i = 0; i < 4; i++)
	    {
	        float delayTime = 0.4f * i;
            CitrusFramework.UnityTimer.Instance.StartTimer
                       (this, delayTime,
                        () => { AudioManager.Instance.PlaySound(AudioType.DailyBonus); });
        }

		yield return new WaitForSeconds(BounsAnimator.GetCurrentAnimatorStateInfo(0).length);
		TextBoxAnimator.SetBool("wait", true);
		AudioManager.Instance.PlaySound(AudioType.DailyBonusCreditsRollUp);
		StartCoroutine(TextExtension.IETickNumber(0, (ulong)DayBon.BonusCoins, TextAnimatorTime, (obj) => { CoinsText.text = StringUtility.FormatNumberStringWithComma(obj); }));

		yield return new WaitForSeconds(TextAnimatorTime + CloseDailyBounsWaitTime);
		AudioManager.Instance.StopSound(AudioType.DailyBonusCreditsRollUp);
        DayBonus.Instance.Hide();
		yield return null;
	}
}
