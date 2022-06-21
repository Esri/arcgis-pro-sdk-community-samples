/*

   Copyright 2018 Esri

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
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Core.Geometry;
using Microsoft.VisualBasic.FileIO;
using SimplePointPlugin.Helpers;

namespace SimplePointPlugin
{
  /// <summary>
  /// (Custom) interface the sample uses to extract row information from the
  /// plugin table
  /// </summary>
  internal interface IPluginRowProvider
  {
    PluginRow FindRow(long oid, IEnumerable<string> columnFilter, SpatialReference sr);
  }

  /// <summary>
  /// Implements a plugin table.
  /// </summary>
  /// <remarks>The plugin table appears as an ArcGIS.Core.Data.Table or FeatureClass to
  /// .NET clients (add-ins) and as an ITable or IFeatureClass to native clients (i.e. Pro)
  /// </remarks>
  public class ProPluginTableTemplate : PluginTableTemplate, IDisposable, IPluginRowProvider
  {

    private string _path;
    private string _name;
    private DataTable _table;
    private RBush.RBush<RBushCoord3D> _rtree;
    private RBush.Envelope _extent;
    private Envelope _gisExtent;
    private SpatialReference _sr;
    private bool _hasZ = false;

    internal ProPluginTableTemplate(string path, string name, SpatialReference sr = null)
    {
      _path = path;
      _name = name;
      _rtree = new RBush.RBush<RBushCoord3D>();
      _sr = sr ?? SpatialReferences.WGS84;
      Open();
    }

    /// <summary>
    /// Get the name of the table
    /// </summary>
    /// <returns>Table name</returns>
    public override string GetName() => _name;

    /// <summary>
    /// Gets whether native row count is supported
    /// </summary>
    /// <remarks>Return true if your table can get the row count without having
    /// to enumerate through all the rows (and count them)....which will be
    /// the default behavior if you return false</remarks>
    /// <returns>True or false</returns>
    public override bool IsNativeRowCountSupported() => true;

    /// <summary>
    /// Gets the native row count (if IsNativeRowCountSupported is true)
    /// </summary>
    /// <returns>The row count</returns>
    public override long GetNativeRowCount() => _rtree?.Count ?? _table.Rows.Count;

    /// <summary>
    /// Search the underlying plugin table using the input QueryFilter
    /// </summary>
    /// <param name="queryFilter"></param>
    /// <remarks>If the PluginDatasourceTemplate.IsQueryLanguageSupported returns
    /// false, the WhereClause will always be empty.<br/>
    /// The QueryFilter is never null (even if the client passed in null to the "outside"
    /// table or feature class).<br/>
    /// A FID set in the ObjectIDs collection of the query filter, if present, acts as
    /// the "super" set - or constraint - from which all selections should be made. 
    /// In other words, if the FID set contains ids {1,5,6,10} then a WhereClause
    /// on the query filter can only select from {1,5,6,10} and not from any other
    /// records.</remarks>
    /// <returns><see cref="PluginCursorTemplate"/></returns>
    public override PluginCursorTemplate Search(QueryFilter queryFilter) =>
                                                  this.SearchInternal(queryFilter);

    /// <summary>
    /// Search the underlying plugin table using the input SpatialQueryFilter
    /// </summary>
    /// <remarks>A SpatialQueryFilter cann only be used by clients if the plugin
    /// table returns a GeometryType other than Unknown from GetShapeType().</remarks>
    /// <param name="spatialQueryFilter"></param>
    /// <returns><see cref="PluginCursorTemplate"/></returns>
    public override PluginCursorTemplate Search(SpatialQueryFilter spatialQueryFilter) =>
                                                  this.SearchInternal(spatialQueryFilter);
    /// <summary>
    /// Gets the supported GeometryType if there is one, otherwise Unknown
    /// </summary>
    /// <remarks>Plugins returning a geometry type get a FeatureClass (which is also a Table) wrapper 
    /// and can be used as data sources for layers. Plugins returning a geometry type of Unknown
    /// get a Table wrapper and can be used as data sources for StandAloneTables only.</remarks>
    /// <returns></returns>
    public override GeometryType GetShapeType()
    {
      //Note: empty tables treated as non-geometry
      return _table.Columns.Contains("SHAPE") ? GeometryType.Point : GeometryType.Unknown;
    }

    /// <summary>
    /// Get the extent for the dataset (if it has one)
    /// </summary>
    /// <remarks>Ideally, your plugin table should return an extent even if it is
    /// empty</remarks>
    /// <returns><see cref="Envelope"/></returns>
    public override Envelope GetExtent()
    {
      if (this.GetShapeType() != GeometryType.Unknown)
      {
        if (_gisExtent == null)
        {
          _gisExtent = _extent.ToEsriEnvelope(_sr, _hasZ);
        }
      }
      return _gisExtent;
    }

    /// <summary>
    /// Get the collection of fields accessible on the plugin table
    /// </summary>
    /// <remarks>The order of returned columns in any rows must match the
    /// order of the fields specified from GetFields()</remarks>
    /// <returns><see cref="IReadOnlyList{PluginField}"/></returns>
    public override IReadOnlyList<PluginField> GetFields()
    {
      var pluginFields = new List<PluginField>();
      foreach (var col in _table.Columns.Cast<DataColumn>())
      {
        var fieldType = ArcGIS.Core.Data.FieldType.String;
        //special handling for OBJECTID and SHAPE
        if (col.ColumnName == "OBJECTID")
        {
          fieldType = ArcGIS.Core.Data.FieldType.OID;
        }
        else if (col.ColumnName == "SHAPE")
        {
          fieldType = ArcGIS.Core.Data.FieldType.Geometry;
        }
        else if (col.ColumnName.Length == 1 || col.ColumnName.StartsWith("POINT_"))
        {
          // columns: X or Y
          fieldType = ArcGIS.Core.Data.FieldType.Double;
        }
        else if (col.ColumnName.StartsWith("LONG_"))
        {
          // Long datatype
          fieldType = ArcGIS.Core.Data.FieldType.Integer;
        }
        else if (col.ColumnName.StartsWith("DATE_"))
        {
          // DateTime datatype
          fieldType = ArcGIS.Core.Data.FieldType.Date;
        }
        pluginFields.Add(new PluginField()
        {
          Name = col.ColumnName,
          AliasName = col.Caption,
          FieldType = fieldType
        });
      }
      return pluginFields;
    }

    #region IPluginRowProvider

    /// <summary>
    /// Custom interface specific to the way the sample is implemented.
    /// </summary>
    public PluginRow FindRow(long oid, IEnumerable<string> columnFilter, SpatialReference srout)
    {
      Geometry shape = null;

      List<object> values = new List<object>();
      var row = _table.Rows.Find(oid);
      //The order of the columns in the returned rows ~must~ match
      //GetFields. If a column is filtered out, an empty placeholder must
      //still be provided even though the actual value is skipped
      var columnNames = this.GetFields().Select(col => col.Name.ToUpper()).ToList();

      foreach (var colName in columnNames)
      {
        if (columnFilter.Contains(colName))
        {
          //special handling for shape
          if (colName == "SHAPE")
          {
            var buffer = row["SHAPE"] as Byte[];
            shape = MapPointBuilderEx.FromEsriShape(buffer, _sr);
            if (srout != null)
            {
              if (!srout.Equals(_sr))
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

    #region Private

    /// <summary>
    /// Implementation of reading a csv which is specific to the way this sample
    /// is implemented. Your mileage may vary. Change to suit your purposes as
    /// needed.
    /// </summary>
    private void Open()
    {
      var lstDoubleFields = new List<int>();
      var lstLongFields = new List<int>();
      var lstDateFields = new List<int>();
      //Read in the CSV
      TextFieldParser parser = new TextFieldParser(_path);
      parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
      parser.SetDelimiters(",");
      parser.HasFieldsEnclosedInQuotes = true;

      //Initialize our data table
      _table = new DataTable();
      //dataTable.PrimaryKey = new DataColumn("OBJECTID", typeof(long));
      var oid = new DataColumn("OBJECTID", typeof(long))
      {
        AutoIncrement = true,
        AutoIncrementSeed = 1
      };
      _table.Columns.Add(oid);
      _table.PrimaryKey = new DataColumn[] { oid };

      //First line must be the column headings
      var fieldIdx = 0;
      foreach (var field in parser.ReadFields())
      {
        var field_name = field.Replace(' ', '_').ToUpper();
        if (field_name.Length == 1 || field_name.StartsWith("POINT_"))
        {
          // field name is X or Y
          _table.Columns.Add(new DataColumn(field_name, typeof(double)));
          lstDoubleFields.Add(fieldIdx);
        }
        else if (field_name.StartsWith("LONG_"))
        {
          _table.Columns.Add(new DataColumn(field_name, typeof(long)));
          lstLongFields.Add(fieldIdx);
        }
        else if (field_name.StartsWith("DATE_"))
        {
          _table.Columns.Add(new DataColumn(field_name, typeof(DateTime)));
          lstDateFields.Add(fieldIdx);
        }
        else _table.Columns.Add(new DataColumn(field_name, typeof(string)));
        fieldIdx++;
      }

      //For spatial data...
      //Domain to verify coordinates (2D)
      var sr_extent = new RBush.Envelope(
        minX: _sr.Domain.XMin,
        minY: _sr.Domain.YMin,
        maxX: _sr.Domain.XMax,
        maxY: _sr.Domain.YMax
      );

      //default to the Spatial Reference domain
      _extent = sr_extent;

      bool hasSpatialData = false;
      bool haveDeterminedSpatialData = false;

      //The first two fields are assumed to be the X and Y coordinates of a point
      //otherwise the file is treated as a table and not a feature class
      //The third field (if there is one) is checked for Z.
      while (!parser.EndOfData)
      {
        var values = parser.ReadFields();

        if (!haveDeterminedSpatialData)
        {
          if (values.Count() >= 2)
          {
            double test = 0;
            if (Double.TryParse(values[0], out test))
            {
              hasSpatialData = Double.TryParse(values[1], out test);
              if (hasSpatialData)
              {
                //add a shape column
                _table.Columns.Add(new DataColumn("SHAPE", typeof(System.Byte[])));
                //do we have a Z?
                double z = 0;
                _hasZ = false;
                if (values.Count() >= 3)
                {
                  _hasZ = Double.TryParse(values[2], out z);
                }
              }
            }
          }
          haveDeterminedSpatialData = true;
        }

        //load the datatable
        var row = _table.NewRow();
        for (int c = 0; c < values.Length; c++)
        {
          //TODO Deal with nulls!!
          // check double types 
          if (lstDoubleFields.Contains(c))
          {
            row[c + 1] = System.DBNull.Value;
            // field value is double
            if (Double.TryParse(values[c], out double dValue))
            {
              row[c + 1] = dValue;//Column "0" is our objectid
            }
          }
          else if (lstLongFields.Contains(c))
          {
            row[c + 1] = System.DBNull.Value;
            // field value is long
            if (long.TryParse(values[c], out long iValue))
            {
              row[c + 1] = iValue;//Column "0" is our objectid
            }
          }
          else if (lstDateFields.Contains(c))
          {
            row[c + 1] = System.DBNull.Value;
            // field value is datetime
            if (DateTime.TryParse(values[c], out DateTime dateValue))
            {
              row[c + 1] = dateValue;//Column "0" is our objectid
            }
          }
          else
          {
            row[c + 1] = values[c] ?? "";//Column "0" is our objectid
          }
        }

        if (hasSpatialData)
        {
          double x = 0, y = 0, z = 0;
          //TODO Deal with nulls!
          Double.TryParse(values[0], out x);
          Double.TryParse(values[1], out y);

          //do we have a Z?
          if (_hasZ)
          {
            //TODO Deal with nulls!
            Double.TryParse(values[2], out z);
          }

          //ensure the coordinate is within bounds
          var coord = new Coordinate3D(x, y, z);
          if (!sr_extent.Contains2D(coord))
            throw new GeodatabaseFeatureException(
              "The feature falls outside the defined spatial reference");

          //store it
          row["SHAPE"] = coord.ToMapPoint().ToEsriShape();

          //add it to the index
          var rbushCoord = new RBushCoord3D(coord, (long)row["OBJECTID"]);
          _rtree.Insert(rbushCoord);

          //update max and min for use in the extent
          if (_rtree.Count == 1)
          {
            //first record
            _extent = rbushCoord.Envelope;
          }
          else
          {
            _extent = rbushCoord.Envelope.Union2D(_extent);
          }
        }
        _table.Rows.Add(row);
      }
    }

    private PluginCursorTemplate SearchInternal(QueryFilter qf)
    {
      var oids = this.ExecuteQuery(qf);
      var columns = this.GetQuerySubFields(qf);

      return new ProPluginCursorTemplate(this,
                                      oids,
                                      columns,
                                      qf.OutputSpatialReference);
    }

    /// <summary>
    /// Implement querying with a query filter
    /// </summary>
    /// <param name="qf"></param>
    /// <returns></returns>
    private List<long> ExecuteQuery(QueryFilter qf)
    {

      //are we empty?
      if (_table.Rows.Count == 0)
        return new List<long>();

      SpatialQueryFilter sqf = null;
      if (qf is SpatialQueryFilter)
      {
        sqf = qf as SpatialQueryFilter;
      }

      List<long> result = new List<long>();
      bool emptyQuery = true;

      //fidset - this takes precedence over anything else in
      //the query. If a fid set is specified then all selections
      //for the given query are intersections from the fidset
      if (qf.ObjectIDs.Count() > 0)
      {
        emptyQuery = false;

        result = null;
        result = _table.AsEnumerable().Where(
          row => qf.ObjectIDs.Contains((long)row["OBJECTID"]))
          .Select(row => (long)row["OBJECTID"]).ToList();

        //anything selected?
        if (result.Count() == 0)
        {
          //no - specifying a fidset trumps everything. The client
          //specified a fidset and nothing was selected so we are done
          return result;
        }
      }

      //where clause
      if (!string.IsNullOrEmpty(qf.WhereClause))
      {
        emptyQuery = false;
        var sort = "OBJECTID";//default
        if (!string.IsNullOrEmpty(qf.PostfixClause))
        {
          //The underlying System.Data.DataTable used by the sample supports "ORDER BY"
          //It should be a comma-separated list of column names and a default direction
          //COL1 ASC, COL2 DESC  (note: "ASC" is not strictly necessary)
          //Anything else and there will be an exception
          sort = qf.PostfixClause;
        }

        //do the selection
        var oids = _table.Select(qf.WhereClause, sort)
                     .Select(row => (long)row["OBJECTID"]).ToList();

        //consolidate whereclause selection with fidset
        if (result.Count > 0 && oids.Count() > 0)
        {
          var temp = result.Intersect(oids).ToList();
          result = null;
          result = temp;
        }
        else
        {
          result = null;
          result = oids;
        }

        //anything selected?
        if (result.Count() == 0)
        {
          //no - where clause returned no rows or returned no rows
          //common to the specified fidset
          return result;
        }
      }

      //filter geometry for spatial select
      if (sqf != null)
      {
        if (sqf.FilterGeometry != null)
        {
          emptyQuery = false;

          bool filterIsEnvelope = sqf.FilterGeometry is Envelope;
          //search spatial index first
          var extent = sqf.FilterGeometry.Extent;
          var candidates = _rtree.Search(extent.ToRBushEnvelope());

          //consolidate filter selection with current fidset
          if (result.Count > 0 && candidates.Count > 0)
          {
            var temp = candidates.Where(pt => result.Contains(pt.ObjectID)).ToList();
            candidates = null;
            candidates = temp;
          }
          //anything selected?
          if (candidates.Count == 0)
          {
            //no - filter query returned no rows or returned no rows
            //common to the specified fidset
            return new List<long>();
          }

          //do we need to refine the spatial search?
          if (filterIsEnvelope &&
            (sqf.SpatialRelationship == SpatialRelationship.Intersects ||
            sqf.SpatialRelationship == SpatialRelationship.IndexIntersects ||
            sqf.SpatialRelationship == SpatialRelationship.EnvelopeIntersects))
          {
            //no. This is our final list
            return candidates.Select(pt => pt.ObjectID).OrderBy(oid => oid).ToList();
          }

          //refine based on the exact geometry and relationship
          List<long> oids = new List<long>();
          foreach (var candidate in candidates)
          {
            if (GeometryEngine.Instance.HasRelationship(
                    sqf.FilterGeometry, candidate.ToMapPoint(_sr),
                      sqf.SpatialRelationship))
            {
              oids.Add(candidate.ObjectID);
            }
          }
          //anything selected?
          if (oids.Count == 0)
          {
            //no - further processing of the filter geometry query
            //returned no rows
            return new List<long>();
          }
          result = null;
          //oids has already been consolidated with any specified fidset
          result = oids;
        }
      }

      //last chance - did we execute any type of query?
      if (emptyQuery)
      {
        //no - the default is to return all rows
        result = null;
        result = _table.Rows.Cast<DataRow>()
          .Select(row => (long)row["OBJECTID"]).OrderBy(x => x).ToList();
      }
      return result;
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

    #endregion Private

    #region IDisposable

    private bool _disposed = false;
    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      //TODO free unmanaged resources here
      System.Diagnostics.Debug.WriteLine("Table being disposed");

      if (_disposed)
        return;

      if (disposing)
      {
        _table?.Clear();
        _table = null;
        _rtree?.Clear();
        _rtree = null;
        _sr = null;
        _gisExtent = null;
      }
      _disposed = true;
    }
    #endregion
  }
}