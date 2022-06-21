/*

   Copyright 2022 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Internal.Core;
using System.Threading.Tasks;
using System.Windows.Media;

namespace QuakeItem.Items
{
	/// <summary>
	/// QuakeProjectItemContainer is a custom project item container.
	/// </summary>
	/// <remarks>Custom project item containers show in the catalog pane as folders. They are
	/// not initially visible. They become visible the momment a custom project item,
	/// in this case, a <i>QuakeProjectItem</i> is added to the project.<br/>
	/// TODO: The images via IProjectItemImage need to become overrides on the
	/// base class</remarks>
	internal class QuakeProjectItemContainer : CustomProjectItemContainer<QuakeProjectItem>
	{
		//This should be an arbitrary unique string. It must match your <content type="..." 
		//in the Config.daml for the container
		public static readonly string ContainerName = "QuakeContainer";
		public QuakeProjectItemContainer() : base(ContainerName)
		{
		}

		//componentType will be your "ComponentTypeValue" value
		//see "this.ComponentType" property also
		public QuakeProjectItemContainer(string componentType) : base(componentType)
		{
		}

		/// <summary>
		/// Create item is called whenever a custom item, registered with the container,
		/// is browsed or fetched (eg the user is navigating through different folders viewing
		/// content in the catalog pane).
		/// </summary>
		/// <param name="name"></param>
		/// <param name="path"></param>
		/// <param name="containerType"></param>
		/// <param name="data"></param>
		/// <returns>A custom item created from the input parameters</returns>
		public override Item CreateItem(string name, string path, string containerType, string data)
		{
			var item = ItemFactory.Instance.Create(path) as QuakeProjectItem;
			if (item != null)
			{
				// IncludeInPackages true ensures that the quake file is included
				// in any project templates and project packages.
				item.IncludeInPackages(true);
				this.Add(item);
			}
			return item;
		}

		/// <summary>
		/// Adds an item to the container. This will trigger the visibility of the
		/// container if it was previously empty.
		/// </summary>
		/// <param name="quakeProjectItem"></param>
		public void AddItem(QuakeProjectItem quakeProjectItem)
		{
			this.Add(quakeProjectItem);
		}

		public override ImageSource LargeImage
		{
			get
			{
				return System.Windows.Application.Current.Resources["Folder32"] as ImageSource;
			}
		}

		public override Task<System.Windows.Media.ImageSource> SmallImage
		{
			get
			{
				return Task.FromResult(
					System.Windows.Application.Current.Resources["Folder16"] as ImageSource);
			}
		}

	}
}
