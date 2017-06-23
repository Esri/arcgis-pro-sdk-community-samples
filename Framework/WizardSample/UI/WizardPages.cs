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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace WizardSample.UI
{

  /// <summary>
  /// A custom class of your own design if you need something to
  /// hold state that can be passed from page to page.
  /// </summary>
  internal class SampleWizardData : INotifyPropertyChanged
  {

    public event PropertyChangedEventHandler PropertyChanged = delegate { };
    public bool SomeImportantStatus { get; set; }
    public string SomeImportantString { get; set; }
    public int SomeImportantNumber { get; set; }
    public string Etc { get; set; }

   
    public void UpdateEverything(bool status, string importantString, int number, string etc)
    {
      SomeImportantStatus = status;
      SomeImportantString = importantString;
      SomeImportantNumber = number;
      Etc = etc;
      NotifyPropertyChanged("SomeImportantStatus");
      NotifyPropertyChanged("SomeImportantString");
      NotifyPropertyChanged("SomeImportantNumber");
      NotifyPropertyChanged("Etc");
    }

    /// <summary>
    /// When using a DataTemplate (as we are in our wizard page UI) the
    /// templated class must support property notification or the displayed
    /// content won't refresh.
    /// </summary>
    private void NotifyPropertyChanged([CallerMemberName] string name = "")
    {
      PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
  }

  /// <summary>
  /// You don't necessarily need your own base class, just make sure that each
  /// page of content in your wizard inherits from ArcGIS.Desktop.Framework.Contracts.WizardPage
  /// </summary>
  /// <remarks>Two points:<br/>
  /// 1. Set IsValid to true to enable "Next" if isPageListVisible="false" (in the Config.daml).<br/>
  /// 2. Set IsValid to true on ~all~ the pages to enable Finish.
  /// </remarks>
  internal class StepBase : WizardPage
  {
    private ICommand _changeDataCommand = null;
    private bool _theCurrentPage = false;
    protected bool _isValid = false;

    private void ChangeData()
    {
      //Just show an example of updating something on the
      //wizard page
      var data = this.WizardContent;
      data.UpdateEverything(!data.SomeImportantStatus,
                           $"last changed by {this.ID} {DateTime.Now.ToString("T")}",
                           data.SomeImportantNumber + 1,
                           data.Etc);
      //Each page must be valid in order for the finish button to show
      IsValid = true;
    }

    /// <summary>
    /// Gets the command that will update wizard page content
    /// </summary>
    public ICommand ChangeDataCommand
    {
      get
      {
        if (_changeDataCommand == null)
          _changeDataCommand = new RelayCommand(() => ChangeData(), () => true);
        return _changeDataCommand;
      }
    }
    /// <summary>
    /// Gets the wizard data available to all the pages
    /// </summary>
    public SampleWizardData WizardContent
    {
      get
      {
        return (SampleWizardData)base.Data;
      }
      set
      {
        base.Data = value;
        NotifyPropertyChanged();
      }
    }

    /// <summary>
    /// Gets whether the page is valid or not.
    /// </summary>
    /// <remarks>Must be set to true to enable the Next button if isPageListVisible="false". 
    /// Must set to true to enable the Finish button (regardless of isPageListVisible).</remarks>
    public override bool IsValid
    {
      get
      {
        return _isValid;
      }
      set
      {
        _isValid = value;
        NotifyPropertyChanged();
      }
    }
  
    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    protected override void NotifyPropertyChanged(string name = "")
    {
      //Detect when we become the current page (if that is
      //important for your pages to know...)
      if (name == "IsCurrentPage") {
        if (this.IsCurrentPage) {
          //I am being set as the current page
          _theCurrentPage = true;
          //etc
        }
        else if (_theCurrentPage && !this.IsCurrentPage) {
          //I am no longer the current page
          _theCurrentPage = false;
          //etc
        }
      }
      base.NotifyPropertyChanged(name);
    }
  }

  internal class Step1 : StepBase
  {
  }

  internal class Step2 : StepBase
  {
  }

  internal class Step3 : StepBase
  {
  }
}
