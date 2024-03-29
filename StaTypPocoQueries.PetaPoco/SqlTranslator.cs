﻿/**
 * Copyright 2018-2021 Aaron Sherber

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

using Microsoft.FSharp.Core;
using Microsoft.FSharp.Quotations;
using PetaPoco;
using PetaPoco.Core;
using StaTypPocoQueries.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StaTypPocoQueries.PetaPoco
{
    public class SqlTranslator
    {
        private readonly IDatabase _db;

        public SqlTranslator(IDatabase db)
        {
            _db = db;
        }

        public Sql Translate<T>(Expression<Func<T, bool>> query)
        {
            var translated = ExpressionToSql.Translate(
                quoter: new DatabaseQuoter(_db), 
                conditions: query,
                includeWhere: true,
                customNameExtractor: GetColumnName,
                customParameterValueMap: GetConvertedValue);

            return new Sql(translated.Item1, translated.Item2);
        }

        public Sql Translate<T>(FSharpExpr<FSharpFunc<T, bool>> query)
        {
            var translated = ExpressionToSql.Translate(
                quoter: new DatabaseQuoter(_db),
                conditions: query,
                includeWhere: true,
                customNameExtractor: FsGetColumnName,
                customParameterValueMap: FsGetConvertedValue);

            return new Sql(translated.Item1, translated.Item2);
        }

        private string GetColumnName(MemberInfo mi, Type type)
        {
            if (mi is PropertyInfo pi)
            {
                var mapper = GetMapper(type);
                var realPI = type.GetProperty(pi.Name);
                var ci = mapper.GetColumnInfo(realPI);
                return ci.ColumnName;
            }
            else
                return mi.Name;
        }

        
        private object GetConvertedValue(PropertyInfo pi, Type type, object input)
        {
            var mapper = GetMapper(type);
            var realPI = type.GetProperty(pi.Name);
            var converter = mapper.GetToDbConverter(realPI);            
            return converter == null ? input : converter(input);
        }

        private IMapper GetMapper(Type type) => Mappers.GetMapper(type, _db.DefaultMapper);

        private FSharpFunc<MemberInfo, FSharpFunc<Type, string>> FsGetColumnName
            => ExpressionToSql.AsFsFunc3<MemberInfo, Type, string>(GetColumnName);

        // Helpful precis: https://codeblog.jonskeet.uk/2012/01/30/currying-vs-partial-function-application/
        private FSharpFunc<PropertyInfo, FSharpFunc<Type, FSharpFunc<object, object>>> FsGetConvertedValue
            => ExpressionToSql.AsFsFunc<PropertyInfo, FSharpFunc<Type, FSharpFunc<object, object>>>(
                pi => ExpressionToSql.AsFsFunc3<Type, object, object>(
                    (type, input) => GetConvertedValue(pi, type, input)
                )
            );
    }
}
