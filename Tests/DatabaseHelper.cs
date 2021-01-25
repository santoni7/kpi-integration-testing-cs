using IIG.CoSFE.DatabaseUtils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
	/// <summary>
	/// Helper class to obtain instances of StorageDatabaseUtils and FlagpoleDatabaseUtils.
	/// Uses default MSSQL Server configuration.
	/// </summary>
	public static class DatabaseHelper
	{
		public const string SERVER = "DESKTOP-0NRUL0K\\MSSQLSERVER01";
		public const string DATABASE_FLAGPOLE = "IIG.CoSWE.FlagpoleDB";
		public const string DATABASE_STORAGE = "IIG.CoSWE.StorageDB"; //
		public const bool IS_TRUSTED = true;
		public const string LOGIN = "sa";
		public const string PASSWORD = "RPSsql12345";
		public const int CONN_TIMEOUT = 15;


		public static StorageDatabaseUtils ProvideStorageDatabaseUtils()
		{
			return new StorageDatabaseUtils(SERVER, DATABASE_STORAGE, IS_TRUSTED, LOGIN, PASSWORD, CONN_TIMEOUT);
		}

		public static FlagpoleDatabaseUtils ProvideFlagpoleDatabaseUtils()
		{
			return new FlagpoleDatabaseUtils(SERVER, DATABASE_FLAGPOLE, IS_TRUSTED, LOGIN, PASSWORD, CONN_TIMEOUT);
		}
	}
}
