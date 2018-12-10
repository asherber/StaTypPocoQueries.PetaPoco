using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Data.Common;
using PetaPoco.Core;
using SQLDatabase.Net.SQLDatabaseClient;
using PetaPoco;

namespace StaTypPocoQueries.PetaPoco.Tests
{
    public class QuoterTests
    {
        private class AngleDatabaseProvider : DatabaseProvider
        {
            public override DbProviderFactory GetFactory() => null;
            public override string EscapeSqlIdentifier(string sqlIdentifier) => $"<{sqlIdentifier}>";
        }

        [Fact]
        public void Quoter_Should_UseCorrectChar()
        {            
            DatabaseProvider.RegisterCustomProvider<AngleDatabaseProvider>("Angle");
            
            using (var db = new Database("asdf", "AngleDatabaseProvider"))
            {
                var quoter = new DatabaseExtensions.Quoter(db);
                var output = quoter.QuoteColumn("Foo");
                output.Should().Be("<Foo>");
            }
        }
    }
}
