using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CollectorPackage
{
    public class AutoRepository
    {
        public IEnumerable<Auto> GetAutos()
        {
            return new List<Auto>()
            {
                new Auto(){modelo = "Subaru", precio= 180000},
                new Auto(){modelo = "Tico", precio= 48000},
                new Auto(){modelo = "Astra", precio= 14000}
            };
        }
    }
}
