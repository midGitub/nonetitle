using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MachineTestIndieGameManager
{
	CoreMachine _machine;

	public MachineTestIndieGameManager(CoreMachine machine)
	{
		_machine = machine;

		InitTapBox();
		InitWheel();
	}

	public MachineTestIndieGameResult Run(MachineTestInput input)
	{
		MachineTestIndieGameResult result = null;
		SmallGameState state = _machine.SmallGameState;

		//add more small game handlers here
		if(state == SmallGameState.TapBox)
			result = RunTapBox(input);
		else if (state == SmallGameState.Wheel)
			result = RunWheel(input);
		
		return result;
	}

	#region TapBox

	void InitTapBox()
	{
		//ensure the game start from ground
		if(_machine.MachineConfig.BasicConfig.HasTapBox)
			UserMachineData.Instance.ClearBetCollectNumDict(_machine.Name);
	}

	MachineTestIndieGameResult RunTapBox(MachineTestInput input)
	{
		MachineTestIndieGameResult result = _machine.TapBox.SimulateUserPlay(input._betAmount);

		_machine.ChangeSmallGameState(SmallGameState.None);

		return result;
	}

	#endregion

	#region wheel
	void InitWheel(){}

	MachineTestIndieGameResult RunWheel(MachineTestInput input){
		MachineTestIndieGameResult result = _machine.WheelGame.SimulateUserPlay(input._betAmount);

		_machine.ChangeSmallGameState(SmallGameState.None);

		return result;
	}
	#endregion
}

