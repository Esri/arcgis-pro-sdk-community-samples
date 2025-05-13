using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace DDLCreateDeleteDimensionFeatureClass
{
  /// <summary>
  /// This sample illustrates how to use the DDL APIs to create and delete dimension feature class.  
  /// </summary>
  /// <remarks>
  /// 1. Open this solution in Visual Studio.
  /// 2. Click the build menu and select Build Solution.  
  /// 3. Click the Start button to open ArCGIS Pro. ArcGIS Pro will open.      
  /// 4. Open any project.  
  /// 5. Click on the Add-in tab and verify that a "Operations" group was added.  
  /// 6. Notice the buttons in the "Dimension Operations" group.  
  /// 7. Tap the "Create Dimension FeatureClass" button.  
  /// ![UI](Screenshots/Screen0.png)  
  /// 8. Add the new Database into the Catalog pane.  
  /// ![UI](Screenshots/Screen1.png)
  /// 9. Notice the structure of the data.
  /// 10. Tap the "Delete Dimension FeatureClass" button  
  /// ![UI](Screenshots/Screen2.png)  
  /// </remarks>
  internal class Module1 : Module
  {
    public static string geodatabasePath = @"C:\temp\mySampleGeoDatabase.gdb";
    public static string dimensionFeautreClassName = "LinesInfo";
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DDLCreateDeleteDimensionFeatureClass_Module");

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

    #endregion Overrides

  }
}
