/*

   Copyright 2017 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Runtime.InteropServices;

namespace ProSqlExpressDb
{
	/// <summary>
	/// Encapsulates feature allowing SQL Express database access from within ArcGIS Pro
	/// </summary>
	public class ProSqlExpressDb : IDisposable
	{
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[DllImport("kernel32.dll")]

		internal static extern uint GetCurrentThreadId();

        internal static string GdbItems = "GDB_Items";
        internal static string GdbGeomColumns = "GDB_GeomColumns";
        internal static string GdbSpatialRefs = "GDB_SpatialRefs";

        private static List<ProSqlExpressTableInfo> _lstSpatialTables = new List<ProSqlExpressTableInfo>();

        private static Dictionary<string, List<ProSqlExpressTableInfo>> SpatialTables = new Dictionary<string, List<ProSqlExpressTableInfo>>();

		public ProSqlExpressDb(string sqlConnection)
        {
            SqlConnectionString = sqlConnection;
        }

        private SqlConnection OpenConnection(string sqlConnection)
        {
            SqlConnection sqlCon = null;
            try
            {
                sqlCon = new SqlConnection(SqlConnectionString);
                sqlCon.Open();
            }
            catch (Exception ex)
            {
                throw new Exception($@"Unable to open this connection {SqlConnectionString}: {ex.Message}");
            }
            return sqlCon;
        }

        public string SqlConnectionString { get; set; }

        public string DatabaseName
        {
            get
            {
                var name = string.Empty;
                using (var sqlCon = new SqlConnection(SqlConnectionString))
                {
                    name = sqlCon.Database;
                }
                return name;
            }
        }

		#region Geodatabase Schema support

		public List<ProSqlExpressTableInfo> GetSpatialTables()
        {
            if (SpatialTables.ContainsKey(SqlConnectionString)) return SpatialTables[SqlConnectionString];
            // Open the connection and execute the select command.
            SpatialTables.Add(SqlConnectionString, new List<ProSqlExpressTableInfo>());
            SqlConnection sqlCon = null;
            try
			{
				var order = @"order by TableName";
				var idDataset = @"{74737149-DCB5-4257-8904-B9724E32A530}";
				var idFeatureClass = @"{70737809-852C-4A03-9E22-2CECEA5B9BFA}";
				var idTable = @"{CD06BC3B-789D-4C51-AAFA-A467912B8965}";

				var sqlGdbItems = $@"select Type, Name, Path, DatasetSubType2 from {GdbItems} where {{0}} order by Type, Name";

				string[] sqlColumns = { "TableName", "FieldName", "ShapeType", "ExtentLeft", "ExtentBottom", "ExtentRight", "ExtentTop", "SRTEXT" };
				var sqlGetGeomCols = $@"select {String.Join(", ", sqlColumns)} from {GdbGeomColumns} LEFT JOIN {GdbSpatialRefs} ON {GdbGeomColumns}.srid = {GdbSpatialRefs}.srid where {{0}} {order}";
                using (sqlCon = OpenConnection(SqlConnectionString))
                {
                    var lstFeatureDatasets = new List<string>();
                    SqlCommand command = new SqlCommand(string.Format(sqlGdbItems, $@"Type = '{idDataset}'"), sqlCon);
                    // read all feature datasets
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lstFeatureDatasets.Add(reader["Name"].ToString());
                        }
                    }
                    lstFeatureDatasets.Add(string.Empty);

                    List<Tuple<string, string, string>> pathNames = new List<Tuple<string, string, string>>();
                    // for each feature dataset read all feature classes and tables within
                    foreach (var featDataset in lstFeatureDatasets)
                    {
                        var fdQuery = string.IsNullOrEmpty(featDataset) ? @"Path = '\' + Name"
                                                        : $@"Path = '\{featDataset}\' + Name";
                        var whereClause = $@"Type in ('{idFeatureClass}','{idTable}') and {fdQuery}";
                        var selection = string.Format(sqlGdbItems, whereClause);
                        if (fdQuery.Contains($@"Path = '\{featDataset}\'"))
                        {
                            //System.Diagnostics.Debug.WriteLine(fdQuery);
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine(fdQuery);
                        }
                        command = new SqlCommand(selection, sqlCon);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pathNames.Add(new Tuple<string, string, string>(reader["Name"].ToString(), reader["Path"].ToString(), featDataset));
                            }
                        }
                    }
                    foreach (var namePath in pathNames)
                    {
                        var whereClause = $@"TableName = '{namePath.Item1}'";
                        //if (whereClause.Contains ("ListOfCi"))
                        //{
                        //	System.Diagnostics.Debug.WriteLine(whereClause);
                        //}
                        command = new SqlCommand(string.Format(sqlGetGeomCols, whereClause), sqlCon);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // we expect one record for feature classes or none for tables
                            if (reader.Read())
                            {
                                var tn = reader["TableName"].ToString();
                                var tbl = new ProSqlExpressTableInfo
                                {
                                    TableName = tn,
                                    Path = namePath.Item2,
                                    FeatureDataset = namePath.Item3,
                                    GeometryFieldName = reader["FieldName"].ToString(),
                                    GeometryType = (int)reader["ShapeType"],
                                    ExtentLeft = (double)reader["ExtentLeft"],
                                    ExtentBottom = (double)reader["ExtentBottom"],
                                    ExtentRight = (double)reader["ExtentRight"],
                                    ExtentTop = (double)reader["ExtentTop"],
                                    SpatialRefString = reader["SRTEXT"].ToString()
                                };
                                SpatialTables[SqlConnectionString].Add(tbl);
                            }
                            else
                            {
                                var tn = namePath.Item1;
                                var tbl = new ProSqlExpressTableInfo
                                {
                                    TableName = tn,
                                    Path = namePath.Item2,
                                    FeatureDataset = namePath.Item3,
                                    GeometryType = 10
                                };
                                SpatialTables[SqlConnectionString].Add(tbl);
                            }
                        }
                    }
                }
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            foreach (var spatialTable in SpatialTables[SqlConnectionString])
            {
                if (spatialTable.FieldInfo == null) spatialTable.FieldInfo = GetFieldInfo(spatialTable.TableName);
            }
            return SpatialTables[SqlConnectionString];
        }

        #endregion Geodatabase Schema support

        #region Table Schema support

        private ProSqlExpressFieldInfo GetFieldInfo(string tableName)
        {
            var fieldInfo = new ProSqlExpressFieldInfo()
            {
                Columns = new List<ProSqlColumnInfo>()
            };
            using (var sqlCon = OpenConnection(SqlConnectionString))
            {

                //Retrieve schema information about the given table.
                var dt = new DataTable();
                string tableSchemaClause = $@"SELECT * FROM {tableName} WHERE 1=0;";
                using (SqlDataAdapter adapter = new SqlDataAdapter(tableSchemaClause, sqlCon))
                {
                    adapter.Fill(dt);
                }
                foreach (DataColumn col in dt.Columns)
                {
                    fieldInfo.Columns.Add(new ProSqlColumnInfo { ColumnName = col.ColumnName, Alias = col.Caption, ColumnDataType = col.DataType });
                }
                var keys = GetKeyNames(tableName, sqlCon);
                fieldInfo.ObjectIdField = keys.Count > 0 ? keys[0] : string.Empty;
            }
            return fieldInfo;
        }

        public static List<string> GetKeyNames(String tableName, SqlConnection conn)
        {
            var returnList = new List<string>();
            DataTable mySchema = conn.GetSchema("Columns", new [] { null, null, tableName });
            // following is a lengthy form of the number '3' :-)
            int columnOrdinalForName = mySchema.Columns["COLUMN_NAME"].Ordinal;
            foreach (DataRow r in mySchema.Rows)
            {
                returnList.Add(r.ItemArray[columnOrdinalForName].ToString());
            }
            return returnList;
        }

        #endregion Table Schema support

        #region Query Support

        public int QueryTable(string tableName, string selectClause, string whereClause,
                                string orderBy, DataTable outputDataTable)
        {
            int iFillCount = 0;
            string queryWhereClause =
                $@"SELECT {selectClause} FROM {tableName} WHERE {(string.IsNullOrEmpty(whereClause) ? "1=1" : whereClause)};";
            if (!string.IsNullOrEmpty(orderBy)) queryWhereClause = $@"{queryWhereClause} ORDER BY {orderBy}";
            using (SqlDataAdapter adapter = new SqlDataAdapter(queryWhereClause, OpenConnection(SqlConnectionString)))
            {
                iFillCount = adapter.Fill(outputDataTable);
            }
            //adapter.SelectCommand.Parameters.Add("@_param1", SqlType.VarChar).Value = "parameter value";
            return iFillCount;
        }

        #endregion 

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AccessDb()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

	internal class ConnectionStatus
	{
		internal int InstanceCount { get; set; }
		internal SqlConnection Connection { get; set; }
	}

}
