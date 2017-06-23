   //Copyright 2017 Esri
   //Licensed under the Apache License, Version 2.0 (the "License");
   //you may not use this file except in compliance with the License.
   //You may obtain a copy of the License at

   //    http://www.apache.org/licenses/LICENSE-2.0

   //Unless required by applicable law or agreed to in writing, software
   //distributed under the License is distributed on an "AS IS" BASIS,
   //WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   //See the License for the specific language governing permissions and
   //limitations under the License. 
using System.Collections.Generic;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Workflow.Models.Queries;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using ArcGIS.Desktop.Workflow.Models.JobModels;
using ArcGIS.Desktop.Workflow;

namespace WorkflowSample
{
    internal class JobManagementViewModel : DockPane
    {
        private const string _dockPaneID = "WorkflowSample_JobManagement";
        private static string PaneTitle = "Job Management";
        private static string JobTypeName = "Work Order";
        private static string CreatePaneHeading = "Create Work Order";
        private static string OpenPaneHeading = "Open Work Order";

        protected JobManagementViewModel()
        {
            _isControl = true;
            _isHeader = false;
            _isCreate = false;
            _isOpen = false;
            _jobOpened = false;
            _isJobOpen = false;
            _isView = true;
            _createWO = new RelayCommand(() => ExecuteCreateWO(), () => CanExecute());
            _openWO = new RelayCommand(() => ExecuteOpenWO(), () => CanExecute());
            _openBtn = new RelayCommand(() => ExecuteOpenBtn(), () => CanExecute());
            _createBtn = new RelayCommand(() => ExecuteCreateBtn(), () => CanExecute());
            _backBtn = new RelayCommand(() => ExecuteBackBtn(), () => CanExecute());
            _viewBtn = new RelayCommand(() => ExecuteViewBtn(), () => CanExecute());
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            JobManagementViewModel pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID) as JobManagementViewModel;
            if (pane == null)
                return;
            if (pane.JobOpened)
            {
                pane.Activate();
            }
            else
            {
                pane.Caption = PaneTitle;
                pane.IsControl = true;
                pane.IsHeader = false;
                pane.IsCreate = false;
                pane.IsOpen = false;
                pane.IsView = true;
                pane.IsJobOpen = false;
                pane.Activate();
            }
        }

        #region Elements

        /// <summary>
        /// Get and set Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }


        private bool _jobOpened;
        /// <summary>
        /// Get and set Flag for if a job is currently opened
        /// </summary>
        public bool JobOpened
        {
            get { return _jobOpened; }
            set
            {
                SetProperty(ref _jobOpened, value, () => JobOpened);
            }
        }

        private bool _isControl;
        /// <summary>
        /// Gets and sets the visibility of the Control Group
        /// </summary>
        public bool IsControl
        {
            get { return _isControl; }
            set
            {
                SetProperty(ref _isControl, value, () => IsControl);
            }
        }

        private bool _isHeader;
        /// <summary>
        /// Gets and sets the visibility of the Header Group
        /// </summary>
        public bool IsHeader
        {
            get { return _isHeader; }
            set
            {
                SetProperty(ref _isHeader, value, () => IsHeader);
            }
        }

        private bool _isCreate;
        /// <summary>
        /// Gets and sets the visibility of the Create Group
        /// </summary>
        public bool IsCreate
        {
            get { return _isCreate; }
            set
            {
                SetProperty(ref _isCreate, value, () => IsCreate);
            }
        }

