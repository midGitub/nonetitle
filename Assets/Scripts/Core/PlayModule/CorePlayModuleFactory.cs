using System.Collections;
using System.Collections.Generic;

public class PlayModuleFactory
{
	public static CorePlayModule CreatePlayModule(SmallGameState state, CoreMachine machine, ICoreGenerator generator)
	{
		//LogUtility.Log ("Create Core PlayModule  , name = " + state.ToString(), Color.green);
		switch (state)
		{
			case SmallGameState.None:
				return new CorePlayModuleNormal (state, machine, generator);
				break;
			case SmallGameState.FreeSpin:
				return new CorePlayModuleFreeSpin (state, machine, generator);
				break;
			case SmallGameState.Rewind:
				return new CorePlayModuleReWind (state, machine, generator);
				break;
			case SmallGameState.FixWild:
				return new CorePlayModuleFixWild (state, machine, generator);
				break;
			case SmallGameState.Wheel:
				return new CorePlayModuleWheel (state, machine, generator);
				break;
			case SmallGameState.Jackpot:
				return new CorePlayModuleJackpot (state, machine, generator);
				break;
			case SmallGameState.TapBox:
				{
					SmallGameMomentType type = machine.MachineConfig.BasicConfig.IsPuzzleTapBox ? SmallGameMomentType.Behind : SmallGameMomentType.Front;
					return new CorePlayModuleTapBox(state, machine, generator, type);
					break;
				}
			case SmallGameState.SwitchSymbol:
				return new CorePlayModuleSwitchSymbol(state, machine, generator);
				break;
			default:
				CoreDebugUtility.Assert(false, "Create CorePlayModule failed: " + state.ToString());
				break;
		}
		return null;
	}
}