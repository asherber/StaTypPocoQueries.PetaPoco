//Copyright © 2018 Dominik Pytlewski. Licensed under Apache License 2.0. See LICENSE file for details

using System;
using System.Collections.Generic;
using Xunit;

namespace StaTypPocoQueries.Core.Tests {
    class SomeEntity {
        public int anInt { get; set; }
        public decimal aDecimal { get; set; }
        public long aLong { get; set; }
        public string aString { get; set; }
        public DateTime aDate { get; set; }
        public bool aBool { get; set; }

        public int? nullableInt { get; set; }
        public decimal? nullableDecimal { get; set; }
        public long? nullableLong { get; set; }
        public DateTime? nullableDate { get; set; }
        public bool? nullableBool { get; set; }
    }
    
    public class TestQuoter : Translator.IQuoter {
        public static Translator.IQuoter Instance => new TestQuoter();

        public string QuoteColumn(string columnName) {
            return $"<{columnName}>";
        }
    }

    public class LibraryTest {
        private static void AreEqual(string expectedSql, object[] expectedParams, Tuple<string,object[]> fact) {
            Assert.Equal(expectedSql, fact.Item1);
            Assert.Equal(expectedParams, new List<object>(fact.Item2)); //to make it IComparable
        }

        private static void AreEqual(string expectedSql, Tuple<string,object[]> fact) {
            Assert.Equal(expectedSql, fact.Item1);
            Assert.Equal(new object[0], new List<object>(fact.Item2)); //to make it IComparable
        }

        [Fact]
        public void TestEqualsNonNullVariable() {            
            // equals not null variable
            
            var aVar = 5;
            AreEqual("where <anInt> = @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt == aVar));
            
            var dt = new DateTime(2001, 2, 3, 4, 5, 6);
            AreEqual("where <aDate> = @0", new object[] {dt}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aDate == dt));
        }
          
        [Fact]
        public void TestGreaterSmaller() {
            var aVar = 5;
            AreEqual("where <anInt> >= @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt >= aVar));
            
            AreEqual("where <anInt> <= @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt <= aVar));
            
            AreEqual("where <anInt> > @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt > aVar));
            
            AreEqual("where <anInt> < @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt < aVar));
        }

        [Fact]
        public void TestEqualsNonNullBoolLiteral() {
            AreEqual("where <aBool> = @0", new object[] {false}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => !x.aBool));
            AreEqual("where <aBool> = @0", new object[] {true}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aBool));
        }
        
        [Fact]
        public void TestEqualsNullableVariable() {            
            AreEqual("where <nullableInt> = @0", new object[] {(int?)0}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableInt == 0));
            AreEqual("where <nullableDecimal> = @0", new object[] {(decimal?)0M}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDecimal == 0M));
            AreEqual("where <nullableLong> = @0", new object[] {(long?)123L}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableLong == 123L));

            var dt = new DateTime(2001, 2, 3, 4, 5, 6);
            AreEqual("where <nullableDate> = @0", new object[] {dt}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDate == dt));

            AreEqual("where <nullableBool> = @0", new object[] {(bool?)true},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableBool == true));
        }
        
        [Fact]
        public void TestEqualsNullableIsNull() {            
            AreEqual("where <nullableInt> is null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableInt == null));
            AreEqual("where <nullableDecimal> is null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDecimal == null));
            AreEqual("where <nullableLong> is null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableLong == null));
            AreEqual("where <nullableDate> is null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDate == null));
            AreEqual("where <nullableBool> is null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableBool == null));            
        }

        [Fact]
        public void TestEqualsNullableIsNotNull() {            
            AreEqual("where <nullableInt> is not null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableInt != null, true));
            AreEqual("where <nullableDecimal> is not null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDecimal != null, true));
            AreEqual("where <nullableLong> is not null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableLong != null));
            AreEqual("where <nullableDate> is not null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDate != null));
            AreEqual("where <nullableBool> is not null", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableBool != null));            
        }

        [Fact]
        public void TestEqualsNonNullLiteral() {
            var dt = new DateTime(2001,2,3);
            AreEqual("where <aDate> = @0", new object[] {new DateTime(2001,2,3)},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aDate == dt));
            AreEqual("where <anInt> = @0", new object[] {0}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt == 0));
            AreEqual("where <aDecimal> = @0", new object[] {0M}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aDecimal == 0M));
            AreEqual("where <aLong> = @0", new object[] {123L}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aLong == 123));
            AreEqual("where <aString> = @0", new object[] {"foo"}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aString == "foo"));
            AreEqual("where <aBool> = @0", new object[] {true}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aBool == true));
        }
        
        [Fact]
        public void TestConjunctions() {
            AreEqual("where <aBool> = @0 and <aBool> = @1", new object[] {true, false}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.aBool && !x.aBool));

            AreEqual("where <anInt> = @0 and <aString> = @1", new object[] {0, "foo"},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 && x.aString == "foo"));

            AreEqual("where <anInt> = @0 or <aString> = @1", new object[] {0, "foo"},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 || x.aString == "foo"));

            AreEqual("where <anInt> = @0 and <aString> = @1 and <aLong> = @2", new object[] {0, "foo", 0L},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 && x.aString == "foo" && x.aLong == 0));
            
            AreEqual("where <anInt> = @0 or <aString> = @1 or <aLong> = @2", new object[] {0, "foo", 0L},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 || x.aString == "foo" || x.aLong == 0));

            AreEqual("where <anInt> = @0 and (<aString> = @1 or <aLong> = @2)", new object[] {0, "foo", 0L},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 && (x.aString == "foo" || x.aLong == 0)));
            
            AreEqual("where (<anInt> = @0 and <aString> = @1) or <aLong> = @2", new object[] {0, "foo", 0L},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => (x.anInt == 0 && x.aString == "foo") || x.aLong == 0));
            
            AreEqual("where (<anInt> = @0 and <aString> = @1) or (<aLong> = @2 and <aString> = @3)", 
                new object[] {0, "foo", 0L, "foo"},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => (x.anInt == 0 && x.aString == "foo") || (x.aLong == 0 && x.aString == "foo")));
        }        
    }
}
