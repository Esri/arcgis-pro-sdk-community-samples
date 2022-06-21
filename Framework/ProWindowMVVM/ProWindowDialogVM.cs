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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProWindowMVVM
{
  public class ProWindowDialogVM : PropertyChangedBase
  {
    public ProWindowDialogVM()
    {
    }

    public string ConnectionString => Module1.ConnectionString;

    private string _userName;
    public string UserName
    {
      get { return _userName; }
      set
      {
        SetProperty(ref _userName, value, () => UserName);
      }
    }

    private string _password;
    public string Password
    {
      get { return _password; }
      set
      {
        SetProperty(ref _password, value, () => Password);
      }
    }

    #region Commands

    public ICommand CmdOk => new RelayCommand((proWindow) =>
      {
        Module1.UserName = UserName;
        Module1.Password = Password;
        // TODO: set dialog result and close the window
        (proWindow as ProWindow).DialogResult = true;
        (proWindow as ProWindow).Close();
      }, () => true);

    public ICommand CmdCancel => new RelayCommand((proWindow) =>
    {
      Module1.UserName = string.Empty;
      Module1.Password = string.Empty;
      // TODO: set dialog result and close the window
      (proWindow as ProWindow).DialogResult = false; 
      (proWindow as ProWindow).Close();
    }, () => true);

    #endregion
  }
}

