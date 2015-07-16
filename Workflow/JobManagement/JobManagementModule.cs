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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Internal.Framework.Controls;
using ArcGIS.Desktop.Workflow.Models;
using ArcGIS.Desktop.Workflow.Models.ConfigItems;
using ArcGIS.Desktop.Workflow.Models.Queries;
using ArcGIS.Desktop.Workflow.Models.JobModels;
using ArcGIS.Desktop.Workflow;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;
using System.Runtime.InteropServices;



namespace WorkflowSample
{
    /// <summary>
    /// This Class is used for populated the combo box for the owner property on the create pane
    /// </summary>
    internal class OwnerComboBoxItem : ArcGIS.Desktop.Framework.Contracts.ComboBoxItem
    {
        public JobAssignmentType AssignmentType { get; private set; }
        public string AssignedTo { get; private set; }

        public OwnerComboBoxItem(string name, JobAssignmentType assignmentType, string assignedTo)
            : base(name)
        {
            switch (assignmentType)
            {
                case JobAssignmentType.AssignedToUser:
                    Icon = "pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/User16.png";
                    break;
                case JobAssignmentType.AssignedToGroup:
                    Icon = "pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/Group_B16.png";
                    break;
                case JobAssignmentType.None:
                case JobAssignmentType.Unassigned:
                default:
                    break;
            }

            AssignmentType = assignmentType;
            AssignedTo = assignedTo;
        }
    }

    /// <summary>
    /// This Sample provides a Dockable Pane that allows the user
    /// to manipulate workflow manager databases while focusing only on one Job Type
    /// </summary>
    /// <remarks>    
    /// 1  Must have a Workflow manager database set up and accessible
    /// 1.a. This is done with the combination of database management and our Workflow manager software.
    /// 1.b. please refer to setting up a Workflow manager database before using this sample.
    /// 2. must designate job type to focus on prior to compiling code
    /// 2.a. Open project in VS 2013. Locate JobManagementModule.cs. Find JobManagementModule. designate the Job Type ID.
    /// 2.b  Use 2 as your ID if you aren't certain and you just used the quick configuration, in your post install.
    /// 2.c  If you have any problems consult your workflow database administrator
    /// 2.d. currently configured to work with a Job Type called 'Work Order' some names maybe needed to change in the xaml and viewmodel
    /// 3. must have a valid workflow database connection active in project before using
    /// 3.a. To acquire a valid worflow database connection just use the add workflow connection under the connections menu on the project tab 
    /// </remarks>

    internal class JobManagementModule : Module
    {
        private static JobManagementModule _this = null;
        private string QueryName = "Jobs assigned to me";
        private int JobTypeID = 401;
        
        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static JobManagementModule Current
        {
            get
            {
                return _this ?? (_this = (JobManagementModule)FrameworkApplication.FindModule("WorkflowSample_Module"));
            }
        }

        //create functions to handle and collect the WMX Data needed

        public void Start()
        {
            
        }

        //create a job based off a pre-defined job type
        //then assigned it to the specified owner
        public int CreateJob(OwnerComboBoxItem owner)
        {
            JobsManager JM = WorkflowModule.GetManager<JobsManager>("");
            var ret = JM.CreateJob(JobTypeID);
            var job = JM.GetJob(ret);
            if (owner != null)
            {
                job.AssignedTo = owner.AssignedTo;
                job.AssignedType = owner.AssignmentType;
            }
            job.Save();
            return ret;
        }

        //get job data
        public Job OpenJob(int JobId)
        {
            JobsManager JM = WorkflowModule.GetManager<JobsManager>("");
            var ret = JM.GetJob(JobId);
            return ret;
        }

        //get pre-defined query
        public QueryResult GetJobs()
        {
            JobsManager JM = WorkflowModule.GetManager<JobsManager>("");
            var ret = JM.ExecuteQuery(QueryName);
            return ret;
        }

        //build columns for open jobs pane
        //based on the return from the query
        public ReadOnlyObservableCollection<DataGridColumn> BuildDataGridColumns(IReadOnlyList<QueryFieldInfo> fields)
        {

            ObservableCollection<DataGridColumn> DGC = new ObservableCollection<DataGridColumn>();

            for (int i = 0; i < fields.Count; i++)
            {
                var item = fields[i];
                DataGridTextColumn newCol = new DataGridTextColumn();
                newCol.Header = item.Alias;
                if(item.Type == FieldType.Date)
                {
                    Binding newBinding = new Binding(String.Format("Values[{0}]", item.Index));
                    newBinding.StringFormat = System.Globalization.DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                    newCol.Binding = newBinding;
                    newCol.SortMemberPath = String.Format("Values[{0}].Ticks", item.Index);
                }
                else
                    newCol.Binding = new Binding(String.Format("Values[{0}]", item.Index));

                if (i == fields.Count - 1)
                    // Make the last column fill the space to hide the virtual extra column
                    newCol.Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                DGC.Add(newCol);
            }

            return new ReadOnlyObservableCollection<DataGridColumn>(DGC);
        }

        public IReadOnlyList<UserInfo> GetUsers()
        {
            ConfigurationManager CM = WorkflowModule.GetManager<ConfigurationManager>("");
            var ret = CM.GetAllUsers();
            return ret;
        }

        public IReadOnlyList<GroupInfo> GetGroups()
        {
            ConfigurationManager CM = WorkflowModule.GetManager<ConfigurationManager>("");
            var ret = CM.GetAllGroups();
            return ret;
        }

        //get list of available owners, groups included
        public IReadOnlyList<OwnerComboBoxItem> GetOwnerList()
        {
            List<OwnerComboBoxItem> Items = new List<OwnerComboBoxItem>();
            Items.Add(new OwnerComboBoxItem("Unassigned", JobAssignmentType.Unassigned, ""));

            var Users = GetUsers();
            var Groups = GetGroups();
            foreach (var user in Users)
            {
                Items.Add(new OwnerComboBoxItem(user.FullName, JobAssignmentType.AssignedToUser, user.UserName));
            }

            foreach (var group in Groups)
            {
                Items.Add(new OwnerComboBoxItem(group.Name, JobAssignmentType.AssignedToGroup, group.Name));
            }
            return Items as IReadOnlyList<OwnerComboBoxItem>;
        }

    }
}
