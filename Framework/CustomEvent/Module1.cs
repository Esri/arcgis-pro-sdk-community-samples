/*

   Copyright 2017 Esri

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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping.Events;

namespace CustomEvent
{

    /// <summary>
    /// Create a custom event argument class to contain any information that needs to be passed with your event. In this case, the old and new caption for our custom app name changed event.
    /// Derived from &lt;see cref="ArcGIS.Core.Events.EventBase"/&gt;
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any existing project or create a blank map.
    /// 1. Click on the Add-in tab on the ribbon and the click on the "Change Name" button to initiate a name change.
    /// 1. Note that the pop-up dialog is instantiated from the event subscriber method.  
    /// ![UI](Screenshots/Screen1.png)
    /// </remarks>
    public class NameChangedEventArgs : EventBase
    {

        /// <summary>
        /// Gets the old name
        /// </summary>
        public string OldName { get; private set; }

        /// <summary>
        /// Gets the new name
        /// </summary>
        public string NewName { get; private set; }

        /// <summary>
        /// Constructor. Create a name changed event passing in the new and old names
        /// </summary>
        /// <param name="oldName">The old name</param>
        /// <param name="newName">The new name</param>
        public NameChangedEventArgs(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    /// <summary>
    /// A custom CompositePresentationEvent to be published when we change the name
    /// of Pro
    /// </summary>
    public class NameChangedEvent : CompositePresentationEvent<NameChangedEventArgs>
    {

        /// <summary>
        /// Allow subscribers to register for our custom event
        /// </summary>
        /// <param name="action">The callback which will be used to notify the subscriber</param>
        /// <param name="keepSubscriberReferenceAlive">Set to true to retain a strong reference</param>
        /// <returns><see cref="ArcGIS.Core.Events.SubscriptionToken"/></returns>
        public static SubscriptionToken Subscribe(Action<NameChangedEventArgs> action, bool keepSubscriberReferenceAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<NameChangedEvent>()
                .Register(action, keepSubscriberReferenceAlive);
        }

        /// <summary>
        /// Allow subscribers to unregister from our custom event
        /// </summary>
        /// <param name="subscriber">The action that will be unsubscribed</param>
        public static void Unsubscribe(Action<NameChangedEventArgs> subscriber)
        {
            FrameworkApplication.EventAggregator.GetEvent<NameChangedEvent>().Unregister(subscriber);
        }
        /// <summary>
        /// Allow subscribers to unregister from our custom event
        /// </summary>
        /// <param name="token">The token that will be used to find the subscriber to unsubscribe</param>
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<NameChangedEvent>().Unregister(token);
        }

        /// <summary>
        /// Event owner calls publish to raise the event and notify subscribers
        /// </summary>
        /// <param name="payload">The associated event information</param>
        internal static void Publish(NameChangedEventArgs payload)
        {
            FrameworkApplication.EventAggregator.GetEvent<NameChangedEvent>().Broadcast(payload);
        }
    }

    /// <summary>
    /// Create a custom event argument class to contain any information that needs to be passed with your event. In this case, the old and new caption for our custom app name changed event.
    /// Derived from [ArcGIS.Core.Events.EventBase](https://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic7815.html)
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any existing project or create a blank map.
    /// 1. Click on the Add-in tab on the ribbon and the click on the "Change Name" button to initiate a name change.
    /// 1. Note that the pop-up dialog is instantiated from the event subscriber method.  
    /// ![UI](Screenshots/Screen1.png)
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CustomEvent_Module"));
            }
        }

        /// <summary>
        /// Internal constructor. We use it as a convenient location to subscribe to our custom event
        /// </summary>
        /// <remarks>The constructor will be called when the button is first clicked</remarks>
        internal Module1()
        {
            NameChangedEvent.Subscribe((args) =>
            {
                MessageBox.Show($"Name has changed:\r\nOld: {args.OldName}\r\nNew: {args.NewName}", "NameChangedEvent");
            });

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

        #endregion Overrides

    }
}
