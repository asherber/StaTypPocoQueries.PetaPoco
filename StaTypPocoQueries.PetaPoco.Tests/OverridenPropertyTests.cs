using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StaTypPocoQueries.PetaPoco.Tests
{
    [Collection("TestsWithMappers")]
    public class OverridenPropertyTests
    {
        public class Parent
        {
            public virtual int ID { get; set; }
        }

        public class Child : Parent
        {
            [Column("Foo")]
            public override int ID { get; set; }
        }

        protected Mock<IDatabase> _mockDb;
        protected Sql _lastSql;

        public OverridenPropertyTests()
        {
            _mockDb = new Mock<IDatabase>();
            _mockDb.Setup(m => m.Query<Child>(It.IsAny<Sql>()))
                .Returns(new List<Child>())
                .Callback<Sql>(s => _lastSql = s);
            _mockDb.Setup(m => m.Provider).Returns(new AngleDatabaseProvider());
            _mockDb.Setup(m => m.DefaultMapper).Returns(new ConventionMapper());

            Mappers.RevokeAll();
        }

        [Theory, AutoData]
        public void Query_Should_Use_Correct_Column_Attribute(int value)
        {
            _mockDb.Object.Query<Child>(c => c.ID == value);
            _lastSql.Should().BeEquivalentTo(new Sql("WHERE <Foo> = @0", value));
        }
    }
}
