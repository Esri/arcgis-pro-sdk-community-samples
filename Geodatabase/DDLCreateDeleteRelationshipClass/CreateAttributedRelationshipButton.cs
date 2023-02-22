using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DDLCreateDeleteRelationshipClass
{
    internal class CreateAttributedRelationshipButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = SchemaBuilder.CreateGeodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\temp\mySampleGeoDatabase.gdb"))))
                {
                    SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);
                    Operations.CreateAttributedRelationship(schemaBuilder);
                }
            });
        }
      
    }
}
