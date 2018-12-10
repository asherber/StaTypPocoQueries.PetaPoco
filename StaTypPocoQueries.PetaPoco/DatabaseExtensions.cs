using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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

        private static Translator.IQuoter GetQuoter(this Database db) => new Quoter(db);

        private static Sql ToSql<T>(this Expression<Func<T, bool>> query, Database db)
        {
            var translated = ExpressionToSql.Translate(db.GetQuoter(), query);
            return new Sql(translated.Item1, translated.Item2);
        }


        public static List<T> Fetch<T>(this Database db, Expression<Func<T, bool>> query) 
            => db.Fetch<T>(query.ToSql(db));

        public static Page<T> Page<T>(this Database db, long page, long itemsPerPage, Expression<Func<T, bool>> query)
            => db.Page<T>(page, itemsPerPage, query.ToSql(db));

        public static List<T> Fetch<T>(this Database db, long page, long itemsPerPage, Expression<Func<T, bool>> query)
            => db.Fetch<T>(page, itemsPerPage, query.ToSql(db));

        public static List<T> SkipTake<T>(this Database db, long skip, long take, Expression<Func<T, bool>> query)
            => db.SkipTake<T>(skip, take, query.ToSql(db));

        public static IEnumerable<T> Query<T>(this Database db, Expression<Func<T, bool>> query)
            => db.Query<T>(query.ToSql(db));

        public static T Single<T>(this Database db, Expression<Func<T, bool>> query)
            => db.Single<T>(query.ToSql(db));

        public static T SingleOrDefault<T>(this Database db, Expression<Func<T, bool>> query)
            => db.SingleOrDefault<T>(query.ToSql(db));

        public static T First<T>(this Database db, Expression<Func<T, bool>> query)
            => db.First<T>(query.ToSql(db));

        public static T FirstOrDefault<T>(this Database db, Expression<Func<T, bool>> query)
            => db.FirstOrDefault<T>(query.ToSql(db));

        public static int Update<T>(this Database db, Expression<Func<T, bool>> query)
            => db.Update<T>(query.ToSql(db));

        public static int Delete<T>(this Database db, Expression<Func<T, bool>> query)
            => db.Delete<T>(query.ToSql(db));
    }
}
