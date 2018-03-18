using System.Collections.Generic;
using UnityEngine;

public class ES2UserType_IAPData : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		IAPData data = (IAPData)obj;

		writer.Write(data.Receipt);
		writer.Write(data.LocalItemId);
		writer.Write((int)data.State);
		writer.Write(data.UpdateTime);
		writer.Write(data.tokens);
		writer.Write(data.StoreSpecificId);
		writer.Write(data.TransactionId);
	}
	
	public override object Read(ES2Reader reader)
	{
		string receipt = reader.Read<System.String>();
		string itemId = reader.Read<System.String>();
		IAPData.IAPState iapState = (IAPData.IAPState)reader.Read<System.Int32>();
		long updateTime = reader.Read<System.Int64>();
		Dictionary<string, string> tokens = reader.ReadDictionary<System.String, System.String>();
		string storeSpecificId = reader.Read<System.String>();
		string transactionId = reader.Read<System.String>();

		return new IAPData(receipt, itemId, storeSpecificId, transactionId, iapState, updateTime, tokens);
	}

	public ES2UserType_IAPData():base(typeof(IAPData)){}
}