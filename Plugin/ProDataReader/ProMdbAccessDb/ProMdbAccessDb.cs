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
using System.Data.OleDb;
using System.Runtime.InteropServices;

namespace ProMdbAccessDb
{
	/// <summary>
	/// Encapsulates feature allowing access to Microsoft Access database from within ArcGIS Pro
	/// </summary>
	public class ProMdbAccessDb : IDisposable
	{
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[DllImport("kernel32.dll")]

		internal static extern uint GetCurrentThreadId();

		private OleDbConnection _connection = null;
        private Dictionary<string, ProMdbTableInfo> _lstSpatialTables = new Dictionary<string, ProMdbTableInfo>();

		private static Dictionary<string, ConnectionStatus> Connections = new Dictionary<string, ConnectionStatus>();


		public ProMdbAccessDb(string accessDbPath)
        {
            Open(accessDbPath);
        }

        private void Open(string accessDbPath)
        {
            DbPath = accessDbPath;
			if (Connections.ContainsKey(accessDbPath))
			{
				_connection = Connections[accessDbPath].Connection;
				Connections[accessDbPath].InstanceCount++;
			}
			else
			{
				_connection = new OleDbConnection($@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DbPath}");
				_connection.Open();
				Connections.Add(accessDbPath, new ConnectionStatus() { InstanceCount = 0, Connection = _connection });
			}
        }

        public void Close ()
        {
			// dispose managed state (managed objects).
			if (!string.IsNullOrEmpty(DbPath))
			{
				if (Connections.ContainsKey(DbPath))
				{
					if (Connections[DbPath].InstanceCount == 0)
					{
						try
						{
							// all connection instances are closed this is the last one
							if (Connections[DbPath].Connection.State == ConnectionState.Open)
								Connections[DbPath].Connection.Close();
							Connections.Remove(DbPath);
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine($@"Can't close connection: {ex.Message}");
						}
					}
					else
					{
						// other instances are still using this connection
						Connections[DbPath].InstanceCount--;
					}
				}
				DbPath = string.Empty;				
			}
        }

        public string DbPath { get; set; }

		#region Geodatabase Schema support

		public Dictionary<string, ProMdbTableInfo> GetSpatialTables()
        {
            if (_lstSpatialTables.Count != 0) return _lstSpatialTables;

            string[] sqlColumns = { "TableName", "FieldName", "ShapeType", "ExtentLeft", "ExtentBottom", "ExtentRight", "ExtentTop", "SRTEXT" };
            var order = @"order by TableName";
            var sqlGetGeomCols = $@"select {String.Join(", ", sqlColumns)} from GDB_GeomColumns LEFT JOIN GDB_SpatialRefs ON GDB_GeomColumns.srid = GDB_SpatialRefs.srid {order}";

            OleDbCommand command = new OleDbCommand(sqlGetGeomCols, _connection);
            // Open the connection and execute the select command.  
            try
            {
                // Execute command  
                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tn = reader["TableName"].ToString();
                        if (tn.Equals("GDB_Items")) continue;
                        var tbl = new ProMdbTableInfo
                        {
                            TableName = tn,
                            GeometryFieldName = reader["FieldName"].ToString(),
                            GeometryType = (int)reader["ShapeType"],
                            ExtentLeft = (double)reader["ExtentLeft"],
                            ExtentBottom = (double)reader["ExtentBottom"],
                            ExtentRight = (double)reader["ExtentRight"],
                            ExtentTop = (double)reader["ExtentTop"],
                            SpatialRefString = reader["SRTEXT"].ToString(),
                            FieldInfo = GetFieldInfo(tn)
                        };
                        _lstSpatialTables.Add(tbl.TableName, tbl);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return _lstSpatialTables;
        }

        #endregion Geodatabase Schema support

        #region Table Schema support

        private ProMdbFieldInfo GetFieldInfo(string tableName)
        {
            var fieldInfo = new ProMdbFieldInfo()
            {
                Columns = new List<ProMdbColumnInfo>()
            };
            //Retrieve schema information about the given table.
            var dt = new DataTable();
            string tableSchemaClause =
                $@"SELECT * FROM {tableName} WHERE 1=0;";
            OleDbDataAdapter adapter = new OleDbDataAdapter(tableSchemaClause, _connection);
            adapter.Fill(dt);

            foreach (DataColumn col in dt.Columns)
            {
                fieldInfo.Columns.Add(new ProMdbColumnInfo { ColumnName = col.ColumnName, Alias = col.Caption, ColumnDataType = col.DataType });
            }
            var keys = GetKeyNames(tableName, _connection);
            fieldInfo.ObjectIdField = keys.Count > 0 ? keys[0] : string.Empty;
            return fieldInfo;
        }

        public static List<string> GetKeyNames(String tableName, OleDbConnection conn)
        {
            var returnList = new List<string>();
            DataTable mySchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Constraint_Column_Usage,
                                    new Object[] { null, null, tableName });
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
            string queryWhereClause =
                $@"SELECT {selectClause} FROM {tableName} WHERE {(string.IsNullOrEmpty(whereClause) ? "1=1" : whereClause)};";
            if (!string.IsNullOrEmpty(orderBy)) queryWhereClause = $@"{queryWhereClause} ORDER BY {orderBy}";
            OleDbDataAdapter adapter = new OleDbDataAdapter(queryWhereClause, _connection);
            //adapter.SelectCommand.Parameters.Add("@_param1", OleDbType.VarChar).Value = "parameter value";
            return adapter.Fill(outputDataTable);
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
                    Close();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _connection = null;
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
		internal OleDbConnection Connection { get; set; }
	}

}
