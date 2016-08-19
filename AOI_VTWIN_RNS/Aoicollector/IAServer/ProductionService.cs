
using AOI_VTWIN_RNS.Src.Config;
using IAServerServiceDll;
using IAServerServiceDll.Mapper;
using System;

namespace AOI_VTWIN_RNS.Aoicollector
{
    public class ProductionService
    {
        public ProduccionMapper result;
        public Exception exception;

        public ProduccionMapper GetProductionInfo(string aoibarcode)
        {
            try
            {
                IAServerService ias = new IAServerService();
                result = ias.GetProductionInfo(aoibarcode);
                if (ias.error != null)
                {
                    throw ias.error;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return result;

        }
    }
}
