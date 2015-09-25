﻿// Copyright (c) 2015 Liam McSherry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static System.Linq.Enumerable;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// <para>
    /// Provides tests for the base <see cref="SemanticVersion"/> functionality.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class SemanticVersionTests
    {
        private const string Category = "Semantic Version Base";

        /// <summary>
        /// <para>
        /// Tests that validation of build metadata items is working
        /// as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MetadataValidation()
        {
            var validItems = new string[]
            {
                // These items are taken from the specification.
                "001",                                  // 0
                "20130313144700",                       // 1
                "exp",                                  // 2
                "sha",                                  // 3
                "5114f85",                              // 4

                // These we came up with ourselves.
                "contains-some-hyphens",                // 5
                "-leading-hyphen",                      // 6
                "trailing-hyphen-",                     // 7
                "-leading-and-trailing-",               // 8

                // The specification doesn't mention
                // leading or trailing hyphens, so we
                // have to assume they're valid (if
                // ugly).
            };

            // Iterate through all the items we expect to be valid,
            // checking that they are valid.
            for (int i = 0; i < validItems.Length; i++)
            {
                Assert.IsTrue(
                    Helper.IsValidMetadata(validItems[i]),
                    $"Unexpected rejection: item {i} (\"{validItems[i]}\")."
                    );
            }

            var invItems = new string[]
            {
                "",                         // 0
                null,                       // 1
                " ",                        // 2
                "infix space",              // 3
                " leading space",           // 4
                "trailing space ",          // 5
                " leading and trailing ",   // 6
                "Tür",                      // 7
                "jalapeño",                 // 8
                "çava",                     // 9
                "?",                        // 10
            };
            // Iterate through all the items we expect to be invalid.
            for (int i = 0; i < invItems.Length; i++)
            {
                Assert.IsFalse(
                    Helper.IsValidIdentifier(invItems[i]),
                    $"Unexpected acceptance: item {i} (\"{invItems[i]}\")."
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests that validation of pre-release identifiers works
        /// as is expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void IdentifierValidation()
        {
            // Pre-release identifiers only differ from metadata items
            // in that they cannot have leading zeroes in their numeric
            // items. This simplifies our tests somewhat.

            // A lone zero does not count as a leading zero, and must be
            // considered valid.
            Assert.IsTrue(Helper.IsValidIdentifier("0"),
                          "Unexpected rejection: single zero.");

            // Leading zeroes don't matter if the identifier is not a
            // numeric identifier.
            Assert.IsTrue(Helper.IsValidIdentifier("00nonnumber"),
                          "Unexpected rejection: non-numeric leading zero.");

            // If the identifier is numeric, then we can't have any leading
            // zeroes.
            Assert.IsFalse(Helper.IsValidIdentifier("0150"),
                           "Unexpected acceptance: numeric leading zero.");
        }

        /// <summary>
        /// <para>
        /// Tests that <see cref="SemanticVersion.ToString"/> produces
        /// the expected result.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Stringifying()
        {
            var sv0 = new SemanticVersion(1, 0, 0);
            var sv1 = new SemanticVersion(1, 1, 1);
            var sv2 = new SemanticVersion(1, 2, 0, new[] { "rc", "1" });
            var sv3 = new SemanticVersion(1, 2, 1, new[] { "rc", "1" });
            var sv4 = new SemanticVersion(1, 3, 0, new[] { "rc", "1" },
                                                   new[] { "20150925", "1" });
            var sv5 = new SemanticVersion(1, 3, 1, new[] { "rc", "1" },
                                                   new[] { "20150925", "1" });

            #region ToString(void)
            // First, we're going to test the "standard" [ToString]
            // method accepting no arguments. This should produce
            // strings formatted as given in the Semantic Versioning
            // spec.
            Assert.AreEqual(sv0.ToString(), "1.0.0", 
                            "ToString() failure (0).");
            Assert.AreEqual(sv1.ToString(), "1.1.1",
                            "ToString() failure (1).");
            Assert.AreEqual(sv2.ToString(), "1.2.0-rc.1",
                            "ToString() failure (2).");
            Assert.AreEqual(sv3.ToString(), "1.2.1-rc.1",
                            "ToString() failure (3).");
            Assert.AreEqual(sv4.ToString(), "1.3.0-rc.1+20150925.1",
                            "ToString() failure (4).");
            Assert.AreEqual(sv5.ToString(), "1.3.1-rc.1+20150925.1",
                            "ToString() failure (5).");
            #endregion
            #region ToString(string, IFormatProvider) + ToString(string)
            // Next, we're going to test the result of each format specifier
            // given to the [IFormattable] implementation of [ToString].

            #region Format Specifier: "G"
            // First up is the "G" specifier, which should produce the same
            // result as normal [ToString].
            Assert.AreEqual(sv0.ToString("G", null), sv0.ToString(),
                            "Format specifier 'G' failure (0).");
            Assert.AreEqual(sv1.ToString("G", null), sv1.ToString(),
                            "Format specifier 'G' failure (1).");
            Assert.AreEqual(sv2.ToString("G", null), sv2.ToString(),
                            "Format specifier 'G' failure (2).");
            Assert.AreEqual(sv3.ToString("G", null), sv3.ToString(),
                            "Format specifier 'G' failure (3).");
            Assert.AreEqual(sv4.ToString("G", null), sv4.ToString(),
                            "Format specifier 'G' failure (4).");
            Assert.AreEqual(sv5.ToString("G", null), sv5.ToString(),
                            "Format specifier 'G' failure (5).");
            // We're also going to test the overload that doesn't take an
            // [IFormatProvider] to make sure it produces the same result.
            Assert.AreEqual(sv0.ToString("G", null), sv0.ToString("G"),
                            "Format specifier 'G' failure (6).");
            Assert.AreEqual(sv1.ToString("G", null), sv1.ToString("G"),
                            "Format specifier 'G' failure (7).");
            Assert.AreEqual(sv2.ToString("G", null), sv2.ToString("G"),
                            "Format specifier 'G' failure (8).");
            Assert.AreEqual(sv3.ToString("G", null), sv3.ToString("G"),
                            "Format specifier 'G' failure (9).");
            Assert.AreEqual(sv4.ToString("G", null), sv4.ToString("G"),
                            "Format specifier 'G' failure (10).");
            Assert.AreEqual(sv5.ToString("G", null), sv5.ToString("G"),
                            "Format specifier 'G' failure (11).");
            #endregion
            #region Format Specifier: "g"
            // The "g" specifier is like "G", but the output is prefixed
            // with a "v".
            Assert.AreEqual(sv0.ToString("g", null), "v" + sv0.ToString(),
                            "Format specifier 'g' failure (0).");
            Assert.AreEqual(sv1.ToString("g", null), "v" + sv1.ToString(),
                            "Format specifier 'g' failure (1).");
            Assert.AreEqual(sv2.ToString("g", null), "v" + sv2.ToString(),
                            "Format specifier 'g' failure (2).");
            Assert.AreEqual(sv3.ToString("g", null), "v" + sv3.ToString(),
                            "Format specifier 'g' failure (3).");
            Assert.AreEqual(sv4.ToString("g", null), "v" + sv4.ToString(),
                            "Format specifier 'g' failure (4).");
            Assert.AreEqual(sv5.ToString("g", null), "v" + sv5.ToString(),
                            "Format specifier 'g' failure (5).");
            // Now we test the single-argument [ToString] method.
            Assert.AreEqual(sv0.ToString("g", null), sv0.ToString("g"),
                            "Format specifier 'g' failure (6).");
            Assert.AreEqual(sv1.ToString("g", null), sv1.ToString("g"),
                            "Format specifier 'g' failure (7).");
            Assert.AreEqual(sv2.ToString("g", null), sv2.ToString("g"),
                            "Format specifier 'g' failure (8).");
            Assert.AreEqual(sv3.ToString("g", null), sv3.ToString("g"),
                            "Format specifier 'g' failure (9).");
            Assert.AreEqual(sv4.ToString("g", null), sv4.ToString("g"),
                            "Format specifier 'g' failure (10).");
            Assert.AreEqual(sv5.ToString("g", null), sv5.ToString("g"),
                            "Format specifier 'g' failure (11).");
            #endregion
            #region Format Specifier: null
            // The null specifier must produce the same result as "G".
            Assert.AreEqual(sv0.ToString(null, null), sv0.ToString(),
                            "Format specifier 'null' failure (0).");
            Assert.AreEqual(sv1.ToString(null, null), sv1.ToString(),
                            "Format specifier 'null' failure (1).");
            Assert.AreEqual(sv2.ToString(null, null), sv2.ToString(),
                            "Format specifier 'null' failure (2).");
            Assert.AreEqual(sv3.ToString(null, null), sv3.ToString(),
                            "Format specifier 'null' failure (3).");
            Assert.AreEqual(sv4.ToString(null, null), sv4.ToString(),
                            "Format specifier 'null' failure (4).");
            Assert.AreEqual(sv5.ToString(null, null), sv5.ToString(),
                            "Format specifier 'null' failure (5).");
            // Test the single-arg [ToString] method, too.
            Assert.AreEqual(sv0.ToString(null, null), sv0.ToString(null),
                            "Format specifier 'null' failure (6).");
            Assert.AreEqual(sv1.ToString(null, null), sv1.ToString(null),
                            "Format specifier 'null' failure (7).");
            Assert.AreEqual(sv2.ToString(null, null), sv2.ToString(null),
                            "Format specifier 'null' failure (8).");
            Assert.AreEqual(sv3.ToString(null, null), sv3.ToString(null),
                            "Format specifier 'null' failure (9).");
            Assert.AreEqual(sv4.ToString(null, null), sv4.ToString(null),
                            "Format specifier 'null' failure (10).");
            Assert.AreEqual(sv5.ToString(null, null), sv5.ToString(null),
                            "Format specifier 'null' failure (11).");
            #endregion
            #region Format Specifier: "C"
            // The "C" specifier gives us the concise format, where some
            // information may be omitted.
            Assert.AreEqual(sv0.ToString("C", null), "1.0",
                            "Format specifier 'C' failure (0).");
            Assert.AreEqual(sv1.ToString("C", null), "1.1.1",
                            "Format specifier 'C' failure (1).");
            Assert.AreEqual(sv2.ToString("C", null), "1.2-rc.1",
                            "Format specifier 'C' failure (2).");
            Assert.AreEqual(sv3.ToString("C", null), "1.2.1-rc.1",
                            "Format specifier 'C' failure (3).");
            Assert.AreEqual(sv4.ToString("C", null), "1.3-rc.1",
                            "Format specifier 'C' failure (4).");
            Assert.AreEqual(sv5.ToString("C", null), "1.3.1-rc.1",
                            "Format specifier 'C' failure (5).");
            // We need to test the single-argument [ToString] method, too.
            Assert.AreEqual(sv0.ToString("C", null), sv0.ToString("C"),
                            "Format specifier 'C' failure (6).");
            Assert.AreEqual(sv1.ToString("C", null), sv1.ToString("C"),
                            "Format specifier 'C' failure (7).");
            Assert.AreEqual(sv2.ToString("C", null), sv2.ToString("C"),
                            "Format specifier 'C' failure (8).");
            Assert.AreEqual(sv3.ToString("C", null), sv3.ToString("C"),
                            "Format specifier 'C' failure (9).");
            Assert.AreEqual(sv4.ToString("C", null), sv4.ToString("C"),
                            "Format specifier 'C' failure (10).");
            Assert.AreEqual(sv5.ToString("C", null), sv5.ToString("C"),
                            "Format specifier 'C' failure (11).");
            #endregion
            #region Format Specifier: "c"
            // The "c" specifier gives us the same output as "C", but
            // prefixed with the letter "v".
            Assert.AreEqual(sv0.ToString("c", null), "v1.0",
                            "Format specifier 'c' failure (0).");
            Assert.AreEqual(sv1.ToString("c", null), "v1.1.1",
                            "Format specifier 'c' failure (1).");
            Assert.AreEqual(sv2.ToString("c", null), "v1.2-rc.1",
                            "Format specifier 'c' failure (2).");
            Assert.AreEqual(sv3.ToString("c", null), "v1.2.1-rc.1",
                            "Format specifier 'c' failure (3).");
            Assert.AreEqual(sv4.ToString("c", null), "v1.3-rc.1",
                            "Format specifier 'c' failure (4).");
            Assert.AreEqual(sv5.ToString("c", null), "v1.3.1-rc.1",
                            "Format specifier 'c' failure (5).");
            // We need to test the single-argument [ToString] method, too.
            Assert.AreEqual(sv0.ToString("c", null), sv0.ToString("c"),
                            "Format specifier 'c' failure (6).");
            Assert.AreEqual(sv1.ToString("c", null), sv1.ToString("c"),
                            "Format specifier 'c' failure (7).");
            Assert.AreEqual(sv2.ToString("c", null), sv2.ToString("c"),
                            "Format specifier 'c' failure (8).");
            Assert.AreEqual(sv3.ToString("c", null), sv3.ToString("c"),
                            "Format specifier 'c' failure (9).");
            Assert.AreEqual(sv4.ToString("c", null), sv4.ToString("c"),
                            "Format specifier 'c' failure (10).");
            Assert.AreEqual(sv5.ToString("c", null), sv5.ToString("c"),
                            "Format specifier 'c' failure (11).");
            #endregion

            #endregion

            // New tests will need to be added here for any new format specifiers.
        }

        /// <summary>
        /// <para>
        /// Tests the <see cref="SemanticVersion"/> constructors to make sure
        /// they act as expected when given invalid input.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ConstructingInvalid()
        {
            // Make sure that the expected exceptions are thrown for
            // invalid input.

            // Negative three-part components
            new Action(() => new SemanticVersion(-1, 0, 0))
                .AssertThrows<ArgumentOutOfRangeException>(
                    "Did not throw on invalid input (0).");
            new Action(() => new SemanticVersion(0, -1, 0))
                .AssertThrows<ArgumentOutOfRangeException>(
                    "Did not throw on invalid input (1).");
            new Action(() => new SemanticVersion(0, 0, -1))
                .AssertThrows<ArgumentOutOfRangeException>(
                    "Did not throw on invalid input (2).");


            // Null identifier/metadata collections
            new Action(() => new SemanticVersion(0, 0, 0, null))
                .AssertThrows<ArgumentNullException>(
                    "Did not throw on invalid input (3).");

            new Action(delegate
            {
                new SemanticVersion(0, 0, 0, Empty<string>(), null);
            }).AssertThrows<ArgumentNullException>(
                "Did not throw on invalid input (4).");


            // Identifier/metadata collections *containing* null.
            new Action(() => new SemanticVersion(0, 0, 0, new string[] { null }))
                .AssertThrows<ArgumentNullException>(
                    "Did not throw on invalid input (5).");

            new Action(delegate
            {
                new SemanticVersion(
                    major:          0,
                    minor:          0,
                    patch:          0,
                    identifiers:    Empty<string>(),
                    metadata:       new string[] { null }
                    );
            }).AssertThrows<ArgumentNullException>(
                "Did not throw on invalid input (6).");


            // Identifier/metadata collections containing an invalid value.
            new Action(() => new SemanticVersion(0, 0, 0, new[] { "0150" }))
                .AssertThrowsExact<ArgumentException>(
                    "Did not throw on invalid input (7).");

            new Action(delegate
            {
                new SemanticVersion(
                    major:          0,
                    minor:          0,
                    patch:          0,
                    identifiers:    Empty<string>(),
                    metadata:       new string[] { "Löffel" } // Invalid character
                    );
            }).AssertThrowsExact<ArgumentException>(
                "Did not throw on invalid input (8).");

            // Negative two-part components
            new Action(() => new SemanticVersion(-1, 0))
                .AssertThrows<ArgumentOutOfRangeException>(
                    "Did not throw on invalid input (9).");
            new Action(() => new SemanticVersion(0, -1))
                .AssertThrows<ArgumentOutOfRangeException>(
                    "Did not throw on invalid input (10).");
        }
        /// <summary>
        /// <para>
        /// Tests the <see cref="SemanticVersion"/> constructors to make sure
        /// they act as expected when given valid input.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ConstructingValid()
        {
            // These tests are fairly simple, we just test each constructor
            // available with something we know is valid and test the values
            // of the [SemanticVersion]'s properties afterwards.

            #region (int, int) 0-4
            {
                var sv = new SemanticVersion(1, 6);

                Assert.AreEqual(sv.Major, 1, "Incorrect initialisation (0).");
                Assert.AreEqual(sv.Minor, 6, "Incorrect initialisation (1).");
                Assert.AreEqual(sv.Patch, 0, "Incorrect initialisation (2).");

                Assert.IsTrue(sv.Identifiers.SequenceEqual(Empty<string>()),
                              "Incorrect initialisation (3).");
                Assert.IsTrue(sv.Metadata.SequenceEqual(Empty<string>()),
                              "Incorrect initialisation (4).");
            }
            #endregion
            #region (int, int, int) 5-9
            {
                var sv = new SemanticVersion(1, 2, 8);

                Assert.AreEqual(sv.Major, 1, "Incorrect initialisation (5).");
                Assert.AreEqual(sv.Minor, 2, "Incorrect initialisation (6).");
                Assert.AreEqual(sv.Patch, 8, "Incorrect initialisation (7).");

                Assert.IsTrue(sv.Identifiers.SequenceEqual(Empty<string>()),
                              "Incorrect initialisation (8).");
                Assert.IsTrue(sv.Metadata.SequenceEqual(Empty<string>()),
                              "Incorrect initialisation (9).");
            }
            #endregion
            #region (int, int, int, IEnumerable<string> 10-14
            {
                var sv = new SemanticVersion(2, 5, 6, new[] { "rc" });

                Assert.AreEqual(sv.Major, 2, "Incorrect initialisation (10).");
                Assert.AreEqual(sv.Minor, 5, "Incorrect initialisation (11).");
                Assert.AreEqual(sv.Patch, 6, "Incorrect initialisation (12).");

                Assert.IsTrue(sv.Identifiers.SequenceEqual(new[] { "rc" }),
                              "Incorrect initialisation (13).");
                Assert.IsTrue(sv.Metadata.SequenceEqual(Empty<string>()),
                              "Incorrect initialisation (14).");
            }
            #endregion
            #region (int, int, int, IEnumerable<string>, IEnumerable<string>)
            {
                var sv = new SemanticVersion(5, 1, 2, new[] { "rc" }, 
                                                      new[] { "2015" });

                Assert.AreEqual(sv.Major, 5, "Incorrect initialisation (15).");
                Assert.AreEqual(sv.Minor, 1, "Incorrect initialisation (16).");
                Assert.AreEqual(sv.Patch, 2, "Incorrect initialisation (17).");

                Assert.IsTrue(sv.Identifiers.SequenceEqual(new[] { "rc" }),
                              "Incorrect initialisation (18).");
                Assert.IsTrue(sv.Metadata.SequenceEqual(new[] { "2015" }),
                              "Incorrect initialisation (19).");
            }
            #endregion
        }

        /// <summary>
        /// <para>
        /// Tests <see cref="SemanticVersion"/>'s implementation of
        /// <see cref="IComparable{T}"/> to ensure it acts as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Comparison()
        {
            Assert.IsTrue(typeof(IComparable<SemanticVersion>)
                            .IsAssignableFrom(typeof(SemanticVersion)),
                          "SemanticVersion is not IComparable<SemanticVersion>.");

            #region Definitions
            var cmp = new SemanticVersion[]
            {
                null,

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "alpha" },
                                    metadata:       Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "alpha", "1" },
                                    metadata:       Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "alpha", "beta" },
                                    metadata:       Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "beta" },
                                    metadata:       Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "beta", "2" },
                                    metadata:       Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "beta", "11" },
                                    metadata:       Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "rc", "1" },
                                    metadata:       Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    Empty<string>(),
                                    metadata:       Empty<string>()),
            };
            #endregion

            #region CompareTo
            // Test the [CompareTo] method.
            Assert.AreEqual(cmp[1].CompareTo<SemanticVersion>(cmp[0]),
                            Ordering.Greater,
                            "Comparison failed (0).");
            Assert.AreEqual(cmp[1].CompareTo<SemanticVersion>(cmp[2]),
                            Ordering.Lesser,
                            "Comparison failed (1).");
            Assert.AreEqual(cmp[2].CompareTo<SemanticVersion>(cmp[3]),
                            Ordering.Lesser,
                            "Comparison failed (2).");
            Assert.AreEqual(cmp[3].CompareTo<SemanticVersion>(cmp[4]),
                            Ordering.Lesser,
                            "Comparison failed (3).");
            Assert.AreEqual(cmp[4].CompareTo<SemanticVersion>(cmp[5]),
                            Ordering.Lesser,
                            "Comparison failed (4).");
            Assert.AreEqual(cmp[5].CompareTo<SemanticVersion>(cmp[6]),
                            Ordering.Lesser,
                            "Comparison failed (5).");
            Assert.AreEqual(cmp[6].CompareTo<SemanticVersion>(cmp[7]),
                            Ordering.Lesser,
                            "Comparison failed (6).");
            Assert.AreEqual(cmp[7].CompareTo<SemanticVersion>(cmp[8]),
                            Ordering.Lesser,
                            "Comparison failed (7).");

            // These tests are mentioned by the MSDN docs, so we're going to
            // use them here just to make sure everything is working fine.
            Assert.AreEqual(cmp[1].CompareTo(cmp[1]), 0,
                            "Comparison failed (9).");
            Assert.AreEqual(cmp[1].CompareTo(cmp[2]),
                            -cmp[2].CompareTo(cmp[1]),
                            "Comparison failed (10).");
            #endregion
            #region Sorting
            // To be extra sure, stick them in a collection, sort it, and
            // check the order they come out of the collection in. We jumble
            // up the items by ordering them using a newly-generated GUID.
            var sl = new List<SemanticVersion>(cmp.OrderBy(ks => Guid.NewGuid()));
            sl.Sort();

            // [cmp] is already in the correct lowest-to-highest order.
            Assert.IsTrue(sl.SequenceEqual(cmp), "Comparison failed (8).");
            #endregion
            #region Operators > and <
            // Now we have to do practically the same tests again, but this time
            // testing the comparison operators rather than the [CompareTo]
            // method.
            Assert.IsTrue(cmp[1] > cmp[0], "Operator comparison failed (0).");
            Assert.IsTrue(cmp[2] > cmp[1], "Operator comparison failed (1).");
            Assert.IsTrue(cmp[3] > cmp[2], "Operator comparison failed (2).");
            Assert.IsTrue(cmp[4] > cmp[3], "Operator comparison failed (3).");
            Assert.IsTrue(cmp[5] > cmp[4], "Operator comparison failed (4).");
            Assert.IsTrue(cmp[6] > cmp[5], "Operator comparison failed (5).");
            Assert.IsTrue(cmp[7] > cmp[6], "Operator comparison failed (6).");
            Assert.IsTrue(cmp[8] > cmp[7], "Operator comparison failed (7).");

            // Same tests, but with the other operator.
            Assert.IsTrue(cmp[0] < cmp[1], "Operator comparison failed (8).");
            Assert.IsTrue(cmp[1] < cmp[2], "Operator comparison failed (9).");
            Assert.IsTrue(cmp[2] < cmp[3], "Operator comparison failed (10).");
            Assert.IsTrue(cmp[3] < cmp[4], "Operator comparison failed (11).");
            Assert.IsTrue(cmp[4] < cmp[5], "Operator comparison failed (12).");
            Assert.IsTrue(cmp[5] < cmp[6], "Operator comparison failed (13).");
            Assert.IsTrue(cmp[6] < cmp[7], "Operator comparison failed (14).");
            Assert.IsTrue(cmp[7] < cmp[8], "Operator comparison failed (15).");

            // These are the ones mentioned by the MSDN docs.
            Assert.IsFalse(cmp[1] > cmp[1], "Operator comparison failed (16).");
            Assert.IsFalse(cmp[1] < cmp[1], "Operator comparison failed (17).");
            Assert.IsTrue((cmp[1] > cmp[2]) == !(cmp[2] > cmp[1]),
                          "Operator comparison failed (18).");
            Assert.IsTrue((cmp[1] < cmp[2]) == !(cmp[2] < cmp[1]),
                          "Operator comparison failed (19).");
            #endregion
            #region Operators >= and <=
            // We're also testing the [>=] and [<=] operators.
            Assert.IsTrue(cmp[8] >= cmp[8], "Operator comparison failed (20).");
            Assert.IsTrue(cmp[8] >= cmp[7], "Operator comparison failed (21).");
            Assert.IsTrue(cmp[8] >= cmp[6], "Operator comparison failed (22).");
            Assert.IsTrue(cmp[8] >= cmp[5], "Operator comparison failed (23).");
            Assert.IsTrue(cmp[8] >= cmp[4], "Operator comparison failed (24).");
            Assert.IsTrue(cmp[8] >= cmp[3], "Operator comparison failed (25).");
            Assert.IsTrue(cmp[8] >= cmp[2], "Operator comparison failed (26).");
            Assert.IsTrue(cmp[8] >= cmp[1], "Operator comparison failed (27).");
            Assert.IsTrue(cmp[8] >= cmp[0], "Operator comparison failed (28).");

            Assert.IsTrue(cmp[0] <= cmp[0], "Operator comparison failed (29).");
            Assert.IsTrue(cmp[0] <= cmp[1], "Operator comparison failed (30).");
            Assert.IsTrue(cmp[0] <= cmp[2], "Operator comparison failed (31).");
            Assert.IsTrue(cmp[0] <= cmp[3], "Operator comparison failed (32).");
            Assert.IsTrue(cmp[0] <= cmp[4], "Operator comparison failed (33).");
            Assert.IsTrue(cmp[0] <= cmp[5], "Operator comparison failed (34).");
            Assert.IsTrue(cmp[0] <= cmp[6], "Operator comparison failed (35).");
            Assert.IsTrue(cmp[0] <= cmp[7], "Operator comparison failed (36).");
            Assert.IsTrue(cmp[0] <= cmp[8], "Operator comparison failed (37).");

            Assert.IsFalse(cmp[1] <= cmp[0], "Operator comparison failed (38).");
            Assert.IsFalse(cmp[0] >= cmp[1], "Operator comparison failed (39).");
            #endregion
        }
        /// <summary>
        /// <para>
        /// Tests that <see cref="SemanticVersion"/>'s implementation
        /// of <see cref="IEquatable{T}"/> works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Equality()
        {
            var sv0 = new SemanticVersion(1, 5, 0);
            var sv1 = new SemanticVersion(1, 5, 0, Empty<string>());
            var sv2 = new SemanticVersion(1, 5, 0, Empty<string>(),
                                                   Empty<string>());
            var sv3 = new SemanticVersion(1, 6, 0);
            var sv4 = new SemanticVersion(1, 6, 0, new[] { "rc", "1" });
            var sv5 = new SemanticVersion(1, 6, 0, Empty<string>(),
                                                   new[] { "d116bf47" });

            Assert.AreEqual(sv0, sv1, "Equality check failed (0).");
            Assert.AreEqual(sv0, sv2, "Equality check failed (1).");
            Assert.AreEqual(sv1, sv2, "Equality check failed (2).");
            Assert.AreNotEqual(sv0, sv3, "Equality check failed (3).");

            // Now we test the equality operators.
            //
            // Our [SemanticVersion] class is immutable, so it makes sense
            // to overload these to check value equality instead of reference
            // equality.
            Assert.IsTrue(sv0 == sv1, "Operator check failed (0).");
            Assert.IsTrue(sv0 == sv2, "Operator check failed (1).");
            Assert.IsTrue(sv1 == sv2, "Operator check failed (2).");
            Assert.IsTrue(sv0 != sv3, "Operator check failed (3).");

            // Now the tests mentioned in the guidelines for overloading
            // [Equals].
            Assert.IsTrue(sv0.Equals(sv1), "Guideline test failed (0).");
            Assert.IsTrue(sv1.Equals(sv0), "Guideline test failed (1).");
            Assert.IsTrue(sv1.Equals(sv0) && sv1.Equals(sv2) && sv0.Equals(sv2),
                          "Guideline test failed (2).");
            Assert.IsFalse(sv0.Equals(null), "Guideline test failed (3).");

            // The guideline tests again, but for our operators.
            Assert.IsTrue(sv0 == sv1, "Guideline test failed (4).");
            Assert.IsTrue(sv1 == sv0, "Guideline test failed (5).");
            Assert.IsTrue(sv1 == sv0 && sv1 == sv2 && sv0 == sv2,
                          "Guideline test failed (6).");
            Assert.IsFalse(sv0 == null, "Guideline test failed (7).");

            // Two null [SemanticVersion]s should be equal, so we're going to
            // test for that, too.
            SemanticVersion nsv0 = null, nsv1 = null;
            Assert.IsTrue(nsv0 == nsv1, "Null equality check failed (0).");

            // [Equals] and the equality operator must take into account metadata
            // and pre-release identifiers.
            Assert.AreNotEqual(sv3, sv4, "Inequality check failed (0).");
            Assert.AreNotEqual(sv3, sv5, "Inequality check failed (1).");
            Assert.AreNotEqual(sv4, sv5, "Inequality check failed (2).");

            Assert.IsFalse(sv3 == sv4, "Inequality check failed (3).");
            Assert.IsFalse(sv3 == sv5, "Inequality check failed (4).");
            Assert.IsFalse(sv4 == sv5, "Inequality check failed (5).");
        }
        /// <summary>
        /// <para>
        /// Tests that <see cref="SemanticVersion.EquivalentTo(SemanticVersion)"/>
        /// works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Equivalence()
        {
            // [EquivalentTo] ignores any information that isn't relevant to
            // determining the compatibility of two versions. Basically, it
            // ignores any metadata.

            var sv0 = new SemanticVersion(1, 7, 0);
            var sv1 = new SemanticVersion(1, 7, 0, new[] { "beta", "3" });
            var sv2 = new SemanticVersion(1, 7, 0, Empty<string>(),
                                                   new[] { "d116bf47" });
            var sv3 = new SemanticVersion(1, 7, 0, new[] { "beta", "3" },
                                                   new[] { "d116bf47" });

            Assert.IsTrue(sv0.EquivalentTo(sv2), "Equivalence check failed (0).");
            Assert.IsTrue(sv1.EquivalentTo(sv3), "Equivalence check failed (1).");

            Assert.IsFalse(sv0.EquivalentTo(sv1), "Equivalence incorrect (0).");
            Assert.IsFalse(sv1.EquivalentTo(sv2), "Equivalence incorrect (1).");
            Assert.IsFalse(sv2.EquivalentTo(sv3), "Equivalence incorrect (2).");

            // Now we want to make sure that equivalence is working in both
            // directs (e.g. A = B and B = A).
            Assert.IsTrue(sv0.EquivalentTo(sv2) && sv2.EquivalentTo(sv0),
                          "Equivalence not commutative (0).");
            Assert.IsTrue(sv1.EquivalentTo(sv3) && sv3.EquivalentTo(sv1),
                          "Equivalence not commutative (1).");

            // And we might as well check the same for inequivalence.
            Assert.IsTrue(!sv0.EquivalentTo(sv1) && !sv1.EquivalentTo(sv0),
                          "Inequivalence not commutative (0).");
            Assert.IsTrue(!sv1.EquivalentTo(sv2) && !sv2.EquivalentTo(sv1),
                          "Inequivalence not commutative (1).");
            Assert.IsTrue(!sv2.EquivalentTo(sv3) && !sv3.EquivalentTo(sv2),
                          "Inequivalence not commutative (2).");
        }
        /// <summary>
        /// <para>
        /// Tests to make sure that the behaviour of the
        ///  <see cref="SemanticVersion.CompatibleWith(SemanticVersion)"/>
        /// method is as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Compatibility()
        {
            // Backwards-compatibility determination is not commutative,
            // so a few of these tests will just be reversed parameters.

            #region "Always False" checks (numbers 0 to 10)
            {
                // First thing we're going to do is a basic test of the "always
                // false" conditions listed in the remarks for [CompatibleWith].
                // These are:
                //
                //      - Major version of zero on either version
                //      - Different Major versions
                //      - Null [SemanticVersion] instance
                //
                // We can't really test here that these are *always* true, but
                // that doesn't mean we shouldn't try testing them anyway.
                var af_sv0 = new SemanticVersion(0, 5, 0);
                var af_sv1 = new SemanticVersion(0, 6, 0);
                var af_sv2 = new SemanticVersion(1, 2, 0);
                var af_sv3 = new SemanticVersion(2, 2, 0);
                var af_sv4 = (SemanticVersion)null;

                // Zero major versions
                Assert.IsFalse(af_sv0.CompatibleWith(af_sv1),
                               "Incorrect compatibility (0).");
                Assert.IsFalse(af_sv1.CompatibleWith(af_sv0),
                               "Incorrect compatibility (1).");
                Assert.IsFalse(af_sv1.CompatibleWith(af_sv2),
                               "Incorrect compatibility (2).");
                Assert.IsFalse(af_sv2.CompatibleWith(af_sv1),
                               "Incorrect compatibility (3).");
                // Zero major versions, equivalent versions
                Assert.IsTrue(af_sv1.CompatibleWith(af_sv1),
                               "Incorrect compatibility (4).");

                // Different major versions
                Assert.IsFalse(af_sv2.CompatibleWith(af_sv3),
                               "Incorrect compatibility (5).");
                Assert.IsFalse(af_sv3.CompatibleWith(af_sv2),
                               "Incorrect compatibility (6).");

                // Null version
                Assert.IsFalse(af_sv0.CompatibleWith(af_sv4),
                               "Incorrect compatibility (7).");
                Assert.IsFalse(af_sv1.CompatibleWith(af_sv4),
                               "Incorrect compatibility (8).");
                Assert.IsFalse(af_sv2.CompatibleWith(af_sv4),
                               "Incorrect compatibility (9).");
                Assert.IsFalse(af_sv3.CompatibleWith(af_sv4),
                               "Incorrect compatibility (10).");
            }
            #endregion
            #region Pre-release Versions (numbers 11 to 22)
            {
                // Pre-release versions are a bit more difficult with
                // their compatibility. See [CompatibleWith] remarks
                // and code comments for more information.

                // No pre-release identifiers for the "control" comparison.
                //
                // This should be compatible with [pr_sv3] and [pr_sv4] as
                // they are both pre-release versions of versions later than
                // this one, so they should (for standards compliance) implement
                // the APIs this depends on.
                var pr_sv0 = new SemanticVersion(1, 0, 0);

                // These two can't be compatible because they are equal in all
                // but pre-release identifiers.
                var pr_sv1 = new SemanticVersion(1, 0, 0, new[] { "rc", "1" });
                var pr_sv2 = new SemanticVersion(1, 0, 0, new[] { "rc", "2" });

                Assert.IsFalse(pr_sv0.CompatibleWith(pr_sv1),
                               "Incorrect compatibility (11).");
                Assert.IsFalse(pr_sv1.CompatibleWith(pr_sv0),
                               "Incorrect compatibility (12).");

                Assert.IsFalse(pr_sv0.CompatibleWith(pr_sv2),
                               "Incorrect compatibility (13).");
                Assert.IsFalse(pr_sv2.CompatibleWith(pr_sv0),
                               "Incorrect compatibility (14).");

                Assert.IsFalse(pr_sv1.CompatibleWith(pr_sv2),
                               "Incorrect compatibility (15).");
                Assert.IsFalse(pr_sv2.CompatibleWith(pr_sv1),
                               "Incorrect compatibility (16).");

                // These two can't be compatible because they are both pre-release
                // versions.
                var pr_sv3 = new SemanticVersion(1, 1, 0, new[] { "rc" });
                var pr_sv4 = new SemanticVersion(1, 2, 0, new[] { "rc" });

                Assert.IsFalse(pr_sv3.CompatibleWith(pr_sv4),
                               "Incorrect compatibility (17).");
                Assert.IsFalse(pr_sv4.CompatibleWith(pr_sv3),
                               "Incorrect compatibility (18).");

                Assert.IsTrue(pr_sv0.CompatibleWith(pr_sv3),
                               "Incorrect compatibility (19).");
                Assert.IsFalse(pr_sv3.CompatibleWith(pr_sv0),
                               "Incorrect compatibility (20).");

                Assert.IsTrue(pr_sv0.CompatibleWith(pr_sv4),
                               "Incorrect compatibility (21).");
                Assert.IsFalse(pr_sv4.CompatibleWith(pr_sv0),
                               "Incorrect compatibility (22).");

            }
            #endregion
            #region Regular checks (numbers 23 to 30)
            {
                var sv0 = new SemanticVersion(1, 0, 0);
                var sv1 = new SemanticVersion(1, 1, 0);

                Assert.IsTrue(sv0.CompatibleWith(sv1),
                              "Incorrect compatibility (23).");
                Assert.IsFalse(sv1.CompatibleWith(sv0),
                               "Incorrect compatibility (24).");

                var sv2 = new SemanticVersion(2, 0, 0);
                var sv3 = new SemanticVersion(2, 2, 0);

                Assert.IsTrue(sv2.CompatibleWith(sv3),
                              "Incorrect compatibility (25).");
                Assert.IsFalse(sv3.CompatibleWith(sv2),
                               "Incorrect compatibility (26).");

                Assert.IsFalse(sv0.CompatibleWith(sv2),
                               "Incorrect compatibility (27).");
                Assert.IsFalse(sv1.CompatibleWith(sv2),
                               "Incorrect compatibility (28).");

                Assert.IsFalse(sv0.CompatibleWith(sv3),
                               "Incorrect compatibility (29).");
                Assert.IsFalse(sv1.CompatibleWith(sv3),
                               "Incorrect compatibility (30).");
            }
            #endregion
        }
    }
}
