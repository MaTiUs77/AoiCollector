using System;
using CollectorPackage.Aoicollector.Inspection.Model;
using System.Data;

namespace CollectorPackage.Aoicollector.Zenith.Controller
{
    public class SqlServerController : SqlServerQuery
    {
        public AoiController aoi;

        public SqlServerController(AoiController _aoi) 
        {
            aoi = _aoi;
        }
    }
}
