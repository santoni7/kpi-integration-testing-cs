using IIG.BinaryFlag;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;

namespace Tests
{
	[TestClass]
	public class Database_BinaryFlag_IntegrationTest
	{
		private static void SetUnevenFlags(MultipleBinaryFlag f, ulong length)
		{
			for (ulong i = 1; i < length; i += 2)
			{
				f.SetFlag(i);
			}
		}

		private static void ResetUnevenFlags(MultipleBinaryFlag f, ulong length)
		{
			for (ulong i = 1; i < length; i += 2)
			{
				f.ResetFlag(i);
			}
		}
		private static void SetAllFlags(MultipleBinaryFlag f, ulong length)
		{
			for (ulong i = 0; i < length; i++)
			{
				f.SetFlag(i);
			}
		}

		[DataTestMethod]
		[DynamicData(nameof(GetDataCombinationsForTest), DynamicDataSourceType.Method)]
		public void AddFlagDatabase_WithDataFrom_MultipleBinaryFlag(ulong length, bool initialValue, bool mutateData)
		{
			using (MultipleBinaryFlag impl = new MultipleBinaryFlag(length, initialValue))
			{
				// 1. Arrange data
				var db = DatabaseHelper.ProvideFlagpoleDatabaseUtils();
				Console.WriteLine("Initial flag status: " + impl.ToString());
				if (mutateData)
				{
					if (initialValue) // set/reset even/uneven flags depending on initial value.
					{
						ResetUnevenFlags(impl, length);
					}
					else
					{
						SetUnevenFlags(impl, length);
					}
					Console.WriteLine("Flag status after permutations with even/uneven: " + impl.ToString());
				}
				string flagView = impl.ToString();
				bool flagValue = impl.GetFlag();
				// Write the flag to DB
				bool dbAdded = db.AddFlag(flagView, flagValue);
				// Get the latest item id from DB:
				var dt = db.GetDataTableBySql("SELECT MAX(MultipleBinaryFlagID) FROM [dbo].[MultipleBinaryFlags]");
				int? lastInsertedId = dt.Rows.Count > 0 ? (int?)dt.Rows[0].ItemArray[0] : null;

				Assert.IsTrue(dbAdded);
				Assert.IsNotNull(lastInsertedId);

				string res_flagView; bool? res_flagValue;
				var dbRead = db.GetFlag((int)lastInsertedId, out res_flagView, out res_flagValue);

				Assert.IsTrue(dbRead);
				Assert.AreEqual(flagView, res_flagView);
				Assert.AreEqual(flagValue, res_flagValue);
			}
		}

		/// <summary>
		/// A method which let's us build the testing range dynamically, instead of statically
		/// </summary>
		/// <returns>A collection of test input data</returns>
		public static IEnumerable<object[]> GetDataCombinationsForTest()
		{
			Tuple<ulong, ulong>[] rangesForImplementationsTest = new Tuple<ulong, ulong>[] {
				new Tuple<ulong, ulong>(2, 32), // 1. length in [2, 32] - UIntConcreteBinaryFlag implementation
				new Tuple<ulong, ulong>(33, 64), // 2. length in [33, 64] - ULongConcreteBinaryFlag implementation
				new Tuple<ulong, ulong>(64, 1024) // 3. length > 64 - UIntArrayConcreteBinaryFlag implementation
				// Note: 1024 is actually not an end of range (it is 17179868704), but for simplicity let's use it for testing
			};
			foreach (Tuple<ulong, ulong> range in rangesForImplementationsTest)
			{
				yield return new object[] { range.Item1, true, true };
				yield return new object[] { range.Item1, true, false };
				yield return new object[] { range.Item1, false, true };
				yield return new object[] { range.Item1, false, false };

				yield return new object[] { range.Item2, true, true };
				yield return new object[] { range.Item2, true, false };
				yield return new object[] { range.Item2, false, true };
				yield return new object[] { range.Item2, false, false };
			}
		}
	}
}
