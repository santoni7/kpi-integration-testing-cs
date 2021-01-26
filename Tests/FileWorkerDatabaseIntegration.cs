using IIG.CoSFE.DatabaseUtils;
using IIG.FileWorker;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
	public class FileWorkerDatabaseIntegration
	{
		/// <summary>
		/// Saves all file content into database
		/// </summary>
		/// <param name="fileName">Name of input file</param>
		/// <param name="db">StorageDatabaseUtils object</param>
		/// <param name="encoding">Encoding to convert string to bytes</param>
		/// <returns>String content which was read from file</returns>
		public static string ReadAllAndSaveToDB(string fileName, StorageDatabaseUtils db, Encoding encoding)
		{
			string contents = BaseFileWorker.ReadAll(fileName);
			var contentBytes = encoding.GetBytes(contents);
			db.AddFile(fileName, contentBytes);
			return contents;
		}
		/// <summary>
		/// Saves specific line from file into database
		/// </summary>
		/// <returns>Specified line from input file</returns>
		public static string ReadLineAndSaveToDB(string fileName, int line, StorageDatabaseUtils db, Encoding encoding)
		{
			string contents = BaseFileWorker.ReadLines(fileName)[line];
			var contentBytes = encoding.GetBytes(contents);
			db.AddFile(fileName, contentBytes);
			return contents;
		}
	}
}
