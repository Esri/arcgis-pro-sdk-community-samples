/*

   Copyright 2017 Esri

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
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;
using ProDataReaderImageMetadata;

namespace ProDataReaderProJpgPluginDatasource
{
  /// <summary>
  /// (Custom) interface the sample uses to extract row information from the
  /// plugin table
  /// </summary>
  internal interface IPluginRowProvider
  {
    PluginRow FindRow(int oid, IEnumerable<string> columnFilter, SpatialReference sr);
  }

  public class ProJpgPluginTableTemplate : PluginTableTemplate, IDisposable, IPluginRowProvider
  {
    private readonly DataTable _dataTable = new DataTable();
    private readonly List<XimgInfo> _ximgInfos;
    private readonly string _tableName;
    private readonly SpatialReference _spatialReference;
    private Envelope _gisExtent;
    private List<PluginField> _pluginFields = null;

    private const string GeometryFieldName = "Shape";
    private const string ObjectIdFieldName = "ObjectId";
    private const string LongFieldName = "Longitude";
    private const string LatFieldName = "Latitude";
    private const string AltFieldName = "Altitude";
    private const string DateTimeFieldName = "TakenOn";
    private const string DirectionFieldName = "Direction";
    //private const string ImageFieldName = "Photo";
    private const string PathFieldName = "Path";
    private const string ContentTypeFieldName = "CONTENT_TYPE";
    private const string NameFieldName = "Name";

    /// <summary>
    /// Ctor using ImageMetadata lib as the image definition
    /// </summary>
    /// <param name="ximgInfos">Image metadata</param>
    /// <param name="tableName">Name of the table</param>
    public ProJpgPluginTableTemplate(List<XimgInfo> ximgInfos, string tableName)
    {
      this._ximgInfos = ximgInfos;
      this._tableName = tableName;
      _spatialReference = SpatialReferences.WGS84;
      CreateTable(this._tableName, this._ximgInfos);
    }

    /// <summary>
    /// Returns a list of PluginFields - in essence the attribute columns of the access database table
    /// </summary>
    /// <returns>list of PluginFields</returns>
    public override IReadOnlyList<PluginField> GetFields()
    {
      if (_pluginFields == null)
      {
        _pluginFields = new List<PluginField>();
        foreach (var col in _dataTable.Columns.Cast<DataColumn>())
        {
          // TODO: process all field types here ... this list is not complete
          var fieldType = FieldType.String;
          System.Diagnostics.Debug.WriteLine($@"{col.ColumnName} {col.DataType}");
          if (col.ColumnName == ObjectIdFieldName ||
            col.ColumnName == GeometryFieldName)
          {
            fieldType = col.ColumnName == GeometryFieldName ? FieldType.Geometry : FieldType.OID;
          }
          else
          {
            switch (col.DataType.Name)
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
              case nameof(System.Drawing.Image):
                fieldType = FieldType.Blob;
                break;
              default:
                System.Diagnostics.Debug.WriteLine($@"Unsupported datatype: {col.DataType.Name} not mapped");
                break;
            }
          }
          _pluginFields.Add(new PluginField()
          {
            Name = col.ColumnName,
            AliasName = col.ColumnName,
            FieldType = fieldType
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

    public override GeometryType GetShapeType()
    {
      return GeometryType.Point;
    }

    /// <summary>
    /// Get the extent for the dataset (if it has one)
    /// </summary>
    /// <remarks>Ideally, your plugin table should return an extent even if it is
    /// empty</remarks>
    /// <returns><see cref="Envelope"/></returns>
    public override Envelope GetExtent() { return _gisExtent; }

    #region Internal Processing

    private void CreateTable(string tableName, List<XimgInfo> ximgInfos)
    {
      _dataTable.TableName = tableName;
      var oidCol = new DataColumn(ObjectIdFieldName, typeof(Int32))
      {
        AutoIncrement = true,
        AutoIncrementSeed = 1
      };
      _dataTable.Columns.Add(oidCol);
      _dataTable.PrimaryKey = new DataColumn[] { oidCol };
      _dataTable.Columns.Add(new DataColumn(GeometryFieldName, typeof(ArcGIS.Core.Geometry.Geometry)));
      _dataTable.Columns.Add(new DataColumn(LongFieldName, typeof(Double)));
      _dataTable.Columns.Add(new DataColumn(LatFieldName, typeof(Double)));
      _dataTable.Columns.Add(new DataColumn(AltFieldName, typeof(Double)));
      _dataTable.Columns.Add(new DataColumn(DirectionFieldName, typeof(Double)));
      _dataTable.Columns.Add(new DataColumn(DateTimeFieldName, typeof(DateTime)));
      //_dataTable.Columns.Add(new DataColumn(ImageFieldName, typeof(byte[])));
      _dataTable.Columns.Add(new DataColumn(PathFieldName, typeof(string)));
      _dataTable.Columns.Add(new DataColumn(ContentTypeFieldName, typeof(string)));
      _dataTable.Columns.Add(new DataColumn(NameFieldName, typeof(string)));
      int objectId = 1;
      foreach (var ximgInfo in ximgInfos)
      {
        var newRow = _dataTable.NewRow();
        newRow[ObjectIdFieldName] = objectId++;
        // let's make a 3d shape
        var coord = new Coordinate3D(ximgInfo.Longitude, ximgInfo.Latitude, ximgInfo.Altitude);
        var mp = MapPointBuilderEx.CreateMapPoint(coord, _spatialReference);
        newRow[GeometryFieldName] = mp;
        newRow[LongFieldName] = ximgInfo.Longitude;
        newRow[LatFieldName] = ximgInfo.Latitude;
        newRow[AltFieldName] = ximgInfo.Altitude;
        newRow[DirectionFieldName] = ximgInfo.ImageBearing;
        newRow[DateTimeFieldName] = ximgInfo.DateTimeOriginal;
        //newRow[ImageFieldName] = ximgInfo.Image;

        newRow[PathFieldName] = $@"file:///{ximgInfo.Path}";
        newRow[ContentTypeFieldName] = @"image/jpeg";
        newRow[NameFieldName] = ximgInfo.Name;
        _dataTable.Rows.Add(newRow);
        _gisExtent = _gisExtent == null
          ? mp.Extent : _gisExtent.Union(mp.Extent);
      }
    }

    private PluginCursorTemplate SearchInternal(QueryFilter qf)
    {
      var oids = this.ExecuteQuery(qf);
      var columns = this.GetQuerySubFields(qf);

      return new ProJpgPluginCursorTemplate(this,
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
          whereClause = $@"{ObjectIdFieldName} in ({string.Join(",", qf.ObjectIDs)})";
        }
      }
      var subFields = string.IsNullOrEmpty(qf.SubFields) ? "*" : qf.SubFields;
      var selectRows = _dataTable.Select(whereClause, qf.PostfixClause);
      int recCount = selectRows.Length;
      if (recCount == 0) return result;
      if (sqf == null)
      {
        result = selectRows.Select(row => (int)row[ObjectIdFieldName]).ToList();
      }
      else
      {
        result = selectRows.Where(Row => CheckSpatialQuery(sqf, Row[GeometryFieldName] as Geometry)).Select(row => (int)row[ObjectIdFieldName]).ToList();
      }
      return result;
    }

    private bool CheckSpatialQuery(SpatialQueryFilter sqf, Geometry geom)
    {
      if (geom == null)
      {
        return false;
      }
      return HasRelationship(GeometryEngine.Instance,
                sqf.FilterGeometry, geom, sqf.SpatialRelationship);
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
      var row = _dataTable.Rows.Find(oid);
      //The order of the columns in the returned rows ~must~ match
      //GetFields. If a column is filtered out, an empty placeholder must
      //still be provided even though the actual value is skipped
      var columnNames = this.GetFields().Select(col => col.Name.ToUpper()).ToList();
      foreach (var colName in columnNames)
      {
        if (columnFilter.Contains(colName))
        {
          //special handling for shape
          if (colName == GeometryFieldName)
          {
            shape = row[GeometryFieldName] as Geometry;
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
          values.Add(System.DBNull.Value);//place holder
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
