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
		private const string CATEGORY_EXPECTING_SUCCESS = "SuccessFlow"; // "Success" flow tests
		private const string CATEGORY_EXPECTING_FAILURE = "FailureFlow"; // "Failure" flow tests

		private Encoding DEFAULT_ENCODING = Encoding.ASCII;
		private Random random = new Random();
		static int MAX_FILE_LENGTH = 156 - Directory.GetCurrentDirectory().Length; // Database has varchar(300) for the fileName column.
		const int MAX_CONTENT_LENGTH_BYTES = 1024;
		const string INPUT_DEFAULT_CONTENT = "This is an example\nmultiline text\r\nto be used as content";
		const string FILE_STATIC_INPUT = "StaticFileInput.txt"; // Existing file. Should be moved to 'bin' folder of Tests proj


		/// <summary>
		/// fullPath - path to existing file
		/// </summary>
		/// <param name="fullPath"></param>
		[DataTestMethod]
		[TestCategory(CATEGORY_EXPECTING_SUCCESS)]
		[DataRow(0)]
		[DataRow(256)]
		[DataRow(512)]
		[DataRow(MAX_CONTENT_LENGTH_BYTES)]
		public void AddFile_WithContentFromReadAll_WithGetFullPath_AddsFileToDatabase(int contentSize)
		{
			// Arrange
			StorageDatabaseUtils db = DatabaseHelper.ProvideStorageDatabaseUtils();
			string fullPath = InputHelper.GenerateInputFile(contentSize);
			string contents = BaseFileWorker.ReadAll(fullPath);
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
		[TestCategory(CATEGORY_EXPECTING_SUCCESS)]
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


		/// <summary>
		/// Insert existing file from file system into DB
		/// Make sure it's there, then delete it. Make sure file has been deleted.
		/// </summary>
		/// <param name="fileNameSize"></param>
		/// <param name="contentSize"></param>
		[DataTestMethod]
		[TestCategory(CATEGORY_EXPECTING_SUCCESS)]
		[DataRow(128, MAX_CONTENT_LENGTH_BYTES)]
		[DataRow(64, 0)]
		[DataRow(-1, MAX_CONTENT_LENGTH_BYTES)]
		public void DeleteFile_WithExistingPathInDB_UsingGetFullPath_DeletesEntry(int fileNameSize = -1, int contentSize = MAX_CONTENT_LENGTH_BYTES)
		{
			// Arrange
			if (fileNameSize < 0) fileNameSize = MAX_FILE_LENGTH;
			string fileName = InputHelper.GenerateInputFile(size: contentSize, fileNameSize: fileNameSize);
			string content = BaseFileWorker.ReadAll(fileName);
			byte[] fileContentBytes = DEFAULT_ENCODING.GetBytes(content);
			Console.WriteLine("Working with " + fileName + ".\nContent: " + content);
			Console.WriteLine("FileName length: " + fileName.Length + "\nContent length: " + content.Length + "\nContent length (bytes): " + fileContentBytes.Length);
			BaseFileWorker.Write(content, fileName);
			string fullPath = BaseFileWorker.GetFullPath(fileName);
			string result_fileName; byte[] result_fileContentBytes;

			StorageDatabaseUtils db = DatabaseHelper.ProvideStorageDatabaseUtils();
			
			db.AddFile(fullPath, fileContentBytes);
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
			CollectionAssert.AreEqual(fileContentBytes, result_fileContentBytes);
			// Cleanup
			File.Delete(fullPath);
			Console.WriteLine("File was deleted from database and from file system.");
		}

		/// <summary>
		/// Max content length is 1024 bytes according to [dbo].[AddFile] or StorageDatabaseUtils implementation. 
		/// Let's check that the method truncates content as expected: leave first 1024 bytes only.
		/// Also, let's try and launch this on 
		/// </summary>
		/// <param name="contentSizeBytes">Desired content size, in bytes, or other suggestions and propositions.</param>
		[DataTestMethod]
		[TestCategory(CATEGORY_EXPECTING_FAILURE)]
		[DataRow(MAX_CONTENT_LENGTH_BYTES + 1)]
		[DataRow(MAX_CONTENT_LENGTH_BYTES * 2)]
		public void AddFile_WithContentFromLargeFile_AddsFile_But_TruncatesContent(int contentSizeBytes = MAX_CONTENT_LENGTH_BYTES)
		{
			// Arrange
			StorageDatabaseUtils db = DatabaseHelper.ProvideStorageDatabaseUtils();
			string path = InputHelper.GenerateInputFile(contentSizeBytes);
			string inputContent = BaseFileWorker.ReadAll(path);
			var inputContentBytes = DEFAULT_ENCODING.GetBytes(inputContent);
			Console.WriteLine("Working with file " + path + "\nContent: " + inputContent);
			Console.WriteLine("FileName length: " + path.Length + "\nContent length: " + inputContent.Length + "\nContent length (bytes): " + inputContentBytes.Length);
			int initialFileCountInDB = db.GetFiles(path).Rows.Count; // initial count of files with specified path in DB
			string result_fileName; byte[] result_fileContentBytes;
			// Act
			bool result = db.AddFile(path, inputContentBytes);
			DataTable dt = db.GetFiles(path);
			var lastRow = dt.Rows[dt.Rows.Count - 1];
			var lastFileId = (int)lastRow.ItemArray[0];
			db.GetFile(lastFileId, out result_fileName, out result_fileContentBytes);
			// Assert
			Assert.IsTrue(result); // Entry added to database successfully
			Assert.AreEqual(initialFileCountInDB + 1, dt.Rows.Count); // Row count increased (new row was added)
			Assert.AreEqual(path, result_fileName); // File path in DB is equal to inputs
			Assert.IsTrue(inputContentBytes.Length > result_fileContentBytes.Length); // Content is being truncated because it exceeds maximum length.
		}
	}
}
