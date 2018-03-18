using UnityEngine;
using System.Collections.Generic;

public class ReloadInfo
{
	public delegate bool ShouldReloadDelegate();
	public delegate void ReloadDelegate();

	public ShouldReloadDelegate _shouldReloadHandler;
	public ReloadDelegate _reloadHandler;

	public ReloadInfo(ShouldReloadDelegate shouldReloadHandler, ReloadDelegate reloadHandler)
	{
		_shouldReloadHandler = shouldReloadHandler;
		_reloadHandler = reloadHandler;
	}
}

public static class GameConfigTable
{
	public static readonly string SubDir = "Game/";

	//Note: when add one new excel sheet in GameConfig, don't forget to add one item here
	public static readonly ExcelSheetInfo[] SheetInfos = new ExcelSheetInfo[] {
		new ExcelSheetInfo(ADSConfig.Name, typeof(ADSData), typeof(ADSSheet)),
		new ExcelSheetInfo(DailyBonusConfig.Name, typeof(DailyBonusData), typeof(DailyBonusSheet)),
		new ExcelSheetInfo(FriendSettingConfig.Name, typeof(FriendSettingData), typeof(FriendSettingSheet)),
		new ExcelSheetInfo(IAPCatalogConfig.Name, typeof(IAPCatalogData), typeof(IAPCatalogSheet)),
		new ExcelSheetInfo(UserLevelConfig.Name, typeof(LevelConfigData), typeof(LevelConfigSheet)),
		new ExcelSheetInfo(MachineUnlockSettingConfig.Name, typeof(MachineUnlockSettingData), typeof(MachineUnlockSettingSheet)),
		new ExcelSheetInfo(BankruptCompensateConfig.Name, typeof(BankruptCompensateData), typeof(BankruptCompensateSheet)),
		new ExcelSheetInfo(MailTextConfig.Name, typeof(MailTextData), typeof(MailTextSheet)),
		new ExcelSheetInfo(MapSettingConfig.Name, typeof(MapSettingData), typeof(MapSettingSheet)),
		new ExcelSheetInfo(PiggyBankConfig.Name, typeof(PiggyBankData), typeof(PiggyBankSheet)),
		new ExcelSheetInfo(PiggyInfoConfig.Name, typeof(PiggyInfoData), typeof(PiggyInfoSheet)),
		new ExcelSheetInfo(VIPConfig.Name, typeof(VIPData), typeof(VIPSheet)),
		new ExcelSheetInfo(RemoteMachineVersionConfig.Name, typeof(RemoteMachineVersionData), typeof(RemoteMachineVersionSheet)),
		new ExcelSheetInfo(DiceConfig.Name, typeof(DiceData), typeof(DiceSheet)),
		new ExcelSheetInfo(BetUnlockSettingConfig.Name, typeof(BetUnlockSettingData), typeof(BetUnlockSettingSheet)),
		new ExcelSheetInfo(PayRotaryTableConfig.Name, typeof(PayRotaryTableData), typeof(PayRotaryTableSheet)),
		new ExcelSheetInfo(GroupConfig.Name, typeof(GroupData), typeof(GroupSheet)),
		new ExcelSheetInfo(BackFlowRewardLTLuckyConfig.Name, typeof(BackFlowRewardLTLuckyData), typeof(BackFlowRewardLTLuckySheet)),
		new ExcelSheetInfo(GroupRuleConfig.Name, typeof(GroupRuleData), typeof(GroupRuleSheet)),
		new ExcelSheetInfo(ActiveGroupRuleConfig.Name, typeof(GroupRule2Data), typeof(GroupRule2Sheet)),
		new ExcelSheetInfo(GroupRepresentConfig.Name, typeof(GroupRepresentData), typeof(GroupRepresentSheet)),
		new ExcelSheetInfo(AdBonusConfig.Name, typeof(AdBonusData), typeof(AdBonusSheet)),
		new ExcelSheetInfo(BetOptionConfig.Name, typeof(BetOptionData), typeof(BetOptionSheet)),
		new ExcelSheetInfo(CompensationConfig.Name, typeof(CompensationData), typeof(CompensationSheet)),
        new ExcelSheetInfo(UiWindowsConfig.Name, typeof(UiWindowsData), typeof(UiWindowsSheet)),
        new ExcelSheetInfo(AdStrategyConfig.Name, typeof(AdStrategyData), typeof(AdStrategySheet)),
        new ExcelSheetInfo(UserSourceConfig.Name, typeof(UserSourceData), typeof(UserSourceSheet)),
    };