        private bool _isOpen;
        /// <summary>
        /// Gets and sets the visibility of the Open Group
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                SetProperty(ref _isOpen, value, () => IsOpen);
            }
        }

        private bool _isView;
        /// <summary>
        /// Gets and sets visibility of view information grid
        /// </summary>
        public bool IsView
        {
            get { return _isView; }
            set
            {
                SetProperty(ref _isView, value, () => IsView);
            }
        }

        private bool _isJobOpen;
        /// <summary>
        /// Gets and sets visibility of Work Order Pane
        /// </summary>
        public bool IsJobOpen
        {
            get { return _isJobOpen; }
            set
            {
                SetProperty(ref _isJobOpen, value, () => IsJobOpen);
            }
        }

        private bool _ownerInfo;
        /// <summary>
        /// Gets and sets visibility of the info icon for owner box
        /// </summary>
        public bool OwnerInfo
        {
            get { return _ownerInfo; }
            set
            {
                SetProperty(ref _ownerInfo, value, () => OwnerInfo);
            }
        }

        private bool _checkInfo;
        /// <summary>
        /// Gets and sets visibility of the info icon for check box
        /// </summary>
        public bool CheckInfo
        {
            get { return _checkInfo; }
            set
            {
                SetProperty(ref _checkInfo, value, () => CheckInfo);
            }
        }

        private IReadOnlyList<OwnerComboBoxItem> _ownerList;
        public IReadOnlyList<OwnerComboBoxItem> OwnerList
        {
            get { return _ownerList; }
            set
            {
                SetProperty(ref _ownerList, value, () => OwnerList);
            }
        }

        private ReadOnlyObservableCollection<DataGridColumn> _columns;
        public ReadOnlyObservableCollection<DataGridColumn> Columns
        {
            get { return _columns; }
            set
            {
                SetProperty(ref _columns, value, () => Columns);
            }
        }

        private IReadOnlyList<JobQueryRow> _openRows;
        public IReadOnlyList<JobQueryRow> OpenRows
        {
            get { return _openRows; }
            set
            {
                SetProperty(ref _openRows, value, () => OpenRows);
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value, () => SelectedItem);
            }
        }

        private bool _previousOpen = false;

        private string _infoCreated;
        public string InfoCreated
        {
            get { return _infoCreated; }
            set
            {
                SetProperty(ref _infoCreated, value, () => InfoCreated);
            }
        }

        private string _infoName;
        public string InfoName
        {
            get { return _infoName; }
            set
            {
                SetProperty(ref _infoName, value, () => InfoName);
            }
        }

        private string _infoID;
        public string InfoID
        {
            get { return _infoID; }
            set
            {
                SetProperty(ref _infoID, value, () => InfoID);
            }
        }

        private string _infoAssigned;
        public string InfoAssigned
        {
            get { return _infoAssigned; }
            set
            {
                SetProperty(ref _infoAssigned, value, () => InfoAssigned);
            }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                SetProperty(ref _isChecked, value, () => IsChecked);
            }
        }

        #endregion
        #region Command Declarations

        private RelayCommand _createWO;
        public ICommand CreateWOCMD
        {
            get { return _createWO; }
        }

        private RelayCommand _openWO;
        public ICommand OpenWOCMD
        {
            get { return _openWO; }
        }

        private RelayCommand _openBtn;
        public ICommand OpenCMD
        {
            get { return _openBtn; }
        }

        private RelayCommand _createBtn;
        public ICommand CreateCMD
        {
            get { return _createBtn; }
        }

        private RelayCommand _backBtn;
        public ICommand BackCMD
        {
            get { return _backBtn; }
        }

        private RelayCommand _viewBtn;
        public ICommand ViewCMD
        {
            get { return _viewBtn; }
        }
        #endregion
        #region Command Implementations

        //for now all commands get the green light
        public bool CanExecute()
        {
            return true;
        }

        public void ExecuteCreateWO()
        {
            clearPane();
            IsHeader = true;
            IsCreate = true;
            Heading = CreatePaneHeading;
            QueuedTask.Run(async () =>
            {
                OwnerList = await JobManagementModule.Current.GetOwnerListAsync();
            });
        }

        public void ExecuteOpenWO()
        {

            clearPane();
            IsHeader = true;
            IsOpen = true;
            Heading = OpenPaneHeading;
            
            QueryResult jobs = QueuedTask.Run(() =>
            {
                //build open job grid
                return JobManagementModule.Current.GetJobsAsync();
            }).Result;
            Columns = JobManagementModule.Current.BuildDataGridColumns(jobs.Fields);
            OpenRows = jobs.Rows;
        }

        public void ExecuteOpenBtn()
        {
            JobQueryRow selected = SelectedItem as JobQueryRow;
            SelectedItem = null;
            if (selected != null)
            {
                _previousOpen = true;
                 QueuedTask.Run(() =>
                 {
                     Open_WorkOrderAsync(selected.JobID);
                 });
            }
        }

        public void ExecuteCreateBtn()
        {
            OwnerComboBoxItem Owner = SelectedItem as OwnerComboBoxItem;
            SelectedItem = null;
            QueuedTask.Run(async () =>
            {
                var jobId = await JobManagementModule.Current.CreateJobAsync(Owner);
                if (IsChecked == true)
                {
                    await Open_WorkOrderAsync(jobId);
                }
            });
        }

        public void ExecuteBackBtn()
        {
            clearPane();
            Caption = PaneTitle;
            if (_previousOpen)
            {
                _previousOpen = false;
                IsHeader = true;
                IsOpen = true;
                Heading = OpenPaneHeading;
            }
            else
                IsControl = true;
        }

        public void ExecuteViewBtn()
        {
            IsView = !IsView;
        }
        #endregion
        #region Helper Methods
        private void clearPane()
        {
            IsControl = false;
            IsHeader = false;
            IsCreate = false;
            IsOpen = false;
            IsJobOpen = false;
            JobOpened = false;
        }

        private async System.Threading.Tasks.Task Open_WorkOrderAsync(string jobId)
        {
            clearPane();
            Caption = JobTypeName;
            IsView = true;
            IsJobOpen = true;
            JobOpened = true;
            IsHeader = true;

            //fill job view grid
            
            Job CurJob = await JobManagementModule.Current.OpenJobAsync(jobId);
            InfoCreated = CurJob.CreatedBy;
            InfoName = Heading = CurJob.Name;
            InfoID = CurJob.ID;
            if (CurJob.AssignedTo == "")
                InfoAssigned = "Unassigned";
            else
                InfoAssigned = CurJob.AssignedTo;
        }
        #endregion
    }

        /// <summary>
        /// Button implementation to show the DockPane.
        /// </summary>
        internal class JobManagement_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
        {
            protected override void OnClick()
            {
                JobManagementViewModel.Show();
            }
        }
    }
