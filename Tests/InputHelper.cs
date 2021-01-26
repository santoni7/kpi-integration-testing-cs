using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
	public class InputHelper
	{

		private static Random rand = new Random();
		public static char GenerateChar(char min = 'A', char max = 'z')
		{
			return (char)rand.Next(min, max);
		}
		public static string GenerateLine()
		{
			return System.DateTime.UtcNow.ToFileTimeUtc().ToString() + rand.Next().ToString();
		}

		static string GenerateFileName(int? size = null)
		{
			if (size == null)
				return Guid.NewGuid().ToString() + ".txt";
			else return GenerateData((int)size - 4,'a','z') + ".txt"; // Size - 4 because of '.txt' suffix
		}

		/// <summary>
		/// Returns a string of 'size' length.
		/// If convert this string using ASCII into byte array, it's length would be equal to 'size'
		/// </summary>
		/// <param name="size">Length of string</param>
		/// <returns>String of length 'size'</returns>
		public static string GenerateData(int size, char min = 'A', char max = 'z')
		{
			var sb = new StringWriter();
			for (int i = 0; i < size; i++)
			{
				sb.Write(GenerateChar(min, max));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Generates a file inside 'path' directory and generates string of 'size' symbols. 
		/// Uses specified encoding to write out to file, or use ASCII by default
		/// </summary>
		/// <returns>Full path to newly generated file</returns>
		public static string GenerateInputFile(int size, string path = "", Encoding encoding = null, int? fileNameSize = null)
		{
			if (encoding == null) encoding = Encoding.ASCII;
			string filePath = path + GenerateFileName(fileNameSize);
			string content = GenerateData(size);
			File.WriteAllText(filePath, content, encoding);
			return Path.GetFullPath(filePath);
		}
	}

}
