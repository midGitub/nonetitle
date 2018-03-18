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
	public class ArchiveI : IArchive
	{
	    public bool IsSending() { return false; }
	    public bool IsReceiving() { return true; }

		private JsonObject m_Obj;
		public ArchiveI(JsonObject Obj)
	    {
			m_Obj = Obj;
	    }

		public void DoSomething(ISerializable t)
	    {
	        t.Serialize(this);
	    }

		public void DoSomething(string tName, ISerializable t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				string jsonStr = code.ToString();
				JsonObject obj = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(jsonStr);
				ArchiveI ai = new ArchiveI(obj);
				ai.DoSomething(t);
			}
		}

		public void DoSomething(string tName, ref byte t)
	    {
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = byte.Parse(code.ToString());
			}
	    }

		public void DoSomething(string tName, ref ushort t)
	    {
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = ushort.Parse(code.ToString());
			}
		}

		public void DoSomething(string tName, ref int t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = int.Parse(code.ToString());
			}
		}

		public void DoSomething(string tName, ref uint t)
	    {
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = uint.Parse(code.ToString());
			}
	    }

		public void DoSomething(string tName, ref long t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = long.Parse(code.ToString());
			}
		}

		public void DoSomething(string tName, ref ulong t)
	    {
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = ulong.Parse(code.ToString());
			}
	    }

		public void DoSomething(string tName, ref float t)
	    {
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = float.Parse(code.ToString());
			}
	    }

		public void DoSomething(string tName, ref double t)
	    {
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = double.Parse(code.ToString());
			}
	    }

		public void DoSomething(string tName, ref string t)
	    {
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = code.ToString();
			}
	    }

		public void DoSomething(string tName, List<string> t)
		{
			object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				List<object> jsonlist = (List<object>)code;
				for(int i = 0; i < jsonlist.Count; i++)
				{
					t.Add(jsonlist[i].ToString());
				}
			}
		}

		public void DoSomething(string tName, List<int> t)
		{
			object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				try
				{
					List<object> jsonlist = (List<object>)code;
					for(int i = 0; i < jsonlist.Count; i++)
					{
						t.Add(int.Parse(jsonlist[i].ToString()));
					}
				}
				catch(Exception e)
				{
					GameDebug.LogError(e);
				}
			}
		}

		public void DoSomething(string tName, List<byte> t)
		{
			object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				try
				{
					List<object> jsonlist = (List<object>)code;
					for(int i = 0; i < jsonlist.Count; i++)
					{
						t.Add(byte.Parse(jsonlist[i].ToString()));
					}
				}
				catch(Exception e)
				{
					GameDebug.LogError(e);
				}
			}
		}

		public void DoSomething(string tName, List<long> t)
		{
			object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				try
				{
					List<object> jsonlist = (List<object>)code;
					for(int i = 0; i < jsonlist.Count; i++)
					{
						t.Add(long.Parse(jsonlist[i].ToString()));
					}
				}
				catch(Exception e)
				{
					GameDebug.LogError(e);
				}
			}
		}

		public void DoSomething<T>(string tName, List<T> t) where T : ISerializable, new()
		{
			object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				List<object> jsonlist = (List<object>)code;
				for(int i = 0; i < jsonlist.Count; i++)
				{
					T obj = new T();
					ArchiveI ai = new ArchiveI((JsonObject)SimpleJson.SimpleJson.DeserializeObject(jsonlist[i].ToString()));
					obj.Serialize(ai);
					t.Add(obj);
				}
			}
		}

		public void DoSomething(string tName, ref ObscuredByte t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = byte.Parse(code.ToString());
			}
		}
		
		public void DoSomething(string tName, ref ObscuredUShort t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = ushort.Parse(code.ToString());
			}
		}
		
		public void DoSomething(string tName, ref ObscuredUInt t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = uint.Parse(code.ToString());
			}
		}
		
		public void DoSomething(string tName, ref ObscuredULong t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = ulong.Parse(code.ToString());
			}
		}
		
		public void DoSomething(string tName, ref ObscuredFloat t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = float.Parse(code.ToString());
			}
		}
		
		public void DoSomething(string tName, ref ObscuredDouble t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = double.Parse(code.ToString());
			}
		}
		
		public void DoSomething(string tName, ref ObscuredString t)
		{
			System.Object code = null;
			if(m_Obj.TryGetValue(tName, out code))
			{
				t = code.ToString();
			}
		}
	}
}