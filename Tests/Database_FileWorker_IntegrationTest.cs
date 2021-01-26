using IIG.FileWorker;
using IIG.CoSFE.DatabaseUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Text;
using System.Data;
using System.IO;

namespace Tests
{
	[TestClass]
	public class Database_FileWorker_IntegrationTest
	{
		private Encoding DEFAULT_ENCODING = Encoding.UTF8;
		private Random random = new Random();
		const string INPUT_DEFAULT_CONTENT = "This is an example\nmultiline text\r\nto be used as content";
		const string FILE_STATIC_INPUT = "StaticFileInput.txt"; // Existing file. Should be moved to 'bin' folder of Tests proj
		const string FILE_STATIC_INPUT_TOO_LARGE = "StaticFileInput1025bytes.txt"; // Existing file. Size > 1kb. Should be moved to 'bin' folder of Tests proj
		//const string FILE_STATIC_INPUT_TOO_LARGE = "StaticFileInputLarge.txt"; // Existing file. Size > 1kb. Should be moved to 'bin' folder of Tests proj

		private static string GenerateRandomFileWithContent(string content, string path = "")
		{
			string fileName = path + Guid.NewGuid().ToString() + ".tmp";
			BaseFileWorker.Write(content, fileName);
			return fileName;
		}

		private static string GenerateContent()
		{
			return Guid.NewGuid().ToString() +"\n"+ INPUT_DEFAULT_CONTENT;
		}

		[TestMethod]
		public void AddFile_WithContentFromReadAll_WithGetFullPath_AddsFileToDatabase()
		{
			// Arrange
			StorageDatabaseUtils db = DatabaseHelper.ProvideStorageDatabaseUtils();
			string contents = BaseFileWorker.ReadAll(FILE_STATIC_INPUT);
			string fullPath = BaseFileWorker.GetFullPath(FILE_STATIC_INPUT);
			var contentBytes = DEFAULT_ENCODING.GetBytes(contents);
			int initialFileCountInDB = db.GetFiles(fullPath).Rows.Count; // initial count of files with path==fullPath
			string result_fileName, result_fileContent; byte[] result_fileContentBytes;
			// Act
			bool result = db.AddFile(fullPath, contentBytes); // Add file to Database
			DataTable dt = db.GetFiles(fullPath); 
			var lastRow = dt.Rows[dt.Rows.Count - 1];
			var lastFileId = (int)lastRow.ItemArray[0];
			db.GetFile(lastFileId, out result_fileName, out result_fileContentBytes);
			result_fileContent = DEFAULT_ENCODING.GetString(result_fileContentBytes);
			// Assert
			Assert.IsTrue(result);
			Assert.AreEqual(initialFileCountInDB + 1, dt.Rows.Count);
			Assert.AreEqual(fullPath, result_fileName);
			CollectionAssert.AreEqual(contentBytes, result_fileContentBytes);
			Assert.AreEqual(contents, result_fileContent);
		}


		[TestMethod]
		public void AddFile_WithContentFromReadLinesFirstLine_WithGetFullPath_AddsFileToDatabase()
		{
			// Arrange
			StorageDatabaseUtils db = DatabaseHelper.ProvideStorageDatabaseUtils();
			string contents = BaseFileWorker.ReadLines(FILE_STATIC_INPUT)[0];
			string path = BaseFileWorker.GetFullPath(FILE_STATIC_INPUT);
			var contentBytes = DEFAULT_ENCODING.GetBytes(contents);
			int initialFileCountInDB = db.GetFiles(path).Rows.Count; // initial count of files with specified path
			string result_fileName, result_fileContent; byte[] result_fileContentBytes;

			// Act
			bool result = db.AddFile(path, contentBytes);
			DataTable dt = db.GetFiles(path);
			var lastRow = dt.Rows[dt.Rows.Count - 1];
			var lastFileId = (int)lastRow.ItemArray[0];
			db.GetFile(lastFileId, out result_fileName, out result_fileContentBytes);
			result_fileContent = DEFAULT_ENCODING.GetString(result_fileContentBytes);
			// Assert
			Assert.IsTrue(result);
			Assert.AreEqual(initialFileCountInDB + 1, dt.Rows.Count);
			Assert.AreEqual(path, result_fileName);
			CollectionAssert.AreEqual(contentBytes, result_fileContentBytes);
			Assert.AreEqual(contents, result_fileContent);
		}

		[DataTestMethod]
		[DataRow("test1")]
		[DataRow("test2")]
		public void DeleteFile_WithExistingPathInDB_UsingGetFullPath_DeletesEntry(string fileName)
		{
			// Arrange
			var content = GenerateContent();
			BaseFileWorker.Write(content, fileName);
			string fullPath = BaseFileWorker.GetFullPath(fileName);
			string result_fileName; byte[] result_fileContentBytes;

			StorageDatabaseUtils db = DatabaseHelper.ProvideStorageDatabaseUtils();
			db.AddFile(fullPath, DEFAULT_ENCODING.GetBytes(content));
			DataTable dt = db.GetFiles(fullPath);
			int initialEntryCount = dt.Rows.Count;
			var lastRow = dt.Rows[dt.Rows.Count - 1];
			var lastFileId = (int)lastRow.ItemArray[0];
			// Act
			db.GetFile(lastFileId, out result_fileName, out result_fileContentBytes);
			bool success = db.DeleteFile(lastFileId);
			int resultEntryCount = db.GetFiles(fullPath).Rows.Count;
			// Assert
			Assert.IsTrue(success);
			Assert.AreEqual(initialEntryCount - 1, resultEntryCount);
			// Cleanup
			File.Delete(fullPath);
		}

		/// <summary>
		/// Max content length is 1024 bytes according to 
		/// </summary>
		[TestMethod]
		public void AddFile_WithContentFromLargeFile_TruncatesContent()
		{
			// Arrange
			StorageDatabaseUtils db = DatabaseHelper.ProvideStorageDatabaseUtils();
			string contents = BaseFileWorker.ReadAll(FILE_STATIC_INPUT_TOO_LARGE);
			string path = BaseFileWorker.GetFullPath(FILE_STATIC_INPUT_TOO_LARGE);
			var contentBytes = DEFAULT_ENCODING.GetBytes(contents);
			int initialFileCountInDB = db.GetFiles(path).Rows.Count; // initial count of files with specified path
			string result_fileName, result_fileContent; byte[] result_fileContentBytes;
			// Act
			bool result = db.AddFile(path, contentBytes);
			DataTable dt = db.GetFiles(path);
			var lastRow = dt.Rows[dt.Rows.Count - 1];
			var lastFileId = (int)lastRow.ItemArray[0];
			db.GetFile(lastFileId, out result_fileName, out result_fileContentBytes);
			// Assert
			Assert.IsTrue(result);
			Assert.AreEqual(initialFileCountInDB + 1, dt.Rows.Count);
			Assert.AreEqual(path, result_fileName);
			Assert.IsTrue(contentBytes.Length > result_fileContentBytes.Length);
		}
	}
}
