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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConditionsQuery.Models {
    /// <summary>
    /// The type of state node
    /// </summary>
    public enum StateType {
        State = 0,
        Or,
        And,
        Not
    };

    /// <summary>
    /// Implementation of the state element from Config.daml. A state can either be
    /// true or false.
    /// </summary>
    /// <remarks>State can either be an id or can be a boolean operator. Boolean
    /// operators are applied to their children. A parent state will evaluate to
    /// true or false depending on the true/false state of its component children</remarks>
    internal class State : IEnumerable<State> {

        private string _id = "";
        private StateType _stateType = StateType.State;//default
        private List<State> _children = new List<State>();

        public State() {
        }

        internal bool Configure(XElement node) {
            var name = node.Name.LocalName;
            _id = "";
            if (name == "state") {
                _stateType = StateType.State;
                _id = node.Attribute("id").Value;
                return true;
            }

            if (!node.HasElements)
                return false;//bogus
            //What kind of state node is this?
            if (name == "and")
                _stateType = StateType.And;
            else if (name == "or")
                _stateType = StateType.Or;
            else if (name == "not")
                _stateType = StateType.Not;
            else {
                return false;//bogus
            }
            //Process the children
            foreach (var elem in node.Elements()) {
                var s = new State();
                if (s.Configure(elem))
                    _children.Add(s);
            }
            return true;
        }

        internal void AddChildren(IEnumerable<State> children) {
            foreach (var childState in children) {
                _children.Add(childState);
            }
        }

        internal void AddChild(State child) {
            _children.Add(child);
        }

        /// <summary>
        /// Does the state contain this id?
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if the state is found</returns>
        public bool Contains(string id) {
            if (this.ID == id)
                return true;
            foreach (var s in _children) {
                if (s.Contains(id))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the contained state ids of the provided StateType.State
        /// </summary>
        /// <param name="st"></param>
        /// <returns>A list of state ids</returns>
        public IReadOnlyList<string> GetStateIdsAsFlattenedList(StateType st = StateType.State) {
            var states = new List<string>();
            if (this.StateType == st)
                states.Add(this.ID);
            foreach (var s in _children) {
                var child_states = s.GetStateIdsAsFlattenedList(st);
                foreach (var c in child_states)
                    states.Add(c);
            }
            return states;
        }

        /// <summary>
        /// Given a list of the active state ids, does the context of this
        /// state match that of the active ids?
        /// </summary>
        /// <param name="activeStates"></param>
        /// <returns>True if the context matches, otherwise false</returns>
        public bool MatchContext(IReadOnlyList<string> activeStates) {

            switch (this.StateType) {
                case StateType.State: {
                        return activeStates.Contains(this.ID);
                    }
                case StateType.Or: {
                        foreach (var child in _children) {
                            if (child.MatchContext(activeStates))
                                return true;
                        }
                        return false;
                    }

                case StateType.And: {
                        foreach (var child in _children) {
                            if (!child.MatchContext(activeStates))
                                return false;
                        }
                        return true;
                    }

                case StateType.Not: {
                        foreach (var child in _children) {
                            if (child.MatchContext(activeStates))
                                return false;
                        }
                        return true;
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets and sets the StateType of the state node
        /// </summary>
        public StateType StateType
        {
            get
            {
                return _stateType;
            }
            set
            {
                _stateType = value;
            }
        }

        /// <summary>
        /// Gets and sets the state id. Can be empty string for
        /// state nodes that are boolean operators
        /// </summary>
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Required for implementation of IEnumerable
        /// </summary>
        /// <returns></returns>
        public IEnumerator<State> GetEnumerator() {
            return _children.GetEnumerator();
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
