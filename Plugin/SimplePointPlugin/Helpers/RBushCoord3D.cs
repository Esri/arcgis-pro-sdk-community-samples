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
using RBush;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SimplePointPlugin.Helpers
{
  internal class RBushCoord3D : ISpatialData, IComparable<RBushCoord3D>
  {

    public static readonly double Tolerance = 0.000001;
    private Envelope _envelope;
    private long _oid;
    private ArcGIS.Core.Geometry.Coordinate3D _coord;

    public RBushCoord3D(ArcGIS.Core.Geometry.Coordinate3D coord, long oid)
    {
      _oid = oid;

      //save the original
      _coord = coord;

      //RBush requires a reference
      //to the envelope so we can't construct it "on-the-fly"
      _envelope = new Envelope(
        MinX: coord.X - Tolerance,
        MinY: coord.Y - Tolerance,
        MaxX: coord.X + Tolerance,
        MaxY: coord.Y + Tolerance);

    }

    //ISpatialData from RBush
    public ref readonly Envelope Envelope => ref _envelope;

    public long ObjectID => _oid;

    public ArcGIS.Core.Geometry.Coordinate3D Coordinate3D => _coord;

    public int CompareTo(RBushCoord3D other)
    {
      return this.ObjectID.CompareTo(other.ObjectID);
    }
  }

  internal static class RBushExtensions
  {

    internal static bool Contains2D(this Envelope envelope, ArcGIS.Core.Geometry.Coordinate3D coord)
    {
      //we are only comparing the x and y!
      return (coord.X < envelope.MaxX &&
              coord.X > envelope.MinX &&
              coord.Y < envelope.MaxY &&
              coord.Y > envelope.MinY);
    }

    internal static Envelope Union2D(this Envelope envelope, Envelope other)
    {
      return new Envelope(
        MinX: Math.Min(envelope.MinX, other.MinX),
        MinY: Math.Min(envelope.MinY, other.MinY),
        MaxX: Math.Max(envelope.MaxX, other.MaxX),
        MaxY: Math.Max(envelope.MaxY, other.MaxY));
    }

    internal static ArcGIS.Core.Geometry.Envelope ToEsriEnvelope(this Envelope envelope,
                                                       ArcGIS.Core.Geometry.SpatialReference sr = null,
                                                       bool hasZ = false,
                                                       bool hasM = false)
    {
      var builder = new ArcGIS.Core.Geometry.EnvelopeBuilderEx(ArcGIS.Core.Geometry.EnvelopeBuilderEx.CreateEnvelope(
                  envelope.MinX,
                  envelope.MinY,
                  envelope.MaxX,
                  envelope.MaxY,
                  sr));

      //Assume 0 for Z
      if (hasZ)
      {
        builder.ZMin = 0;
        builder.ZMax = 0;
      }
      builder.HasZ = hasZ;
      builder.HasM = hasM;
      return builder.ToGeometry();
    }

    internal static Envelope ToRBushEnvelope(this ArcGIS.Core.Geometry.Envelope esriEnvelope)
    {
      //Spatial index does not handle Z
      return new Envelope(
        MinX: esriEnvelope.XMin,
        MinY: esriEnvelope.YMin,
        MaxX: esriEnvelope.XMax,
        MaxY: esriEnvelope.YMax);
    }

    internal static ArcGIS.Core.Geometry.MapPoint ToMapPoint(this RBushCoord3D rbushCoord,
                                                 ArcGIS.Core.Geometry.SpatialReference sr)
    {
      return ArcGIS.Core.Geometry.MapPointBuilderEx.CreateMapPoint(rbushCoord.Coordinate3D, sr);
    }

    internal static bool HasRelationship(this ArcGIS.Core.Geometry.IGeometryEngine engine,
                                          ArcGIS.Core.Geometry.Geometry geom1,
                                          ArcGIS.Core.Geometry.Geometry geom2,
                                          ArcGIS.Core.Data.SpatialRelationship relationship)
    {

      switch (relationship)
      {
        case ArcGIS.Core.Data.SpatialRelationship.Intersects:
          return engine.Intersects(geom1, geom2);
        case ArcGIS.Core.Data.SpatialRelationship.IndexIntersects:
          return engine.Intersects(geom1, geom2);
        case ArcGIS.Core.Data.SpatialRelationship.EnvelopeIntersects:
          return engine.Intersects(geom1.Extent, geom2.Extent);
        case ArcGIS.Core.Data.SpatialRelationship.Contains:
          return engine.Contains(geom1, geom2);
        case ArcGIS.Core.Data.SpatialRelationship.Crosses:
          return engine.Crosses(geom1, geom2);
        case ArcGIS.Core.Data.SpatialRelationship.Overlaps:
          return engine.Overlaps(geom1, geom2);
        case ArcGIS.Core.Data.SpatialRelationship.Touches:
          return engine.Touches(geom1, geom2);
        case ArcGIS.Core.Data.SpatialRelationship.Within:
          return engine.Within(geom1, geom2);
      }
      return false;//unknown relationship
    }
  }
}