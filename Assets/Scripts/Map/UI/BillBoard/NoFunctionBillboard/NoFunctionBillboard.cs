
using UnityEngine;

public class NoFunctionBillboard : SimpleSingleton<NoFunctionBillboard> {

    public void AddBillboards()
    {
        if (MapSettingConfig.Instance.IsVipRoomEnable)
        {
            GameObject vipMachine = UGUIUtility.InstantiateUI(UIManager.VipMachineBillboardPath);
            vipMachine.SetActive(false);
            VipMachineBillboard billboard = vipMachine.GetComponent<VipMachineBillboard>();
            BillBoardManager.Instance.Add(billboard);
        }
    }
}
