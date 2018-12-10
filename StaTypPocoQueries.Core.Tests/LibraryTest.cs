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
            AreEqual("WHERE <anInt> = @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt == aVar));
            
            var dt = new DateTime(2001, 2, 3, 4, 5, 6);
            AreEqual("WHERE <aDate> = @0", new object[] {dt}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aDate == dt));
        }
          
        [Fact]
        public void TestGreaterSmaller() {
            var aVar = 5;
            AreEqual("WHERE <anInt> >= @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt >= aVar));
            
            AreEqual("WHERE <anInt> <= @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt <= aVar));
            
            AreEqual("WHERE <anInt> > @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt > aVar));
            
            AreEqual("WHERE <anInt> < @0", new object[] {5}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt < aVar));
        }

        [Fact]
        public void TestEqualsNonNullBoolLiteral() {
            AreEqual("WHERE <aBool> = @0", new object[] {false}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => !x.aBool));
            AreEqual("WHERE <aBool> = @0", new object[] {true}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aBool));
        }
        
        [Fact]
        public void TestEqualsNullableVariable() {            
            AreEqual("WHERE <nullableInt> = @0", new object[] {(int?)0}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableInt == 0));
            AreEqual("WHERE <nullableDecimal> = @0", new object[] {(decimal?)0M}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDecimal == 0M));
            AreEqual("WHERE <nullableLong> = @0", new object[] {(long?)123L}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableLong == 123L));

            var dt = new DateTime(2001, 2, 3, 4, 5, 6);
            AreEqual("WHERE <nullableDate> = @0", new object[] {dt}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDate == dt));

            AreEqual("WHERE <nullableBool> = @0", new object[] {(bool?)true},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableBool == true));
        }
        
        [Fact]
        public void TestEqualsNullableIsNull() {            
            AreEqual("WHERE <nullableInt> IS NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableInt == null));
            AreEqual("WHERE <nullableDecimal> IS NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDecimal == null));
            AreEqual("WHERE <nullableLong> IS NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableLong == null));
            AreEqual("WHERE <nullableDate> IS NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDate == null));
            AreEqual("WHERE <nullableBool> IS NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableBool == null));            
        }

        [Fact]
        public void TestEqualsNullableIsNotNull() {            
            AreEqual("WHERE <nullableInt> IS NOT NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableInt != null, true));
            AreEqual("WHERE <nullableDecimal> IS NOT NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDecimal != null, true));
            AreEqual("WHERE <nullableLong> IS NOT NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableLong != null));
            AreEqual("WHERE <nullableDate> IS NOT NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableDate != null));
            AreEqual("WHERE <nullableBool> IS NOT NULL", 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.nullableBool != null));            
        }

        [Fact]
        public void TestEqualsNonNullLiteral() {
            var dt = new DateTime(2001,2,3);
            AreEqual("WHERE <aDate> = @0", new object[] {new DateTime(2001,2,3)},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aDate == dt));
            AreEqual("WHERE <anInt> = @0", new object[] {0}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.anInt == 0));
            AreEqual("WHERE <aDecimal> = @0", new object[] {0M}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aDecimal == 0M));
            AreEqual("WHERE <aLong> = @0", new object[] {123L}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aLong == 123));
            AreEqual("WHERE <aString> = @0", new object[] {"foo"}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aString == "foo"));
            AreEqual("WHERE <aBool> = @0", new object[] {true}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, x => x.aBool == true));
        }
        
        [Fact]
        public void TestConjunctions() {
            AreEqual("WHERE <aBool> = @0 AND <aBool> = @1", new object[] {true, false}, 
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.aBool && !x.aBool));

            AreEqual("WHERE <anInt> = @0 AND <aString> = @1", new object[] {0, "foo"},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 && x.aString == "foo"));

            AreEqual("WHERE <anInt> = @0 OR <aString> = @1", new object[] {0, "foo"},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 || x.aString == "foo"));

            AreEqual("WHERE <anInt> = @0 AND <aString> = @1 AND <aLong> = @2", new object[] {0, "foo", 0L},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 && x.aString == "foo" && x.aLong == 0));
            
            AreEqual("WHERE <anInt> = @0 OR <aString> = @1 OR <aLong> = @2", new object[] {0, "foo", 0L},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 || x.aString == "foo" || x.aLong == 0));

            AreEqual("WHERE <anInt> = @0 AND (<aString> = @1 OR <aLong> = @2)", new object[] {0, "foo", 0L},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => x.anInt == 0 && (x.aString == "foo" || x.aLong == 0)));
            
            AreEqual("WHERE (<anInt> = @0 AND <aString> = @1) OR <aLong> = @2", new object[] {0, "foo", 0L},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => (x.anInt == 0 && x.aString == "foo") || x.aLong == 0));
            
            AreEqual("WHERE (<anInt> = @0 AND <aString> = @1) OR (<aLong> = @2 AND <aString> = @3)", 
                new object[] {0, "foo", 0L, "foo"},
                ExpressionToSql.Translate<SomeEntity>(TestQuoter.Instance, 
                    x => (x.anInt == 0 && x.aString == "foo") || (x.aLong == 0 && x.aString == "foo")));
        }        
    }
}
