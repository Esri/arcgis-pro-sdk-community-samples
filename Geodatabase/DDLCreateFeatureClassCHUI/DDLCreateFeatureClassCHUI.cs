//   Copyright 2026 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Hosting;
using Microsoft.Win32;

namespace DDLCreateFeatureClassCHUI
{

  /// <summary>
  /// Represents the entry point of the application.
  /// </summary>
  /// <remarks>
  /// This class contains the <see cref="Main"/> method, which initializes the application host and
  /// starts the WPF application. The <c>[STAThread]</c> attribute is required for the application's entry point to
  /// ensure proper threading behavior for WPF.
  /// </remarks>
  internal class Program
  {
    //[STAThread] must be present on the Application entry point
    [STAThread]
    static void Main(string[] args)
    {
      //Call Host.Initialize before constructing any objects from ArcGIS.Core
      Host.Initialize();

      // Start the WPF application
      MainWindow mainWindow = new();
      mainWindow.Show();
      mainWindow.Activate();
      mainWindow.Closing += (sender, e) =>
      {
        //Stop the WPF application message loop
        System.Windows.Threading.Dispatcher.ExitAllFrames();
      };

      //Start the WPF application message loop
      System.Windows.Threading.Dispatcher.Run();
    }
  }
}
