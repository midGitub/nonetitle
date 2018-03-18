using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

//Reference:
//https://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
public static class EncryptUtility
{
	#region Original

	public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
	{
		byte[] encryptedBytes = null;

		// Set your salt here, change it to meet your flavor:
		// The salt bytes must be at least 8 bytes.
		byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

		using (MemoryStream ms = new MemoryStream())
		{
			using (RijndaelManaged AES = new RijndaelManaged())
			{
				AES.KeySize = 256;
				AES.BlockSize = 128;

				var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
				AES.Key = key.GetBytes(AES.KeySize / 8);
				AES.IV = key.GetBytes(AES.BlockSize / 8);

				AES.Mode = CipherMode.CBC;

				using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
				{
					cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
					cs.Close();
				}
				encryptedBytes = ms.ToArray();
			}
		}

		return encryptedBytes;
	}

	public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
	{
		byte[] decryptedBytes = null;

		// Set your salt here, change it to meet your flavor:
		// The salt bytes must be at least 8 bytes.
		byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

		using (MemoryStream ms = new MemoryStream())
		{
			using (RijndaelManaged AES = new RijndaelManaged())
			{
				AES.KeySize = 256;
				AES.BlockSize = 128;

				var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
				AES.Key = key.GetBytes(AES.KeySize / 8);
				AES.IV = key.GetBytes(AES.BlockSize / 8);

				AES.Mode = CipherMode.CBC;

				using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
					cs.Close();
				}
				decryptedBytes = ms.ToArray();
			}
		}

		return decryptedBytes;
	}

	public static string EncryptText(string input, string password)
	{
		// Get the bytes of the string
		byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
		byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

		// Hash the password with SHA256
		passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

		byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

		string result = Convert.ToBase64String(bytesEncrypted);

		return result;
	}

	public static string DecryptText(string input, string password)
	{
		// Get the bytes of the string
		byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
		byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
		passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

		byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

		string result = Encoding.UTF8.GetString(bytesDecrypted);

		return result;
	}

	#endregion

	#region Convenient

	private static readonly string _m = "0123456789abcdefg";

	static string GetRabbitHole()
	{
		string version = BuildUtility.GetBundleVersion();
		VersionInfo info = VersionUtility.GetVersionInfo(version);
		string s1 = info._major.ToString("D3") + info._minor.ToString("D3");
		string s2 = _m[0] + "x" + _m[13] + _m[14] + _m[10] + _m[13] + _m[11] + _m[14] + _m[14] + _m[15]; //0xdeadbeef
		string result = s1 + s2;
		Debug.Assert(result.Length == 16);
		return result;
	}

	static byte[] GetKeyBytes()
	{
		string r = GetRabbitHole();
		return Encoding.UTF8.GetBytes(r);
	}

	public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted)
	{
		byte[] k = GetKeyBytes();
		return AES_Encrypt(bytesToBeEncrypted, k);
	}

	public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted)
	{
		byte[] k = GetKeyBytes();
		return AES_Decrypt(bytesToBeDecrypted, k);
	}

	public static string EncryptText(string input)
	{
		string r = GetRabbitHole();
		return EncryptText(input, r);
	}

	public static string DecryptText(string input)
	{
		string r = GetRabbitHole();
		return DecryptText(input, r);
	}

	#endregion
}
