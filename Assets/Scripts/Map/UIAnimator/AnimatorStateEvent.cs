using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorStateEvent : StateMachineBehaviour
{
	public UnityEvent OnStateEnterEvent = new UnityEvent();

	public UnityEvent OnStateExitEvent = new UnityEvent();

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		OnStateEnterEvent.Invoke();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, UnityEngine.Experimental.Director.AnimatorControllerPlayable controller)
	{
		base.OnStateExit(animator, stateInfo, layerIndex, controller);
		OnStateExitEvent.Invoke();
	}
}
