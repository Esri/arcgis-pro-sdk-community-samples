/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;
using ProSqlExpressDb;

namespace ProSqlExpressPluginDatasource
{
    /// <summary>
    /// (Custom) interface the sample uses to extract row information from the
    /// plugin table
    /// </summary>
    internal interface IPluginRowProvider
    {
        PluginRow FindRow(int oid, IEnumerable<string> columnFilter, SpatialReference sr);
    }

    /// <summary>
    /// Acts as a conduit between a data structure in a third-party data source (Microsoft Access DB)
    /// and a ArcGIS.Core.Data.Table (or ArcGIS.Core.Data.FeatureClass) in ArcGIS Pro. 
    /// </summary>
    public class ProSqlPluginTableTemplate : PluginTableTemplate, IDisposable, IPluginRowProvider
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [DllImport("kernel32.dll")]

        internal static extern uint GetCurrentThreadId();

        private readonly ProSqlExpressDb.ProSqlExpressDb _sqlDb;
        private readonly string _tableName;
        private readonly DataTable _dataTable = new DataTable();
        private Envelope _gisExtent;

        private readonly SpatialReference _spatialReference;
        private readonly ProSqlExpressTableInfo _tableInfo;

		private List<PluginField> _pluginFields = null;

		/// <summary>
		/// Ctor using ProSqlExpressDb from datasource as parameter
		/// </summary>
		/// <param name="ProSqlExpressDb">ProSqlExpressDb from datasource</param>
		/// <param name="tableInfo">ProSqlExpressTableInfo of table/feature class that has been opened</param>
		public ProSqlPluginTableTemplate(ProSqlExpressDb.ProSqlExpressDb ProSqlExpressDb, ProSqlExpressTableInfo tableInfo)
        {
            _sqlDb = ProSqlExpressDb;
            _tableName = tableInfo.TableName;
			if (!string.IsNullOrEmpty(tableInfo.SpatialRefString))
			{
				_spatialReference = SpatialReferenceBuilder.CreateSpatialReference(tableInfo.SpatialRefString);
			}
			_tableInfo = tableInfo;
        }

