using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AOI_VTWIN_RNS.Aoicollector.IAServer
{
    class DeclareService : IAServer
    {
        public IEnumerable<XElement> Declare(string barcode)
        {
            string path = url + barcode + "/declare";
            result = Consume(path);

            return result;
        }

        public bool IsDeclared()
        {
            bool pendiente = false;
            bool declarado = false;

            string isPendiente = ReadTag(result.Elements("pendiente"));
            string isDeclarado = ReadTag(result.Elements("declarado"));

            if (!isPendiente.Equals(""))
            {
                pendiente = Convert.ToBoolean(Convert.ToInt32(isPendiente));
            }

            if (!isDeclarado.Equals(""))
            {
                declarado = Convert.ToBoolean(Convert.ToInt32(isDeclarado));
            }

            if (pendiente)
            {
                return true;
            }
            else {
                if (declarado)
                {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
    }
}
