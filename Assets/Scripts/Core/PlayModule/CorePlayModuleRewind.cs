using System.Collections;
using System.Collections.Generic;

public class CorePlayModuleReWind : CorePlayModule
{
	private int _respinCount;

	public CorePlayModuleReWind(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
		: base(state, machine, generator){
		_respinCount = 0;
		_momentType = SmallGameMomentType.Front;
	}

	public override CoreSpinResult SpinHandler (CoreSpinInput spinInput)
	{
		CoreSpinResult result;
		result = _coreMachine.SpinResult;
		result.IsReversedSpin = true;
		return result;
	}

	public override bool IsTriggerSmallGameState(){
		return _coreMachine.LastSmallGameState == SmallGameState.None
			&& _coreMachine.SmallGameState == SmallGameState.Rewind;
	}

	public override bool ShouldRespin ()
	{
		return true;
	}
	protected override bool CheckSwitchSmallGameStateFront(CoreSpinResult spinResult){
		_coreMachine.SaveLastSmallGameState ();

		CoreDebugUtility.Log ("Rewind respinCount is "+_respinCount);
		if (spinResult.Type == SpinResultType.Win) {
			float[] rewindProbs = spinResult.PayoutData.RewindHits;

			if (!CheckTriggerRewind (rewindProbs, _respinCount)) {
				_coreMachine.ChangeSmallGameState (SmallGameState.None);
			}
		} 

		return _coreMachine.LastSmallGameState != _coreMachine.SmallGameState;
	}

	protected override bool CheckSwitchSmallGameStateBehind(CoreSpinResult spinResult){
		return false;
	}

	private bool CheckTriggerRewind(float[] rewindProbs, int respinCount)
	{
		int respinIndex = (respinCount < rewindProbs.Length) ? respinCount : (rewindProbs.Length - 1);
		float prob = rewindProbs[respinIndex];
		RollHelper helper = new RollHelper(prob);
		int index = helper.RollIndex(_coreMachine.Roller);
		bool result = index == 0;
		return result;
	}

	private void ClearRespinCount(){
		_respinCount = 0;
	}

	private void IncreaseRespinCount(){
		++_respinCount;
	}

	public override void Enter ()
	{
	}

	public override void Exit ()
	{
	}

	public override void TryStartRound(){
		ClearRespinCount ();
	}

	public override void StartRespin(){
		IncreaseRespinCount ();
	}
}