	//Note: when add one new excel sheet in GameConfig, don't forget to add one item here
	private static readonly Dictionary<string, ReloadInfo> _reloadTable = new Dictionary<string, ReloadInfo>()
	{
		{
			ADSConfig.Name,
			new ReloadInfo(ADSConfig.HasInstance, ADSConfig.Reload)
		},
		{
			DailyBonusConfig.Name,
			new ReloadInfo(DailyBonusConfig.HasInstance, DailyBonusConfig.Reload)
		},
		{
			FriendSettingConfig.Name,
			new ReloadInfo(FriendSettingConfig.HasInstance, FriendSettingConfig.Reload)
		},
		{
			IAPCatalogConfig.Name,
			new ReloadInfo(IAPCatalogConfig.HasInstance, IAPCatalogConfig.Reload)
		},
		{
			UserLevelConfig.Name,
			new ReloadInfo(UserLevelConfig.HasInstance, UserLevelConfig.Reload)
		},
		{
			MachineUnlockSettingConfig.Name,
			new ReloadInfo(MachineUnlockSettingConfig.HasInstance, MachineUnlockSettingConfig.Reload)
		},
		{
			BankruptCompensateConfig.Name,
			new ReloadInfo(BankruptCompensateConfig.HasInstance, BankruptCompensateConfig.Reload)
		},
		{
			MailTextConfig.Name,
			new ReloadInfo(MailTextConfig.HasInstance, MailTextConfig.Reload)
		},
		{
			MapSettingConfig.Name,
			new ReloadInfo(MapSettingConfig.HasInstance, MapSettingConfig.Reload)
		},
		{
			PiggyBankConfig.Name,
			new ReloadInfo(PiggyBankConfig.HasInstance, PiggyBankConfig.Reload)
		},
		{
			PiggyInfoConfig.Name,
			new ReloadInfo(PiggyInfoConfig.HasInstance, PiggyInfoConfig.Reload)
		},
		{
			VIPConfig.Name,
			new ReloadInfo(VIPConfig.HasInstance, VIPConfig.Reload)
		},
		{
			RemoteMachineVersionConfig.Name,
			new ReloadInfo(RemoteMachineVersionConfig.HasInstance, RemoteMachineVersionConfig.Reload)
		},
		{
			DiceConfig.Name,
			new ReloadInfo(DiceConfig.HasInstance, DiceConfig.Reload)
		},
		{
			BetUnlockSettingConfig.Name,
			new ReloadInfo(BetUnlockSettingConfig.HasInstance, BetUnlockSettingConfig.Reload)
		},
        {
            PayRotaryTableConfig.Name,
            new ReloadInfo(PayRotaryTableConfig.HasInstance, PayRotaryTableConfig.Reload)
        },
		{
			GroupConfig.Name,
			new ReloadInfo(GroupConfig.HasInstance, GroupConfig.Reload)
		},
		{
			BackFlowRewardLTLuckyConfig.Name,
			new ReloadInfo(BackFlowRewardLTLuckyConfig.HasInstance, BackFlowRewardLTLuckyConfig.Reload)
		},
		{
			GroupRuleConfig.Name,
			new ReloadInfo(GroupRuleConfig.HasInstance, GroupRuleConfig.Reload)
		},
		{
			ActiveGroupRuleConfig.Name,
			new ReloadInfo(ActiveGroupRuleConfig.HasInstance,ActiveGroupRuleConfig.Reload)
		},
		{
			GroupRepresentConfig.Name,
			new ReloadInfo(GroupRepresentConfig.HasInstance, GroupRepresentConfig.Reload)
        },
	    {
	        AdBonusConfig.Name,
            new ReloadInfo(AdBonusConfig.HasInstance, AdBonusConfig.Reload)
	    },
		{
			BetOptionConfig.Name,
			new ReloadInfo(BetOptionConfig.HasInstance, BetOptionConfig.Reload)
		},
        {
            CompensationConfig.Name,
            new ReloadInfo(CompensationConfig.HasInstance, CompensationConfig.Reload)
        },
	    {
            UiWindowsConfig.Name,
            new ReloadInfo(UiWindowsConfig.HasInstance, UiWindowsConfig.Reload)
        },
        {
            AdStrategyConfig.Name,
            new ReloadInfo(AdStrategyConfig.HasInstance, AdStrategyConfig.Reload)
        },
        {
            UserSourceConfig.Name,
            new ReloadInfo(UserSourceConfig.HasInstance, UserSourceConfig.Reload)
        },
    };

	static GameConfigTable()
	{
	}

	public static ReloadInfo GetReloadInfo(string assetName)
	{
		ReloadInfo result = null;
		foreach(var pair in _reloadTable)
		{
			string fileName = ExcelConfig.GetExportFileName(GameConfig.ExcelName, pair.Key);
			if(fileName.ToLower() == assetName.ToLower())
			{
				result = pair.Value;
				break;
			}
		}
		return result;
	}
}

