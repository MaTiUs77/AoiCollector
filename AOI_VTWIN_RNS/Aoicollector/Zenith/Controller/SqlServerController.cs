using System;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using System.Data;

namespace AOI_VTWIN_RNS.Aoicollector.Zenith.Controller
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
