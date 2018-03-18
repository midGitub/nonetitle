using System.Collections;
using System.Collections.Generic;

// 邮件信息类
public class MailInfor
{
	public string Title = "";
	public string Message = "";
	public int Bonus = 0;
	public string Type ="";
	public MailState State = MailState.WaitingConfirm;
}