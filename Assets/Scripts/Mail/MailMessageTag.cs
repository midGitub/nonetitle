using System.Collections;
using System.Collections.Generic;

// 系统邮件tag类
public class MailMessageTag
{
	public string TagName;// mail的msg扩充tag
	public int Num;// 该tag下元素个数

	public MailMessageTag(string name, int num){
		TagName = name;
		Num = num;
	}

	public MailMessageTag(){
		TagName = "DefaultTag";
		Num = 0;
	}
}
