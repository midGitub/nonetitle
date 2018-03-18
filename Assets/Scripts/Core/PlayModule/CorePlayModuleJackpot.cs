using System.Collections;
using System.Collections.Generic;

public class CorePlayModuleJackpot : CorePlayModule
{
	public CorePlayModuleJackpot(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
		: base(state, machine, generator){
		_momentType = SmallGameMomentType.Front;
	}

	public override bool IsTriggerSmallGameState(){
		return _coreMachine.LastSmallGameState == SmallGameState.None
			&& _coreMachine.SmallGameState == SmallGameState.Jackpot;
	}
}
