using PetaPoco.Core;
using System;
using Xunit;
using PetaPoco.Providers;
using PetaPoco;
using System.Data;
using StaTypPocoQueries.PetaPoco;
using FluentAssertions;
using System.Data.Common;
using Moq;
using System.Collections.Generic;
using AutoFixture.Xunit2;

namespace StaTypPocoQueries.PetaPoco.Tests
{
    /**
     * The unit tests in StaTypPocoQueries.Core confirm that the translator works correctly.
     * The tests in PetaPoco confirm that PP executes the provided queries correctly.
     * So the goal here is just to confirm that the query gets constructed correctly.
     * 
     * Since all of the methods that select records ultimately call Query(), it's sufficient 
     * just to test that. Delete needs to be tested separately.
     */

    public class QueryTests
    {
        private class MyClass
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        private Mock<IDatabase> _mockDb;
        private Sql _lastSql;

        public QueryTests()
        {
            _mockDb = new Mock<IDatabase>();
            _mockDb.Setup(m => m.Query<MyClass>(It.IsAny<Sql>()))
                .Returns(new List<MyClass>())
                .Callback<Sql>(s => _lastSql = s);
            _mockDb.Setup(m => m.Delete<MyClass>(It.IsAny<Sql>()))
                .Callback<Sql>(s => _lastSql = s);
            _mockDb.Setup(m => m.Provider).Returns(new AngleDatabaseProvider());
        }

        [Theory, AutoData]
        public void Query(int id)
        {            
            _mockDb.Object.Query<MyClass>(c => c.ID == id);
            _lastSql.Should().BeEquivalentTo(new Sql("WHERE <ID> = @0", id));
        }

        [Theory, AutoData]
        public void Delete(string name)
        {
            _mockDb.Object.Delete<MyClass>(c => c.Name == name);
            _lastSql.Should().BeEquivalentTo(new Sql("WHERE <Name> = @0", name));
        }
    }
}
