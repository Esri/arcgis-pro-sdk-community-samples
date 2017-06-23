//Copyright 2017 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.using System;
using System;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Events;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.DataReviewer;
using ArcGIS.Desktop.DataReviewer.Models;
using ArcGIS.Desktop.Framework.Dialogs;

namespace DataReviewerProSDKSamples
{
    /// <summary>
    /// The add-in contains basic and advanced examples to demonstrate how to add and remove Data Reviewer project items.  
    /// The basic example focuses solely on Data Reviewer interfaces and objects.
    /// The advanced example demonstrates how you can use Data Reviewer interfaces and objects with other interfaces in ArcGIS Pro
	/// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a Reviewer workspace and Reviewer batch Jobs needed by the sample add-in.  Make sure that the Sample data is unzipped in c:\data and c:\data\DataReviewer is available.
    /// 2. You can modify AddReviewerResults_Basic() method to update the path of the Reviewer Workspace.
    /// 3. You can modify AddBatchJobs_Basic() to update the path of the Reviewer Batch job.
    /// 4. In Visual Studio click the Build menu. Then select Build Solution.
    /// 5. Click Start button to open ArcGIS Pro.
    /// 6. ArcGIS Pro will open. 
    /// 7. Open any project file. Click the Reviewer Sample - Basic Tab to use basic samples.
	/// ![UI](Screenshots/Screen.png)
    /// 7.a Make sure that the Project pane is open.
    /// 7.b Click Add Reviewer Results button. The Reviewer Results item will be added to the Project pane.
    /// 7.c Click Add Sessions button. All the sessions that are in the Reviewer Dataset will be added to the Project pane as child items to Reviewer Results.
    /// 7.d Click Mark Default button. The first session will be marked as default and its icon will be updated with a home icon in the Project pane.
    /// 7.e Click Remove Session button. A session that is not marked as default will be removed from the Project pane.
    /// 7.f Click Remove Reviewer Results button. The Reviewer Results item will be removed from the Project pane.
    /// 7.g Click Add Reviewer Batch Jobs button. All Reviewer batch jobs that are in the sample data will be added to the Project pane.
    /// 7.h Click Remove Reviewer Batch Job button. The first Batch Job will be removed from the Project pane.	
    /// ![UI](Screenshots/Screen1.png)
    /// 8. Click the Reviewer Sample - Advanced Tab to use advanced samples.
    /// 8.a Make sure that Project pane is open.
    /// 8.b Click Add Reviewer Results button. A browse dialog will open and you can select a Reviewer workspace to add it to the Project pane.
    /// 8.c Click Add Sessions button. All the sessions that are in the Reviewer Dataset will be added to the Project pane as child items to Reviewer Results. These sessions will also be added to the Reviewer Sessions gallery.
    /// 8.d Right-click a session in the Reviewer Sessions gallery and click Mark Default. This session will be marked as default and its icon will be updated with a home icon in the Project pane.
    /// 8.e Right-click a session in the Reviewer Sessions gallery and click Remove. This session will be removed from the Project pane.
    /// 8.f Click Remove Reviewer Results button. The Reviewer Results item will be removed from the Project pane.
    /// 8.g Click Add Reviewer Batch Jobs button. A browse dialog will open and you can select one or more Reviewer Batch Jobs to add to the Project pane. These Batch Jobs will also be added to the Reviewer Batch Jobs gallery.
    /// 8.h Right-click a Batch Job in Reviewer Batch Jobs gallery and click Remove. This Batch Job will be removed from the Project Pane.
    /// ![UI](Screenshots/Screen2.png)
    /// </remarks>
    internal class DataReviewerProSDKModule : Module
    {
        #region Private member variables
        private static DataReviewerProSDKModule _currentModule = null;
        private static string _lastBrowseLocation = "";
        #endregion

        #region Setup Module
        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static DataReviewerProSDKModule CurrentModule
        {
            get
            {
                return _currentModule ?? (_currentModule = (DataReviewerProSDKModule)FrameworkApplication.FindModule(Constants.ThisModuleName));
            }
        }

