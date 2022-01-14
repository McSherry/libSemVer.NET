// Copyright (c) 2015-20 Liam McSherry
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace McSherry.SemanticVersioning.Ranges
{
    /// <summary>
    /// <para>
    /// Provides tests for the base <see cref="VersionRange"/> functionality.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class RangeTests
    {
        private const string Category = "Version Range Base";


        /// <summary>
        /// <para>
        /// Tests that <see cref="VersionRange"/> implements the
        /// <see cref="VersionRange.IComparator"/> interface, which is to be
        /// used in implementing advanced range syntax.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ImplementsIComparator()
        {
            var vr = new VersionRange("=1.0.0");

            Assert.IsInstanceOfType(vr, typeof(VersionRange.IComparator));
        }

        /// <summary>
        /// <para>
        /// Tests the version range equals operator, including the implicit
        /// equals operator.
        /// Ensures that <see cref="VersionRange"/> implements the interface
        /// <see cref="VersionRange.IComparator"/>.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_Equals()
        {
            VersionRange im = new VersionRange("1.2.3"),
                         ex = new VersionRange("=1.2.3");

            void Test(string TID, VersionRange vr)
            {
                (string VID, SemanticVersion Version, bool Expected)[] vectors =
                {
                    ("T1", (SemanticVersion)"1.2.3",              true),
                    ("T2", (SemanticVersion)"1.2.3+meta.data",    true),

                    ("F1", (SemanticVersion)"1.2.2",          false),
                    ("F2", (SemanticVersion)"1.2.3-alpha",    false),
                    ("F3", (SemanticVersion)"1.2.4",          false),
                    ("F4", (SemanticVersion)"1.1.3",          false),
                    ("F5", (SemanticVersion)"1.3.3",          false),
                    ("F6", (SemanticVersion)"0.2.3",          false),
                    ("F7", (SemanticVersion)"2.2.3",          false),
                };

                foreach (var vector in vectors)
                {
                    Assert.AreEqual(
                        expected:   vector.Expected,
                        actual:     vr.SatisfiedBy(vector.Version),
                        message:    $"Failure: {TID}, vector {vector.VID}"
                        );
                }
            }

            Test("Implicit", im);
            Test("Explicit", ex);
        }
        /// <summary>
        /// <para>
        /// Tests the version range caret operator, which represents a range
        /// where the leftmost non-zero component of the major-minor-patch trio
        /// cannot change.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_Caret()
        {
            // Tests for ranges where the major version is non-zero
            var vr1 = new VersionRange("^1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.3",          true),
                ("T1.2", (SemanticVersion)"1.2.4",          true),
                ("T1.3", (SemanticVersion)"1.3.3",          true),
                ("T1.4", (SemanticVersion)"1.5.0+mta.dta",  true),

                ("F1.1", (SemanticVersion)"1.2.2",          false),
                ("F1.2", (SemanticVersion)"1.2.3-alpha",    false),
                ("F1.3", (SemanticVersion)"1.2.4-alpha",    false),
                ("F1.4", (SemanticVersion)"1.1.3",          false),
                ("F1.5", (SemanticVersion)"0.2.3",          false),
                ("F1.6", (SemanticVersion)"2.0.0",          false),
                ("F1.7", (SemanticVersion)"2.2.3",          false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges where the minor version is non-zero
            var vr2 = new VersionRange("^0.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"0.2.3",          true),
                ("T2.2", (SemanticVersion)"0.2.4",          true),
                ("T2.3", (SemanticVersion)"0.2.5+mta.dta",  true),

                ("F2.1", (SemanticVersion)"0.2.2",          false),
                ("F2.2", (SemanticVersion)"0.2.3-alpha",    false),
                ("F2.3", (SemanticVersion)"0.2.4-alpha",    false),
                ("F2.4", (SemanticVersion)"1.2.3",          false),
                ("F2.5", (SemanticVersion)"0.3.0",          false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges where the patch version is non-zero
            var vr3 = new VersionRange("^0.0.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors3 =
            {
                ("T3.1", (SemanticVersion)"0.0.5",          true),

                ("F3.1", (SemanticVersion)"0.0.6",          false),
                ("F3.2", (SemanticVersion)"0.0.4",          false),
                ("F3.3", (SemanticVersion)"0.1.0",          false),
                ("F3.4", (SemanticVersion)"1.0.0",          false),
                ("F3.5", (SemanticVersion)"0.1.6",          false),
                ("F3.6", (SemanticVersion)"1.0.6",          false),
                ("F3.7", (SemanticVersion)"0.0.5-alpha.2",  false),
                ("F3.8", (SemanticVersion)"0.0.6-alpha.2",  false),
            };

            foreach (var vector in vectors3)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr3.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers and where a
            // trio component other than the patch version is non-zero
            var vr4 = new VersionRange("^1.2.3-alpha.2");

            (string VID, SemanticVersion Version, bool Expected)[] vectors4 =
            {
                ("T4.1", (SemanticVersion)"1.2.3-alpha.2",  true),
                ("T4.2", (SemanticVersion)"1.2.3-alpha.3",  true),
                ("T4.3", (SemanticVersion)"1.2.3-beta",     true),
                ("T4.4", (SemanticVersion)"1.2.3-beta.2",   true),
                ("T4.5", (SemanticVersion)"1.2.3",          true),
                ("T4.6", (SemanticVersion)"1.2.4",          true),
                ("T4.7", (SemanticVersion)"1.3.0",          true),

                ("F4.1", (SemanticVersion)"1.2.2",          false),
                ("F4.2", (SemanticVersion)"1.2.3-alpha.1",  false),
            };

            foreach (var vector in vectors4)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr4.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers and where
            // the patch trio component is non-zero
            var vr5 = new VersionRange("^0.0.5-alpha.2");

            (string VID, SemanticVersion Version, bool Expected)[] vectors5 =
            {
                ("T5.1", (SemanticVersion)"0.0.5-alpha.3",  true),
                ("T5.2", (SemanticVersion)"0.0.5-beta",     true),
                ("T5.3", (SemanticVersion)"0.0.5-beta.2",   true),

                ("F5.1", (SemanticVersion)"0.0.6",          false),
                ("F5.2", (SemanticVersion)"0.0.6-alpha.2",  false),
            };

            foreach (var vector in vectors5)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr5.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests the version range tilde operator, which allows patch-level
        /// changes where a minor version is specified and minor-level changes
        /// where a minor version isn't specified.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_Tilde()
        {
            // Tests for ranges where a minor version is specified, with and
            // without a patch version specified
            var vr1a = new VersionRange("~1.2.3");
            var vr1b = new VersionRange("~1.2");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.3",  true),
                ("T1.2", (SemanticVersion)"1.2.4",  true),
                ("T1.3", (SemanticVersion)"1.2.99", true),


                ("F1.1", (SemanticVersion)"1.1.3",          false),
                ("F1.2", (SemanticVersion)"1.3.0",          false),
                ("F1.3", (SemanticVersion)"2.0.0",          false),
                ("F1.4", (SemanticVersion)"0.2.3",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.5", (SemanticVersion)"1.2.4-alpha.2",  false),
            };

            void Test1(string TID, VersionRange vr)
            {
                foreach (var vector in vectors1)
                {
                    Assert.AreEqual(
                        expected:   vector.Expected,
                        actual:     vr.SatisfiedBy(vector.Version),
                        message:    $"Failure: {TID}, vector {vector.VID}"
                        );
                }
            }

            Test1("Patch present", vr1a);
            Test1("Patch omitted", vr1b);


            // Tests for ranges where a minor version isn't specified
            var vr2 = new VersionRange("~1");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.0.0",  true),
                ("T2.2", (SemanticVersion)"1.1.0",  true),
                ("T2.3", (SemanticVersion)"1.99.0", true),


                ("F2.1", (SemanticVersion)"0.9.0",          false),
                ("F2.2", (SemanticVersion)"2.0.0",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F2.3", (SemanticVersion)"1.1.0-alpha.2",  false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr3 = new VersionRange("~1.2.3-alpha.2");

            (string VID, SemanticVersion Version, bool Expected)[] vectors3 =
            {
                ("T3.1", (SemanticVersion)"1.2.3-alpha.2",  true),
                ("T3.2", (SemanticVersion)"1.2.3-alpha.3",  true),
                ("T3.3", (SemanticVersion)"1.2.3-beta.2",   true),
                ("T3.4", (SemanticVersion)"1.2.3",          true),
                ("T3.5", (SemanticVersion)"1.2.4",          true),
                ("T3.6", (SemanticVersion)"1.2.99",         true),

                ("F3.1", (SemanticVersion)"1.2.3-alpha.1",  false),
                ("F3.2", (SemanticVersion)"1.2.4-alpha.2",  false),
                ("F3.3", (SemanticVersion)"1.3.0",          false),
                ("F3.4", (SemanticVersion)"1.2.2",          false),
                ("F3.5", (SemanticVersion)"2.2.4",          false),
                ("F3.6", (SemanticVersion)"0.2.4",          false),
            };

            foreach (var vector in vectors3)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr3.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests the version range greater-than operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_GreaterThan()
        {
            // Tests for ranges not including pre-release identifiers
            var vr1 = new VersionRange(">1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.4",          true),
                ("T1.2", (SemanticVersion)"1.3.0",          true),
                ("T1.3", (SemanticVersion)"2.0.0",          true),
                ("T1.4", (SemanticVersion)"2.0.0+ab.cd",    true),


                ("F1.1", (SemanticVersion)"1.2.3",          false),
                ("F1.2", (SemanticVersion)"1.2.2",          false),
                ("F1.3", (SemanticVersion)"1.1.0",          false),
                ("F1.4", (SemanticVersion)"0.2.4",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.5", (SemanticVersion)"1.2.4-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr2 = new VersionRange(">1.2.3-alpha.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.6",      true),
                ("T2.2", (SemanticVersion)"1.2.3-alpha.6.7",    true),
                ("T2.3", (SemanticVersion)"1.2.3-beta",         true),
                ("T2.4", (SemanticVersion)"1.2.3-alpha.a",      true),
                ("T2.5", (SemanticVersion)"1.2.4",              true),


                ("F2.1", (SemanticVersion)"1.2.3-alpha.4",  false),
                ("F2.2", (SemanticVersion)"1.2.3-alpha",    false),
                ("F2.3", (SemanticVersion)"1.2.4-alpha.2",  false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests the version range less-than operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_LessThan()
        {
            // Tests for ranges not including pre-release identifiers
            var vr1 = new VersionRange("<1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.2",          true),
                ("T1.2", (SemanticVersion)"1.1.0",          true),
                ("T1.3", (SemanticVersion)"0.5.0",          true),
                ("T1.4", (SemanticVersion)"0.5.0+ab.cd",    true),


                ("F1.1", (SemanticVersion)"1.2.3",          false),
                ("F1.2", (SemanticVersion)"1.2.4",          false),
                ("F1.3", (SemanticVersion)"1.3.0",          false),
                ("F1.4", (SemanticVersion)"2.5.0",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.5", (SemanticVersion)"1.2.4-alpha",    false),
                ("F1.6", (SemanticVersion)"1.2.3-alpha",    false),
                ("F1.7", (SemanticVersion)"1.2.2-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr2 = new VersionRange("<1.2.3-alpha.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.4",  true),
                ("T2.2", (SemanticVersion)"1.2.3-alph",     true),
                ("T2.3", (SemanticVersion)"1.2.3-5678",     true),
                ("T2.4", (SemanticVersion)"1.2.3-5678+abc", true),
                ("T2.5", (SemanticVersion)"1.2.2",          true),

                ("F2.2", (SemanticVersion)"1.2.3-alpha.6",      false),
                ("F2.3", (SemanticVersion)"1.2.3-alpha.5.6",    false),
                ("F2.3", (SemanticVersion)"1.2.3-beta.2",       false),
                ("F2.4", (SemanticVersion)"1.2.2-beta.2",       false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests the version range greater-than-or-equal operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_GreaterThanOrEqual()
        {
            // Tests for ranges not including pre-release identifiers
            var vr1 = new VersionRange(">=1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.3",          true),
                ("T1.2", (SemanticVersion)"1.2.4",          true),
                ("T1.3", (SemanticVersion)"1.3.0",          true),
                ("T1.4", (SemanticVersion)"2.0.0",          true),
                ("T1.5", (SemanticVersion)"2.0.0+ab.cd",    true),


                ("F1.1", (SemanticVersion)"1.2.2",          false),
                ("F1.2", (SemanticVersion)"1.1.0",          false),
                ("F1.3", (SemanticVersion)"0.2.4",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.4", (SemanticVersion)"1.2.4-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr2 = new VersionRange(">=1.2.3-alpha.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.5",      true),
                ("T2.2", (SemanticVersion)"1.2.3-alpha.6",      true),
                ("T2.3", (SemanticVersion)"1.2.3-alpha.6.7",    true),
                ("T2.4", (SemanticVersion)"1.2.3-alpha.a",      true),
                ("T2.5", (SemanticVersion)"1.2.4",              true),

                ("F2.1", (SemanticVersion)"1.2.3-alpha.4",      false),
                ("F2.2", (SemanticVersion)"1.2.3-alpha",        false),
                ("F2.3", (SemanticVersion)"1.2.4-alpha.2",      false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests the version range less-than-or-equal operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_LessThanOrEqual()
        {
            // Tests for ranges not including pre-release identifiers
            var vr1 = new VersionRange("<=1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.3",          true),
                ("T1.2", (SemanticVersion)"1.2.2",          true),
                ("T1.3", (SemanticVersion)"1.1.0",          true),
                ("T1.4", (SemanticVersion)"0.5.0",          true),
                ("T1.5", (SemanticVersion)"0.5.0+ab.cd",    true),


                ("F1.1", (SemanticVersion)"1.2.4",          false),
                ("F1.2", (SemanticVersion)"1.3.0",          false),
                ("F1.3", (SemanticVersion)"2.5.0",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.4", (SemanticVersion)"1.2.4-alpha",    false),
                ("F1.5", (SemanticVersion)"1.2.3-alpha",    false),
                ("F1.6", (SemanticVersion)"1.2.2-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr2 = new VersionRange("<=1.2.3-alpha.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.5",  true),
                ("T2.2", (SemanticVersion)"1.2.3-alpha.4",  true),
                ("T2.3", (SemanticVersion)"1.2.3-alph",     true),
                ("T2.4", (SemanticVersion)"1.2.3-5678",     true),
                ("T2.5", (SemanticVersion)"1.2.3-5678+abc", true),
                ("T2.6", (SemanticVersion)"1.2.2",          true),

                ("F2.2", (SemanticVersion)"1.2.3-alpha.6",      false),
                ("F2.3", (SemanticVersion)"1.2.3-alpha.5.6",    false),
                ("F2.3", (SemanticVersion)"1.2.3-beta.2",       false),
                ("F2.4", (SemanticVersion)"1.2.2-beta.2",       false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }

        /// <summary>
        /// <para>
        /// Tests the version range hyphen operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_Hyphen()
        {
            // Tests for the general case, where the versions on both sides of
            // the operator have all their version components specified
            //
            // '1.2.3 - 2.3.4' --> '>=1.2.3 <=2.3.4'
            //
            var vr1 = new VersionRange("1.2.3 - 2.3.4");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.3",  true),
                ("T1.2", (SemanticVersion)"1.2.4",  true),
                ("T1.3", (SemanticVersion)"1.3.2",  true),
                ("T1.4", (SemanticVersion)"2.3.4",  true),
                ("T1.5", (SemanticVersion)"2.3.3",  true),
                ("T1.6", (SemanticVersion)"2.1.5",  true),

                ("F1.1", (SemanticVersion)"1.1.4",          false),
                ("F1.2", (SemanticVersion)"1.2.2",          false),
                ("F1.3", (SemanticVersion)"2.3.5",          false),
                ("F1.4", (SemanticVersion)"2.4.3",          false),
                ("F1.5", (SemanticVersion)"1.2.3-alpha.2",  false),
                ("F1.6", (SemanticVersion)"1.2.4-alpha.3",  false),
                ("F1.7", (SemanticVersion)"2.3.4-alpha.4",  false),
                ("F1.8", (SemanticVersion)"2.4.3-alpha.5",  false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for the case where the version on the left-hand side has
            // components omitted
            //
            // '1.2 - 2.4.0' --> '>=1.2.0 <=2.4.0'
            var vr2a = new VersionRange("1.2.0 - 2.4.0");
            var vr2b = new VersionRange("1.2 - 2.4.0");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.0",  true),
                ("T2.2", (SemanticVersion)"1.2.1",  true),
                ("T2.3", (SemanticVersion)"1.3.0",  true),
                ("T2.4", (SemanticVersion)"2.4.0",  true),
                ("T2.5", (SemanticVersion)"2.3.99", true),
                ("T2.6", (SemanticVersion)"2.2.2",  true),

                ("F2.1", (SemanticVersion)"1.1.99",         false),
                ("F2.2", (SemanticVersion)"2.4.1",          false),
                ("F2.3", (SemanticVersion)"0.5.0",          false),
                ("F2.4", (SemanticVersion)"3.2.1",          false),
                ("F2.5", (SemanticVersion)"1.2.0-alpha.2",  false),
                ("F2.6", (SemanticVersion)"1.2.1-alpha.3",  false),
                ("F2.7", (SemanticVersion)"2.4.0-alpha.4",  false),
                ("F2.8", (SemanticVersion)"2.2.0-alpha.5",  false),
                ("F2.9", (SemanticVersion)"2.5.0-alpha.6",  false),
            };

            void Test2(string TID, VersionRange vr)
            {
                foreach (var vector in vectors2)
                {
                    Assert.AreEqual(
                        expected:   vector.Expected,
                        actual:     vr.SatisfiedBy(vector.Version),
                        message:    $"Failure: {TID}, vector {vector.VID}"
                        );
                }
            }

            Test2("Patch present", vr2a);
            Test2("Patch omitted", vr2b);


            // Follow on where minor is omitted
            //
            // '1 - 2.4.0'   --> '>=1.0.0 <=2.4.0'
            //
            var vr3a = new VersionRange("1.0.0 - 1.6.0");
            var vr3b = new VersionRange("1.0 - 1.6.0");
            var vr3c = new VersionRange("1 - 1.6.0");

            (string VID, SemanticVersion Version, bool Expected)[] vectors3 =
            {
                ("T3.1", (SemanticVersion)"1.0.0",  true),
                ("T3.2", (SemanticVersion)"1.1.0",  true),
                ("T3.3", (SemanticVersion)"1.2.3",  true),
                ("T3.4", (SemanticVersion)"1.6.0",  true),
                ("T3.5", (SemanticVersion)"1.5.99", true),
                ("T3.6", (SemanticVersion)"1.3.62", true),

                ("F3.1", (SemanticVersion)"0.9.99",         false),
                ("F3.2", (SemanticVersion)"1.6.1",          false),
                ("F3.3", (SemanticVersion)"2.5.0",          false),
                ("F3.4", (SemanticVersion)"1.0.0-alpha.2",  false),
                ("F3.5", (SemanticVersion)"1.1.0-alpha.3",  false),
                ("F3.6", (SemanticVersion)"1.6.0-alpha.4",  false),
                ("F3.7", (SemanticVersion)"1.5.9-alpha.5",  false),
                ("F3.8", (SemanticVersion)"2.0.0-alpha.6",  false),
            };

            void Test3(string TID, VersionRange vr)
            {
                foreach (var vector in vectors3)
                {
                    Assert.AreEqual(
                        expected:   vector.Expected,
                        actual:     vr.SatisfiedBy(vector.Version),
                        message:    $"Failure: {TID}, vector {vector.VID}"
                        );
                }
            }

            Test3("Minor, Patch present", vr3a);
            Test3("Minor present, Patch omitted", vr3b);
            Test3("Minor, Patch omitted", vr3c);


            // Tests for the case where the patch version is omitted on the
            // right-hand side version.
            //
            // Omissions on the right-hand side mean that any comparand version
            // cannot have components which are greater than those specified.
            //
            // '1.0 - 2.5' --> '>=1.0.0 <2.6.0'
            //
            var vr4 = new VersionRange("1.0.0 - 2.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors4 =
            {
                ("T4.1", (SemanticVersion)"1.0.0",  true),
                ("T4.2", (SemanticVersion)"2.5.0",  true),
                ("T4.3", (SemanticVersion)"2.5.99", true),
                ("T4.4", (SemanticVersion)"2.4.1",  true),
                ("T4.5", (SemanticVersion)"1.7.8",  true),

                ("F4.1", (SemanticVersion)"0.9.99",         false),
                ("F4.2", (SemanticVersion)"2.6.0",          false),
                ("F4.3", (SemanticVersion)"3.1.0",          false),
                ("F4.4", (SemanticVersion)"1.0.0-alpha.2",  false),
                ("F4.5", (SemanticVersion)"2.6.0-alpha.3",  false),
                ("F4.6", (SemanticVersion)"1.3.5-alpha.4",  false),
            };

            foreach (var vector in vectors4)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr4.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for the case where the minor version is omitted on the
            // right-hand side version.
            //
            // '1.5.7 - 2' --> '>=1.5.7 <3.0.0'
            //
            var vr5 = new VersionRange("1.5.7 - 2");

            (string VID, SemanticVersion Version, bool Expected)[] vectors5 =
            {
                ("T5.1", (SemanticVersion)"1.5.7",  true),
                ("T5.2", (SemanticVersion)"1.5.8",  true),
                ("T5.3", (SemanticVersion)"1.6.1",  true),
                ("T5.4", (SemanticVersion)"2.5.3",  true),
                ("T5.5", (SemanticVersion)"2.9.99", true),
                ("T5.6", (SemanticVersion)"2.4.2",  true),

                ("F5.1",  (SemanticVersion)"1.5.6",         false),
                ("F5.2",  (SemanticVersion)"1.5.0",         false),
                ("F5.3",  (SemanticVersion)"1.4.8",         false),
                ("F5.4",  (SemanticVersion)"0.9.9",         false),
                ("F5.5",  (SemanticVersion)"3.0.0",         false),
                ("F5.6",  (SemanticVersion)"1.5.7-alpha.2", false),
                ("F5.7",  (SemanticVersion)"1.5.8-alpha.3", false),
                ("F5.8",  (SemanticVersion)"1.9.5-alpha.4", false),
                ("F5.9",  (SemanticVersion)"3.0.0-alpha.5", false),
                ("F5.10", (SemanticVersion)"2.5.5-alpha.6", false),
            };

            foreach (var vector in vectors5)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr5.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for the case where the left-hand version has pre-release
            // identifiers
            //
            // '1.3.2-alpha.2 - 1.4.0' --> '>=1.3.2-alpha.2 <=1.4.0'
            //
            var vr6 = new VersionRange("1.3.2-alpha.2 - 1.4.0");

            (string VID, SemanticVersion Version, bool Expected)[] vectors6 =
            {
                ("T6.1", (SemanticVersion)"1.3.2-alpha.2",  true),
                ("T6.2", (SemanticVersion)"1.3.2-alpha.3",  true),
                ("T6.3", (SemanticVersion)"1.3.3",          true),
                ("T6.4", (SemanticVersion)"1.4.0",          true),
                ("T6.5", (SemanticVersion)"1.3.99",         true),

                ("F6.1", (SemanticVersion)"1.3.2-alpha.1",  false),
                ("F6.2", (SemanticVersion)"1.3.3-alpha.2",  false),
                ("F6.3", (SemanticVersion)"1.4.0-alpha.3",  false),
                ("F6.4", (SemanticVersion)"1.3.1",          false),
                ("F6.5", (SemanticVersion)"1.3.1-alpha.4",  false),
                ("F6.6", (SemanticVersion)"1.4.1",          false),
                ("F6.7", (SemanticVersion)"1.4.1-alpha.5",  false),
                ("F6.8", (SemanticVersion)"2.3.2",          false),
                ("F6.9", (SemanticVersion)"2.3.2-alpha.6",  false),
            };

            foreach (var vector in vectors6)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr6.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for the case where the right-hand version has pre-release
            // identifiers
            //
            // '2.1 - 2.3.1-alpha.2' --> '>=2.1.0 <=2.3.1-alpha.2'
            //
            var vr7 = new VersionRange("2.1 - 2.3.1-alpha.2");

            (string VID, SemanticVersion Version, bool Expected)[] vectors7 =
            {
                ("T7.1", (SemanticVersion)"2.1.0",          true),
                ("T7.2", (SemanticVersion)"2.1.1",          true),
                ("T7.3", (SemanticVersion)"2.2.99",         true),
                ("T7.4", (SemanticVersion)"2.3.1-alpha.2",  true),
                ("T7.5", (SemanticVersion)"2.3.1-alpha.1",  true),
                ("T7.6", (SemanticVersion)"2.3.0",          true),

                ("F7.1", (SemanticVersion)"2.0.999",        false),
                ("F7.2", (SemanticVersion)"1.2.5",          false),
                ("F7.3", (SemanticVersion)"2.3.1-alpha.3",  false),
                ("F7.4", (SemanticVersion)"2.3.1",          false),
                ("F7.5", (SemanticVersion)"2.2.5-alpha.4",  false),
                ("F7.6", (SemanticVersion)"2.3.0-alpha.1",  false),
            };

            foreach (var vector in vectors7)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr7.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests wildcards within version ranges, called 'X-ranges' by the
        /// 'node-semver' specification.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_Wildcard()
        {
            // Tests for the case where the patch version is a wildcard
            var vr1a = new VersionRange("1.2.x");
            var vr1b = new VersionRange("1.2.X");
            var vr1c = new VersionRange("1.2.*");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.0",  true),
                ("T1.2", (SemanticVersion)"1.2.1",  true),
                ("T1.3", (SemanticVersion)"1.2.99", true),

                ("F1.1", (SemanticVersion)"1.1.99",         false),
                ("F1.2", (SemanticVersion)"1.3.0",          false),
                ("F1.3", (SemanticVersion)"0.2.5",          false),
                ("F1.4", (SemanticVersion)"2.2.5",          false),
                ("F1.5", (SemanticVersion)"1.2.6-alpha.2",  false),
                ("F1.6", (SemanticVersion)"1.3.0-alpha.3",  false),
            };

            void Test1(string TID, VersionRange vr)
            {
                foreach (var vector in vectors1)
                {
                    Assert.AreEqual(
                        expected:   vector.Expected,
                        actual:     vr.SatisfiedBy(vector.Version),
                        message:    $"Failure: {TID}, vector {vector.VID}"
                        );
                }
            }

            Test1("Lowercase", vr1a);
            Test1("Uppercase", vr1b);
            Test1("Asterisk", vr1c);


            // Tests for the case where the minor version is a wildcard
            var vr2a = new VersionRange("1.x");
            var vr2b = new VersionRange("1.X");
            var vr2c = new VersionRange("1.*");
            var vr2d = new VersionRange("1.*.*");
            var vr2e = new VersionRange("1.x.x");
            var vr2f = new VersionRange("1.X.X");
            var vr2g = new VersionRange("1.x.X");
            var vr2h = new VersionRange("1.*.X");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.0.0",  true),
                ("T2.2", (SemanticVersion)"1.2.0",  true),
                ("T2.3", (SemanticVersion)"1.99.0", true),
                ("T2.4", (SemanticVersion)"1.7.2",  true),

                ("F2.1", (SemanticVersion)"0.9.99",         false),
                ("F2.2", (SemanticVersion)"2.0.0",          false),
                ("F2.3", (SemanticVersion)"0.6.4",          false),
                ("F2.4", (SemanticVersion)"3.1.9",          false),
                ("F2.5", (SemanticVersion)"1.6.8-alpha.2",  false),
                ("F2.6", (SemanticVersion)"2.0.0-alpha.3",  false),
            };

            void Test2(string TID, VersionRange vr)
            {
                foreach (var vector in vectors2)
                {
                    Assert.AreEqual(
                        expected:   vector.Expected,
                        actual:     vr.SatisfiedBy(vector.Version),
                        message:    $"Failure {TID}, vector {vector.VID}"
                        );
                }
            }

            Test2("Lowercase", vr2a);
            Test2("Uppercase", vr2b);
            Test2("Asterisk", vr2c);
            Test2("Asterisk, redundant", vr2d);
            Test2("Lowercase, redundant", vr2e);
            Test2("Uppercase, redundant", vr2f);
            Test2("Mixed (xX), redundant", vr2g);
            Test2("Mixed (*X), redundant", vr2h);


            // Tests for the case where the major version is a wildcard
            //
            // The major version being a wildcard should result in any and
            // every version, other than those with pre-release identifers, being
            // accepted. We'll generate a number of random versions and see if
            // anything falls over.
            //
            // If something does fall over, we'll need to add a specific test
            // for it as we're not guaranteed to get the same value again.

            var vr3 = new (string Name, VersionRange Range)[]
            {
                ("Asterisk, one", new VersionRange("*")),
                ("Lowercase, one", new VersionRange("x")),
                ("Uppercase, one", new VersionRange("X")),
                ("Asterisk, two", new VersionRange("*.*")),
                ("Lowercase, three", new VersionRange("x.x.x")),
                ("Mixed, two", new VersionRange("x.*")),
                ("Mixed, three", new VersionRange("x.*.X")),
            };

            var rng = new Random();

            // Anything without pre-release identifiers should be true
            for (int i = 0; i < 1000; i++)
            {
                var sv = new SemanticVersion(
                    major: rng.Next(),
                    minor: rng.Next(),
                    patch: rng.Next()
                    );

                foreach (var vr in vr3)
                {
                    Assert.IsTrue(
                        condition: vr.Range.SatisfiedBy(sv),
                        message: $"Failure, {vr.Name}, T3: {sv}"
                        );
                }
            }

            // Anything with them should be false
            for (int i = 0; i < 1000; i++)
            {
                SemanticVersion sv = null;

                var major = rng.Next();
                var minor = rng.Next();
                var patch = rng.Next();
                var ids = GetRandomIdentifiers();

                try
                {
                    sv = new SemanticVersion(major, minor, patch, ids);
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        $"Parse failure, F3: {ex}\n\n" +
                        $"{major}.{minor}.{patch}, identifiers: " +
                        ids.Aggregate(new StringBuilder(), (sb, id) => sb.Append($"{id}, "))
                           .ToString()
                        );
                }

                foreach (var vr in vr3)
                {
                    Assert.IsFalse(
                        condition: vr.Range.SatisfiedBy(sv),
                        message: $"Failure, {vr.Name}, F3: {sv}"
                        );
                }
            }

            IEnumerable<string> GetRandomIdentifiers()
            {
                // We're going to generate a random number of identifiers, but
                // with a reasonable limit so we don't generate stupidly long
                // strings.
                //
                // [Next] with a maximum specified is exclusive.
                var noIdentifiers = rng.Next(1, 65);

                var sb = new StringBuilder();

                var alphabet = new char[63]
                {
                    '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
                    'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
                    'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
                    'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
                    'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
                    'y', 'z', '-'
                };

                for (int i = 0; i < noIdentifiers; i++)
                {
                    // And then a random length for each identifier, capped to
                    // what seems like a reasonable maximum
                    var len = rng.Next(1, 256);

                    // Each character in the identifier being a random ASCII
                    // character in [0-9A-Za-z-], avoiding leading zeroes.
                    for (int j = 0; j < len; j++)
                    {
                        sb.Append(
                            alphabet[rng.Next(alphabet.Length)]
                            );
                    }

                    string s = sb.ToString();

                    // Trim any leading zeroes
                    if (len > 1)
                    {
                        s = s.TrimStart('0');

                        // It's possible that we could generate a string of
                        // only '0' characters, which would be invalid
                        if (s.Length == 0)
                            continue;
                    }

                    yield return s;

                    sb.Clear();
                }
            }


            // Tests to ensure that wildcards with other operators are not
            // accepted by the version range parser
            (string VID, string Range)[] vectors4 =
            {
                ("V4.1",  "=1.0.x"),
                ("V4.2",  "=1.X"),
                ("V4.3",  "=*"),

                ("V4.4",  ">2.1.*"),
                ("V4.5",  ">2.x"),
                ("V4.6",  ">X"),

                ("V4.7",  "<3.2.X"),
                ("V4.8",  "<3.*"),
                ("V4.9",  "<x"),

                ("V4.10", ">=4.3.x"),
                ("V4.11", ">=4.x"),
                ("V4.12", ">=x"),

                ("V4.13", "<=5.4.x"),
                ("V4.14", "<=5.x"),
                ("V4.15", "<=x"),

                ("V4.16", "~6.5.x"),
                ("V4.17", "~6.x"),
                ("V4.18", "~x"),

                ("V4.19", "^7.6.x"),
                ("V4.20", "^7.x"),
                ("V4.21", "^x"),

                ("V4.22", "8.7.x - 9.8.7"),
                ("V4.23", "8.7.6 - 9.8.x"),
                ("V4.24", "8.x - 9.8.7"),
                ("V4.25", "8.7 - 9.x"),
                ("V4.26", "x - 9.8.7"),
                ("V4.27", "8 - x"),
                ("V4.28", "8.7.x - 9.8.x"),
                ("V4.29", "8.x - 9.x"),
                ("V4.30", "x - x"),
            };

            foreach (var vector in vectors4)
            {
                Assert.ThrowsException<FormatException>(
                    action:     () => new VersionRange(vector.Range),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }

        /// <summary>
        /// <para>
        /// Tests version range comparison using a single comparator set with
        /// multiple comparators in it.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MultipleComparators()
        {
            // Most of this test is taken from the README of the 'node-semver'
            // repository, because what better source than the horse's mouth?

            var vr1 = new VersionRange(">=1.2.7 <1.3.0");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.7",  true),
                ("T1.2", (SemanticVersion)"1.2.8",  true),
                ("T1.3", (SemanticVersion)"1.2.99", true),

                ("F1.1", (SemanticVersion)"1.2.6",          false),
                ("F1.2", (SemanticVersion)"1.3.0",          false),
                ("F1.3", (SemanticVersion)"1.1.0",          false),
                ("F1.4", (SemanticVersion)"1.2.8-alpha",    false),
                ("F1.5", (SemanticVersion)"1.3.0-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Test to ensure that pre-release versions in comparators are 
            // handled correctly.
            var vr2 = new VersionRange(">=1.2.3-alpha.2 <1.5.0");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.3",  true),
                ("T2.2", (SemanticVersion)"1.2.4",          true),
                ("T2.3", (SemanticVersion)"1.3.0",          true),

                ("F2.1", (SemanticVersion)"1.2.3-alpha.1",  false),
                ("F2.2", (SemanticVersion)"1.2.4-alpha",    false),
                ("F2.3", (SemanticVersion)"1.5.0",          false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests to ensure wildcards are handled correctly
            var vr3 = new VersionRange("1.5.x >1.5.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors3 =
            {
                ("T3.1", (SemanticVersion)"1.5.6",  true),
                ("T3.2", (SemanticVersion)"1.5.99", true),

                ("F3.1",    (SemanticVersion)"1.5.0",           false),
                ("F3.2",    (SemanticVersion)"1.5.1",           false),
                ("F3.3",    (SemanticVersion)"1.5.2",           false),
                ("F3.4",    (SemanticVersion)"1.5.3",           false),
                ("F3.5",    (SemanticVersion)"1.5.4",           false),
                ("F3.6",    (SemanticVersion)"1.5.5",           false),
                ("F3.7",    (SemanticVersion)"1.4.99",          false),
                ("F3.8",    (SemanticVersion)"1.7.0",           false),
                ("F3.9",    (SemanticVersion)"1.6.0",           false),
                ("F3.10",   (SemanticVersion)"1.6.99",          false),
                ("F3.11",   (SemanticVersion)"0.5.4",           false),
                ("F3.12",   (SemanticVersion)"1.5.0-alpha.2",   false),
                ("F3.13",   (SemanticVersion)"1.7.1-alpha.3",   false),
                ("F3.14",   (SemanticVersion)"3.2.2-alpha.4",   false),
            };

            foreach (var vector in vectors3)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr3.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests to ensure binary comparators are handled correctly
            var vr4 = new VersionRange("1.2.0 - 1.3.0 <1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors4 =
            {
                ("T4.1", (SemanticVersion)"1.2.0",  true),
                ("T4.2", (SemanticVersion)"1.2.1",  true),
                ("T4.3", (SemanticVersion)"1.2.2",  true),
                ("T4.4", (SemanticVersion)"1.0.99", true),
                ("T4.5", (SemanticVersion)"1.0.0",  true),
                ("T4.6", (SemanticVersion)"0.6.2",  true),

                ("F4.1",    (SemanticVersion)"1.3.0",           false),
                ("F4.2",    (SemanticVersion)"1.2.3",           false),
                ("F4.3",    (SemanticVersion)"1.2.6",           false),
                ("F4.4",    (SemanticVersion)"1.1.0",           false),
                ("F4.5",    (SemanticVersion)"1.1.7",           false),
                ("F4.6",    (SemanticVersion)"1.4.9",           false),
                ("F4.7",    (SemanticVersion)"2.5.2",           false),
                ("F4.8",    (SemanticVersion)"1.3.0-alpha.2",   false),
                ("F4.9",    (SemanticVersion)"1.1.0-alpha.3",   false),
                ("F4.10",   (SemanticVersion)"1.2.7-alpha.4",   false),
                ("F4.11",   (SemanticVersion)"1.0.6-alpha.5",   false),
                ("F4.12",   (SemanticVersion)"0.9.7-alpha.6",   false),
            };
        }
        /// <summary>
        /// <para>
        /// Tests version range comparison using multiple comparator sets.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MultipleComparatorSets()
        {
            // Like in the [MultipleComparators] test, this test is straight
            // from the 'node-semver' README, and there's only a single test
            // because individual comparators and multiple comparators are
            // already tested.
            var vr1 = new VersionRange("1.2.7 || >=1.2.9 <2.0.0");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.7",  true),
                ("T1.2", (SemanticVersion)"1.2.9",  true),
                ("T1.3", (SemanticVersion)"1.4.6",  true),

                ("F1.1", (SemanticVersion)"1.2.8",          false),
                ("F1.2", (SemanticVersion)"2.0.0",          false),
                ("F1.3", (SemanticVersion)"1.3.0-alpha.2",  false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Test that a pre-release version in one comparator set does not
            // allow the other comparator set to be satisfied.
            var vr2 = new VersionRange(">1.2.3-alpha.2 <1.3.0 || <1.1.0");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.3",  true),
                ("T2.2", (SemanticVersion)"1.2.4",          true),
                ("T2.3", (SemanticVersion)"1.0.5",          true),

                ("F2.1", (SemanticVersion)"1.2.3-alpha.2",  false),
                ("F2.2", (SemanticVersion)"1.3.0",          false),
                ("F2.3", (SemanticVersion)"1.1.0",          false),
                ("F2.4", (SemanticVersion)"1.1.0-alpha",    false),
                ("F2.5", (SemanticVersion)"1.0.6-alpha",    false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests that a wildcard comparator in one set works as expected.
            var vr3 = new VersionRange("1.7.x || 1.8.x");

            (string VID, SemanticVersion Version, bool Expected)[] vectors3 =
            {
                ("T3.1", (SemanticVersion)"1.7.0",  true),
                ("T3.2", (SemanticVersion)"1.7.1",  true),
                ("T3.3", (SemanticVersion)"1.7.99", true),
                ("T3.4", (SemanticVersion)"1.8.0",  true),
                ("T3.5", (SemanticVersion)"1.8.2",  true),

                ("F3.1", (SemanticVersion)"1.6.99",         false),
                ("F3.2", (SemanticVersion)"2.0.0",          false),
                ("F3.3", (SemanticVersion)"2.7.2",          false),
                ("F3.4", (SemanticVersion)"0.8.9",          false),
                ("F3.5", (SemanticVersion)"1.7.0-alpha.2",  false),
                ("F3.6", (SemanticVersion)"1.8.0-alpha.3",  false),
                ("F3.7", (SemanticVersion)"1.9.6-alpha.4",  false),
                ("F3.8", (SemanticVersion)"2.0.0-alpha.5",  false),
            };

            foreach (var vector in vectors3)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr3.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests that a hyphen operator in one set works as expected
            var vr4 = new VersionRange("1.2.0 - 1.6.0 || 2.0.X");

            (string VID, SemanticVersion Version, bool Expected)[] vectors4 =
            {
                ("T4.1", (SemanticVersion)"1.2.0",  true),
                ("T4.2", (SemanticVersion)"1.3.0",  true),
                ("T4.3", (SemanticVersion)"1.6.0",  true),
                ("T4.4", (SemanticVersion)"2.0.0",  true),
                ("T4.5", (SemanticVersion)"2.0.6",  true),
                ("T4.6", (SemanticVersion)"2.0.99", true),

                ("F4.1",    (SemanticVersion)"1.1.99",          false),
                ("F4.2",    (SemanticVersion)"1.6.1",           false),
                ("F4.3",    (SemanticVersion)"1.7.2",           false),
                ("F4.4",    (SemanticVersion)"2.1.0",           false),
                ("F4.5",    (SemanticVersion)"0.4.3",           false),
                ("F4.6",    (SemanticVersion)"2.2.4",           false),
                ("F4.7",    (SemanticVersion)"1.2.0-alpha.2",   false),
                ("F4.8",    (SemanticVersion)"1.6.0-alpha.3",   false),
                ("F4.9",    (SemanticVersion)"2.0.0-alpha.4",   false),
                ("F4.10",   (SemanticVersion)"2.1.0-alpha.5",   false),
            };

            foreach (var vector in vectors4)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr4.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }

        /// <summary>
        /// Tests that <see cref="VersionRange.MemoizationAgent"/> is used when
        /// assigned to, and that the correct values are assigned.
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Memoize_General()
        {
            var dict = new Dictionary<SemanticVersion, bool>();
            var vr = new VersionRange(">1.5.0");

            vr.MemoizationAgent = dict;


            // True evaluation
            var sv0 = SemanticVersion.Parse("1.5.1");
            var rs0 = vr.SatisfiedBy(sv0);

            Assert.IsTrue(dict.ContainsKey(sv0));
            Assert.IsTrue(rs0);
            Assert.AreEqual(rs0, dict[sv0]);
            Assert.AreEqual(1, dict.Count);


            // False evaluation
            var sv1 = SemanticVersion.Parse("1.4.0");
            var rs1 = vr.SatisfiedBy(sv1);

            Assert.IsTrue(dict.ContainsKey(sv1));
            Assert.IsFalse(rs1);
            Assert.AreEqual(rs1, dict[sv1]);
            Assert.AreEqual(2, dict.Count);
        }


        // When adding to [CompareTo] test vectors, remember to add the vector and
        // its corresponding versions to all tests to ensure consistency.

        [DataRow("1.7.0",           "1.7.1; 1.8; 2.0.1")]
        [DataRow("=1.7.0",          "1.7.1; 1.8; 2.0.1")]
        [DataRow("<1.7.0",          "1.7; 1.7.1; 1.8; 2.0.1")]
        [DataRow("<=1.7.0",         "1.7.1; 1.8; 2.0.1")]
        [DataRow(">1.7.0 <2.0.1",   "2.0.2; 2.1; 3.9.10")]
        [DataRow(">1.7.0 <1.7.8",   "1.7.9; 2.0.1; 2.1; 3.9.10")]
        [DataRow(">1.7 <1.8 || ~2.0", "2.1.0")]
        [DataRow("1.7.0 - 2.0.1",   "2.0.2; 2.1")]
        [DataRow("1.7 - 2.0.1",     "2.0.2; 2.1")]
        [DataRow("1.7.x",           "1.8; 2.0.1")]
        [DataRow("1.x",             "2.0.1; 4.9.10")]
        [DataRow("~1.7.0",          "1.8; 2.0.1")]
        [DataRow("~1.7",            "2.0.1; 4.9.10")]
        [DataRow("~1",              "2.0; 2.0.1")]
        [DataRow("^1.7.0",          "2.0; 2.0.1")]
        [DataRow("^0.7.0",          "0.8; 1.7.0; 2.0.1")]
        [DataRow("^0.0.1",          "0.7; 1.7; 2.0.1")]
        [DataTestMethod, TestCategory(Category)]
        public void CompareTo_Greater(string range, string versions)
        {
            var r = new VersionRange(range);
            var v = versions.Split(';').Select(vs => (SemanticVersion)vs.Trim());

            foreach (var ver in v)
            {
                var res = r.CompareTo(ver);
                
                Assert.IsTrue(res > 0, $"Version: {ver}, Result: {res}");
            }
        }

        [DataRow("1.7.0",               "1.7")]
        [DataRow("=1.7.0",              "1.7")]
        [DataRow("<1.7.0",              "1.6.10; 0.9.0")]
        [DataRow("<=1.7.0",             "1.7; 1.6.10; 0.9.0")]
        [DataRow("<1.7.3 >1.7.5",       "1.7.3; 1.7.4; 1.7.5")]
        [DataRow(">1.7.0 <2.0.1",       "1.7.1; 1.8; 2.0")]
        [DataRow(">1.7.0 || <2.0.1",    "2.0.2; 2.1; 3.9.10")]
        [DataRow(">1.7.0 <1.7.8",       "1.7.1; 1.7.7")]
        [DataRow(">1.7 <1.8 || ~2.0",   "1.7.0; 1.7.999; 1.8.1; 1.9.10; 2.0.5")]
        [DataRow("1.7.0 - 2.0.1",       "1.7; 1.7.1; 1.8; 2.0; 2.0.1")]
        [DataRow("1.7 - 2.0.1",         "1.7; 1.7.1; 1.8; 2.0; 2.0.1")]
        [DataRow("1.7.x",               "1.7.0; 1.7.1; 1.7.10")]
        [DataRow("1.x",                 "1.0; 1.3; 1.7; 1.9.3")]
        [DataRow("x",                   "0.1; 1.1; 1.7.5; 2.0.1; 999.999.999")]
        [DataRow("~1.7.0",              "1.7; 1.7.3; 1.7.999")]
        [DataRow("~1.7",                "1.7; 1.7.3; 1.7.999")]
        [DataRow("~1",                  "1.0; 1.1; 1.7.3; 1.7.999; 1.8; 1.999.999")]
        [DataRow("^1.7.0",              "1.7; 1.7.3; 1.8; 1.999.999")]
        [DataRow("^0.7.0",              "0.7; 0.7.3; 0.7.999")]
        [DataRow("^0.0.1",              "0.0.1")]
        [DataTestMethod, TestCategory(Category)]
        public void CompareTo_Zero(string range, string versions)
        {
            var r = new VersionRange(range);
            var v = versions.Split(';').Select(vs => (SemanticVersion)vs.Trim());

            foreach (var ver in v)
            {
                var res = r.CompareTo(ver);

                Assert.IsTrue(res == 0, $"Version: {ver}, Result: {res}");
            }
        }
        
        [DataRow("1.7.0",           "1.6.999; 1.5; 0.9")]
        [DataRow("=1.7.0",          "1.6.999; 1.5; 0.9")]
        [DataRow(">1.7.0 <2.0.1",   "1.6.999; 1.5; 0.9")]
        [DataRow(">1.7.0 <1.7.8",   "1.6.999; 1.5; 0.9")]
        [DataRow(">1.7.1 <1.7.8",   "1.7; 1.6.999; 1.5; 0.9")]
        [DataRow(">1.7 <1.8 || ~2.0", "1.7.0; 1.2.2; 0.5")]
        [DataRow("1.7.0 - 2.0.1",   "1.6.999; 1.5; 0.9")]
        [DataRow("1.7.1 - 2.0.1",   "1.7; 1.6.999; 1.5; 0.9")]
        [DataRow("1.7 - 2.0.1",     "1.6.999; 1.5; 0.9")]
        [DataRow("1.7.x",           "1.6.999; 1.5; 0.9")]
        [DataRow("1.x",             "0.9; 0.3.3")]
        [DataRow("~1.7.0",          "1.6.999; 1.5; 0.9")]
        [DataRow("~1.7.1",          "1.7; 1.6.999; 1.5; 0.9")]
        [DataRow("~1.7",            "1.6.999; 1.5; 0.9")]
        [DataRow("~1",              "0.9; 0.6.2")]
        [DataRow("^1.7.0",          "1.6.999; 1.5; 0.9")]
        [DataRow("^1.7.1",          "1.7; 1.6.999; 1.5; 0.9")]
        [DataRow("^0.7.0",          "0.6; 0.2.1")]
        [DataRow("^0.7.3",          "0.7.2; 0.6; 0.2.1")]
        [DataRow("^0.0.1",          "0.0.0")]
        [DataTestMethod, TestCategory(Category)]
        public void CompareTo_Lesser(string range, string versions)
        {
            var r = new VersionRange(range);
            var v = versions.Split(';').Select(vs => (SemanticVersion)vs.Trim());

            foreach (var ver in v)
            {
                var res = r.CompareTo(ver);

                Assert.IsTrue(res < 0, $"Version: {ver}, Result: {res}");
            }
        }
    }
}
