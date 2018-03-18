//using UnityEngine;
//using System.Collections.Generic;
//using System.IO;
//
//namespace CitrusFramework
//{
//	public class AccountManager
//	{
//		public AccountManager()
//		{
//			string savedAccountStr = PlayerPrefs.GetString ("savedAccount");
//
//			if (!string.IsNullOrEmpty (savedAccountStr)) 
//			{
//				string[] savedAccountList = savedAccountStr.Split (new char[]{ '|' });
//				bool shouldSaveAgain = false;
//				foreach (string Account in savedAccountList)
//				{
//					FileInfo t = new FileInfo (Application.persistentDataPath + "/" + Account);
//					if (t.Exists) 
//					{
//						m_accountList.Add (Account);
//					} 
//					else 
//					{
//						shouldSaveAgain = true;
//					}
//				}
//				
//				if (shouldSaveAgain)
//					saveAllToPrefs ();
//			}
//		}
//
//		public bool isAccountExists(string userId)
//		{
//			return AccountList.Contains(userId);
//		}
//		
//		//call when login success
//		public void SaveAccount(string userId)
//		{
//			if (!isAccountInfrist (userId))
//			{
//				if (isAccountExists (userId)) 
//				{
//					m_accountList.Remove (userId);
//				}
//				m_accountList.Insert (0, userId);
//				saveAllToPrefs ();
//			}
//		}
//
//		//call when Relogin fail
//		public void RemoveAccount(string userId)
//		{
//			if (m_accountList.Remove (userId))
//				saveAllToPrefs ();
//		}
//
//		private bool isAccountInfrist(string userId)
//		{
//			int index = AccountList.FindIndex ( s => s == userId);
//			return index == 0 ? true : false;
//		}
//
//		private void saveAllToPrefs()
//		{
//			if (m_accountList.Count > 0)
//			{
//				string AccountStr = "";
//				m_accountList.ForEach (s => AccountStr += s + "|");
//				AccountStr = AccountStr.Substring (0, AccountStr.Length - 1);
//				PlayerPrefs.SetString ("savedAccount", AccountStr);
//			}
//			else 
//			{
//				PlayerPrefs.SetString ("savedAccount", "");
//			}
//
//		}
//
//		public List<string> AccountList{ get { return m_accountList; } }
//		private List<string> m_accountList = new List<string>();
//	}
//}
