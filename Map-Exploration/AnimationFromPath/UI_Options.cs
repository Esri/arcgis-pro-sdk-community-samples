//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace AnimationFromPath
{
  class UI_Options
  {
  }

  internal class CreateAnimationFromPathButton : Button
  {
    protected async override void OnClick()
    {
      await CreateAnimationFromPath.CreateKeyframes();
    }
  }

  internal class CustomPitchEditBox : EditBox
  {
    CustomPitchEditBox()
    {
      Text = "-90";
    }
    protected override void OnTextChange(string text)
    {
      base.OnTextChange(text);

      //set default value if text box is empty
      if (Text == null) Text = "-90";

      //double pitch = System.Convert.ToDouble(Text);

      CreateAnimationFromPath.CustomPitch = System.Convert.ToDouble(Text);
    }

  }

  internal class CameraZOffsetEditBox : EditBox
  {
    CameraZOffsetEditBox()
    {
      Text = "0";
    }
    protected override void OnTextChange(string text)
    {
      base.OnTextChange(text);

      //set default value if text box is empty
      if (Text == null) Text = "0";

      CreateAnimationFromPath.CameraZOffset = System.Convert.ToDouble(Text);
    }

  }

  internal class TotalDurationEditBox : EditBox
  {
    TotalDurationEditBox()
    {
      Text = "30";
    }
    protected override void OnTextChange(string text)
    {
      base.OnTextChange(text);

      //set default value if text box is empty
      if (Text == null) Text = "30";

      CreateAnimationFromPath.TotalDuration = System.Convert.ToDouble(Text);
    }
  }

  internal class KeyEveryNSecondEditBox : EditBox
  {
    KeyEveryNSecondEditBox()
    {
      Text = "1";
    }
    protected override void OnTextChange(string text)
    {
      base.OnTextChange(text);

      //set default value if text box is empty
      if (Text == null) Text = "1";

      CreateAnimationFromPath.KeyEveryNSecond = System.Convert.ToDouble(Text);
    }
  }

  /// <summary>
  /// Represents the method ComboBox
  /// </summary>
  internal class ChooseMethodComboBox : ComboBox
  {

    private bool _isInitialized;

    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public ChooseMethodComboBox()
    {
      UpdateCombo();
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>

    private void UpdateCombo()
    {
      // TODO – customize this method to populate the combobox with your desired items  
      if (_isInitialized)
        SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


      if (!_isInitialized)
      {
        Clear();

        Add(new ComboBoxItem("Keyframes along path"));
        Add(new ComboBoxItem("Keyframes every N seconds"));
        Add(new ComboBoxItem("Keyframes only at vertices"));

        _isInitialized = true;
      }


      Enabled = true; //enables the ComboBox
      SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override void OnSelectionChange(ComboBoxItem item)
    {

      if (item == null)
        return;

      if (string.IsNullOrEmpty(item.Text))
        return;

      // TODO  Code behavior when selection changes.    
      CreateAnimationFromPath.SelectedMethod = item.Text;

      if (CreateAnimationFromPath.SelectedMethod == "Keyframes every N seconds")
      {
        FrameworkApplication.State.Activate("nSeconds_state");
      }
      else
      {
        if (FrameworkApplication.State.Contains("nSeconds_state"))
        {
          FrameworkApplication.State.Deactivate("nSeconds_state");
        }
      }
    }
  }

  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class ChooseViewComboBox : ComboBox
  {

    private bool _isInitialized;

    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public ChooseViewComboBox()
    {
      UpdateCombo();
      var activeViewChanged = ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe(onActiveMapViewChanged(), false);
    }

    private Action<ActiveMapViewChangedEventArgs> onActiveMapViewChanged()
    {
      return delegate (ActiveMapViewChangedEventArgs args)
      {
        _isInitialized = false;

        //reset target point to null when view is changed
        CreateAnimationFromPath.TargetPoint = null;

        UpdateCombo();
      };
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>

    private void UpdateCombo()
    {
      // TODO – customize this method to populate the combobox with your desired items  
      if (_isInitialized)
        SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


      if (!_isInitialized)
      {
        Clear();

        if (MapView.Active == null)
          return;

        if (MapView.Active.ViewingMode == ArcGIS.Core.CIM.MapViewingMode.SceneGlobal ||
                              MapView.Active.ViewingMode == ArcGIS.Core.CIM.MapViewingMode.SceneLocal)
        {
          Add(new ComboBoxItem("View along"));
          Add(new ComboBoxItem("Top down"));
          Add(new ComboBoxItem("Top down - face north"));
          Add(new ComboBoxItem("Face target"));
          Add(new ComboBoxItem("Face backward"));
          Add(new ComboBoxItem("Custom pitch"));
        }
        else if (MapView.Active.ViewingMode == ArcGIS.Core.CIM.MapViewingMode.Map)
        {
          Add(new ComboBoxItem("Top down - face north"));
          Add(new ComboBoxItem("Top down"));
        }

        _isInitialized = true;
      }

      Enabled = true; //enables the ComboBox
      SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override void OnSelectionChange(ComboBoxItem item)
    {

      if (item == null)
        return;

      if (string.IsNullOrEmpty(item.Text))
        return;

      // TODO  Code behavior when selection changes.    
      CreateAnimationFromPath.SelectedCameraView = item.Text;

      if (item.Text != "Face target")
      {
        CreateAnimationFromPath.TargetPoint = null;
      }

      if (CreateAnimationFromPath.SelectedCameraView == "Custom pitch")
      {
        FrameworkApplication.State.Activate("customPitch_state");
      }
      else
      {
        if (FrameworkApplication.State.Contains("customPitch_state"))
        {
          FrameworkApplication.State.Deactivate("customPitch_state");
        }
      }
    }

  }
}
