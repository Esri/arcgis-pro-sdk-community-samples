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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConditionsQuery.Models {

    /// <summary>
    /// Provides an implementation of the daml condition element.
    /// </summary>
    /// <remarks>Conditions can either be true or false depending on whether
    /// their corresponding 'state' is true or false. State, within Pro, is
    /// constantly changing in response to the UI Context, selected items,
    /// Project content changes, etc. Context changes are propogated to the UI
    /// in many cases via conditions. Whenever a condition changes from true to
    /// false, any UI elements using that condition are enabled/disabled or visible/hidden
    /// depending on what kind of UI element they are.<br/>
    /// You can read more about conditions here:<br/>
    /// <a href="https://github.com/ArcGIS/arcgis-pro-sdk/wiki/ProConcepts-Framework#condition-and-state">
    /// https://github.com/ArcGIS/arcgis-pro-sdk/wiki/ProConcepts-Framework#condition-and-state</a></remarks>
    internal class Condition : INotifyPropertyChanged, IEnumerable<State> {

        private List<State> _states = null;
        private List<string> _flattenedStates = null;
        private bool _hasNotState = false;
        private bool _hasNotStateIsInitialized = false;
        private string _xml = "";

        public Condition() {
        }

        private string _id = "";
        private string _caption = "";
        private bool _enabled = false;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// The condition id from the DAML
        /// </summary>
        public string ID => _id;

        /// <summary>
        /// Condition caption as read from the DAML
        /// </summary>
        /// <remarks>Optional</remarks>
        public string Caption => string.IsNullOrEmpty(_caption) ? _id : _caption;

        /// <summary>
        /// Gets and Sets whether the condition is currently enabled
        /// </summary>
        /// <remarks>The condition enabled/disabled state is controlled
        /// by whether or not Pro's active state matches the condition's
        /// state combination(s).
        /// The combination of state that a given condition may have can
        /// be quite complex and include Or, And, and Not relationships that
        /// can be nested.
        ///</remarks>
        public bool IsEnabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the xml of the condition as read from the daml
        /// </summary>
        public string Xml => _xml;

        /// <summary>
        /// Configure the condition and state based on the condition xml read
        /// from the daml
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool Configure(XElement condition) {
            //the condition must have children
            if (!condition.HasElements)
                //The condition node must have at least one child
                return false;
            _xml = condition.ToString(SaveOptions.DisableFormatting);
            //var reader = condition.CreateReader();
            //reader.MoveToContent();
            //_xml = reader.ReadInnerXml();

            _id = condition.Attribute("id").Value;
            var attrib = condition.Attributes().FirstOrDefault(a => a.Name == "caption");
            _caption = attrib?.Value;

            //The first child MUST be one of Or, And, or Not
            //The Or is implicit and so must be added if it was not
            //specified in the DAML
            bool needsOrStateNodeAdded = condition.Elements().First().Name.LocalName == "state";

            //Get all the child state nodes
            var states = new List<State>();
            foreach (var child in condition.Elements()) {
                //check what the first child node is
                var state = new State();
                if (state.Configure(child)) {
                    states.Add(state);
                }
            }
            //Do we need an Or Node added?
            if (needsOrStateNodeAdded) {
                _states = new List<State>();
                _states.Add(new State() {
                    StateType = StateType.Or
                });
                _states[0].AddChildren(states);
            }
            else {
                _states = states;
            }
            return _states.Count > 0;//There has to be at least one state
        }

        /// <summary>
        /// Creates a condition for the given pane
        /// </summary>
        /// <remarks>Pane ids in Pro are implicitly conditions that are set true/false
        /// whenever their corresponding pane is activated/deactivated</remarks>
        /// <param name="pane"></param>
        /// <returns></returns>
        public bool ConfigurePane(XElement pane) {
            _id = pane.Attribute("id").Value;
            var attrib = pane.Attributes().FirstOrDefault(a => a.Name == "caption");
            _caption = attrib?.Value;
            _states = new List<State>();
            _states.Add(new State() {
                StateType = StateType.Or
            });
            _states[0].AddChild(new State() {
                StateType = StateType.State,
                ID = _id
            });
            return true;
        }

        /// <summary>
        /// Does the condition contain the given state id?
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public bool ContainsState(string stateID) {
            var states = this.GetStatesAsFlattenedList();
            return states.Any(s => s == stateID);
        }

        /// <summary>
        /// Get the contained ids of the condition's states
        /// </summary>
        /// <remarks>Strips out the hierarchy and any boolean operators</remarks>
        /// <returns></returns>
        public IReadOnlyList<string> GetStatesAsFlattenedList() {
            //This can be cached. Once Pro has started it does not change
            if (_flattenedStates == null) {
                _flattenedStates = new List<string>();
                foreach (var s in _states) {
                    var fl = s.GetStateIdsAsFlattenedList();
                    foreach (var c in fl)
                        _flattenedStates.Add(c);
                }
            }
            return _flattenedStates;
        }

        /// <summary>
        /// Gets whether or not the condition contains any "Not"
        /// state operators
        /// </summary>
        /// <remarks>Conditions containing Not state require special handling</remarks>
        /// <returns>True if the condition contains a "not" state node</returns>
        public bool HasNotState() {
            if (!_hasNotStateIsInitialized) {
                bool hasNotState = false;
                foreach (var s in _states) {
                    var fl = s.GetStateIdsAsFlattenedList(StateType.Not);
                    if (fl.Count > 0) {
                        hasNotState = true;
                        break;
                    }
                }
                _hasNotState = hasNotState;
                _hasNotStateIsInitialized = true;
            }
            return _hasNotState;
        }
        /// <summary>
        /// Evaluates the condition against the given list of state ids
        /// </summary>
        /// <param name="activeStates"></param>
        /// <returns>True if the condition evaluates to true for the given state</returns>
        public bool MatchContext(IReadOnlyList<string> activeStates) {
            if (_states.Count > 1) {
                //implicit Or
                foreach (var state in _states) {
                    if (state.MatchContext(activeStates))
                        return true;
                }
                return false;
            }
            else {
                return _states[0].MatchContext(activeStates);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Required for implementation of IEnumerable
        /// </summary>
        /// <returns></returns>
        public IEnumerator<State> GetEnumerator() {
            return _states.GetEnumerator();
        }

        /// <summary>
        /// Required for implementation of IEnumerable
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}
