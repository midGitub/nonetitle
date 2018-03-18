using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class LogUtility {

	public static void Log(string str){
		#if DEBUG
		Debug.Log (str);
		#endif
	}

	public static void Log(string str, Color color){
		#if DEBUG
		StringBuilder fullStr = new StringBuilder("");
		string realColor ="#"+ColorUtility.ToHtmlStringRGB(color); 
		fullStr.Append ("<color=").Append (realColor).Append(">")
			.Append (str).Append ("</color>");
		Debug.Log (fullStr);
		#endif
	}

	public static void Log(string logTitle, List<string> strList){
		#if DEBUG
		string str = "";
		ListUtility.ForEach (strList, (string s) => {
			str += s + ",";
		});
		Log (logTitle + " : " + str);
		#endif
	}

	public static void Log(string logTitle, List<string> strList, Color color){
		#if DEBUG
		string str = "";
		ListUtility.ForEach (strList, (string s) => {
			str += s + ",";
		});
		Log (logTitle + " : " + str, color);
		#endif
	}
}
