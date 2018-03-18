using System.Collections;
using System.Collections.Generic;

public class BaseJoyDistConfig
{
	protected MachineConfig _machineConfig; //ref
	protected float[] _overallHitArray;
	protected float _totalProb;
	protected IJoyDistData[] _joyDistDataArray;

	public float[] OverallHitArray { get { return _overallHitArray; } }
	public float TotalProb { get { return _totalProb; } }
	public IJoyDistData[] JoyDataArray { get { return _joyDistDataArray; } }

	protected void Init(MachineConfig machineConfig, IJoyDistData[] dataArray)
	{
		_machineConfig = machineConfig;
		_joyDistDataArray = dataArray;

		InitOverallHitArray();
		InitTotalProb();
	}

	private void InitOverallHitArray()
	{
		_overallHitArray = new float[_joyDistDataArray.Length];
		for(int i = 0; i < _joyDistDataArray.Length; i++)
		{
			IJoyDistData data = _joyDistDataArray[i];
			_overallHitArray[i] = data.OverallHit;
		}
	}

	private void InitTotalProb()
	{
		_totalProb = 0.0f;
		for(int i = 0; i < _joyDistDataArray.Length; i++)
		{
			IJoyDistData data = _joyDistDataArray[i];
			_totalProb += data.OverallHit;
		}
	}
}
