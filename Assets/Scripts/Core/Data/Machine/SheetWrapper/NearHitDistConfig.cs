using System.Collections;
using System.Collections.Generic;

public class NearHitDistConfig : BaseJoyDistConfig
{
	public static readonly string Name = "NearHitDist";
	public static readonly string LuckySheetName = "LuckyNearHitDist";

	private NearHitDistSheet _sheet;
	public NearHitDistSheet Sheet { get { return _sheet; } }

	public NearHitDistConfig(NearHitDistSheet sheet, MachineConfig machineConfig)
	{
		_sheet = sheet;

		base.Init(machineConfig, _sheet.dataArray);
	}
}
