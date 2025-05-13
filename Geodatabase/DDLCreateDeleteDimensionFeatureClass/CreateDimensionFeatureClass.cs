using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;

namespace DDLCreateDeleteDimensionFeatureClass
{
  internal class CreateDimensionFeatureClass : Button
  {
    protected override void OnClick()
    {
      QueuedTask.Run(() =>
      {
        using (Geodatabase geodatabase = SchemaBuilder.CreateGeodatabase(new FileGeodatabaseConnectionPath(new Uri(Module1.geodatabasePath))))
        {
          SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);
          Operations.CreateDimensionFeatureClass(schemaBuilder);
        }
      });
    }
  }
}
