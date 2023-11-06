import arcpy
import arcpy.analysis
import sys

try:
  #print (arcpy.GetSystemEnvironment("PATH"))
  #environments = arcpy.ListEnvironments()

  # Sort the environment names
  #environments.sort()

  #for environment in environments:
      # Format and print each environment and its current setting.
      # (The environments are accessed by key from arcpy.env.)
  #    print("{0:<30}: {1}".format(environment, arcpy.env[environment]))
  print(f"Input: {arcpy.GetParameterAsText(0)}")
  print(f"Output: {arcpy.GetParameterAsText(1)}")
  arcpy.analysis.Buffer(
      in_features=arcpy.GetParameterAsText(0),
      out_feature_class=arcpy.GetParameterAsText(1),
      buffer_distance_or_field="100 Meters",
      line_side="FULL",
      line_end_type="ROUND",
      dissolve_option="NONE",
      dissolve_field=None,
      method="PLANAR"
  )
  arcpy.AddMessage ("Hello - this message is from a TEST Python script")
    
except Exception as ex:
    arcpy.AddError(f"Exception occurred: {ex}")

print ("Done")