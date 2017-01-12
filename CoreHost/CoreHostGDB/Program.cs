//Copyright 2017 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using CoreHostGDB.UI;
using Application = System.Windows.Application;

namespace CoreHostGDB {
    /// <summary>
    /// WPF application that implements a generic File GDB reader
    /// </summary>
    /// <remarks>
    /// 1. Open this solution in Visual Studio 
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to run the WPF app.  
    /// 1. Specify a valid path to a file geodatabase path in the 'Open a GDB' input field and click the 'Open' button.  
    /// 1. The 'Open a Dataset' dropdown is filled with all available datasets.  
    /// 1. Select a dataset on the 'Open a Dataset' dropdown and click the 'Read' button.
    /// 1. View the table showing the dataset's content.
    /// ![UI](Screenshots/Screen.png)
    /// </remarks>
    class Program {

        static Window w = null;

        [STAThread]
        static void Main(string[] args) {

            //Initialize CoreHost (we can do it in Window Initialize but this way
            //we don't even pop the form if Initialize fails

            ArcGIS.Core.Hosting.Host.Initialize();//this will throw!

            Application app = new Application();
            w = new Window();
            w.Height = 500;
            w.Width = 700;
            Grid g = new Grid();
            g.Children.Add(new GDBGrid());
            w.Content = g;
            w.Title = "ArcGIS Pro CoreHost GDB Sample: File GDB Reader";
            app.Run(w);
            
        }
    }
}
