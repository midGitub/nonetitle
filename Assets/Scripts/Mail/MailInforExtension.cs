using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 邮件信息扩充类，系统邮件中从mailinfor的message的json字段转化而来
/// </summary>
public class MailInforExtension{
	public string Msg{ get; set; }// ui显示的message
	public SystemMailType SystemType { get; set; }// 系统邮件类型
	public ulong Credits{ get; set; }// 筹码
	public int Exp{ get; set; }// 经验
	public int Level{ get; set; }// 等级
	public int VipExp{ get; set; }// vip经验
	public int VipLevel{ get; set; }// vip等级
	public int LongLucky{ get; set; }// longlucky
	public ulong PiggyBankCredits{ get; set; }// 小猪银行筹码
	public int TotalSpinCount{ get; set; }// 总SPIN次数
	public int Priority { get; set; } // 优先级, 0 低  99 高

	public MailInforExtension(){
		Msg = "";
		SystemType = SystemMailType.None;
		Credits = 0;
		Exp = 0;
		Level = 0;
		VipExp = 0;
		VipLevel = 0;
		LongLucky = 0;
		PiggyBankCredits = 0;
		TotalSpinCount = 0;
		Priority = 0;
	}
}