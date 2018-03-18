using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWaitTimePlay : StateMachineBehaviour 
{
	public float _RandomWaitSecond = 2;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		animator.speed = 0;
		CitrusFramework.UnityTimer.Instance.StartTimer(Random.Range(0, _RandomWaitSecond), () => { animator.speed = 1; });
	}
}
