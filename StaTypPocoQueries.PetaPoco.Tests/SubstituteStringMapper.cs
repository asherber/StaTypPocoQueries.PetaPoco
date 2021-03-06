using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaTypPocoQueries.PetaPoco.Tests
{
    public class SubstituteStringMapper : ConventionMapper
    {
        public SubstituteStringMapper()
        {            
            ToDbConverter = pi =>
            {
                if (pi.PropertyType == typeof(string))
                    return o => "SUBSTITUTE STRING";
                else
                    return base.ToDbConverter(pi);
            };
        }
    }
}
