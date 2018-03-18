using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using ICSharpCode.SharpZipLib.GZip;
public static class Compress{

	public static byte[] CompressString(byte[] input)
	{
		using (var result = new MemoryStream ())
		{
			using (var outStream = new GZipOutputStream (result))
			{
				outStream.Write(input, 0, input.Length);
				outStream.Flush();
				outStream.Finish();
			}
			return result.ToArray();
		}
		/*
		using (var result = new MemoryStream())
		{
			using (var compressionStream = new GZipStream(result,
				CompressionMode.Compress))
			{
				compressionStream.Write(input, 0, input.Length);
				compressionStream.Flush();
			}
			return result.ToArray();
		}
		*/
	}

	public static byte[] Decompress(byte[] input)
	{
		using (MemoryStream source = new MemoryStream(input))
		{
			using (GZipInputStream decompressionStream = new GZipInputStream(source))
			{
				Byte[] buffer = new byte[4096];
				int read = 0;
				using (MemoryStream output = new MemoryStream ())
				{
					while ((read = decompressionStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						output.Write(buffer, 0, read);
					}
					return output.ToArray();
				}
			}
		}
	}
}


