using System.Collections;
using System.Collections.Generic;

public class NearHitConfig : BaseJoyConfig
{
	public static readonly string Name = "NearHit";
	public static readonly string LuckySheetName = "LuckyNearHit";

	private NearHitSheet _sheet;
	public NearHitSheet Sheet { get { return _sheet; } }

	// zhousen fixWild模式用
	private float[] _fix1ReelOverallHitArray;
	private float[] _fix2ReelOverallHitArray;
	public float[] Fix1ReelOverallHitArray { get { return _fix1ReelOverallHitArray; } }
	public float[] Fix2ReelOverallHitArray { get { return _fix2ReelOverallHitArray; } }

	public NearHitConfig(NearHitSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;

		base.Init(machineConfig, _sheet.dataArray);
		InitFixReelOverallHitArray ();

		#if UNITY_EDITOR
		DebugVerifyData();
		#endif
	}

	private void DebugVerifyData()
	{
	}

	private void InitFixReelOverallHitArray()
	{
		// zhousen
		_fix1ReelOverallHitArray = new float[_sheet.dataArray.Length];
		_fix2ReelOverallHitArray = new float[_sheet.dataArray.Length];
		for(int i = 0; i < _sheet.dataArray.Length; i++)
		{
			NearHitData data = _sheet.dataArray[i];
			_fix1ReelOverallHitArray [i] = data.Fix1ReelOverallHit;
			_fix2ReelOverallHitArray [i] = data.Fix2ReelOverallHit;
		}
	}

	public List<int> GetNeighborOffsets(NearHitData data){
		List<int> resultList = new List<int> ();
		if (data != null) {
			if (data.MultiLine) {
				for (int i = 1; i < CoreDefine.PaylineHorizonCount; ++i) {
					resultList.AddRange (new List<int>(){-i, i});
				}
			} else {
				resultList = new List<int> (){ -1, 1 };
			}
		}

		return resultList;
	}
}