        /// <summary>
        /// Create an instance of the DataReviewerModule
        /// </summary>
        internal static DataReviewerModule DataReviewerModule
        {
            get { return FrameworkApplication.FindModule(Constants.DataReviewerModuleName) as DataReviewerModule; }
        } 
        #endregion

        #region Basic Examples

        /// <summary>
        /// This command shows the basic functionality of how to add a connection to Reviewer Results which are stored in an existing Reviewer workspace.
        /// </summary>
        /// <returns></returns>
        internal static async Task AddReviewerResults_Basic()
        {
            try
            {
                // You can add only one ResultsProjectItem connection to a project
                // If FrameworkApplication contains esri_datareviewer_addReviewerWorkspaceSettingState state that means the ResultsProjectItem is already added therefore no need to do anything
                if (FrameworkApplication.State.Contains(Constants.CanAddReviewerResultsState))
                {
                    MessageBox.Show("Data Reviewer results have already been added to the project.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                //Get the data folder path
                string dataRootPath = @"C:\Data\DataReviewer";

                //Get path of the Reviewer Workspace
                string strReviewerResultsWorkspacePath = System.IO.Path.Combine(dataRootPath, "ReviewerWorkspace.gdb");
                if (!System.IO.Directory.Exists(strReviewerResultsWorkspacePath))
                {
                    MessageBox.Show("Unable to locate " + strReviewerResultsWorkspacePath + " geodatabase", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                IProjectItem reviewerResultProjectItem = null;
                await QueuedTask.Run(() =>
                {
                    //Step 1: First check if the Reviewer workspace contains current Reviewer dataset.
                    if (DataReviewerModule.HasValidReviewerDataset(strReviewerResultsWorkspacePath))
                    {
                        //Step 2: Create the Reviewer ResultsItem
                        reviewerResultProjectItem = DataReviewerModule.CreateResultsProjectItem(strReviewerResultsWorkspacePath);
                    }
                    else
                    {
                        MessageBox.Show("The geodatabase specified does not contain a current Reviewer dataset.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }

                    //Step 3: Add ResultsItem to the current project
                    if (null != reviewerResultProjectItem)
                    {
                        try
                        {
                        bool itemAdded = Project.Current.AddItem(reviewerResultProjectItem);
                        }
                        catch (Exception ex1)
                        {
                        MessageBox.Show(ex1.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                      
                    }
                     
                });
            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// This command shows the basic functionality of how to remove a connection to Reviewer Results from the current project.
        /// </summary>
        /// <returns></returns>
        internal static async Task RemoveReviewerResults_Basic()
        {
            try
            {
                //Step 1: Get Reviewer Results project item
                //A project can contain only one Reviewer Results project item
                IEnumerable<ReviewerResultsProjectItem> projectItems = Project.Current.GetItems<ReviewerResultsProjectItem>();
                if (projectItems.Count() > 0)
                {
                    //Step 2: Remove Reviewer Results project item from the current project
                    ReviewerResultsProjectItem resultsProjectItem = projectItems.FirstOrDefault();

                    await QueuedTask.Run(() =>
                    {
                        Project.Current.RemoveItem(resultsProjectItem as IProjectItem);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This command shows the basic functionality of how to add all sessions in a Reviewer Workspace to the current project.
        /// </summary>
        /// <returns></returns>
        internal static async Task AddSessions_Basic()
        {
            try
            {
                //Step 1: Get Reviewer Results project item
                //A project can contain only one Reviewer Results project item
                IEnumerable<ReviewerResultsProjectItem> projectItems = Project.Current.GetItems<ReviewerResultsProjectItem>();
                ReviewerResultsProjectItem resultsProjectItem = null;
                if (projectItems.Count() > 0)
                    resultsProjectItem = projectItems.FirstOrDefault();
                else
                {
                    MessageBox.Show(string.Format("Current project does not have a connection to the Reviewer Results.{0}Please add a connection to the Reviewer Results", Environment.NewLine), "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                if (null != resultsProjectItem)
                {
                    //Step 2: Get the Reviewer Dataset associated with the Reviewer Results project item
                    ReviewerDataset reviewerDataset = resultsProjectItem.ReviewerDataset;
                    if (null != reviewerDataset)
                    {
                        IEnumerable<Session> reviewerSessions = null;
                        await QueuedTask.Run(() =>
                        {
                            //Step 3: Get all Reviewer session that are in the Reviewer dataset
                            reviewerSessions = reviewerDataset.GetSessions();
                            foreach (Session session in reviewerSessions)
                            {
                                //Step 4: Add each Reviewer session to the current project
                                Item sessionItem = resultsProjectItem.CreateSessionItem(session);
                                resultsProjectItem.AddSessionItemAsync(sessionItem);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// This command shows the basic functionality of how to mark a Reviewer Session as default.
        /// In this example the first Reviewer Session that is referenced in the current project will be be marked as default
        /// </summary>
        /// <returns></returns>
        internal static async Task MarkDefaultSession_Basic()
        {
            try
            {
                await QueuedTask.Run(() =>
                {
                    //Step 1: Get Reviewer Results project item
                    //A project can contain only one Reviewer Results project item
                    IEnumerable<ReviewerResultsProjectItem> projectItems = Project.Current.GetItems<ReviewerResultsProjectItem>();
                    ReviewerResultsProjectItem resultsProjectItem = null;
                    if (projectItems.Count() > 0)
                        resultsProjectItem = projectItems.FirstOrDefault() as ReviewerResultsProjectItem;
                    else
                    {
                        MessageBox.Show(string.Format("Current project does not have a connection to the Reviewer Results.{0}Please add a connection to the Reviewer Results and add Reviewer Sessions.", Environment.NewLine), "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }

                    if (null != resultsProjectItem)
                    {
                        //Step 2: Get all session items that are currently referenced in the project.
                        IEnumerable<Item> sessionItems = resultsProjectItem.GetItems();
                        if (null != sessionItems && sessionItems.Count() > 0)
                        {
                            //Step 3: Mark first session as Default
                            resultsProjectItem.DefaultSessionItem = sessionItems.FirstOrDefault();

                        }
                        else
                            MessageBox.Show(string.Format("There are no Reviewer Sessions referenced in the current project.{0}Please add at least one Reviewer Session to the current project.", Environment.NewLine), "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// This command shows the basic functionality of how to remove the reference of a Reviewer Session from the current project.
        /// In this example the first Reviewer Session that is referenced in the current project will be removed.
        /// </summary>
        /// <returns></returns>
        internal static async Task RemoveSession_Basic()
        {
            try
            {
                await QueuedTask.Run(async () =>
                {
                    //Step 1: Get Reviewer Results project item
                    //A project can contain only one Reviewer Results project item
                    IEnumerable<ReviewerResultsProjectItem> projectItems = Project.Current.GetItems<ReviewerResultsProjectItem>();
                    ReviewerResultsProjectItem resultsProjectItem = null;
                    if (projectItems.Count() > 0)
                        resultsProjectItem = projectItems.FirstOrDefault() as ReviewerResultsProjectItem;
                    else
                        return;

                    if (null != resultsProjectItem)
                    {
                        //Step 2: Get all session items that are currently referenced in the project.
                        IEnumerable<Item> sessionItems = resultsProjectItem.GetItems();
                        if (null != sessionItems && sessionItems.Count() > 0)
                        {
                            //Step 3: Remove the first Session that is not default from Project
                            if(sessionItems.Count() > 0)
                            {
                                var firstItem = sessionItems.ToList()[0];
                                if (!firstItem.IsDefault)
                                    await resultsProjectItem.RemoveSessionItemAsync(firstItem);
                                else
                                {
                                    if(sessionItems.Count() > 1)
                                    {
                                        var secondItem = sessionItems.ToList()[1];
                                        await resultsProjectItem.RemoveSessionItemAsync(secondItem);
                                    }
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }
        
        /// <summary>
        /// This command shows the basic functionality of how to add existing Batch Jobs to the current project.
        /// In this example we will add 3 batch jobs in the current project
        /// The path of these batch jobs is defined in the  Constants.cs. Update these values as per the local path on your machine.
        /// </summary>
        /// <returns></returns>
        internal static async Task AddBatchJobs_Basic()
        {
            try
            {
                //Get the data folder path

                string dataRootPath = @"C:\Data\DataReviewer";

                //Step 1: Create Reviewer BatchJobItems
                string batchJob1, batchJob2, batchJob3;
                batchJob1 = System.IO.Path.Combine(dataRootPath, "Parcel Validation.rbj");
                batchJob2 = System.IO.Path.Combine(dataRootPath, "Point Validation.rbj");
                batchJob3 = System.IO.Path.Combine(dataRootPath, "Attribute Validation.rbj");

                if (!System.IO.File.Exists(batchJob1) || !System.IO.File.Exists(batchJob2) || !System.IO.File.Exists(batchJob3))
                {
                    MessageBox.Show("Batch Job files are not found", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                IProjectItem reviewerBatchJobProjectItem1 = null;
                IProjectItem reviewerBatchJobProjectItem2 = null;
                IProjectItem reviewerBatchJobProjectItem3 = null;
                await QueuedTask.Run(() =>
                {
                    reviewerBatchJobProjectItem1 = DataReviewerModule.CreateBatchJobProjectItem(batchJob1);
                    reviewerBatchJobProjectItem2 = DataReviewerModule.CreateBatchJobProjectItem(batchJob2);
                    reviewerBatchJobProjectItem3 = DataReviewerModule.CreateBatchJobProjectItem(batchJob3);

                    //Step 2: Add BatchJobItems to the current project
                    if (null != reviewerBatchJobProjectItem1)
                        Project.Current.AddItem(reviewerBatchJobProjectItem1);

                    if (null != reviewerBatchJobProjectItem2)
                        Project.Current.AddItem(reviewerBatchJobProjectItem2);

                    if (null != reviewerBatchJobProjectItem3)
                        Project.Current.AddItem(reviewerBatchJobProjectItem3);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This command shows the basic functionality of how to remove the reference of a Batch Job from the project.
        /// In this example the first Reviewer Batch Job that is referenced in the current project will be removed.
        /// </summary>
        /// <returns></returns>
        internal static async Task RemoveBatchJob_Basic()
        {
            try
            {
                //Step 1: Get all Reviewer Batch Jobs that are referenced in the current project.
                IEnumerable<ReviewerBatchJobProjectItem> projectItems = Project.Current.GetItems<ReviewerBatchJobProjectItem>();

                if (null != projectItems && projectItems.Count()>  0)
                {
                    //Step 2: Remove the first batch Job that is referenced in the current project
                    await QueuedTask.Run(() =>
                    {
                        Project.Current.RemoveItem(projectItems.FirstOrDefault());
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion

        #region Advanced Examples
        
        public static Session DefaultSession { get; set; }

        /// <summary>
        /// This command shows how to add a connection to Reviewer Results which are stored in an existing Reviewer workspace.
        /// In this example you can browse to a Reviewer workspace
        /// </summary>
        /// <returns></returns>
        internal static async Task AddReviewerResults_Advanced()
        {
            try
            {
                // You can add only one ResultsProjectItem connection to a project
                // If FrameworkApplication contains esri_datareviewer_addReviewerWorkspaceSettingState state that means the ResultsProjectItem is already added therefore no need to do anything
                if (FrameworkApplication.State.Contains(Constants.CanAddReviewerResultsState))
                {
                    MessageBox.Show("Data Reviewer results have already been added to the project.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                    

                //Step 1: Browse to a Reviewer Workspace and get its path
                string strReviewerResultsWorkspacePath = OpenBrowseGeodatabaseDialog();
                if (string.IsNullOrEmpty(strReviewerResultsWorkspacePath))
                    return;

                IProjectItem reviewerResultProjectItem = null;
                await QueuedTask.Run(() =>
                {
                    //Step 2: Check if the Reviewer workspace contains current Reviewer dataset.
                    if (DataReviewerModule.HasValidReviewerDataset(strReviewerResultsWorkspacePath))
                    {
                        //Step 3: Create the Reviewer ResultsItem
                        reviewerResultProjectItem = DataReviewerModule.CreateResultsProjectItem(strReviewerResultsWorkspacePath);
                    }
                    else
                    {
                        MessageBox.Show("The geodatabase specified does not contain a current Reviewer dataset.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }

                    //Step 4: Add ResultsItem to the current project
                    if (null != reviewerResultProjectItem)
                        Project.Current.AddItem(reviewerResultProjectItem);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This command shows how to add all sessions in a Reviewer Workspace to the current project.
        /// These sessions are also added in the Reviewer Sessions Gallery for quick access
        /// </summary>
        /// <returns></returns>
        internal static async Task AddSessions_Advanced()
        {
            try
            {
                //Step 1: Get Reviewer Results project item
                //A project can contain only one Reviewer Results project item
                IEnumerable<ReviewerResultsProjectItem> projectItems = Project.Current.GetItems<ReviewerResultsProjectItem>();
                ReviewerResultsProjectItem resultsProjectItem = null;
                if (projectItems.Count() > 0)
                    resultsProjectItem = projectItems.FirstOrDefault() as ReviewerResultsProjectItem;
                else
                {
                    MessageBox.Show(string.Format("Current project does not have a connection to the Reviewer Results.{0}Please add a connection to the Reviewer Results", Environment.NewLine), "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (null != resultsProjectItem)
                {
                    //Step 2: Get the Reviewer Dataset associated with the Reviewer Results project item
                    ReviewerDataset reviewerDataset = resultsProjectItem.ReviewerDataset;
                    if (null != reviewerDataset)
                    {
                        IEnumerable<Session> reviewerSessions = null;
                        await QueuedTask.Run(() =>
                        {
                            //Step 3: Get all Reviewer session that are in the Reviewer dataset
                            reviewerSessions = reviewerDataset.GetSessions();
                            foreach (Session session in reviewerSessions)
                            {
                                //Step 4: Add each Reviewer session to the current project
                                Item sessionItem = resultsProjectItem.CreateSessionItem(session);
                                resultsProjectItem.AddSessionItemAsync(sessionItem);

                                //Step 5: Raise GalleryItemsChangedEvent to add the Reviewer session to the Gallery
                                GalleryItemsChangedEvent.Publish(new GalleryItem(sessionItem.TypeID, sessionItem.Path, sessionItem.Name), true);
                            }
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This command shows how to mark a Reviewer Session as default.
        /// In this example you can right click a session in the Reviewer Session gallery and mark it as default
        /// </summary>
        /// <returns></returns>
        internal static async Task MarkDefaultSession_Advanced()
        {
            try
            {
                //Step 1: Get the Reviewer Session that is right clicked 
                var galleryItem = Utilities.GetContext<GalleryItem>().FirstOrDefault();
                if (null == galleryItem)
                    return;

                //Step 2: Get Reviewer Results project item
                //A project can contain only one Reviewer Results project item
                IEnumerable<ReviewerResultsProjectItem> projectItems = Project.Current.GetItems<ReviewerResultsProjectItem>();
                ReviewerResultsProjectItem resultsProjectItem = null;
                if (projectItems.Count() > 0)
                    resultsProjectItem = projectItems.FirstOrDefault() as ReviewerResultsProjectItem;
                else
                {
                    MessageBox.Show(string.Format("Current project does not have a connection to the Reviewer Results.{0}Please add a connection to the Reviewer Results and add Reviewer Sessions.", Environment.NewLine), "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (null != resultsProjectItem)
                {
                    //Step 3: Get all session items that are currently referenced in the project.
                    IEnumerable<Item> sessionItems = resultsProjectItem.GetItems();
                    if (null != sessionItems && sessionItems.Count() > 0)
                    {
                        //Step 4: Find the session item (in the project) that is right clicked in the gallery
                        Item markDefaultItem = sessionItems.Where(p => (p as Item).Path == galleryItem.Path).FirstOrDefault() as Item;
                        if (null != markDefaultItem)
                        {
                            //Step 5: Mark this session as Default
                            resultsProjectItem.DefaultSessionItem = markDefaultItem;

                            //Step 6: Update Default Session property
                            DefaultSession = resultsProjectItem.GetSessionFromItem(markDefaultItem);
                        }
                    }
                    else
                        MessageBox.Show(string.Format("There are no Reviewer Sessions referenced in the current project.{0}Please add at least one Reviewer Session to the current project.", Environment.NewLine), "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This command shows how to remove the reference of a Reviewer Session from the current project.
        /// In this example you can right click a session in the Reviewer Session gallery and remove it from the current project.
        /// </summary>
        /// <returns></returns>
        internal static async Task RemoveSessions_Advanced()
        {
            try
            {
                //Step 1: Get the session that is right clicked 
                var galleryItem = Utilities.GetContext<GalleryItem>().FirstOrDefault();
                if (null == galleryItem)
                    return;
                else
                {
                    //Step 2: Remove the Session from Gallery 
                    GalleryItemsChangedEvent.Publish(galleryItem, false);
                }

                await QueuedTask.Run(async() =>
                {
                    //Step 3: Get Reviewer Results project item
                    //A project can contain only one Reviewer Results project item
                    IEnumerable<ReviewerResultsProjectItem> projectItems = Project.Current.GetItems<ReviewerResultsProjectItem>();
                    ReviewerResultsProjectItem resultsProjectItem = null;
                    if (projectItems.Count() > 0)
                        resultsProjectItem = projectItems.FirstOrDefault() as ReviewerResultsProjectItem;

                    if (null != resultsProjectItem)
                    {
                        //Step 4: Get all session items that are currently referenced in the project
                        IEnumerable<Item> sessionItems = resultsProjectItem.GetItems();
                        if (null != sessionItems)
                        {
                            //Step 5: Find the session item (in the project) that is right clicked in the gallery
                            Item itemToRemove = sessionItems.Where(p => (p as Item).Path == galleryItem.Path).FirstOrDefault() as Item;
                            if (null != itemToRemove)
                            {
                                //Step 6: Remove this Session from the current project
                                await resultsProjectItem.RemoveSessionItemAsync(itemToRemove);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// This command shows how to add existing Batch Jobs to the current project.
        /// In this example you can browse and select multiple Reviewer Batch Jobs to add those to the current project.
        /// If the selected batch jobs contain any invalid batch jobs, those batch jobs will be reported to the user.
        /// </summary>
        /// <returns></returns>
        internal static async Task AddBatchJobs_Advanced()
        {
            try
            {
                //Step 1: Browse to select one or multiple batch jobs and get their path
                IEnumerable<string> rbjPaths = OpenBrowseBatchJobFileDialog();
                if (null == rbjPaths || rbjPaths.Count() < 1)
                    return;

                string strInvalidBatchJobs = "";
                int invalidRbjCount = 0;
                await QueuedTask.Run(() =>
                {
                    foreach (string rbjPath in rbjPaths)
                    {
                        try
                        {
                            //Step 2:  Create Reviewer BatchJobItems
                            //This is done inside a try-catch so that you can Skip invalid batch jobs and continue with valid batch jobs
                            IProjectItem rbjItem = DataReviewerModule.CreateBatchJobProjectItem(rbjPath);
                            if (null != rbjItem)
                            {
                                //Step 3: Add BatchJobItems to the current project
                                Project.Current.AddItem(rbjItem as IProjectItem);
                                ReviewerBatchJobProjectItem rbjProjectItem = rbjItem as ReviewerBatchJobProjectItem;

                                //Step 3: Add BatchJobItems to the gallery
                                if (null != rbjProjectItem)
                                    GalleryItemsChangedEvent.Publish(new GalleryItem(rbjProjectItem.TypeID, rbjProjectItem.Path, rbjProjectItem.Name), true);
                            }
                        }
                        catch(Exception e)
                        {
                            //Exception for invalid batch job
                            //Create a string to report invalid Batch Jobs
                            invalidRbjCount++;
                            strInvalidBatchJobs = strInvalidBatchJobs + "{0}" + invalidRbjCount.ToString() + ". " + rbjPath;
                        }
                    }
                });

                //Display error message for invalid Batch Jobs
                if (invalidRbjCount > 0)
                {
                    MessageBox.Show(string.Format("Selected Batch Jobs contain following invalid Batch Jobs :" + strInvalidBatchJobs, Environment.NewLine) + Environment.NewLine + "These Batch Jobs are not added to the project.",
                        "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// This command shows how to remove the reference of a Batch Job from the project.
        /// In this example you can right click a Batch Job in the Reviewer Batch Jobs gallery and remove it from the current project.
        /// </summary>
        /// <returns></returns>
        internal static async Task RemoveBatchJob_Advanced()
        {
            try
            {
                //Step 1: Get the Batch Job that is right clicked 
                var galleryItem = Utilities.GetContext<GalleryItem>().FirstOrDefault();
                if (null == galleryItem)
                    return;
                else
                {
                    //Step 2: Remove the Batch Job from the Gallery
                    GalleryItemsChangedEvent.Publish(galleryItem, false); 
                }

                //Step 3: Get all Reviewer Batch Job project items from the current project
                IEnumerable<ReviewerBatchJobProjectItem> projectItems = Project.Current.GetItems<ReviewerBatchJobProjectItem>();

                await QueuedTask.Run(() =>
                {
                    //Step 4: Find the Batch Job (in the current project) that is right clicked in the gallery
                    IProjectItem projectItemToRemove = projectItems.Where(p => (p as ReviewerBatchJobProjectItem).Path == galleryItem.Path).FirstOrDefault() as IProjectItem;
                    if (null != projectItemToRemove)
                    {
                        //Step 5: Remove the Batch Job from Project
                        Project.Current.RemoveItem(projectItemToRemove);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        #endregion

        #region Private methods
        
        /// <summary>
        /// This method is used to get the path of the geodatabase that user selects by browsing
        /// </summary>
        /// <param name="eResourceType"></param>
        /// <returns></returns>
        private static string OpenBrowseGeodatabaseDialog()
        {
            OpenItemDialog browseGeodatabaseDialog = new OpenItemDialog();

            browseGeodatabaseDialog.Title = "Browse Reviewer Workspace";
            if (string.IsNullOrEmpty(_lastBrowseLocation))
                browseGeodatabaseDialog.InitialLocation = @"C:\";
            else
                browseGeodatabaseDialog.InitialLocation = _lastBrowseLocation;

            browseGeodatabaseDialog.Filter = ItemFilters.geodatabases;
            
            
            browseGeodatabaseDialog.MultiSelect = false;
            browseGeodatabaseDialog.ShowDialog();

            if ((browseGeodatabaseDialog.Items == null) || (browseGeodatabaseDialog.Items.Count() < 1))
                return null;

            string strPath = browseGeodatabaseDialog.Items.FirstOrDefault().Path;

            //If the selected item is a file geodatabase
            if(System.IO.Directory.Exists(strPath)) 
                _lastBrowseLocation = System.IO.Directory.GetParent(strPath).FullName;
            else //If the selected item is a sde geodatabase file
                _lastBrowseLocation = new System.IO.FileInfo(strPath).Directory.FullName;

            return strPath;
        }

        /// <summary>
        /// This method is used to get the path of all Reviewer Batch jobs that user selects by browsing
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> OpenBrowseBatchJobFileDialog()
        {
            IEnumerable<string> batchJobs = null;

            OpenItemDialog browseGeodatabaseDialog = new OpenItemDialog();

            browseGeodatabaseDialog.Title = "Browse Reviewer Batch Jobs";
            if (string.IsNullOrEmpty(_lastBrowseLocation))
                browseGeodatabaseDialog.InitialLocation = @"C:\";
            else
                browseGeodatabaseDialog.InitialLocation = _lastBrowseLocation;

            //esri_browseDialogFilters_batchjobs is the filter for Reviewer Batch Job
            browseGeodatabaseDialog.Filter = "esri_browseDialogFilters_batchjobs";

            browseGeodatabaseDialog.MultiSelect = true;
            browseGeodatabaseDialog.ShowDialog();

            if ((browseGeodatabaseDialog.Items == null) || (browseGeodatabaseDialog.Items.Count() < 1))
                return null;

            batchJobs = browseGeodatabaseDialog.Items.Select(item => item.Path);

            if(null != batchJobs && batchJobs.Count()>0)
                _lastBrowseLocation = new System.IO.FileInfo(batchJobs.FirstOrDefault()).Directory.FullName;

            return batchJobs;
        }
        #endregion
    }
}
