using System.Collections;

public interface IRandomGenerator
{
	uint NextUInt();
	uint NextUInt(uint maxValue);
	int NextInt();
	double NextDouble();
	float NextFloat();
	float NextFloat(float min, float max);
}

