using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaTypPocoQueries.PetaPoco.Tests
{
    public class UnderscoreMapper : ConventionMapper
    {
        public UnderscoreMapper()
        {
            InflectColumnName = (i, cn) => i.Underscore(cn);
            InflectTableName = (i, tn) => i.Underscore(tn);
        }
    }
}
