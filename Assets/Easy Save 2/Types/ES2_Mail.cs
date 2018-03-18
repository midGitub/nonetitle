using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

#if false
public class ES2_MailInfo : ES2Type{
	private static readonly string _title_tag = "mail_info_title";
	private static readonly string _type_tag = "mail_info_type";
	private static readonly string _message_tag = "mail_info_message";
	private static readonly string _bonus_tag = "mail_info_bonus";
	private static readonly string _state_tag = "mail_info_state";

	public ES2_MailInfo() : base(typeof(MailInfor)){
	}

	public override object Read(ES2Reader reader){
		MailInfor data = new MailInfor ();
		// TODO: 这里怎么判断tag是否存在啊？？？
		data.Title = reader.Read<string> (_title_tag);
		data.Type = reader.Read<string> (_type_tag);
		data.Message = reader.Read<string> (_message_tag);
		data.Bonus = reader.Read<int> (_bonus_tag);
		data.State = (MailState)reader.Read<int> (_state_tag);
		return data;
	}

	public override void Write(object data, ES2Writer writer){
		MailInfor info = (MailInfor)data;
		writer.Write<string> (info.Title, _title_tag);
		writer.Write<string> (info.Type, _type_tag);
		writer.Write<string> (info.Message, _message_tag);
		writer.Write<int> (info.Bonus, _bonus_tag);
		writer.Write<int> ((int)info.State, _state_tag);
	}

	private bool IsExistTag(string tag){
		ES2.Exists (tag);
	}
}

#else

public class ES2_Mail : ES2Type
{
	public ES2_Mail() : base(typeof(MailInfor))
	{
	}

	public override object Read(ES2Reader reader)
	{
		return Json2Mail(new JSONObject(reader.Read<System.String>()));
	}

	public override void Write(object data, ES2Writer writer)
	{
		MailInfor mf = (MailInfor)data;
		writer.Write(MailInfor2Json(mf));
	}


	private string MailInfor2Json(MailInfor mif)
	{
		Dictionary<string, object> baseDic = new Dictionary<string, object>();
		baseDic.Add("Title", mif.Title);
		baseDic.Add("Message", mif.Message);
		baseDic.Add("Bonus", mif.Bonus);
		baseDic.Add("State", (int)mif.State);
		baseDic.Add("Type", mif.Type);

		return Json.Serialize(baseDic);
	}

	private MailInfor Json2Mail(JSONObject jsob)
	{
		MailInfor mf = new MailInfor
		{
			Title = jsob.GetField("Title").str,
			Message = jsob.GetField("Message").str,
			Bonus = (int)jsob.GetField("Bonus").n,
			State = (MailState)(int)jsob.GetField("State").n,
			Type = jsob.GetField("Type").str,
		};

		return mf;
	}
}
#endif