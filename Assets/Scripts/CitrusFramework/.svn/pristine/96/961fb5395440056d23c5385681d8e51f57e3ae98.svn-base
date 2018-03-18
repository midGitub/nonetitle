using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeStage.AntiCheat.ObscuredTypes;

namespace CitrusFramework
{
	public interface IArchive
	{
	    bool IsSending();
	    bool IsReceiving();
		void DoSomething(ISerializable t);
		void DoSomething(string tName, ISerializable t);
	    void DoSomething(string tName, ref byte t);
		void DoSomething(string tName, ref ushort t);
		void DoSomething(string tName, ref int t);
		void DoSomething(string tName, ref uint t);
		void DoSomething(string tName, ref long t);
		void DoSomething(string tName, ref ulong t);
		void DoSomething(string tName, ref float t);
		void DoSomething(string tName, ref double t);
		void DoSomething(string tName, ref string t);
		void DoSomething(string tName, List<string> t);
		void DoSomething(string tName, List<int> t);
		void DoSomething(string tName, List<byte> t);
		void DoSomething(string tName, List<long> t);
		void DoSomething<T>(string tName, List<T> t)where T : ISerializable, new();
		void DoSomething(string tName, ref ObscuredByte t);
		void DoSomething(string tName, ref ObscuredUShort t);
		void DoSomething(string tName, ref ObscuredUInt t);
		void DoSomething(string tName, ref ObscuredULong t);
		void DoSomething(string tName, ref ObscuredFloat t);
		void DoSomething(string tName, ref ObscuredDouble t);
		void DoSomething(string tName, ref ObscuredString t);
	}
}