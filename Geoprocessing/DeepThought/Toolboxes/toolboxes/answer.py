import arcpy
import sixbynine
import time

msg1 = arcpy.GetIDMessage("dt1")
msg2 = arcpy.GetIDMessage("dt2")
msg3 = arcpy.GetIDMessage("dt3")

arcpy.AddWarning(arcpy.GetIDMessage("dt1") + arcpy.GetIDMessage("dt2"))

arcpy.SetProgressor("step", "", 0, 100, 1)

prog_msg = arcpy.GetIDMessage("dt3") + arcpy.GetIDMessage("dt2")

for i in range(1, 10):
    arcpy.SetProgressorLabel(f"{i} {prog_msg}")
    time.sleep(1)

arcpy.SetParameter(0, sixbynine.compute())
