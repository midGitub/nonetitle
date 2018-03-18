using CitrusFramework;

public class DiceGameStartEvent : CitrusGameEvent
{
	public int Result;
	public int DiceCount;
	public float DelayTime;

	public DiceGameStartEvent(int result, int diceCount, float delayTime)
	{
		Result = result;
		DiceCount = diceCount;
		DelayTime = delayTime;
	}
}
