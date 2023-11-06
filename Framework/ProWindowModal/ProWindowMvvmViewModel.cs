/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProWindowModal
{
  public class ProWindowMvvmViewModel : PropertyChangedBase
  {
    ProWindowMvvm _view = null;

    public ProWindowMvvmViewModel(ProWindowMvvm view)
    {
      _view = view;
    }

    private string _TheText = "Mvvm Pro Window";
    public string TheText
    {
      get { return _TheText; }
      set
      {
        SetProperty(ref _TheText, value);
      }
    }

    public ICommand CmdOk => new RelayCommand(() =>
    {
      MessageBox.Show($@"Text: {TheText} - Ok was clicked => closing ProWindow");
      _view.DialogResult = true;
      _view.Close();
    });

    public ICommand CmdCancel => new RelayCommand(() =>
    {
      MessageBox.Show($@"Cancel was clicked => closing ProWindow");
      _view.DialogResult = false;
      _view.Close();
    });
  }
}
