using PetaPoco.Core;
using System;
using Xunit;
using PetaPoco.Providers;
using PetaPoco;
using System.Data;
using SQLDatabase.Net.SQLDatabaseClient;
using StaTypPocoQueries.PetaPoco;
using FluentAssertions;
using System.Data.Common;

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

    public class QueryTests: IDisposable
    {
        private class MyClass
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        private class SqlDatabaseProvider : DatabaseProvider
        {
            public override DbProviderFactory GetFactory() => null;
        }

        private IDbConnection _conn;

        public QueryTests()
        {
            DatabaseProvider.RegisterCustomProvider<SqlDatabaseProvider>("SQLD");

            _conn = new SqlDatabaseConnection("SchemaName=PetaPoco;uri=@memory");
            _conn.Open();

            // Create an empty table, since we don't care about the results
            var cmd = _conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "CREATE TABLE MYCLASS (ID INTEGER INTEGER NOT NULL, NAME TEXT)";
            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _conn.Dispose();
        }

        [Fact]
        public void Query()
        {
            using (var db = new Database(_conn))
            {
                db.Fetch<MyClass>(c => c.ID == 4);

                db.LastSQL.Should().Be("SELECT [MyClass].[ID], [MyClass].[Name] FROM [MyClass] where [ID] = @0");
                db.LastArgs.Should().BeEquivalentTo(4);             
            }
        }

        [Fact]
        public void Delete()
        {
            using (var db = new Database(_conn))
            {
                db.Delete<MyClass>(c => c.Name == "Bob");

                db.LastSQL.Should().Be("DELETE FROM [MyClass]\nwhere [Name] = @0");
                db.LastArgs.Should().BeEquivalentTo(new[] { "Bob" });
            }
        }
    }
}
