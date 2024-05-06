/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Editing.Attributes;
using Attribute = ArcGIS.Desktop.Editing.Attributes.Attribute;
using System.Windows.Shapes;

namespace EditorInspectorUI.InspectorUIProvider
{
  /// <summary>
  /// This class provides a custom implementation of the InspectorProvider class
  /// </summary>
  class MyProvider : ArcGIS.Desktop.Editing.InspectorProvider
  {
    //Override this to highlight specific attributes
    public override bool? IsHighlighted(ArcGIS.Desktop.Editing.Attributes.Attribute attr)
    {
      if (attr.FieldName == "Address")
        return true;

      return false;
    }
    //Override this to hide specific attributes
    public override bool? IsVisible(ArcGIS.Desktop.Editing.Attributes.Attribute attr)
    {
      foreach (var item in _fieldsToHide)
      {
        if (attr.FieldName == item)
          return false;
      }
      return true;
    }
    //Override this to display the attributes in a specific order
    public override IEnumerable<Attribute> AttributesOrder(IEnumerable<Attribute> attrs)
    {
      var newList = new List<ArcGIS.Desktop.Editing.Attributes.Attribute>();
      foreach (var field in _fieldOrder)
      {
        if (attrs.Any(a => a.FieldName == field))
        {
          newList.Add(attrs.First(a => a.FieldName == field));
        }
      }
      return newList;
    }
    //Override this to display the attributes with a custom alias
    public override string CustomName(ArcGIS.Desktop.Editing.Attributes.Attribute attr)
    {
      if (attr.FieldName == "Job_Number")
        return "ID";
      if (attr.FieldName == "Job_Type")
        return "Job Type";
      if (attr.FieldName == "Job_Status")
        return "Status";
      if (attr.FieldName == "Job_Desc")
        return "Job Description";
      if (attr.FieldName == "DateComplt")
        return "Date Completed";

      return attr.FieldName;
    }
    //Override this to make specific attributes read-only
    public override bool? IsEditable(ArcGIS.Desktop.Editing.Attributes.Attribute attr)
    {
      if (attr.FieldName == "Ownership")
        return false;

      return true;
    }
    public override void BeginLoad(IEnumerable<Attribute> attrs)
    {
      base.BeginLoad(attrs);
    }
    public override void EndLoad()
    {
      base.EndLoad();
    }

    private List<string> _fieldsToHide = new List<string>()
      {
        "OBJECTID",
        "Shape",
        "ResidFlag",
        "NonresFlag",
        "CompltYear",
        "PermitYear",
        "ClassAInit",
        "ClassAProp",
        "ClassANet",
        "HotelInit",
        "HotelProp",
        "CenBlock20",
      };
    private List<string> _fieldOrder = new List<string>()
      {
        "Job_Number",
        "Job_Type",
        "Address",
        "Job_Status",
        "Job_Desc",
        "DateComplt",
        "Ownership"
      };
    #region Info for 3.1 users
    // bug in 3.1.  Make sure you override this method and return non-null otherwise you will get a crash.
    //  fixed in 3.1.3
    //public override IEnumerable<Attribute> AttributesOrder(IEnumerable<Attribute> attrs)
    //{
    //  var newList = new List<ArcGIS.Desktop.Editing.Attributes.Attribute>();
    //  foreach (var attr in attrs)
    //  {
    //    newList.Add(attr);
    //  }
    //  return newList;
    //}

    // use BeginLoad/EndLoad if you need the entire set of attributes at once
    //  to perform any pre-processing
    // EndLoad can be used to clear any cached values
    #endregion
  }
}
