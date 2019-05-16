using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Data.Common;
using PetaPoco.Core;
using PetaPoco;

namespace StaTypPocoQueries.PetaPoco.Tests
{
    public class QuoterTests
    {
        [Fact]
        public void Quoter_Should_UseCorrectChar()
        {            
            DatabaseProvider.RegisterCustomProvider<AngleDatabaseProvider>("Angle");
            
            using (var db = new Database("asdf", "AngleDatabaseProvider"))
            {
                var quoter = new DatabaseQuoter(db);
                var output = quoter.QuoteColumn("Foo");
                output.Should().Be("<Foo>");
            }
        }
    }
}
