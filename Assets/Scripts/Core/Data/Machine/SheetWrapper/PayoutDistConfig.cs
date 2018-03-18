using System.Collections;
using System.Collections.Generic;

public class PayoutDistConfig : BaseJoyDistConfig
{
	public static readonly string Name = "PayoutDist";
	public static readonly string LuckySheetName = "LuckyPayoutDist";

	PayoutDistSheet _sheet;
	public PayoutDistSheet Sheet { get { return _sheet; } }

	float[] _freeSpinOverallHitArray;
	float _freeSpinTotalProb;

	public float[] FreeSpinOverallHitArray { get { return _freeSpinOverallHitArray; } }
	public float FreeSpinTotalProb { get { return _freeSpinTotalProb; } }

	public PayoutDistConfig(PayoutDistSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;

		base.Init(machineConfig, _sheet.dataArray);

		InitFreeSpinOverallHitArray();
		InitFreeSpinTotalProb();
	}

	private void InitFreeSpinOverallHitArray()
	{
		_freeSpinOverallHitArray = new float[_sheet.DataArray.Length];
		for(int i = 0; i < _sheet.DataArray.Length; i++)
		{
			PayoutDistData data = _sheet.DataArray[i];
			_freeSpinOverallHitArray[i] = data.FreeSpinOverallHit;
		}
	}

	private void InitFreeSpinTotalProb()
	{
		_freeSpinTotalProb = 0.0f;
		for(int i = 0; i < _sheet.DataArray.Length; i++)
		{
			PayoutDistData data = _sheet.DataArray[i];
			_freeSpinTotalProb += data.FreeSpinOverallHit;
		}
	}
}
