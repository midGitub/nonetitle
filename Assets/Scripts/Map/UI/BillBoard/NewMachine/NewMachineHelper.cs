using CitrusFramework;
using UnityEngine;
public class NewMachineHelper :SimpleSingleton<NewMachineHelper>{
	public void AddNewMachineBoard()
	{
		string machinestring =  MapSettingConfig.Instance.Read("NewMachine", null);
		string machinename = MapSettingConfig.Instance.Read("NewMachineName",null);
		string machineposition = MapSettingConfig.Instance.Read("NewMachinePosition", null);
        if (!machinename.IsNullOrEmpty() && !machinestring.IsNullOrEmpty() && !machineposition.IsNullOrEmpty())
        {
            string[] machinelist = machinestring.Split(new char[] { ',' });
            string[] machinenamelist = machinename.Split(new char[] { ',' });
            for (int i = 0; i < machinelist.Length; i++)
            {
                string str = machinelist[i];
                Sprite sprite = Resources.Load("Images/UI/NewMachine/" + str, typeof(Sprite)) as Sprite;

                if (sprite != null)
                {
                    GameObject gb = GameObject.Find("MapScene").GetComponent<MapScene>().CustomRoomScrollRect;
                    Transform contentTrans = gb.transform.Find("Viewport").Find("Content");

                    GameObject machine = contentTrans.Find("MapMachine_" + machinenamelist[i] + "(Clone)").gameObject;
                    GameObject gameobject = machine.transform.Find("GameObject").gameObject;
                    GameObject button = gameobject.transform.Find("Button").gameObject;
                    MachineButton machinebutton = button.GetComponent<MachineButton>();

                    GameObject machineboardgameobject = UGUIUtility.InstantiateUI("Game/UI/BillBoard/NewMachine");
                    machineboardgameobject.SetActive(false);
                    NewMachine newmachine = machineboardgameobject.GetComponent<NewMachine>();
                    newmachine.MachineName = machinenamelist[i];
                    newmachine.ThisButton.image.sprite = sprite;
                    newmachine.ThisButton.onClick.AddListener(() =>
                    {
                        CitrusEventManager.instance.Raise(new AskSlideToMachinePosEvent(newmachine.MachineName,
                            () => { machinebutton.OnPointerClick(null); }, false, true));
                    });
                    BillBoardManager.Instance.Add(newmachine);
                }
            }
        }
    }
}
