using System.Collections;
using System.Collections.Generic;
using CitrusFramework;

public class GiftMailEvent : CitrusGameEvent {
	public string _mailStr = "";// 邮件服务器类型
	public GiftMailEvent(string mailStr) : base()
	{
		_mailStr = mailStr;
	}
}
