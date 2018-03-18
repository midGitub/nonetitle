using System.Collections;
using System.Collections.Generic;

public class PayoutConfig : BaseJoyConfig
{
	public static readonly string Name = "Payout";
	public static readonly string LuckySheetName = "LuckyPayout";

	private PayoutSheet _sheet;
	private float[] _freeSpinOverallHitArray;
	private float[] _freeSpinStopOverallHitArray;
	// zhousen fixWild模式用
	private float[] _fix1ReelOverallHitArray;
	private float[] _fix2ReelOverallHitArray;
	private float _totalFreeSpinProb;

	public PayoutSheet Sheet { get { return _sheet; } }
	public float[] FreeSpinOverallHitArray { get { return _freeSpinOverallHitArray; } }
	public float[] FreeSpinStopOverallHitArray { get { return _freeSpinStopOverallHitArray; } }
	public float[] Fix1ReelOverallHitArray { get { return _fix1ReelOverallHitArray; } }
	public float[] Fix2ReelOverallHitArray { get { return _fix2ReelOverallHitArray; } }
	public float TotalFreeSpinProb { get { return _totalFreeSpinProb; } }

	public PayoutConfig(PayoutSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;

		base.Init(machineConfig, _sheet.dataArray);
		InitFreeSpinOverallHitArray();
		InitTotalFreeSpinProb();

		#if UNITY_EDITOR
		DebugVerifyData();
		#endif
	}

	private void InitFreeSpinOverallHitArray()
	{
		_freeSpinOverallHitArray = new float[_sheet.dataArray.Length];
		_freeSpinStopOverallHitArray = new float[_sheet.dataArray.Length];
		// zhousen
		_fix1ReelOverallHitArray = new float[_sheet.dataArray.Length];
		_fix2ReelOverallHitArray = new float[_sheet.dataArray.Length];
		for(int i = 0; i < _sheet.dataArray.Length; i++)
		{
			PayoutData data = _sheet.dataArray[i];
			_freeSpinOverallHitArray[i] = data.FreeSpinOverallHit;
			_freeSpinStopOverallHitArray[i] = data.FreeSpinStopOverallHit;
			_fix1ReelOverallHitArray [i] = data.Fix1ReelOverallHit;
			_fix2ReelOverallHitArray [i] = data.Fix2ReelOverallHit;
		}
	}

	private void InitTotalFreeSpinProb()
	{
		_totalFreeSpinProb = 0.0f;
		for(int i = 0; i < _sheet.dataArray.Length; i++)
		{
			PayoutData data = _sheet.dataArray[i];
			_totalFreeSpinProb += data.FreeSpinOverallHit;
		}
	}

	private void DebugVerifyData()
	{
		PayoutData []dataArray = _sheet.dataArray;
		for(int i = 0; i < dataArray.Length; i++)
		{
			PayoutData data = dataArray[i];
			if(data.PayoutType == PayoutType.Ordered)
			{
				CoreDebugUtility.Assert(data.IsFixed, "PayoutType.Ordered should set isFixed TRUE");
			}
			else if(data.PayoutType == PayoutType.All)
			{
				CoreDebugUtility.Assert(data.Symbols.Length == 1, "PayoutType.All should have only one symbol");
				CoreDebugUtility.Assert(!data.IsFixed, "PayoutType.All should set isFixed FALSE");
			}
		}
	}
}
