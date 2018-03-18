using System.Collections;
using System.Collections.Generic;

public class WheelConfig
{
	private WheelSheet _sheet;
	private MachineConfig _machineConfig;
	private string _name;

	public float ProbSum { get; set; }

	public WheelSheet Sheet { get{ return _sheet; } }
	public WheelData[] DataArray { get { return _sheet.dataArray; } }
	public string Name { get { return _name; } }

	public WheelConfig(WheelSheet sheet, MachineConfig machineConfig, string wheelName) {
		_sheet = sheet;
		_machineConfig = machineConfig;
		_name = wheelName;

		Init ();
	}

	public void Init(){
		ProbSum = getProbSum ();
	}

	private float getProbSum(){
		float sum = ListUtility.FoldList(_sheet.dataArray, 0, (float s, WheelData data) => {
			return s + data.Prob;
		});
		return sum;
	}

	public WheelData GetFetchData(float fetch){
		float sum = 0;

		for (int i = 0; i < _sheet.dataArray.Length; ++i) {
			sum += _sheet.dataArray [i].Prob;
			if (fetch <= sum) {
				return _sheet.dataArray [i];
			}
		}
		CoreDebugUtility.Log ("no wheel data fetch can get , fetch is " + fetch);
		return null;
	}

	public int GetLayerCount()
	{
		return _sheet.dataArray[0].Ratio.Length;
	}
}
