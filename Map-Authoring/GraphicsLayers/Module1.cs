/*

   Copyright 2020 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Layouts.Events;

namespace GraphicsLayers
{
  /// <summary>
  /// This sample demonstrates working with graphics layers and graphic elements in a map.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro. 
  /// 1. Open any project that contains a Map.
  /// 1. You will need a Graphics Layer in the map to work with this sample.  If the map doesn't have a Graphics Layer, click the Map tab and use the Add Graphics layer button to insert a new layer in the map.
  /// 1. Notice the Graphics Example tab that appears on the Pro ribbon when a Graphics Layer exists in the map.
  /// ![UI](screenshots/GraphicsExample.png)
  /// 1. This tab has various controls that demonstrates common Graphic Element functionalities such as:
  ///      * Create Graphics layer
  ///      * Create Graphic elements (Text, point, line, polygon and image)
  ///      * Clipboard
  ///      * Modify graphic element symbology
  ///      * Select graphic elements
  ///      * Group, Order and Align graphic elements.
  ///      * Move Graphic elements
  /// 1. **Create Graphics Layer:** To create a new graphics layer, click the New graphics layer button. A new graphics layer will appear on the TOC.
  /// 1. **Create Graphic elements:** The Create Graphic elements gallery has a collection of tools to add Text, 
  /// Shape and Image elements to your map. In order to add an element, select a graphic layer in the map TOC. 
  /// The graphic element gets added to the selected layer in the Map TOC.
  /// 1. **Clipboard:** The **Copy** button copies the selected elements to the clipboard.  The **Paste** button pastes the elements in the clipboard to the selected Graphics Layer in the TOC. The **Paste into Group** button allows you to paste the selected graphics into a Group element.
  /// 1. **Select graphic elements:** The Select tool palette provides you with tools to select graphic elements. 
  /// You can select using a rectangle and a lasso. There is also a tool that allows you select only text graphic elements that lie within a selection rectangle. Buttons to Select all graphics, Clear selection and Delete Graphics are also provided.
  /// 1. **Modify graphic element symbology:** You can change the selected Text element's font, size, etc using the text symbol properties controls. Similarly, you can change the selected point, line or polygon graphic elements symbology.
  /// 1. **Group, Order and Align graphic elements:** When you select multiple symbols from the same graphics layer, you can group them using the Group button. Grouped graphics can be ungrouped using the Un-group button. The "Select graphics to group" tool allows you to select graphics using a rectangle and then groups them. 
  /// You can change the Z-Order of graphics using the Bring to Front and Send Back buttons. 
  /// There are two alignment tools that allows you to select graphics to align them to the left or to the top.
  /// 1. **Move Graphics:** Right click the selected Graphic element. In the context menu, click the Move Graphic option. This will move the anchor point of the selected graphic.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;    
    public GraphicsLayer SelectedGraphicsLayerTOC;
    public Dictionary<GraphicsLayer, List<Element>> GraphicsLayerSelectedElements = new Dictionary<GraphicsLayer, List<Element>>();
    public Dictionary<GraphicsLayer, List<Element>> GLWithElements = new Dictionary<GraphicsLayer, List<Element>>();
    public Dictionary<GraphicsLayer, List<Element>> ClipboardGraphicsLyrSelelectedElements;
    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("GraphicsLayerExamples_Module"));
      }
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
    protected override bool Initialize()
    {
      ArcGIS.Desktop.Layouts.Events.ElementEvent.Subscribe(OnElementSelectionChanged);
      ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(OnTOCSelectionChanged);
      CommandFilterOrder cmdFilter = new CommandFilterOrder(); //command filter is on.
      return base.Initialize();
    }

    //private void OnElementSelectionChanged(ElementEventArgs obj)
    //{
    //  throw new NotImplementedException();
    //}

    /// <summary>
    /// Even handler for TOCSelection changed event
    /// </summary>
    /// <param name="obj"></param>
    /// <remarks>
    /// The context for Graphics element creation is set to the currently selected Graphics Layer.
    /// </remarks>
    private void OnTOCSelectionChanged(MapViewEventArgs obj)
    {
      var selectedGraphicLayers = obj.MapView?.GetSelectedLayers().OfType<GraphicsLayer>();
      if (selectedGraphicLayers == null) return;
      if (selectedGraphicLayers.Count() == 0){ //nothing selected. So clear the selected graphic layer.
        SelectedGraphicsLayerTOC = null;
        return; 
      }
      SelectedGraphicsLayerTOC = selectedGraphicLayers.FirstOrDefault();
    }
    /// <summary>
    /// Event handler for graphics element changed event
    /// </summary>
    /// <param name="obj"></param>
    /// <remarks>
    /// When graphics element selection changes, a dictionary is updated with the Graphics Layer as the key and the elements in the layer that are currently selected as the values.
    /// </remarks>
    private async void OnElementSelectionChanged(ElementEventArgs obj)
    {
      if (obj.Hint != ElementEventHint.SelectionChanged) return;
      System.Diagnostics.Debug.WriteLine($"Debug Message: OnElementSelectionChanged");
      if (obj.Container == null)
      {
        //skip – this is viewer initialization
        System.Diagnostics.Debug.WriteLine($"This is viewer initialization");
        return;
      }
      if (obj.Container != null)
      {
        System.Diagnostics.Debug.WriteLine($"ElementContainer is not null");
        var gl = obj.Container as GraphicsLayer;
        if (gl is GraphicsLayer)
        {
          System.Diagnostics.Debug.WriteLine($"gl is GraphicsLayer: {gl.GetType().Name}");
          System.Diagnostics.Debug.WriteLine($"Selected Elements count: {obj.Elements.Count()}");
          if (GraphicsLayerSelectedElements.ContainsKey(gl)) //gl exists in the dictionary so only update value
          {
            GraphicsLayerSelectedElements[gl] = gl.GetSelectedElements().ToList();
            System.Diagnostics.Debug.WriteLine($"Elements selected in {gl}. Collection already contained {gl}");
          }
          else //doesn't exist, add it.
          {
            GraphicsLayerSelectedElements.Add(gl, gl.GetSelectedElements().ToList());
            System.Diagnostics.Debug.WriteLine($"Elements selected in {gl}. Collection did NOT contain {gl}");
          }

          //Weed out the layers with 0 selected items and maintain list
          GLWithElements.Clear();
          foreach (var kvp in GraphicsLayerSelectedElements)
          {
            if (kvp.Value.Count > 0) //Layers with some selected items.
            {
              System.Diagnostics.Debug.WriteLine($"Weed out layers with no elements. Layer: {kvp.Key}. Elements {kvp.Value}");
              GLWithElements.Add(kvp.Key, kvp.Value);
            }
          }
          //Analyze the GraphicsLayerSelectedElements dictionary to determine state
          var canMoveBackward = await CanSendToBack();
          var canBringForward = await CanBringForward();
          //set state for ordering 
          SetState("can_send_backward_state", canMoveBackward);
          SetState("can_bring_forward_state", canBringForward);
          //set state for Grouping/Ungrouping
          SetState("can_group_graphics_state", CanGroupElements());
          SetState("can_ungroup_graphics_state", CanUnGroupElements());
        }
      }
    }
    internal bool CanGroupElements()
    {
      if (GLWithElements.Count == 0)
        return false;

      // Cannot have multiple layers in the dictionary
      // Meaning, all the elements that need to "be grouped" have to be on the same layer (Same parent).
      if (GLWithElements.Count > 1)
        return false;
      //Only one graphics layer has elements selected
      //Count elements selected. Need more than 1 to group.
      var selectedElements = GLWithElements.FirstOrDefault().Value;
      return selectedElements.Count > 1 ? true : false;
    }

    

    internal bool CanUnGroupElements()
    {
      if (GLWithElements.Count == 0)
        return false;

      // Cannot have multiple layers in the dictionary
      // Meaning, all the elements that need to "be grouped" have to be on the same layer (Same parent).
      if (GLWithElements.Count > 1)
        return false;
      //Only one graphics layer has elements selected
      //Count elements selected. Need more atleast 1 to ungroup.
      var selectedElements = GLWithElements.FirstOrDefault().Value;
      if (selectedElements?.Any() == false)//must be at least 1.
        return false;
      //selected elements need to be grouped
      return selectedElements.Count() == selectedElements.OfType<GroupElement>().Count(); 
    }
    /// <summary>
    /// Checks if the selected elements can be send back (ZOrder)
    /// </summary>
    /// <returns></returns>
    internal async Task<bool> CanSendToBack()
    {
      if (GLWithElements.Count == 0)
        return false;

      // Cannot have multiple layers in the dictionary
      // Meaning, all the elements that need to "go back" have to be on the same layer.
      if (GLWithElements.Count > 1)
        return false;

      //Only one item in dictionary now
      bool canMoveBackward = true;
      //Get Graphics Layer and its selected elements      
      var gl = GLWithElements.FirstOrDefault().Key;
      var selectedElements = GLWithElements.FirstOrDefault().Value;

      //check every element in that Graphic Layer
      //If it can move backwards.
      await QueuedTask.Run(() =>
      {
        canMoveBackward = gl.CanSendBackward(selectedElements) ? true : false;
      });
      return canMoveBackward;
    }
    /// <summary>
    /// Checks if the selected elements can be brought to the front (ZOrder)
    /// </summary>
    /// <returns></returns>
    internal async Task<bool> CanBringForward()
    {
      if (GLWithElements.Count == 0)
        return false;

      // Cannot have multiple layers in the dictionary
      // Meaning, all the elements that need to "bring forward" have to be on the same layer.
      if (GLWithElements.Count > 1)
        return false;

      //Only one item in dictionary now
      //Process the item
      bool canBringForward = true;
      //Get Graphics Layer and its selected elements      
      var gl = GLWithElements.FirstOrDefault().Key;
      var selectedElements = GLWithElements.FirstOrDefault().Value;

      //check every element in that Graphic Layer
      //If it can be brought forward.
      await QueuedTask.Run(() =>
      {
        canBringForward = gl.CanBringForward(selectedElements) ? true : false;
      });
      return canBringForward;
    }
    
    /// <summary>
    /// Set the "state" to true or false.
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="stateCondition"></param>
    internal static void SetState(string stateName, bool stateCondition)
    {
      //if (FrameworkApplication.State.Contains(stateName) == active) return;
      if (stateCondition)
      {
        //activates the state
        FrameworkApplication.State.Activate(stateName);
      }
      else
      {
        //deactivates the state
        FrameworkApplication.State.Deactivate(stateName);
      }
    }
    #endregion Overrides
  }
}
