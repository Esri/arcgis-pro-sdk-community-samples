//   Copyright 2017 Esri
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
using System.Net;
using System.Runtime.Serialization;
using System.Windows.Input;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGISOnlineConnect.ArcGISOnlineHelpers;

namespace ArcGISOnlineConnect
{
    internal class AgolDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ArcGISOnlineConnect_AgolDockpane";
        private AgolUser _AgolUser;
        private AgolSearchResult _AgolSearchResult;
        private AgolUserContent _AgolUserContent;
        private AgolFolderContent _AgolFolderContent;

        protected AgolDockpaneViewModel()
        {
            _CommandDoQuery = new RelayCommand(() => CmdDoQuery(), () => CanDoQuery());
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            
            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "AGOL Query Test";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        /// <summary>
        /// Returns the current active ArcGIS Online string
        /// </summary>
        private string _AgolUrl;
        public string AgolUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_AgolUrl)) _AgolUrl = ArcGISPortalManager.Current.GetActivePortal().ToString();
                return _AgolUrl;
            }
            set
            {
                SetProperty(ref _AgolUrl, value, () => AgolUrl);
            }
        }

        /// <summary>
        /// Agol Queries:  all queries are loaded into this dictionary and presented for user selection
        /// </summary>
        public IDictionary<ArcGISOnlineQueries.AGSQueryType, string> AgolQueries
        {
            get { return ArcGISOnlineQueries.AGSQueries.ArcGisOnlineQueryTypesDictionary; }
        }

        /// <summary>
        /// Selected query: one of the AGOL queries has been selected by the user
        /// </summary>
        private KeyValuePair<ArcGISOnlineQueries.AGSQueryType, string> _AgolQuery;
        public KeyValuePair<ArcGISOnlineQueries.AGSQueryType, string> AgolQuery
        {
            get { return _AgolQuery; }
            set
            {
                SetProperty(ref _AgolQuery, value, () => AgolQuery);
                // fill CallParams
                CallParams.Clear();
                CommandQuery = string.Format("Run {0} ArcGIS Online Query", value.Key);
                var parts = value.Value.Split(";".ToCharArray());
                if (parts.Length > 2)
                {
                    var names = parts[1].Split(",".ToCharArray()); 
                    var values = parts[2].Split(",".ToCharArray());
                    for (var idx = 0; idx < (names.Length > values.Length ? names.Length : values.Length); idx++)
                    {
                        var theValue = values[idx];
                        if (_AgolUser != null)
                        {
                            switch (theValue)
                            {
                                case "<Enter your ArcGIS Online Username>":
                                    theValue = _AgolUser.username;
                                    break;
                                case "<Enter one of you ArcGIS Group Ids>":
                                    if (_AgolUser.groups != null && _AgolUser.groups.Count > 0)
                                    {
                                        theValue = _AgolUser.groups[0].id;
                                    }
                                    break;
                            }
                        }
                        if (_AgolSearchResult != null)
                        {
                            switch (theValue)
                            {
                                case "<Enter an ArcGIS Online Item Id>":
                                    if (_AgolFolderContent != null && _AgolFolderContent.items != null
                                        && _AgolFolderContent.items.Length > 0)
                                    {
                                        theValue = _AgolFolderContent.items[0].id;
                                        break;
                                    }
                                    if (_AgolUserContent != null && _AgolUserContent.items != null
                                        && _AgolUserContent.items.Length > 0)
                                    {
                                        theValue = _AgolUserContent.items[0].id;
                                        break;
                                    }
                                    if (_AgolSearchResult.results != null && _AgolSearchResult.results.Length > 0)
                                    {
                                        theValue = _AgolSearchResult.results[0].id;
                                    }
                                    break;
                            }
                        }
                        if (_AgolUserContent != null)
                        {
                            switch (theValue)
                            {
                                case "<Enter a content folder id>":
                                    if (_AgolUserContent.folders != null && _AgolUserContent.folders.Length > 0)
                                    {
                                        theValue = _AgolUserContent.folders[0].id;
                                    }
                                    break;
                            }
                        }
                        CallParams.Add(new MyParam { Name = names[idx], Param = theValue });
                    }
                }
                NotifyPropertyChanged(() => CallParams);
            }
        }

        /// <summary>
        /// The list of parameters for the selected query
        /// </summary>
        private ObservableCollection<MyParam> _CallParams = new ObservableCollection<MyParam>();
        public ObservableCollection<MyParam> CallParams
        {
            get { return _CallParams; }
            set
            {
                SetProperty(ref _CallParams, value, () => CallParams);
            }
        }

        /// <summary>
        /// Command for executing queries.  Bind to this property in the view.
        /// </summary>
        private ICommand _CommandDoQuery;
        public ICommand CommandDoQuery
        {
            get { return _CommandDoQuery; }
        }
        
        /// <summary>
        /// Query Result display area
        /// </summary>
        private string _QueryResult;
        public string QueryResult
        {
            get
            {
                return _QueryResult;
            }
            set
            {
                SetProperty(ref _QueryResult, value, () => QueryResult);
            }
        }

        /// <summary>
        /// text for command query button
        /// </summary>
        private string _CommandQuery = "Run ArcGIS Online Query";
        public string CommandQuery
        {
            get { return _CommandQuery; }
            set
            {
                SetProperty(ref _CommandQuery, value, () => CommandQuery);
            }
        }

        /// <summary>
        /// Execute the query command
        /// </summary>
        private void CmdDoQuery()
        {
            try
            {
                var httpEsri = new EsriHttpClient() { BaseAddress = ArcGISPortalManager.Current.GetActivePortal().PortalUri };
                // value has the following format: url with {};Redlands,...,...;Search Query,...,...
                // where Redlands ... are values and search query are keys
                var parts = AgolQuery.Value.Split(";".ToCharArray());
                var getUrl = parts[0];

                // get parameters
                if (CallParams.Count > 0)
                {
                    var lstParams = new List<object>() as IList<object>;
                    foreach (var kv in CallParams)
                        lstParams.Add(kv.Param);
                    getUrl = string.Format(parts[0], lstParams.ToArray());
                }
                System.Diagnostics.Debug.WriteLine(getUrl);
                var httpResponse = httpEsri.Get(getUrl);
                if (httpResponse.StatusCode == HttpStatusCode.OK && httpResponse.Content != null)
                {
                    var content = httpResponse.Content.ReadAsStringAsync().Result;
                    QueryResult = string.Format(@"{0}{1}", getUrl, System.Environment.NewLine);
                    QueryResult += string.Format(@"IsSuccessStatusCode: {0}{1}", httpResponse.IsSuccessStatusCode, System.Environment.NewLine);
                    QueryResult += string.Format(@"StatusCode: {0}{1}", httpResponse.StatusCode, System.Environment.NewLine);
                    QueryResult += content;
                    if (AgolQuery.Key == ArcGISOnlineQueries.AGSQueryType.GetSelf)
                    {
                        _AgolUser = AgolUser.LoadAgolUser(content);
                    }
                    else if (AgolQuery.Key == ArcGISOnlineQueries.AGSQueryType.GetSearch)
                    {
                        _AgolSearchResult = AgolSearchResult.LoadAgolSearchResult(content);
                    }
                    else if (AgolQuery.Key == ArcGISOnlineQueries.AGSQueryType.GetUserContent)
                    {
                        _AgolUserContent = AgolUserContent.LoadAgolUserContent(content);
                        _AgolFolderContent = null;
                    }
                    else if (AgolQuery.Key == ArcGISOnlineQueries.AGSQueryType.GetUserContentForFolder)
                    {
                        _AgolFolderContent = AgolFolderContent.LoadAgolFolderContent(content);
                    }
                    System.Diagnostics.Debug.WriteLine(QueryResult);
                }
            }
            catch (Exception ex)
            {
                QueryResult = ex.ToString();
            }
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the query button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanDoQuery()
        {
            return AgolQuery.Key != ArcGISOnlineQueries.AGSQueryType.None;
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class AgolDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            AgolDockpaneViewModel.Show();
        }
    }

    [DataContract]
    internal class MyParam
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Param { get; set; }
    }
}
