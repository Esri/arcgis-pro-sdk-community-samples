using ArcGIS.Core.CIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateReport
{
  class ReportFieldComparer : IEqualityComparer<CIMReportField>
  {
    bool IEqualityComparer<CIMReportField>.Equals(CIMReportField x, CIMReportField y)
    {
      return (x.Name == y.Name);
    }

    int IEqualityComparer<CIMReportField>.GetHashCode(CIMReportField obj)
    {
      return obj.Name.GetHashCode() ^ obj.Name.GetHashCode();
    }
  }
 
}
