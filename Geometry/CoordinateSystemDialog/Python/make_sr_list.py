#!/usr/bin/python
# -*- coding: utf-8 -*-
#
# Run this python script to regenerate the list of spatial references
#
import sys
import arcpy
import subprocess

out_file = open("C:\Temp\sr_out.csv",'w')

srs = arcpy.ListSpatialReferences()

for sr_string in srs:
    sr = arcpy.SpatialReference(sr_string)
    sr_wkt = sr.exportToString().split(';')[0]
    srLine = "%s|%s|%i|%s" % (sr_string, sr.name, sr.factoryCode, sr_wkt)
    #print srLine
    out_file.write(srLine)
    
out_file.close()


