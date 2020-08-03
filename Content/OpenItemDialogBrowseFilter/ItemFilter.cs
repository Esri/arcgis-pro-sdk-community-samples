/*

   Copyright 2020 Esri

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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenItemDialogBrowseFilter
{
  class ItemFilter : IComparable<ItemFilter>
  {
    public static readonly string browseFilterXML = @"C:\Program Files\ArcGIS\Pro\Resources\SearchResources\Schema\BrowseFilters.xml";
    public static readonly string esriItemsXML = @"C:\Program Files\ArcGIS\Pro\Resources\SearchResources\Schema\ItemTraits.xml";
    private string _damlFileName;
    private string _elementName;
    private string _container;
    private string _parent;
    private string _id;
    private string _caption;
    private string _content;
    private List<string> _filterFlags = new List<string>();
    private static string _currentControl;
    public string ElementName => _elementName;
    public string ID => _id;
    public string Content => _content;
    /// <summary>
    /// Creates a browse project filter or esri_item item
    /// </summary>
    /// <remarks>
    /// This is only for browse filters and esri_item
    /// </remarks>
    public ItemFilter(string damlFile, string elementName, XElement xElement, string container)
    {
      //initialize members
      _damlFileName = damlFile;
      _elementName = elementName;

      _parent = xElement.Parent.Name.LocalName;
      _container = container;
      _id = xElement.Attribute("id").Value;
      //This is esri_browseFilters
      if (elementName == "esri_browseFilters")
      {
        XElement contentChild = xElement.Descendants().FirstOrDefault();
        _caption = contentChild.Attribute("displayName") == null ? "" : contentChild.Attribute("displayName").Value;
      }
      //this is an esri_item
      if (elementName == "esri_item")
      {
        //This gives us the filterFlag element
        var descendents = xElement.Descendants();        
        _content = xElement.ToString(SaveOptions.None);
        XElement filterFlagsElements = xElement.Descendants().Where(d => d.Name.LocalName == "filterFlags").FirstOrDefault();
        //Get the "type child elements (can be multiple children)
        var typeElements = filterFlagsElements?.Descendants().Where(d => d.Name.LocalName == "type");
        //iterate through all types to get the filterFlags for this typeID
        foreach (var type in typeElements)
        {
          if (type.Attribute("id") == null) continue;
          _filterFlags.Add(type.Attribute("id").Value);
        }
      }
    }

    /// <summary>
    /// A collection of the DAML Items from the public daml files is created
    /// </summary>
    /// <returns></returns>
    public static List<ItemFilter> GetDamlItems()
    {
      var items = new List<ItemFilter>();

      //Read BrowseFilters.xml for list of browse filters in Pro
      items.AddRange(GetFiltersOrItems(browseFilterXML, "esri_browseFilters"));
      items.AddRange(GetFiltersOrItems(esriItemsXML, "esri_item"));
      return items;
    }
    /// <summary>
    /// To select the elements from the DAML
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    /// /// tag: updateCategory attribute: refID="esri_browseFilters"
    ///   -tag: insertComponent get id attribute: id = "esri_browseDialogFilters_all" 
    private static bool SelectInsertComponentElementPredicate(XElement element)
    {
      if ((element.Attribute("id") == null))
      {
        //no id attribute
        return false;
      }

      if (!(element.Name.ToString().Contains("insertComponent")))
      {
        //Need insertComponent as element name
        return false;
      }
      return true;
    }
    private static List<ItemFilter> GetFiltersOrItems(string fileFullPath, string currentControl)
    {
      if (!File.Exists(fileFullPath))
        return null;
      var items = new List<ItemFilter>();
      var content = File.ReadAllText(fileFullPath);
      XDocument bfXamlDoc = XDocument.Parse(content);
      _currentControl = currentControl;//  "esri_browseFilters" or esri_item;
      IEnumerable<XElement> listOfBfs =
          bfXamlDoc.Root.Descendants().Where(SelectInsertComponentElementPredicate)
              .OrderBy(elem => elem.Attribute("id").Value); //Order by id attribute
      if (listOfBfs.Any())
      {
        foreach (var elem in listOfBfs)
        {
          // public DAMLItem(string damlFile, string elementName, XElement xElement, string refIDFilter )
          items.Add(new ItemFilter(Path.GetFileNameWithoutExtension(browseFilterXML), currentControl, elem, currentControl));
        }
      }
      return items;
    }
    
    int IComparable<ItemFilter>.CompareTo(ItemFilter other)
    {
      return this.ID.CompareTo(other.ID);
    }
  }
}
