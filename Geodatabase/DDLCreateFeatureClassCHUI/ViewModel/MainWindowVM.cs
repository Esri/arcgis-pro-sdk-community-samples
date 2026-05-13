/*

   Copyright 2026 Esri

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
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDLCreateFeatureClassCHUI.ViewModel
{
  public class MainWindowVM : PropertyChangedBase
  {
    public WizardViewModel Wizard { get; } = new();

    public ObservableCollection<object> Steps { get; }
    public int StepCount => Steps.Count;

    private int _currentStepIndex;
    public int CurrentStepIndex
    {
      get => _currentStepIndex;
      set
      {
        SetProperty(ref _currentStepIndex, value);

        CurrentStepView = Steps[CurrentStepIndex];
        CurrentStepIsValid = (Steps[CurrentStepIndex] as UserControl)?.DataContext is StepViewModelBase vm && vm.IsValid;
        ComputeStatus();
      }
    }

    private void ComputeStatus()
    {
      CanGoBack = CurrentStepIndex > 0;
      CanGoNext = CurrentStepIndex < Steps.Count - 1 && CurrentStepIsValid;
      ShowFinish = CurrentStepIndex == Steps.Count - 1;
      CanFinish = CurrentStepIndex == Steps.Count - 1 && CurrentStepIsValid;
    }

    private bool _showFinish = false;
    public bool ShowFinish
    {
      get => _showFinish;
      set => SetProperty(ref _showFinish, value);
    }

    private object _currentStepView;
    public object CurrentStepView
    {
        get => _currentStepView ?? Steps[CurrentStepIndex];
        set => SetProperty(ref _currentStepView, value);
    }

    private bool _canGoBack = false;
    public bool CanGoBack
    {  get => _canGoBack;
       set => SetProperty(ref _canGoBack, value);
    }

    private bool _canGoNext = false;
    public bool CanGoNext
    {
      get => _canGoNext;
      set => SetProperty(ref _canGoNext, value);
    }

    private bool _canFinish = false;
    public bool CanFinish
    {
      get => _canFinish;
      set => SetProperty(ref _canFinish, value);
    }

    private bool _currentStepIsValid = false;
    public bool CurrentStepIsValid
    {
      get => _currentStepIsValid;
      set
      {
        SetProperty(ref _currentStepIsValid, value);
        ComputeStatus();
      }
    }

    public ICommand NextCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand FinishCommand { get; }
    public ICommand CancelCommand { get; }

    public static MainWindowVM currentMainWindow { get; private set; }

    public MainWindowVM()
    {
      currentMainWindow = this;

      Steps = new ObservableCollection<object>
            {
                new WizardSteps.Step0Control { DataContext = Wizard.Step0 },
                new WizardSteps.Step1Control { DataContext = Wizard.Step1 },
                new WizardSteps.Step2Control { DataContext = Wizard.Step2 }
                // Add more steps as needed
            };

      NextCommand = new RelayCommand(_ => CurrentStepIndex++, _ => CanGoNext);
      BackCommand = new RelayCommand(_ => CurrentStepIndex--, _ => CanGoBack);
      FinishCommand = new RelayCommand(_ => OnFinish(), _ => CanFinish);
      CancelCommand = new RelayCommand(_ => OnCancel());
    }

    private void OnCancel()
    {
      // Implement cancel logic here
    }

    private void OnFinish()
    {
      CreateFeatureClass();
    }

    private void CreateFeatureClass()
    {
      // Gather all the information and create the feature class

      var hasZ = Wizard.Step1.ZValues;
      var hasM = Wizard.Step1.MValues;

      // Create a ShapeDescription object
      var shapeDescription = new ShapeDescription(Enum.Parse<GeometryType>(Wizard.Step1.FeatureClassType), SpatialReferences.WebMercator)
      {
        HasM = hasM,
        HasZ = hasZ
      };
      var objectIDFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("ObjectID", FieldType.OID);

      // Step 0 has the information to connect to the datastore
      using (var geoDb = BuildDatastore(Wizard.Step0))
      {
        var fcName = String.Empty;

        if (!String.IsNullOrWhiteSpace(Wizard.Step1.Name))
          fcName = Wizard.Step1.Name;
        try
        {
          // Assemble a list of all of our field descriptions
          var fieldDescriptions = new List<ArcGIS.Core.Data.DDL.FieldDescription>() {
                    objectIDFieldDescription
                };
          foreach (var item in Wizard.Step2.Fields)
          {
            fieldDescriptions.Add(new ArcGIS.Core.Data.DDL.FieldDescription(item.FieldName, item.DataType switch
            {
              "String" => FieldType.String,
              "Integer" => FieldType.Integer,
              "Double" => FieldType.Double,
              "Date" => FieldType.Date,
              _ => throw new InvalidOperationException($"Unsupported field type: {item.DataType}")
            }));
          }
          // Create a FeatureClassDescription object to describe the feature class
          // that we want to create
          var fcDescription =
            new FeatureClassDescription(fcName, fieldDescriptions, shapeDescription);

          if (!string.IsNullOrEmpty(Wizard.Step1.Alias))
            fcDescription.AliasName = Wizard.Step1.Alias;

          // Create a SchemaBuilder object
          SchemaBuilder schemaBuilder = new SchemaBuilder(geoDb);

          // Add the creation of the new feature class to our list of DDL tasks
          schemaBuilder.Create(fcDescription);

          // Execute the DDL
          bool success = schemaBuilder.Build();
          if (success)
            MessageBox.Show($@"Feature Class '{fcName}' created successfully in datastore '{Wizard.Step0.DatastoreCategory.Name}'.");
          else
            MessageBox.Show($@"Feature Class '{fcName}' creation failed in datastore '{Wizard.Step0.DatastoreCategory.Name}'. Check the log for details.");
        }
        catch (Exception ex)
        {
          MessageBox.Show($@"Exception: {ex}");
        }
      }
    }

    private Geodatabase BuildDatastore(Step0ViewModel step0)
    {
      return step0.DatastoreCategory.OpenDatastore(new Uri(step0.DataPath));
    }

  }
}
