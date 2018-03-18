using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
//Note: Not work because C# doesn't have some type contraint like "this type has an attribute"
//

//public class KeyValConfigBase<SheetType, DataType>
//{
//	private SheetType _sheet;
//
//	public SheetType Sheet { get { return _sheet; } }
//
//	public KeyValConfigBase(SheetType sheet)
//	{
//		_sheet = sheet;
//	}
//
//	protected int GetIntValueFromKey(string key)
//	{
//		string str = ValueFromKey(key);
//		Debug.Assert(!string.IsNullOrEmpty(str));
//		return int.Parse(str);
//	}
//
//	protected float GetFloatValueFromKey(string key)
//	{
//		string str = ValueFromKey(key);
//		Debug.Assert(!string.IsNullOrEmpty(str));
//		return float.Parse(str);
//	}
//
//	protected string ValueFromKey(string key)
//	{
//		string result = "";
//		int index = ListUtility.Find(_sheet.dataArray, (LuckyData data) => {
//			return data.Key == key;
//		});
//		if(index >= 0)
//			result = _sheet.dataArray[index].Val;
//		return result;
//	}
//}
