using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Quotations;
using PetaPoco;
using PetaPoco.Providers;
using StaTypPocoQueries.Core;

namespace StaTypPocoQueries.PetaPoco
{
    public static class DatabaseExtensions
    {
        private class Quoter : Translator.IQuoter
        {
            private readonly Database _db;

            public Quoter(Database db)
            {
                _db = db;
            }

            public string QuoteColumn(string columnName) => _db.Provider.EscapeSqlIdentifier(columnName);
        }

        private static Sql ToSql<T>(this Expression<Func<T, bool>> query, Database db)
        {
            var translated = ExpressionToSql.Translate(new Quoter(db), query);
            return new Sql(translated.Item1, translated.Item2);
        }

        private static Sql ToSql<T>(this FSharpExpr<FSharpFunc<T, bool>> query, Database db)
        {
            var translated = ExpressionToSql.Translate(new Quoter(db), query);
            return new Sql(translated.Item1, translated.Item2);
        }


        #region C#
        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List holding the results of the query</returns>
        public static List<T> Fetch<T>(this Database db, Expression<Func<T, bool>> query) 
            => db.Fetch<T>(query.ToSql(db));

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.  It will also execute a second query to retrieve the
        ///     total number of records in the result set.
        /// </remarks>
        public static Page<T> Page<T>(this Database db, long page, long itemsPerPage, Expression<Func<T, bool>> query)
            => db.Page<T>(page, itemsPerPage, query.ToSql(db));

        /// <summary>
        ///     Retrieves a page of records (without the total count)
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.
        /// </remarks>
        public static List<T> Fetch<T>(this Database db, long page, long itemsPerPage, Expression<Func<T, bool>> query)
            => db.Fetch<T>(page, itemsPerPage, query.ToSql(db));

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public static List<T> SkipTake<T>(this Database db, long skip, long take, Expression<Func<T, bool>> query)
            => db.SkipTake<T>(skip, take, query.ToSql(db));

        /// <summary>
        ///     Runs an SQL query, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>An enumerable collection of result records</returns>
        /// <remarks>
        ///     For some DB providers, care should be taken to not start a new Query before finishing with
        ///     and disposing the previous one. In cases where this is an issue, consider using Fetch which
        ///     returns the results as a List rather than an IEnumerable.
        /// </remarks>
        public static IEnumerable<T> Query<T>(this Database db, Expression<Func<T, bool>> query)
            => db.Query<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return a single row.
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one matching record
        /// </remarks>
        public static T Single<T>(this Database db, Expression<Func<T, bool>> query)
            => db.Single<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition, or default(T) if no matching rows</returns>
        public static T SingleOrDefault<T>(this Database db, Expression<Func<T, bool>> query)
            => db.SingleOrDefault<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return at least one row
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set</returns>
        public static T First<T>(this Database db, Expression<Func<T, bool>> query)
            => db.First<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        public static T FirstOrDefault<T>(this Database db, Expression<Func<T, bool>> query)
            => db.FirstOrDefault<T>(query.ToSql(db));

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class whose attributes specify the name of the table to delete from</typeparam>
        /// <param name="query">
        ///     An Expression identifying the rows to delete (ie:
        ///     everything after "DELETE FROM tablename"
        /// </param>
        /// <returns>The number of affected rows</returns>
        public static int Delete<T>(this Database db, Expression<Func<T, bool>> query)
            => db.Delete<T>(query.ToSql(db));
        #endregion

        #region F#
        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List holding the results of the query</returns>
        public static List<T> Fetch<T>(this Database db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.Fetch<T>(query.ToSql(db));

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.  It will also execute a second query to retrieve the
        ///     total number of records in the result set.
        /// </remarks>
        public static Page<T> Page<T>(this Database db, long page, long itemsPerPage, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.Page<T>(page, itemsPerPage, query.ToSql(db));

        /// <summary>
        ///     Retrieves a page of records (without the total count)
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified page.
        /// </remarks>
        public static List<T> Fetch<T>(this Database db, long page, long itemsPerPage, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.Fetch<T>(page, itemsPerPage, query.ToSql(db));

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public static List<T> SkipTake<T>(this Database db, long skip, long take, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SkipTake<T>(skip, take, query.ToSql(db));

        /// <summary>
        ///     Runs an SQL query, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>An enumerable collection of result records</returns>
        /// <remarks>
        ///     For some DB providers, care should be taken to not start a new Query before finishing with
        ///     and disposing the previous one. In cases where this is an issue, consider using Fetch which
        ///     returns the results as a List rather than an IEnumerable.
        /// </remarks>
        public static IEnumerable<T> Query<T>(this Database db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.Query<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return a single row.
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one matching record
        /// </remarks>
        public static T Single<T>(this Database db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.Single<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition, or default(T) if no matching rows</returns>
        public static T SingleOrDefault<T>(this Database db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SingleOrDefault<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return at least one row
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set</returns>
        public static T First<T>(this Database db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.First<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        public static T FirstOrDefault<T>(this Database db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FirstOrDefault<T>(query.ToSql(db));

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class whose attributes specify the name of the table to delete from</typeparam>
        /// <param name="query">
        ///     An Expression identifying the rows to delete (ie:
        ///     everything after "DELETE FROM tablename"
        /// </param>
        /// <returns>The number of affected rows</returns>
        public static int Delete<T>(this Database db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.Delete<T>(query.ToSql(db));
        #endregion
    }
}
