using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PayRotaryTableConfig : SimpleSingleton<PayRotaryTableConfig> {

	public static readonly string Name = "PayRotaryTable";

    private PayRotaryTableSheet _sheet;
    public List<PayRotaryTableData> ListSheet { private set; get; }

    public PayRotaryTableConfig()
    {
        LoadData();
    }

    private void LoadData()
    {
		_sheet = GameConfig.Instance.LoadExcelAsset<PayRotaryTableSheet>(Name);
        ListSheet = _sheet.dataArray.ToList();
    }

    public static void Reload()
    {
        Debug.Log("Reload PayRotaryTableConfig");
        Instance.LoadData();
    }
}
