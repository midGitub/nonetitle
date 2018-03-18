using System.Collections;

public class LCG : IRandomGenerator
{
	public delegate void SaveSeedDelegate(uint seed);

	// LCG algorithm:
	// X(n) = (A * X(n-1) + C) % M
	// A: multiplier
	// C: increment
	// M: modulus
	// The value of A, C and M is the same as glibc and Ansi C
	private const int _multiplier = 1103515245;
	private const int _increment = 12345;
	private const uint _modulus = uint.MaxValue;
	private SaveSeedDelegate _saveSeedCallback;

	public uint Seed { get; private set; }
	public uint LastValue { get; set; }

	public LCG(uint seed, SaveSeedDelegate callback)
	{
		this.Seed = seed;
		this.LastValue = seed;
		_saveSeedCallback = callback;
	}

	private void TrySaveSeed(uint seed)
	{
		if(_saveSeedCallback != null)
			_saveSeedCallback(seed);
	}

	public uint NextUInt()
	{
		uint result = (_multiplier * this.LastValue + _increment) % _modulus;
		this.LastValue = result;
		TrySaveSeed(result);
		return result;
	}

	public uint NextUInt(uint maxValue)
	{
		uint result = NextUInt() % maxValue;
		return result;
	}

	public int NextInt()
	{
		int result = (int)(NextUInt() % (uint)int.MaxValue);
		return result;
	}

	public double NextDouble()
	{
		double result = (double)NextUInt() / (double)uint.MaxValue;
		return result;
	}

	public float NextFloat()
	{
		float result = (float)NextUInt() / (float)uint.MaxValue;
		return result;
	}

	public float NextFloat(float min, float max){
		float delta = max - min;
		float ratio = (float)NextUInt() / (float)uint.MaxValue;
		float result = min + ratio * delta;
		return result;
	}
}
