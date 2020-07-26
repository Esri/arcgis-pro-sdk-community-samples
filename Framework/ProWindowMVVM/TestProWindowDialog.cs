/*

   Copyright 2020 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ProWindowMVVM
{
  internal class TestProWindowDialog : Button
  {
    protected override async void OnClick()
    {
      try
      {
        // this connection file has no user/password entered
        var sdeConProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.SQLServer)
        {
          AuthenticationMode = AuthenticationMode.DBMS,
          Instance = @"localhost"
        };
        // prompt for username / password using a ProWindow
        var userPwDlg = new ProWindowDialog();
        // ProWindowDialog uses Module1.ConnectionString to display 
        // initial values
        Module1.ConnectionString = $@"{sdeConProperties.AuthenticationMode} {sdeConProperties.DBMS} {sdeConProperties.Instance}";
        userPwDlg.Owner = FrameworkApplication.Current.MainWindow;
        userPwDlg.Closed += (o, e) => {
          // in case special processing after dialog closed has be done
          System.Diagnostics.Debug.WriteLine("Pro Window Dialog closed");
        };
        var dlgResult = userPwDlg.ShowDialog();
        MessageBox.Show($@"ProWindow Dialog was closed with ShowDialog return value: {dlgResult.Value}");
        if (Module1.HasCredentials == false) return;
        sdeConProperties.User = Module1.UserName;
        sdeConProperties.Password = Module1.Password;

        var hasConnected = await QueuedTask.Run(() =>
        {
          bool bConnected = false;
          try
          {
            using (Geodatabase sqlServerGeodatabase = new Geodatabase(sdeConProperties))
            {
              IReadOnlyList<Definition> fcList = sqlServerGeodatabase.GetDefinitions<FeatureClassDefinition>();
              foreach (var fc in fcList)
                System.Diagnostics.Debug.WriteLine(fc.GetName());
              // Use the geodatabase.
              bConnected = true;
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show($@"Unable to connect: {Module1.ConnectionString} reason: {ex.Message}");
          }
          return bConnected;
        });
        if (hasConnected) MessageBox.Show("Connected");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.Message}");
      }
    }
  }
}
