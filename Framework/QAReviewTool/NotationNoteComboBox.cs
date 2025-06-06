/*

   Copyright 2019 Esri

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace QAReviewTool
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class NotationNoteComboBox : ComboBox
  {
    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public NotationNoteComboBox()
    {
      Module1.Current.NotationNoteComboBox = this;
      UpdateCombo();
    }

    internal void AddItem(object comboitem)
    {
      Add(comboitem);
    }

    internal void ClearItems()
    {
      Clear();
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>

    private void UpdateCombo()
    {
      Clear();
      Enabled = true; //enables the ComboBox
    }

    protected override void OnEnter()
    {
      Module1.Current.AddNewEditNoteValue(this.Text);
    }

  }
}
