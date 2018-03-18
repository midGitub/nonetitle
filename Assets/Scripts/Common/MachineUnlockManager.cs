using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class MachineUnlockManager {

    public static string  NewUnlockMachine = "";
	public MachineUnlockManager(){
		Init();
	}

	private void Init(){
		CitrusEventManager.instance.AddListener<MachineUnlockEvent>(HandleUnlock);
	}

	private void HandleUnlock(MachineUnlockEvent e){
		int level = (int)UserBasicData.Instance.UserLevel.Level;
		UpdateMachineUnlockSelectPosition(level);
	}

	// 升级时需要判断是否会触发解锁，如果触发需要更新下次进入大厅位置
	private void UpdateMachineUnlockSelectPosition(int level){
		// 该等级能够解锁的最高等级机台名
		string machine = MachineUnlockHelper.CheckHighestLevelUnlockMachine(level);
		// 本地是否已经解锁
		bool isUnlock = UserMachineData.Instance.IsMachineUnlock(machine);
		if (!isUnlock) {
		    NewUnlockMachine = machine;
		}
	}
}

