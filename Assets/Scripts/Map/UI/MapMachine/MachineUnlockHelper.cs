using System.Collections;
using System.Collections.Generic;

public static class MachineUnlockHelper  {

	public static bool CheckMachineUnlock(string machineName)
	{
		bool unlock = false;

		int unlockLevel = MachineUnlockSettingConfig.Instance.GetUnlockLevel(machineName);
		int userLevel = (int)UserBasicData.Instance.UserLevel.Level;
	    int unlockVipLevel = MachineUnlockSettingConfig.Instance.GetUnlockVipLevel(machineName);
	    int userVipLevel = VIPConfig.Instance.GetPointAboutVIPLevel(UserBasicData.Instance.VIPPoint);

        #if UNITY_IOS
		bool isNewer = true;
#else
        bool isNewer = !LiveUpdateManager.Instance.HasResourceVersionFolder("1.3.*");
#endif

        bool isVersion1_3 = ListUtility.IsAnyElementSatisfied(MachineUnlockSettingConfig.Instance.AllMachineNameVersion1_3
            /*CoreDefine.AllMachineNamesVersion1_3*/, (string machine) => {
                                                        return machine.Equals(machineName);
                                                    });

        if (!isNewer && isVersion1_3)
        {
            unlock = true;
        }
        else
        {
            unlock = MachineUnlockSettingConfig.Instance.IsVipMachine(machineName)
                ? UserBasicData.Instance.IsAllVipMachineUnlock || userVipLevel >= unlockVipLevel
                : userLevel >= unlockLevel;
        }

        return unlock;
	}

	public static string CheckHighestLevelUnlockMachine(int level){
		int highestLv = 0;
		string machine = "";
		ListUtility.ForEach (CoreDefine.AllMachineNames, (string s) => {
			int lv = MachineUnlockSettingConfig.Instance.GetUnlockLevel(s);
		    bool isVipMachine = MachineUnlockSettingConfig.Instance.IsVipMachine(s);
			if (!isVipMachine && level >= lv) {
				if (highestLv < lv) {
					highestLv = lv;
					machine = s;
				}
			}
		});

		return machine;
	}

    public static List<string> NewUnlockVipMachineList(int vipLv)
    {
        List<string> result = new List<string>();
        ListUtility.ForEach(CoreDefine.AllMachineNames, (string s) => {
            bool isVipMachine = MachineUnlockSettingConfig.Instance.IsVipMachine(s);
            if (isVipMachine && !UserMachineData.Instance.IsMachineUnlock(s))
            {
                int unlockVipLv = MachineUnlockSettingConfig.Instance.GetUnlockVipLevel(s);
                if (vipLv >= unlockVipLv)
                    result.Add(s);
            }           
        });

        return result;
    }
}
