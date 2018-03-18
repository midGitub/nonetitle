using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CitrusFramework
{
	public class ProtocolFactoryBase 
	{
		public IProtocolMessageBase CreateProtocolMessage(string Id)
		{
			IProtocolMessageBase cmd = null;
			try
			{
				cmd = m_Message[Id];
				cmd = (IProtocolMessageBase)Activator.CreateInstance(cmd.GetType());
			}
			catch(KeyNotFoundException)
			{
				GameDebug.Log("Message factory/handler for Id "+ Id + ", not registered. See HandlerFactory.");
			}
			return cmd;
		}

		public void RegisterMessage(IProtocolMessageBase message)
		{
			m_Message [message.GetId()] = message;
		}

		Dictionary<string, IProtocolMessageBase> m_Message = new Dictionary<string, IProtocolMessageBase>();
	}
}
