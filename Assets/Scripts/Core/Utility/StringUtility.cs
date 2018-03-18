using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
#if UNITY_EDITOR
using UnityEngine;
#endif

public static class StringUtility
{
	private static readonly char _delimiter = ',';
	public static readonly char[] _numberArray = new char[]{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
	};
	private static readonly string _splitTag = "&";

	public static string ConvertDigitalIntToString(int num)
	{
		return FormatNumberStringWithComma((ulong)num);
	}

	public static string ConvertDigitalULongToString(ulong num)
	{
		return FormatNumberStringWithComma(num);
	}

	static string ClearUselessChars(string str)
	{
		// remove whitespace between each of element
		str = new string(str.ToCharArray()
			.Where(ch => !Char.IsWhiteSpace(ch))
			.ToArray());

		// remove ',', if it is found at the end.
		char[] charToTrim = { ',', ' ' };
		str = str.TrimEnd(charToTrim);

		return str;
	}

	public static T[] ParseToArray<T>(string str)
	{
		str = ClearUselessChars(str);

		object[] temp = str.Split(_delimiter);
		T[] result = new T[temp.Length];

		for(int i = 0; i < temp.Length; i++)
		{
			object o = temp[i];
			result[i] = (T)Convert.ChangeType(o, typeof(T));
		}

		return result;
	}

	public static T[] ParseToEnumArray<T>(string str) where T : struct, IConvertible 
	{
		string[] array = ParseToArray<string>(str);
		T[] result = new T[array.Length];

		for (int i = 0; i < array.Length; ++i) {
			result [i] = TypeUtility.GetEnumFromString<T> (array [i]);
		}

		return result;
	}

	public static Dictionary<K, V> ParseToDictionary<K, V>(string str)
	{
		str = ClearUselessChars(str);

		object[] temp = str.Split(_delimiter);
		Dictionary<K, V> result = new Dictionary<K, V>();

		CoreDebugUtility.Assert(temp.Length % 2 == 0, "The array length should be even");

		for(int i = 0; i < temp.Length - 1; i += 2)
		{
			object k = temp[i];
			object v = temp[i + 1];
			K key = (K)Convert.ChangeType(k, typeof(K));
			V value = (V)Convert.ChangeType(v, typeof(V));
			result[key] = value;
		}

		return result;
	}

	public static string FormatNumberString(ulong num, bool addComma, bool useMillion)
	{
		string result = "";
		if(useMillion && num >= 1000000000)
		{
			//123456789 => kNum:123456, intPart:123, decPart:456
			ulong kNum = num / 1000;
			ulong intPart = kNum / 1000;
			ulong decPart = kNum - intPart * 1000;

			if(addComma)
				result = FormatNumberStringWithComma(intPart);
			else
				result = intPart.ToString();

			if(decPart >= 0)
				result += "." + decPart.ToString("000");

			result += "M";
		}
		else
		{
			if(addComma)
				result = FormatNumberStringWithComma(num);
			else
				result = num.ToString();
		}

		return result;
	}

	/// <summary>
	/// 数字转换成在千位带有逗号的字符串
	/// </summary>
	/// <returns>The to coins string.</returns>
	/// <param name="num">Number.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static string FormatNumberStringWithComma(ulong num)
	{
		string newstring = string.Empty;

		newstring = num.ToString("N");
		newstring = newstring.Split('.')[0];
		return newstring;
	}

	public static string ConstructString<T>(IList<T> array, string symbol){
		string result = "";
		ListUtility.ForEach<T>(array, (T value)=>{
			result += value.ToString() + symbol;
		});
		return result;
	}

	public static void SetValueArray<T>(ref T[] array, string str, bool clear = true){
			if (!str.IsNullOrEmpty()){
			string[] strs = str.Split(',');
			int length = strs.Length;

			if (clear){
				int max = array.Length;
				array = new T[max];
			}

			for(int i = 0; i < length; ++i){
				T value = default(T);
				SetValue(ref value, strs[i]);
				array[i] = value;
			}
		}
	}

	public static void SetValueList<T>(ref List<T> list, string str, bool clear = true){
		if (!str.IsNullOrEmpty()){
			string[] strs = str.Split(',');
			int length = strs.Length;

			if (clear){
				list.Clear();
			}

			for(int i = 0; i < length; ++i){
				T value = default(T);
				SetValue(ref value, strs[i]);
				list.Add(value);
			}
		}
	}

