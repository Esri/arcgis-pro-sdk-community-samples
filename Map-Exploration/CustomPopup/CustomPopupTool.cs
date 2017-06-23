//   Copyright 2014 Esri
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ArcGIS.Desktop.Framework.Dialogs;
using System.Globalization;

namespace CustomPopup
{
  /// <summary>
  /// Implementation of custom pop-up tool.
  /// </summary>
  internal class CustomPopupTool : MapTool
  {
    /// <summary>
    /// Define the tool as a sketch tool that draws a rectangle in screen space on the view.
    /// </summary>
    public CustomPopupTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Screen; //required for 3D selection and identify.
    }

    /// <summary>
    /// Called when a sketch is completed.
    /// </summary>
    protected override async Task<bool> OnSketchCompleteAsync(ArcGIS.Core.Geometry.Geometry geometry)
    {
      var popupContent = await QueuedTask.Run(() =>
      {
        var mapView = MapView.Active;
        if (mapView == null)
          return null;

        //Get the features that intersect the sketch geometry.
        var result = mapView.GetFeatures(geometry);

        //For each feature in the result create a new instance of our custom popup content class.
        List<PopupContent> popups = new List<PopupContent>();
        foreach (var kvp in result)
        {
          kvp.Value.ForEach(id => popups.Add(new DynamicPopupContent(kvp.Key, id)));
        }

        //Flash the features that intersected the sketch geometry.
        mapView.FlashFeature(result);

        //return the collection of popup content object.
        return popups;
      });

      //Create the list of custom popup commands to show at the bottom of the pop-up window.
      var commands = CreateCommands();

      //Show the custom pop-up with the custom commands and the default pop-up commands. 
      MapView.Active.ShowCustomPopup(popupContent, CreateCommands(), true);
      return true;
    }

    /// <summary>
    /// Create and return a new collection of popup commands
    /// </summary>
    /// <returns></returns>
    private List<PopupCommand> CreateCommands()
    {
      var commands = new List<PopupCommand>
            {
                //Add a new instance of a popup command passing in the delegate to be run when the button is clicked.
                new PopupCommand(OnPopupCommand, CanExecutePopupCommand,
              "Show statistics",
              new BitmapImage(new Uri("pack://application:,,,/CustomPopup;component/Images/GenericButtonRed12.png")) as ImageSource)
                {
                    IsSeparator = true
                }
            };
      return commands;
    }

    /// <summary>
    /// The method called when the custom popup command is clicked.
    /// </summary>
    void OnPopupCommand(PopupContent content)
    {
      //Cast the content parameter to our custom popup content class.
      DynamicPopupContent dynamicContent = content as DynamicPopupContent;
      if (dynamicContent == null)
        return;

      //Call a method on the custom popup content object to show some statistics for the current popup content.
      dynamicContent.ShowStatistics();
    }

    /// <summary>
    /// The method called periodically by the framework to determine if the command should be enabled.
    /// </summary>
    bool CanExecutePopupCommand(PopupContent content)
    {
      return content != null;
    }
  }

  /// <summary>
  /// Implementation of a custom popup content class
  /// </summary>
  internal class DynamicPopupContent : PopupContent
  {
    private Dictionary<FieldDescription, double> _values = new Dictionary<FieldDescription, double>();

    /// <summary>
    /// Constructor initializing the base class with the layer and object id associated with the pop-up content
    /// </summary>
    public DynamicPopupContent(MapMember mapMember, long id) : base(mapMember, id)
    {
      //Set property indicating the html content will be generated on demand when the content is viewed.
      IsDynamicContent = true;
    }

    /// <summary>
    /// Called the first time the pop-up content is viewed. This is good practice when you may show a pop-up for multiple items at a time. 
    /// This allows you to delay generating the html content until the item is actually viewed.
    /// </summary>
    protected override Task<string> OnCreateHtmlContent()
    {
      return QueuedTask.Run(() =>
      {
        var invalidPopup = "<p>Pop-up content could not be generated for this feature.</p>";

        var layer = MapMember as BasicFeatureLayer;
        if (layer == null)
          return invalidPopup;

        //Get all the visible numeric fields for the layer.
        var fields = layer.GetFieldDescriptions().Where(f => IsNumericFieldType(f.Type) && f.IsVisible);

        //Create a query filter using the fields found above and a where clause for the object id associated with this pop-up content.
        var tableDef = layer.GetTable().GetDefinition();
        var oidField = tableDef.GetObjectIDField();
        var qf = new QueryFilter() { WhereClause = string.Format("{0} = {1}", oidField, ID), SubFields = string.Join(",", fields.Select(f => f.Name)) };
        var rows = layer.Search(qf);

        //Get the first row, there should only be 1 row.
        if (!rows.MoveNext())
          return invalidPopup;

        var row = rows.Current;

        //Loop through the fields, extract the value for the row and add to a dictionary.
        foreach (var field in fields)
        {
          var val = row[field.Name];
          if (val is DBNull || val == null)
            continue;
              double value;
          if (!Double.TryParse(val.ToString(), out value))
            continue;

          if (value < 0)
            continue;

          _values.Add(field, value);
        }

        if (_values.Count == 0)
          return invalidPopup;

        //Construct a new html string that we will use to update our html template.
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("data.addColumn('string', 'Age')"); //Choose a label that makes sense for the numeric fields shown.
        sb.AppendLine("data.addColumn('number', 'Number of People')"); //Choose a label that makes sense for the values shown in those fields.
        sb.AppendLine("data.addColumn('number', 'Percentage')");
        sb.AppendLine("data.addRows([");

        //Add each value to the html string.
        foreach (var v in _values)
        {
          var percentage = (v.Value / _values.Sum(kvp => kvp.Value)) * 100;
          sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "['{0}', {{v: {1} }}, {{v: {2} }}],", v.Key.Alias, v.Value, percentage));
        }

        sb.AppendLine("]);");

        //Get the html from the template file on disk that we have packaged with our add-in.
        var htmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "template.html");
        var html = File.ReadAllText(htmlPath);

        //Update the template with our custom html and return it to be displayed in the pop-up window.
        html = html.Replace("//data.addColumn", sb.ToString());
        return html;
      });
    }

    /// <summary>
    /// Show a message box with the Min and Max value and associated field.
    /// </summary>
    internal void ShowStatistics()
    {
      var maxVal = _values.Values.Max();
      var minVal = _values.Values.Min();
      var maxElements = _values.Where(v => v.Value == maxVal);
      var minElements = _values.Where(v => v.Value == minVal);

      StringBuilder sb = new StringBuilder();
      sb.AppendLine(string.Format("Max ({0}): {1}", maxElements.First().Value, string.Join(",", maxElements.Select(x => x.Key.Alias))));
      sb.Append(string.Format("Min ({0}): {1}", minElements.First().Value, string.Join(",", minElements.Select(x => x.Key.Alias))));
      MessageBox.Show(sb.ToString(), "Statistics");
    }

    /// <summary>
    /// Test if the field is a numeric type.
    /// </summary>
    private bool IsNumericFieldType(FieldType type)
    {
      switch (type)
      {
        case FieldType.Double:
        case FieldType.Integer:
        case FieldType.Single:
        case FieldType.SmallInteger:
          return true;
        default:
          return false;
      }
    }
  }
}
