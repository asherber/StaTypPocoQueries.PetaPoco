/**
 * Copyright 2018-2020 Aaron Sherber

 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Quotations;
using StaTypPocoQueries.Core;
using StaTypPocoQueries.PetaPoco;

namespace PetaPoco
{
    public static partial class DatabaseExtensions
    {
        private static readonly FSharpFunc<MemberInfo, string> FsExtractColumnName 
            = ExpressionToSql.AsFsFunc<MemberInfo, string>(ExtractColumnName);

        // Helpful precis: https://codeblog.jonskeet.uk/2012/01/30/currying-vs-partial-function-application/
        private static readonly FSharpFunc<PropertyInfo, FSharpFunc<object, object>> FsInvokeValueConverter
            = ExpressionToSql.AsFsFunc<PropertyInfo, FSharpFunc<object, object>>(
                pi => ExpressionToSql.AsFsFunc<object, object>(
                    input => InvokeValueConverter(pi, input)
                )
            );

        private static Sql ToSql<T>(this FSharpExpr<FSharpFunc<T, bool>> query, IDatabase db)
        {
            var translated = ExpressionToSql.Translate(new DatabaseQuoter(db), query, 
                includeWhere: true, 
                customNameExtractor: FsExtractColumnName, 
                customParameterValueMap: FsInvokeValueConverter);
            return new Sql(translated.Item1, translated.Item2);
        }


        #region sync
        /// <summary>
        /// Checks for the existence of a row matching the specified condition
        /// </summary>
        /// <typeparam name="T">The Type representing the table being queried</typeparam>
        /// <param name="db"></param>
        /// <param name="query">An Expression describing the condition to be tested.</param>
        /// <returns></returns>
        public static bool Exists<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
        {
            var sql = query.ToSql(db);
            return db.Exists<T>(sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List holding the results of the query</returns>
        public static List<T> Fetch<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
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
        public static Page<T> Page<T>(this IDatabase db, long page, long itemsPerPage, FSharpExpr<FSharpFunc<T, bool>> query)
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
        public static List<T> Fetch<T>(this IDatabase db, long page, long itemsPerPage, FSharpExpr<FSharpFunc<T, bool>> query)
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
        public static List<T> SkipTake<T>(this IDatabase db, long skip, long take, FSharpExpr<FSharpFunc<T, bool>> query)
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
        public static IEnumerable<T> Query<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
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
        public static T Single<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.Single<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition, or default(T) if no matching rows</returns>
        public static T SingleOrDefault<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SingleOrDefault<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return at least one row
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set</returns>
        public static T First<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.First<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        public static T FirstOrDefault<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
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
        public static int Delete<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.Delete<T>(query.ToSql(db));
        #endregion
        #region async      
        /// <summary>
        /// Checks for the existence of a row matching the specified condition
        /// </summary>
        /// <typeparam name="T">The Type representing the table being queried</typeparam>
        /// <param name="db"></param>
        /// <param name="query">An Expression describing the condition to be tested.</param>
        /// <returns></returns>
        public static Task<bool> ExistsAsync<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
        {
            var sql = query.ToSql(db);
            return db.ExistsAsync<T>(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// Checks for the existence of a row matching the specified condition
        /// </summary>
        /// <typeparam name="T">The Type representing the table being queried</typeparam>
        /// <param name="db"></param>
        /// <param name="query">An Expression describing the condition to be tested.</param>
        /// <returns></returns>
        public static Task<bool> ExistsAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                FSharpExpr<FSharpFunc<T, bool>> query)
        {
            var sql = query.ToSql(db);
            return db.ExistsAsync<T>(cancellationToken, sql.SQL, sql.Arguments);
        }

        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List holding the results of the query</returns>
        public static Task<List<T>> FetchAsync<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FetchAsync<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>A List holding the results of the query</returns>
        public static Task<List<T>> FetchAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FetchAsync<T>(cancellationToken, query.ToSql(db));

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
        public static Task<Page<T>> PageAsync<T>(this IDatabase db, long page, long itemsPerPage, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.PageAsync<T>(page, itemsPerPage, query.ToSql(db));

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
        public static Task<Page<T>> PageAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                long page, long itemsPerPage, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.PageAsync<T>(cancellationToken, page, itemsPerPage, query.ToSql(db));

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
        public static Task<List<T>> FetchAsync<T>(this IDatabase db, long page, long itemsPerPage, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FetchAsync<T>(page, itemsPerPage, query.ToSql(db));

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
        public static Task<List<T>> FetchAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                long page, long itemsPerPage, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FetchAsync<T>(cancellationToken, page, itemsPerPage, query.ToSql(db));

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
        public static Task<List<T>> SkipTakeAsync<T>(this IDatabase db, long skip, long take, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SkipTakeAsync<T>(skip, take, query.ToSql(db));

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
        public static Task<List<T>> SkipTakeAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                long skip, long take, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SkipTakeAsync<T>(cancellationToken, skip, take, query.ToSql(db));

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
        public static Task QueryAsync<T>(this IDatabase db, Action<T> receivePocoCallback, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.QueryAsync<T>(receivePocoCallback, query.ToSql(db));

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
        public static Task QueryAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                Action<T> receivePocoCallback, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.QueryAsync<T>(receivePocoCallback, cancellationToken, query.ToSql(db));

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
        public static Task<IAsyncReader<T>> QueryAsync<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.QueryAsync<T>(query.ToSql(db));

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
        public static Task<IAsyncReader<T>> QueryAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                FSharpExpr<FSharpFunc<T, bool>> query)
            => db.QueryAsync<T>(cancellationToken, query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return a single row.
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one matching record
        /// </remarks>
        public static Task<T> SingleAsync<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SingleAsync<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return a single row.
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition</returns>
        /// <remarks>
        ///     Throws an exception if there are zero or more than one matching record
        /// </remarks>
        public static Task<T> SingleAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SingleAsync<T>(cancellationToken, query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition, or default(T) if no matching rows</returns>
        public static Task<T> SingleOrDefaultAsync<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SingleOrDefaultAsync<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return either a single row, or no rows
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the record to be retrieved."/> </param>
        /// <returns>The single record matching the specified condition, or default(T) if no matching rows</returns>
        public static Task<T> SingleOrDefaultAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                FSharpExpr<FSharpFunc<T, bool>> query)
            => db.SingleOrDefaultAsync<T>(cancellationToken, query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return at least one row
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set</returns>
        public static Task<T> FirstAsync<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FirstAsync<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query that should always return at least one row
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set</returns>
        public static Task<T> FirstAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FirstAsync<T>(cancellationToken, query.ToSql(db));

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        public static Task<T> FirstOrDefaultAsync<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FirstOrDefaultAsync<T>(query.ToSql(db));

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">An Expression describing the records to be retrieved."/> </param>
        /// <returns>The first record in the result set, or default(T) if no matching rows</returns>
        public static Task<T> FirstOrDefaultAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                FSharpExpr<FSharpFunc<T, bool>> query)
            => db.FirstOrDefaultAsync<T>(cancellationToken, query.ToSql(db));

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class whose attributes specify the name of the table to delete from</typeparam>
        /// <param name="query">
        ///     An Expression identifying the rows to delete (ie:
        ///     everything after "DELETE FROM tablename"
        /// </param>
        /// <returns>The number of affected rows</returns>
        public static Task<int> DeleteAsync<T>(this IDatabase db, FSharpExpr<FSharpFunc<T, bool>> query)
            => db.DeleteAsync<T>(query.ToSql(db));

        /// <summary>
        ///     Performs an SQL Delete
        /// </summary>
        /// <typeparam name="T">The POCO class whose attributes specify the name of the table to delete from</typeparam>
        /// <param name="query">
        ///     An Expression identifying the rows to delete (ie:
        ///     everything after "DELETE FROM tablename"
        /// </param>
        /// <returns>The number of affected rows</returns>
        public static Task<int> DeleteAsync<T>(this IDatabase db, CancellationToken cancellationToken,
                FSharpExpr<FSharpFunc<T, bool>> query)
            => db.DeleteAsync<T>(cancellationToken, query.ToSql(db));
        #endregion

    }
}
