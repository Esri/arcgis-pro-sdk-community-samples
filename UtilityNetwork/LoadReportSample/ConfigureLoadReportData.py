# ConfigureLoadReportData.py
#
# This script should be run against a database created by the ArcGIS for Electric electric distribution asset package
# It configures the database and utility network to have the necessary schema to run the Create Load Report SDK sample
#
# 1. Before running this script, make sure to shut off all services that use this database.
# 2. Run this script when connected via client-server

import arcpy

# Parameters

utilityNetwork = arcpy.GetParameterAsText(0)
electricDistributionDeviceFeatureClass = arcpy.GetParameterAsText(1)

# Constants

DomainNetwork = "ElectricDistribution"
ServicePointCategoryName = "ServicePoint"
LoadField = "SERVICE_LOAD"
LoadAttribute = "Customer Load"

# Disable the Utility Network topology
arcpy.AddMessage("Disabling Utility Network Topology")
arcpy.un.DisableNetworkTopology(utilityNetwork)

# Create a Network Attribute to represent Load
arcpy.AddMessage("Creating Customer Load Network Attribute")
arcpy.un.AddNetworkAttribute(utilityNetwork, LoadAttribute, "Long", False, None, False)

# Assign the network attribute to the SERVICE_LOAD field in the ElectricDistributionDevice table
arcpy.AddMessage("Assigning Customer Load Network Attribute to Field")
arcpy.un.SetNetworkAttribute(utilityNetwork, LoadAttribute, DomainNetwork, electricDistributionDeviceFeatureClass, LoadField )

# Add a ServicePoint category
arcpy.AddMessage("Adding ServicePoint Category")
arcpy.un.AddNetworkCategory(utilityNetwork, ServicePointCategoryName)

# Assign the ServicePoint category to the asset types in the ServicePoint asset group
arcpy.AddMessage("Assigning ServicePoint Category")
arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Primary Meter", ServicePointCategoryName)
arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Single Phase Low Voltage Meter", ServicePointCategoryName)
arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Three Phase Low Voltage Meter", ServicePointCategoryName)


# Re-enable the utility network topology
arcpy.AddMessage("Enabling Utility Network Topology")
arcpy.un.EnableNetworkTopology(utilityNetwork)