using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MailScrollviewController : DynamicScrollviewController<string, MailUIItem>
{
	internal class PrioritySet
	{
		public string Key;
		public int Priority;
		public PrioritySet(string key, int pri){
			Key = key;
			Priority = pri;
		}
	}

	private Dictionary<string, MailInfor> newMailDic = new Dictionary<string, MailInfor>();
	private Dictionary<string, MailInfor> readedMailDic = new Dictionary<string, MailInfor>();
	private Dictionary<string, MailInfor> allconfirmDoneDic = new Dictionary<string, MailInfor>();
	List<PrioritySet> allMailList = new List<PrioritySet>();

	public CoinCollectController _coinEffectController;
	public MailUIController _mailUIController;


	public void InitMSVC()
	{
		// Debug.Log("初始化了");
		LogUtility.Log("InitMSVC", Color.yellow);
		FillDic();
		//foreach(var item in UserBasicData.Instance.MailInforDic)
		//{
		//	Debug.Log(item.Key);
		//}
		#if false
		InitScrollView(new List<string>(allconfirmDoneDic.Keys));
		#else
		List<string> list = ListUtility.MapList(allMailList, (PrioritySet item)=>{
			return item.Key;
		});
		InitScrollView(list);
		#endif
	}

	private void FillDic()
	{
		var currDic = UserBasicData.Instance.MailInforDic;
		DictionaryUtility.DictionaryWhere<string,MailInfor> (currDic, newMailDic, (obj) => {
			return obj.State == MailState.DoneConfirm;
		});
		DictionaryUtility.DictionaryWhere (currDic, readedMailDic, (arg) => {
			return arg.State == MailState.Readed;
		});
		DictionaryUtility.DictionaryWhere (currDic, allconfirmDoneDic, (arg) => {
			return arg.State != MailState.WaitingConfirm;
		}); 
		//readedMailDic = (Dictionary<string, MailInfor>)currDic.Where((arg) => { return arg.Value.State == MailState.Readed; });
		//allconfirmDoneDic = (Dictionary<string, MailInfor>)currDic.Where((arg) => { return arg.Value.State != MailState.WaitingConfirm; });

		// TODO: 按照优先级排序可见邮件
		allMailList.Clear();
		foreach(var info in allconfirmDoneDic){
			MailInforExtension extension = MailUtility.MailInfor2MailInforExtension(info.Value);
			if (extension != null){
				allMailList.Add(new PrioritySet(info.Key, extension.Priority));
			}
		}
		allMailList.Sort (SortPriority);
	}

	// 改成降序排列
	private int SortPriority(PrioritySet left, PrioritySet right){
		if (left.Priority > right.Priority)
			return -1;
		else if (left.Priority < right.Priority)
			return 1;
		else
			return 0;
	}

	public void DataUpdate(bool changeScrollView = true)
	{
		FillDic();
		#if false
		DataUpdateList(new List<string>(allconfirmDoneDic.Keys), changeScrollView);
		#else
		List<string> list = ListUtility.MapList(allMailList, (PrioritySet item)=>{
			return item.Key;
		});
		DataUpdateList(list, changeScrollView);
		#endif
	}

	protected override void ItemUpdateFunc(MailUIItem item, string data)
	{
		//Debug.Log(data);
		item.UpdateData(data);
	}

	public void DeleteReadedMaile()
	{
		var willDlist = new List<string>(readedMailDic.Keys);
		for(int i = 0; i < willDlist.Count; i++)
		{
			UserBasicData.Instance.MailInforDic.Remove(willDlist[i]);
		}
		UserBasicData.Instance.Save();
		DataUpdate();
	}

	public void GetAllBonus()
	{
		var willDoDic = new List<string>(newMailDic.Keys);
		bool canGetBonus = false;

		for(int i = 0; i < willDoDic.Count; i++)
		{
			var currMail = UserBasicData.Instance.MailInforDic[willDoDic[i]];
			if (currMail.State != MailState.Readed) {
				//zhousen 对应新的领取奖励接口
				MailInforExtension extension = MailUtility.MailInfor2MailInforExtension (currMail);
				MailUtility.GetMailReward(currMail, extension);
				AnalysisManager.Instance.ReadMail (currMail, extension);
				currMail.State = MailState.Readed;

				canGetBonus = true;
			}
		}

		if (canGetBonus) {
			_mailUIController.TryShowCollectAllButton ();
			UserBasicData.Instance.Save();
			DataUpdate();

			if (_coinEffectController != null) {
				_coinEffectController.Show (true);
			}
		}
	}

}
