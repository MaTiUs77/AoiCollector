using AOI_VTWIN_RNS.Src.Config;
using IAServerServiceDll;
using IAServerServiceDll.Mapper;
using System;

namespace AOI_VTWIN_RNS.Aoicollector.IAServer
{
    public class PanelService 
    {
        public PanelMapper result;
        public Exception exception;

        public PanelMapper GetInspectionInfo(string barcode)
        {
            try
            {
                IAServerService ias = new IAServerService();
                result = ias.GetInspectionInfo(barcode);
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
