using PetaPoco.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaTypPocoQueries.PetaPoco.Tests
{
    public class AngleDatabaseProvider : DatabaseProvider
    {
        public override DbProviderFactory GetFactory() => null;
        public override string EscapeSqlIdentifier(string sqlIdentifier) => $"<{sqlIdentifier}>";
    }
}
