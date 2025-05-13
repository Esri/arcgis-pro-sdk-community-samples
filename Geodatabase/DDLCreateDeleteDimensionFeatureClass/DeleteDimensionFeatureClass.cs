using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using ArcGIS.Core.Data.Mapping;

namespace DDLCreateDeleteDimensionFeatureClass
{
  internal class DeleteDimensionFeatureClass : Button
  {
    protected override void OnClick()
    {
      QueuedTask.Run(() =>
      {
        using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Module1.geodatabasePath))))
        using (DimensionFeatureClassDefinition dimensionFeatureClassDefinition = geodatabase.GetDefinition<DimensionFeatureClassDefinition>(Module1.dimensionFeautreClassName))
        {
          SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);
          Operations.DeleteDimensionFeatureClass(schemaBuilder, dimensionFeatureClassDefinition);
        }
      });
    }
  }
}
