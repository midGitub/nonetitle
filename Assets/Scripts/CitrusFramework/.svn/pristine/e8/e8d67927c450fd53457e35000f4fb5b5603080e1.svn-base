using UnityEngine;
using System.Collections.Generic;

namespace CitrusFramework
{
	public class HandlerFactoryBase
	{
		public HandlerFactoryBase(ProtocolFactoryBase factory)
		{
			m_ProtocolFactory = factory;
		}
		
		public INetworkHandlerBase GetHandler(IProtocolMessageBase cmd)
		{
			INetworkHandlerBase Handler = null;
			try
			{
				Handler =  m_Handlers[cmd.GetId()];
			}
			catch(KeyNotFoundException)
			{
				GameDebug.LogError("Handler not found,please check the Handler is Register,Id = "+ cmd.GetId());
			}
			return Handler;
		}
		
		public void RegisterHandler(INetworkHandlerBase Handler)
		{
			IProtocolMessageBase cmd = Handler.GetProtocolMessage();
			m_ProtocolFactory.RegisterMessage(cmd);
			
			m_Handlers[cmd.GetId()] = Handler;
		}
		
		Dictionary<string, INetworkHandlerBase> m_Handlers = new Dictionary<string, INetworkHandlerBase> ();
		public Dictionary<string, INetworkHandlerBase> Handlers{ get{ return m_Handlers; } }
		private ProtocolFactoryBase m_ProtocolFactory;
		public ProtocolFactoryBase ProtocolFactory
		{
			get
			{
				return m_ProtocolFactory;
			}
		}
	}
}