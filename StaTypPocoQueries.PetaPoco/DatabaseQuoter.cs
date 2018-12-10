using PetaPoco;
using StaTypPocoQueries.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaTypPocoQueries.PetaPoco
{
    public class DatabaseQuoter: Translator.IQuoter
    {
        private readonly Database _db;

        public DatabaseQuoter(Database db)
        {
            _db = db;
        }

        /// <summary>
        /// Use the Database's Provider to escape the SQL identifer.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string QuoteColumn(string columnName) => _db.Provider.EscapeSqlIdentifier(columnName);
    }
}
