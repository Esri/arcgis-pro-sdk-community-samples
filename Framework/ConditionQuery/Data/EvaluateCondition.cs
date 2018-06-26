/*

   Copyright 2018 Esri

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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using ArcGIS.Desktop.Framework;
using ConditionsQuery.Models;

namespace ConditionsQuery.Data {

    /// <summary>
    /// Given the state, at any given time in Pro, evaluate which conditions
    /// are currently true.
    /// </summary>
    internal class EvaluateCondition {
        
        private List<Condition>  _conditions = new List<Condition>();
        private ObservableCollection<Condition> _enabledConditions = new ObservableCollection<Condition>();
        private ObservableCollection<string> _activeStates = new ObservableCollection<string>();
        private Dictionary<string, List<string>> _stateConditionXRef = new Dictionary<string, List<string>>();
        
        private List<string> _conditionsWithNot = new List<string>();
        private static readonly object _lock = new object();

        private static readonly string DAML_FILE_FILTER = @"*.daml";

        public EvaluateCondition() {
            Initialize();
        }

        private void Initialize() {
            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location).ToUpper();
            string extensions = Path.Combine(exePath, "Extensions");

            DirectoryInfo diRoot = new DirectoryInfo(extensions);
            FileInfo[] damlFiles = diRoot.GetFiles(DAML_FILE_FILTER, SearchOption.AllDirectories);
            List<string> damlFilePaths = damlFiles.Select(fi => fi.FullName).ToList();
            HashSet<string> _damlFileMatches = new HashSet<string>();

            foreach (string damlFilePath in damlFilePaths) {
                if (_damlFileMatches.Contains(Path.GetFileName(damlFilePath)))
                    continue;
                _damlFileMatches.Add(Path.GetFileName(damlFilePath));
                System.Diagnostics.Debug.WriteLine($@"file: {damlFilePath}");
                var content = File.ReadAllText(damlFilePath);
                LoadConditions(content);
            }
        }

        private void LoadConditions(string contents) {
            var doc = XDocument.Parse(contents);
            //XNamespace defaultNS = "http://schemas.esri.com/DADF/Registry";
            XNamespace defaultNS = doc.Root.GetDefaultNamespace();

            //Get the configurations first
            var conditions = doc.Root.Element(defaultNS + "conditions");
            if (conditions != null) {
                foreach (var conditionElement in conditions.Elements()) {
                    var condition = new Condition();
                    if (condition.Configure(conditionElement)) {
                        _conditions.Add(condition);
                        //Get the states and make a cross-reference
                        //to the condition
                        var states = condition.GetStatesAsFlattenedList();
                        foreach (var state in states) {
                            if (!_stateConditionXRef.ContainsKey(state)) {
                                _stateConditionXRef.Add(state, new List<string> { condition.ID });
                            }
                            else {
                                var lc = _stateConditionXRef[state];
                                if (!lc.Contains(condition.ID))
                                    lc.Add(condition.ID);
                            }
                        }
                        //special cases are the conditions with "Not" states
                        if (condition.HasNotState())
                            _conditionsWithNot.Add(condition.ID);
                    }
                        
                }
            }
            //Get all the panes from all the modules
            var namespaceMgr = new XmlNamespaceManager(new NameTable());
            namespaceMgr.AddNamespace("ns", defaultNS.NamespaceName);
            var panes = doc.XPathSelectElements("//ns:modules/ns:insertModule/ns:panes/ns:pane", namespaceMgr);
            if (panes != null) {
                foreach (var paneElement in panes) {
                    var condition = new Condition();
                    //Panes act as a state with the pane id
                    condition.ConfigurePane(paneElement);
                    _conditions.Add(condition);
                    //there is only one "state" for a pane condition
                    var state = condition.GetStatesAsFlattenedList()[0];
                    if (!_stateConditionXRef.ContainsKey(state)) {
                        _stateConditionXRef.Add(state, new List<string> { condition.ID });
                    }
                    else {
                        var lc = _stateConditionXRef[state];
                        if (!lc.Contains(condition.ID))
                            lc.Add(condition.ID);
                    }
                    //last, the pane id is a state
                }
            }
        }

        /// <summary>
        /// Process the condition enabled/disabled status based on the provided
        /// list of active state ids
        /// </summary>
        public Task UpdateConditionsAndStateAsync() {

            //Collect all the states that are currently active
            return System.Threading.Tasks.Task.Run(() => {
                lock (_lock) {
                    _activeStates.Clear();
                    _enabledConditions.Clear();
                }

                var pane = FrameworkApplication.Panes.ActivePane;
                var states = _stateConditionXRef.Keys.ToList();
                var activeStates = new List<string>();
                foreach (var state in states) {
                    if (FrameworkApplication.State.Contains(state)) {
                        activeStates.Add(state);
                    }
                    else {
                        if (pane != null) {
                            if (pane.State.Contains(state)) {
                                activeStates.Add(state);
                            }
                        }
                    }
                }

                var processedConditions = new HashSet<string>();
                var enabledConditions = new List<Condition>();
                foreach (var c in _conditions)
                    c.IsEnabled = false; //reset
                foreach (var s in activeStates) {
                    var condition_ids = _stateConditionXRef[s];
                    foreach (var cid in condition_ids) {
                        if (processedConditions.Contains(cid))
                            continue; //already processed this condition
                        var condition = _conditions.First(c => c.ID == cid);
                        condition.IsEnabled = condition.MatchContext(activeStates);
                        if (condition.IsEnabled) {
                            enabledConditions.Add(condition);
                        }
                        processedConditions.Add(cid);
                    }
                }
                //Not conditions
                foreach (var not_c in _conditionsWithNot) {
                    if (processedConditions.Contains(not_c))
                        continue; //already processed this condition
                    var condition = _conditions.First(c => c.ID == not_c);
                    condition.IsEnabled = condition.MatchContext(activeStates);
                    if (condition.IsEnabled) {
                        enabledConditions.Add(condition);
                    }
                    processedConditions.Add(not_c);
                }
                //Sort the results
                lock (_lock) {
                    foreach (var state in activeStates.OrderBy(s => s)) {
                        _activeStates.Add(state);
                    }
                    foreach (var condition in enabledConditions.OrderBy(c => c.ID)) {
                        _enabledConditions.Add(condition);
                    }
                }
            });
        }

        /// <summary>
        /// Gets a collection of the active states
        /// </summary>
        public ObservableCollection<string> ActiveStates => _activeStates;

        /// <summary>
        /// Gets a collection of the enabled conditions
        /// </summary>
        public ObservableCollection<Condition> EnabledConditions => _enabledConditions;

        /// <summary>
        /// Gets the lock being used to synchronize the collections
        /// </summary>
        public static object Lock => _lock;

    }
}
