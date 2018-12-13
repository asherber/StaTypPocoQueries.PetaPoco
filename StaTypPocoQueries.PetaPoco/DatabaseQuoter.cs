/**
 * Copyright 2018 Aaron Sherber

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