	public static void SetValue<T>(ref T value, string str){
		bool success = false;
		T result = ParseTo<T>(str, out success);
		if (success){
			value = result;
		}else {
			CoreDebugUtility.Log("SetValue failed : " + str);
		}
	}

	public static T ParseTo<T>(string str, out bool success){
		success = true;

		if (typeof(T) == typeof(int)) {
			int result;
			if (!int.TryParse (str, out result) ){
				CoreDebugUtility.Log("string utility parse int failed "+ str);
				success = false;
			}
			return (T)Convert.ChangeType(result, typeof(T));
		}
		else if (typeof(T) == typeof(float)) {
			float result;
			if (!float.TryParse (str, out result) ){
				CoreDebugUtility.Log("string utility parse float failed "+ str);
				success = false;
			}
			return (T)Convert.ChangeType (result, typeof(T));
		}
		else if (typeof(T) == typeof(double)) {
			double result;
			if (!double.TryParse (str, out result) ){
				CoreDebugUtility.Log("string utility parse double failed "+ str);
				success = false;
			}
			return (T)Convert.ChangeType (result, typeof(T));
		}
		else if (typeof(T) == typeof(decimal)) {
			decimal result;
			if (!decimal.TryParse (str, out result) ){
				CoreDebugUtility.Log("string utility parse decimal failed "+ str);
				success = false;
			}
			return (T)Convert.ChangeType (result, typeof(T));
		}
		else if (typeof(T) == typeof(uint)) {
			uint result;
			if (!uint.TryParse (str, out result) ){
				CoreDebugUtility.Log("string utility parse uint failed "+ str);
				success = false;
			}
			return (T)Convert.ChangeType (result, typeof(T));
		}

		int ret = -1;
		CoreDebugUtility.Log("string utility parse invalid failed "+ str);
		success = false;
		return (T)Convert.ChangeType(ret, typeof(T));
	}

	/// <summary>
	/// 得到字符串的MD5
	/// </summary>
	/// <returns>The string MD.</returns>
	/// <param name="str">String.</param>
	public static string GetStringMD5(string str)
	{
		// 得到MD5
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(str);
		byte[] hash = md5.ComputeHash(inputBytes);

		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < hash.Length; i++)
		{
			sb.Append(hash[i].ToString("X2"));
		}
		return sb.ToString();
	}

	// 首尾trim
	public static string Trim(string str, char[] matchArray){
		int firstIndex = str.IndexOfAny (matchArray);
		int lastIndex = str.LastIndexOfAny (matchArray);
		string result = "";

		if (firstIndex == -1 || lastIndex == -1)
			return result;
		
		if (firstIndex != lastIndex) {
			int length = lastIndex - firstIndex + 1;
			result = str.Substring (firstIndex, length);
		} else {
			result = str.Substring (firstIndex, 1);
		}

		return result;
	}

	// 去字符
	public static string SubstractStr(string str, string refStr){
		if (str.Equals (refStr))
			return str;
		
		int index = str.IndexOf (refStr);
		if (index != -1) {
			int length = str.Length;
			int refLength = refStr.Length;

			if (index + refLength >= length) {
				// 去尾
				return str.Substring(0, index);
			}else if (index == 0){
				// 去头
				return str.Substring(index, length - index);
			}else {
				// 去中间
				string str1 = str.Substring(0, index);
				string str2 = str.Substring (index + refLength, length - (index + refLength));
				return str1 + str2;
			}
		}

		return str;
	}

	public static float ConstructPrice(string currentPriceStr){
		string priceStr = Trim(currentPriceStr, _numberArray);
		float price = Convert.ToSingle(priceStr, System.Globalization.CultureInfo.InvariantCulture);
		return price;
	}

	// 解析字段
	public static string AnalyzeURL(string url, string tag){
		int startIndex = 0;
		int length = 0;
		string str = "default";

		if (url.Contains (tag)) {
			str = url;
			startIndex = url.IndexOf (tag);
			str = url.Substring (startIndex);
			length = str.IndexOf (_splitTag);
			if (length == -1) {
				length = str.Length;
			}
			str = str.Substring(0, length);
			str = str.Replace (tag + "=", "");
		}

		//LogUtility.Log("AnalyzeURL tag = " + tag + " value = " + str, Color.yellow);
		return str;
	}

#if UNITY_EDITOR
	public static void Test(){
		string str = "%12,454,2323#";
		float price = ConstructPrice(str);
		Debug.Log("Test price = " + price);
	}
#endif
}
