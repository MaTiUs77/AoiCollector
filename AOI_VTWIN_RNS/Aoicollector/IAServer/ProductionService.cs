
using CollectorPackage.Aoicollector.IAServer.Mapper;
using CollectorPackage.Src.Util.Convertion;
using Newtonsoft.Json;
using System;

namespace CollectorPackage.Aoicollector
{
    public class ProductionService : Api
    {
        public string routeMode;
        public string routeName;
        public bool routeDeclare = false;

        public ProdInfoMapper result;

        public ProdInfoMapper GetProdInfo(string aoibarcode)
        {
            result = new ProdInfoMapper();
            hasResponse = false;

            try
            {
                string path = string.Format("{0}/api/aoicollector/prodinfo/{1}", apiUrl, aoibarcode);
                
                string jsonData = Consume(path);
                hasResponse = true;
                result = JsonConvert.DeserializeObject<ProdInfoMapper>(jsonData);
                DefineRoute();
            }
            catch (Exception ex)
            {
                error = ex;
            }

            return result;
        }

        public bool isFinished()
        {
            if(result.produccion.smt.prod_aoi >= result.produccion.smt.qty)
            {
                return true;
            } else
            {
                return false;
            }
        }

        private void DefineRoute()
        {
            if (result.produccion.route != null)
            {
                routeMode = "iaserver";
                routeName = result.produccion.route.name;
                routeDeclare = Convertion.stringToBool(result.produccion.route.declare);
            }
            else
            {
                if (result.produccion.sfcs != null)
                {
                    routeMode = "sfcs";
                    routeName = result.produccion.sfcs.puesto;
                    routeDeclare = Convertion.stringToBool(result.produccion.sfcs.declara);
                }
            }
        }
    }
}
