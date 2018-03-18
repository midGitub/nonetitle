using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CitrusFramework
{
	public class Protocol
	{
		public enum Cmd
		{
			Gate,
			Connector,
		}

		public enum Gate
		{
			NULL_PARA,
			QueryEntry,
		}

		public enum Connector
		{
			NULL_PARA,
			Enter,
		}
	}
}
