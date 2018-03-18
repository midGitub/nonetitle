using UnityEngine;
using System.Collections;
using SimpleJson;

namespace CitrusFramework
{
	public class ProtocolMessage : JsonObject, IProtocolMessageBase,ISerializable
	{
		public ProtocolMessage()
		{

		}
		public void SetMessage(string cmdId, bool isNormalRespond = false)
		{
			Id = cmdId;
			JSONKEY_REQUEST = Id + "_request";
			if(isNormalRespond)
				JSONKEY_RESPOND = "normalcmd_respond";
			else
				JSONKEY_RESPOND = Id + "_respond";
		}

		public string GetId()
		{
			return Id;
		}

		public virtual void Serialize(IArchive ar)
		{
			ar.DoSomething("HTTPUUID", ref uuid);
			if(ar.IsReceiving())
			{
				ar.DoSomething("Code", ref m_code);
			}
		}

		public CmdCode code{ get{ return (CmdCode)m_code; } set{
				m_code = (int)value;
				this["Code"] = m_code;
			}}

		//Url pathname. like "fetchnotice"
		protected string Id;
		//Error code
		private int m_code = (int)CmdCode.RespondSuccess;
		//if false, there will only one in NetworkProgarm
		public bool CanSendMulti = false;
		//this is a Httpobject's hashcode, use to find Httpobject in client,when http respond/
		public string uuid;
        //cmd send to which server
        public ServerType Server = ServerType.Game;
        //cmd send type
        public CmdSendType SendType = CmdSendType.Protobuf;

		//protobuf's jsonkey
		public string JSONKEY_REQUEST;
		public string JSONKEY_RESPOND;
	}

	public enum CmdCode
	{
		None,
		TimeOut,
		RequestError,
		RespondSuccess,
		RespondError
	}

    public enum CmdSendType
    {
        Protobuf,
        Json
    }
}
