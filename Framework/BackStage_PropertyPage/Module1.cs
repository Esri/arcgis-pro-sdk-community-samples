//   Copyright 2015 Esri
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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using System.Xml;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;

using ArcGIS.Desktop.Internal.Core;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using System.IO;

namespace BackStage_PropertyPage
{
    /// <summary>
    /// This sample illustrates how to 
    /// 1. add a new backstage item  
    /// 2. add property sheet items into the Options property pages
    /// 3. save and restore project settings
    /// </summary>
    /// <remarks>
    /// Backstage items can be either a tab or a button. As per other controls they have a reference in the config.daml file.  However they are different from other controls 
    /// in that they are not children of the module tag - they are children of the backstage tag. This sample shows how to add a new tab following the MVVM pattern. 
    /// The tab can be positioned using the "insert" and "placeWith" attributes in the config.daml.  The SampleBackstageTabView xaml file uses ArcGIS Pro styles to 
    /// allow the custom tab to look those those in the existing application.
    /// <para>
    /// Property sheets are used to capture settings. They can be either project or application settings. You can insert your custom property sheets into the existing Options
    /// property sheets which are displayed from the backstage Options tab.  This is achieved in the config.daml by using the updateSheet xml tag and specifying the 
    /// esri_core_optionsPropertySheet id.  Use the group attribute on the insertPage tag to specify whether your view/viewmodel represents project or application settings. 
    /// This sample has an example of both project and application settings, including illustrating how these settings can be saved. 
    /// </para>
    /// <para>
    /// Modules can write out their own set of properties when a project is saved. Correspondingly, modules can read their own settings when a project is opened.  The module
    /// contains two methods OnReadStateAsync and OnWriteStateAsync which should be overriden to read and write module specific settings or properties. 
    /// </para>
    /// 1. Open this solution in Visual Studio 2013.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open any project - it can be an existing project containing data or a new empty project.
    /// 1. Click the project tab.  See that there is a new Sample Tab item in the backstage.  Select it and it will show the new backstage tab.
    /// 1. Click the project tab and select the Options backstage item.  The options property page will display.
    /// 1. See that there is a Sample Project Settings under Project and a Sample App Settings under Application. 
    /// 1. Change the project settings and application settings. 
    /// 1. Save the project.
    /// 1. Open another project (or create new); return to the Project|Options|Sample Project Settings and see that the settings have been reset.
    /// 1. Open the project from step4; return to the Project|Options|Sample Project Settings and see that the settings have been restored.
    /// ![UI](Screenshots/Screen.png)
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        private static string ModuleID = "BackStage_PropertyPage_Module";

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule(Module1.ModuleID));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        /// <summary>
        /// Generic implementation of ExecuteCommand to allow calls to
        /// <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
        /// your Module.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected override Func<Task> ExecuteCommand(string id)
        {

            //TODO: replace generic implementation with custom logic
            //etc as needed for your Module
            var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
            if (command == null)
                return () => Task.FromResult(0);
            if (!command.CanExecute(null))
                return () => Task.FromResult(0);

            return () =>
            {
                command.Execute(null); // if it is a tool, execute will set current tool
                return Task.FromResult(0);
            };
        }
        #endregion Overrides


        private bool hasSettings = false;
        /// <summary>
        /// Module constructor.  Subscribe to the ProjectOpened and ProjectClosed events.
        /// </summary>
        private Module1()
        {
            ProjectOpenedEvent.Subscribe(OnProjectOpen);
            ProjectClosedEvent.Subscribe(OnProjectClosed);
        }

        /// <summary>
        /// Uninitialize method.  Make sure the module unsubscribes from the events.
        /// </summary>
        protected override void Uninitialize()
        {
            base.Uninitialize();

            ProjectOpenedEvent.Unsubscribe(OnProjectOpen);
            ProjectClosedEvent.Unsubscribe(OnProjectClosed);
        }

        /// <summary>
        /// Reads the module settings from a project.  This method is called when a project is opened if the project contains this module's settings. 
        /// Use the <paramref name="settings"/> to obtain the module values.
        /// </summary>
        /// <param name="settings">Contains the module settings</param>
        /// <returns>A Task that represents the OnReadStateAsync method</returns>
        //protected override Task OnReadStateAsync(System.IO.Stream stream)
        protected override Task OnReadSettingsAsync(ModuleSettingsReader settings)
        {
          // set the flag
          hasSettings = true;

          // clear existing setting values
          _moduleSettings.Clear();

          if (settings == null)
            return Task.FromResult(0);

          string[] keys = new string[] {"Setting1", "Setting2"};
          foreach (string key in keys)
          {
            object value = settings.Get(key);
            if (value != null)
            {
              if (_moduleSettings.ContainsKey(key))
                _moduleSettings[key] = value.ToString();
              else
                _moduleSettings.Add(key, value.ToString());
            }
          }
 
          return Task.FromResult(0);
        }

        /// <summary>
        /// Writes the module's settings.  This method is called when a project is saved.  Populate the modules settings into the ModuleSettingsWriter settings.
        /// </summary>
        /// <param name="settings">The settings which will be written out</param>
        /// <returns>A Task that represents the OnWriteStateAsync method</returns>
        protected override Task OnWriteSettingsAsync(ModuleSettingsWriter settings)
        {
          foreach (string key in _moduleSettings.Keys)
          {
            settings.Add(key, _moduleSettings[key]);
          }

          return Task.FromResult(0);
        }

        /// <summary>
        /// Project opened event.  
        /// </summary>
        /// <remarks>
        /// This is necessary because OnReadStateAsync is NOT called if a project does not contain the module settings. This provides a way to restore the settings to 
        /// default when a project not containing our settings is opened.
        /// </remarks>
        /// <param name="args">project opened event arguments</param>
        private void OnProjectOpen(ProjectEventArgs args)
        {
          // if flag has not been set then we didn't enter OnReadStateAsync - and we want to restore the module settings to default
          if (!hasSettings)
            _moduleSettings.Clear();
        }

        /// <summary>
        /// Project closed event.  Make sure we reset the settings flag.
        /// </summary>
        /// <param name="args">project closed event arguments</param>
        private void OnProjectClosed(ProjectEventArgs args)
        {
          // reset the flag
          hasSettings = false;
        }

        #region Project Module Settings

        /// <summary>
        /// the dictionary of project settings
        /// </summary>
        private Dictionary<string, string> _moduleSettings = new Dictionary<string, string>();
        internal Dictionary<string, string> Settings
        {
            get { return _moduleSettings; }
            set { _moduleSettings = value; }
        }

        private string CreateXml(string attributeName, string value)
        {
            return String.Format("<{0}>{1}</{0}>", attributeName, value);
        }

        #endregion
    }
}
