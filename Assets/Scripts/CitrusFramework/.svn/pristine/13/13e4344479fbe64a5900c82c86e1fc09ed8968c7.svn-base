using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using SimpleJson;

namespace CitrusFramework
{
	public class ArchiveO : IArchive
	{
		public bool IsSending() { return true; }
		public bool IsReceiving() { return false; }

		private JsonObject m_Obj;
		public ArchiveO(JsonObject obj)
		{
			m_Obj = obj;
		}

		public void DoSomething(ISerializable t)
		{
			t.Serialize(this);
		}

		public void DoSomething(string tName, ISerializable t)
		{
			JsonObject obj = new JsonObject();
			ArchiveO or = new ArchiveO(obj);
			or.DoSomething(t);
			m_Obj.Add(tName, obj);
		}

		public void DoSomething(string tName, ref byte t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ushort t)
		{ 
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref int t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref uint t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref long t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ulong t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref float t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref double t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref string t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, List<string> t)
		{
			JsonArray array = new JsonArray();
			for(int i = 0; i < t.Count; i++)
			{
				array.Add(t[i]);
			}
			m_Obj.Add (tName, array);
		}

		public void DoSomething(string tName, List<int> t)
		{
			JsonArray array = new JsonArray();
			for(int i = 0; i < t.Count; i++)
			{
				array.Add(t[i]);
			}
			m_Obj.Add(tName, array);
		}

		public void DoSomething(string tName, List<byte> t)
		{

			JsonArray array = new JsonArray();
			for(int i = 0; i < t.Count; i++)
			{
				array.Add(t[i]);
			}
			m_Obj.Add(tName, array);
		}

		public void DoSomething(string tName, List<long> t)
		{

			JsonArray array = new JsonArray();
			for(int i = 0; i < t.Count; i++)
			{
				array.Add(t[i]);
			}
			m_Obj.Add(tName, array);
		}

		public void DoSomething<T>(string tName, List<T> t) where T : ISerializable, new()
		{
			for(int i = 0; i < t.Count; i++)
			{
				T item = t[i];
				item.Serialize(this);
			}
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ObscuredByte t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ObscuredUShort t)
		{ 
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ObscuredUInt t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ObscuredULong t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ObscuredFloat t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ObscuredDouble t)
		{
			m_Obj.Add(tName, t);
		}

		public void DoSomething(string tName, ref ObscuredString t)
		{
			m_Obj.Add(tName, t);
		}
	}
}