        /// <summary>
        /// Returns a list of PluginFields - in essence the attribute columns of the sql database table
        /// </summary>
        /// <returns>list of PluginFields</returns>
        public override IReadOnlyList<PluginField> GetFields()
        {
            if (_pluginFields == null)
            {
                _pluginFields = new List<PluginField>();
                foreach (var col in _tableInfo.FieldInfo.Columns)
                {
                    // TODO: process all field types here ... this list is not complete
                    var fieldType = FieldType.String;
                    //System.Diagnostics.Debug.WriteLine($@"{col.ColumnName} {col.ColumnDataType}");
                    if (col.ColumnName == _tableInfo.FieldInfo.ObjectIdField)
                    {
                        fieldType = FieldType.OID;
                    }
                    else
                    {
                        if (col.ColumnName.Equals(_tableInfo.GeometryFieldName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            fieldType = FieldType.Geometry;
                        }
                        else
                        {
                            switch (col.ColumnDataType.Name)
                            {
                                case nameof(DateTime):
                                    fieldType = FieldType.Date;
                                    break;
                                case nameof(Double):
                                    fieldType = FieldType.Double;
                                    break;
                                case nameof(Int16):
                                    fieldType = FieldType.Integer;
                                    break;
                                case nameof(Int32):
                                    fieldType = FieldType.Integer;
                                    break;
                                case nameof(Guid):
                                    fieldType = FieldType.GUID;
                                    break;
                                case nameof(String):
                                    fieldType = FieldType.String;
                                    break;
                                case nameof(Single):
                                    fieldType = FieldType.Single;
                                    break;
                                default:
                                    System.Diagnostics.Debug.WriteLine($@"Unsupported datatype: {col.ColumnDataType.Name} not mapped");
                                    break;
                            }
                        }
                    }
                    
                    _pluginFields.Add(new PluginField()
                        {
                            Name = col.ColumnName,
                            AliasName = col.Alias,
                            FieldType = fieldType,
                            Length = col.ColumnLength
                        });
                    
                    }
                }
            return _pluginFields;
        }

        /// <summary>
        /// Get the name of the table
        /// </summary>
        /// <returns>Table name</returns>
        public override string GetName() => _tableName;

        /// <summary>
        /// Gets whether native row count is supported
        /// </summary>
        /// <remarks>Return true if your table can get the row count without having
        /// to enumerate through all the rows (and count them)....which will be
        /// the default behavior if you return false</remarks>
        /// <returns>True or false</returns>
        public override bool IsNativeRowCountSupported() => true;

        /// <summary>
        /// Search data in this table (feature class) using a given QueryFilter
        /// </summary>
        /// <param name="queryFilter">QueryFilter to perform selection on table</param>
        /// <returns>returns a PluginCursorTemplate</returns>
        public override PluginCursorTemplate Search(QueryFilter queryFilter) =>
                                                      this.SearchInternal(queryFilter);

        /// <summary>
        /// Search data in this table (feature class) using a given SpatialQueryFilter
        /// </summary>
        /// <param name="spatialQueryFilter">SpatialQueryFilter to perform selection on table</param>
        /// <returns>returns a PluginCursorTemplate</returns>
        public override PluginCursorTemplate Search(SpatialQueryFilter spatialQueryFilter) =>
                                                      this.SearchInternal(spatialQueryFilter);

        /// <summary>
        /// Get the extent for the dataset (if it has one)
        /// </summary>
        /// <remarks>Ideally, your plugin table should return an extent even if it is
        /// empty</remarks>
        /// <returns><see cref="Envelope"/></returns>
        public override Envelope GetExtent()
        {
            if (_gisExtent == null)
            {
                var builder = new EnvelopeBuilderEx(EnvelopeBuilderEx.CreateEnvelope(
                    _tableInfo.ExtentLeft,
                    _tableInfo.ExtentBottom,
                    _tableInfo.ExtentRight,
                    _tableInfo.ExtentTop,
                    _spatialReference));
                //Assume 0 for Z
                {
                    builder.ZMin = 0;
                    builder.ZMax = 0;
                }
                builder.HasZ = false;
                builder.HasM = false;
                return builder.ToGeometry();
            }
            return _gisExtent;
        }

        /// <summary>
        /// Returns geometry type supported by this feature class
        /// </summary>
        /// <returns>GeometryType of the feature class</returns>
        public override GeometryType GetShapeType()
        {
            var geomType = GeometryType.Unknown;
            switch (_tableInfo.GeometryType)
            {
                case 1:
                    geomType = GeometryType.Point;
                    break;
                case 3:
                    geomType = GeometryType.Polyline;
                    break;
                case 4:
                    geomType = GeometryType.Polygon;
                    break;
            }
            return geomType;
        }

        #region Internal Processing

        private PluginCursorTemplate SearchInternal(QueryFilter qf)
        {
            var oids = this.ExecuteQuery(qf);
            var columns = this.GetQuerySubFields(qf);

            return new ProSqlPluginCursorTemplate(this,
                                            oids,
                                            columns,
                                            qf.OutputSpatialReference);
        }

        /// <summary>
        /// Implement querying with a query filter
        /// </summary>
        /// <param name="qf"></param>
        /// <returns></returns>
        private List<int> ExecuteQuery(QueryFilter qf)
        {
            List<int> result = new List<int>();
            SpatialQueryFilter sqf = null;
            if (qf is SpatialQueryFilter)
            {
                sqf = qf as SpatialQueryFilter;
            }
            var whereClause = string.Empty;
            if (!string.IsNullOrEmpty(qf.WhereClause))
            {
                whereClause = qf.WhereClause;
            }
            else
            {
                if (qf.ObjectIDs.Count() > 0)
                {
                    whereClause = $@"{_tableInfo.FieldInfo.ObjectIdField} in ({string.Join (",", qf.ObjectIDs)})";
                }
            }
            var subFields = string.IsNullOrEmpty (qf.SubFields) ? "*" : qf.SubFields;
            _dataTable.Clear();
            int recCount = _sqlDb.QueryTable(_tableName, subFields, whereClause, qf.PostfixClause, _dataTable);
            _dataTable.PrimaryKey = new DataColumn[] { _dataTable.Columns[_tableInfo.FieldInfo.ObjectIdField] };
            if (recCount == 0) return result;
            if (sqf == null)
            {
                result = _dataTable.AsEnumerable().Select(row => (int)row[_tableInfo.FieldInfo.ObjectIdField]).ToList();
            }
            else
            {
                result = _dataTable.AsEnumerable().Where(Row => CheckSpatialQuery (sqf, Row[_tableInfo.GeometryFieldName])).Select(row => (int)row[_tableInfo.FieldInfo.ObjectIdField]).ToList();
            }
            return result;
        }

        private bool CheckSpatialQuery (SpatialQueryFilter sqf, object geomFromDb)
        {
            var geom = GetGeometryFromBuffer ((byte [])geomFromDb, _spatialReference);
            if (geom == null)
            {
                return false;
            }
            return HasRelationship(GeometryEngine.Instance,
                                sqf.FilterGeometry, geom, sqf.SpatialRelationship);
        }

        private static Geometry GetGeometryFromBuffer (byte[] geomBuffer, SpatialReference sr)
        {
            var geomType = GetGeometryType(geomBuffer);
            switch (geomType)
            {
                case GeometryType.Point:
                    {
                        int offset = 4;
                        double x = DoubleWithNaN(BitConverter.ToDouble(geomBuffer, offset));
						offset += 8;
                        double y = DoubleWithNaN(BitConverter.ToDouble(geomBuffer, offset));

                        var mp = MapPointBuilderEx.FromEsriShape(geomBuffer, sr);
                        //System.Diagnostics.Debug.WriteLine($@"x: {x} = {mp.X} y: {y} = {mp.Y}");
                        return mp;
                    }
                case GeometryType.Polyline:
                    {
                        var line = PolylineBuilderEx.FromEsriShape(geomBuffer, sr);
                        return line;
                    }

                case GeometryType.Polygon:
                    {
                        var poly = PolygonBuilderEx.FromEsriShape(geomBuffer, sr);
                        return poly;
                    }
            }
            return null;
        }

        private static double DoubleWithNaN(double d)
        {
            return (d < -1.0e38) ? double.NaN : d;
        }

        private static GeometryType GetGeometryType(byte[] buffer, int offset = 0)
        {
            // read the shape type            
            int typeInt = BitConverter.ToInt32(buffer, offset);
            int type = (int)(typeInt & (int)0x000000FF);
            switch (type)
            {
                case 0:
                    return GeometryType.Unknown;
                case 1:
                    // A point consists of a pair of double-precision coordinates.
                case 21:
                    // A PointM consists of a pair of double-precision coordinates in the order X, Y, plus a measure M.
                case 11:
                    // A PointZM consists of a triplet of double-precision coordinates plus a measure.
                case 9:
                    // A PointZ consists of a triplet of double-precision coordinates in the order X, Y, Z where Z usually represents height.
                    return GeometryType.Point;
                case 3:
                    // PolyLine is an ordered set of vertices that consists of one or more parts. A part is a
                    // connected sequence of two or more points. Parts may or may not be connected to one
                    // another. Parts may or may not intersect one another.
                case 23:
                    // A shapefile PolyLineM consists of one or more parts. A part is a connected sequence of
                    // two or more points. Parts may or may not be connected to one another. Parts may or may
                    // not intersect one another.
                case 13:
                    // A shapefile PolyLineZM consists of one or more parts. A part is a connected sequence of
                    // two or more points. Parts may or may not be connected to one another. Parts may or may
                    // not intersect one another.
                case 10:
                    // A PolyLineZ consists of one or more parts. A part is a connected sequence of two or
                    // more points. Parts may or may not be connected to one another. Parts may or may not
                    // intersect one another.
                    return GeometryType.Polyline;
                case 5:
                    // A polygon consists of one or more rings. A ring is a connected sequence of four or more
                    // points that form a closed, non-self-intersecting loop. A polygon may contain multiple
                    // outer rings. The order of vertices or orientation for a ring indicates which side of the ring
                    // is the interior of the polygon. The neighborhood to the right of an observer walking along
                    // the ring in vertex order is the neighborhood inside the polygon. Vertices of rings defining
                    // holes in polygons are in a counterclockwise direction. Vertices for a single, ringed
                    // polygon are, therefore, always in clockwise order. The rings of a polygon are referred to
                    // as its parts.
                case 25:
                    // A PolygonM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
                case 15:
                    // A PolygonZM consists of a number of rings. A ring is a closed, non-self-intersecting loop.
                case 19:
                    // A PolygonZ consists of a number of rings. A ring is a closed, non-self-intersecting loop.
                    // A PolygonZ may contain multiple outer rings. The rings of a PolygonZ are referred to as
                    // its parts.
                    return GeometryType.Polygon;
				case 50:
					// GeneralPolyline
					return GeometryType.Polyline;
				case 51:
					// 	GeneralPolygon 
					return GeometryType.Polygon;
				case 52:
					//	GeneralPoint
					return GeometryType.Point;
				// not supported: 31: MultiPatchM
				// not supported: 32: MultiPatch
				// not supported: 53: GeneralMultiPoint
				// not supported: 54: GeneralMultiPatch
				default:
                    throw new Exception($@"Unknown shape type {type}");
            }
        }

        internal static bool HasRelationship(IGeometryEngine engine,
                                          Geometry geom1,
                                          Geometry geom2,
                                          SpatialRelationship relationship)
        {
            switch (relationship)
            {
                case SpatialRelationship.Intersects:
                    return engine.Intersects(geom1, geom2);
                case SpatialRelationship.IndexIntersects:
                    return engine.Intersects(geom1, geom2);
                case SpatialRelationship.EnvelopeIntersects:
                    return engine.Intersects(geom1.Extent, geom2.Extent);
                case SpatialRelationship.Contains:
                    return engine.Contains(geom1, geom2);
                case SpatialRelationship.Crosses:
                    return engine.Crosses(geom1, geom2);
                case SpatialRelationship.Overlaps:
                    return engine.Overlaps(geom1, geom2);
                case SpatialRelationship.Touches:
                    return engine.Touches(geom1, geom2);
                case SpatialRelationship.Within:
                    return engine.Within(geom1, geom2);
            }
            return false;//unknown relationship
        }

        private List<string> GetQuerySubFields(QueryFilter qf)
        {
            //Honor Subfields in Query Filter
            string columns = qf.SubFields ?? "*";
            List<string> subFields;
            if (columns == "*")
            {
                subFields = this.GetFields().Select(col => col.Name.ToUpper()).ToList();
            }
            else
            {
                var names = columns.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                subFields = names.Select(n => n.ToUpper()).ToList();
            }

            return subFields;
        }

        #endregion Internal Processing

        #region IPluginRowProvider

        /// <summary>
        /// Find a given row (using Object ID) and retrieve attributes using columnFilter and output spatial reference
        /// </summary>
        /// <param name="oid">Search for this record using this Object ID</param>
        /// <param name="columnFilter">List of Column Names to be returned</param>
        /// <param name="srout">project spatial data using this output spatial reference</param>
        /// <returns>PlugInRow</returns>
        public PluginRow FindRow(int oid, IEnumerable<string> columnFilter, SpatialReference srout)
        {
            Geometry shape = null;

            List<object> values = new List<object>();
            // oid happens to be the primary key as well
            var row = _dataTable.Rows.Find (oid);
            //The order of the columns in the returned rows ~must~ match
            //GetFields. If a column is filtered out, an empty placeholder must
            //still be provided even though the actual value is skipped
            var columnNames = this.GetFields().Select(col => col.Name.ToUpper()).ToList();
            foreach (var colName in columnNames)
            {
                if (columnFilter.Contains(colName))
                {
                    //special handling for shape
                    if (colName.Equals (_tableInfo.GeometryFieldName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var geomBuffer = (byte [])row[_tableInfo.GeometryFieldName];
                        shape = GetGeometryFromBuffer(geomBuffer, _spatialReference);
                        if (srout != null)
                        {
                            if (!srout.Equals(_spatialReference))
                                shape = GeometryEngine.Instance.Project(shape, srout);
                        }
                        values.Add(shape);
                    }
                    else
                    {
                        values.Add(row[colName]);
                    }
                }
                else
                {
                    values.Add(DBNull.Value);//place holder
                }
            }
            return new PluginRow() { Values = values };
        }

        #endregion IPluginRowProvider

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Clean up resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_dataTable == null)
                    return;

                if (disposing)
                {
                    _dataTable?.Clear();
                    _gisExtent = null;
                }
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProPluginTableTemplate()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

    }
}
