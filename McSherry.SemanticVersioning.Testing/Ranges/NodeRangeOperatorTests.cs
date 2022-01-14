// The tests cases in these files are taken from the 'node-semver' code and are
// licensed differently to the other portions of the project. The text of the
// licence, which is very similar to the MIT licence, is as follows:
//
//      Copyright (c) Isaac Z. Schlueter and Contributors
//
//      Permission to use, copy, modify, and/or distribute this software for any
//      purpose with or without fee is hereby granted, provided that the above
//      copyright notice and this permission notice appear in all copies.
//
//      THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
//      WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
//      MERCHANTABILITY AND FITNESS.IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
//      ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
//      WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
//      ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
//      OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
//
// This file should not be modified other than to add or correct tests to keep
// parity with the 'node-semver' tests.
//
// The 'node-semver' test cases are available here:
//
//      https://github.com/npm/node-semver/blob/master/test/fixtures/version-gt-range.js
//      https://github.com/npm/node-semver/blob/master/test/fixtures/version-lt-range.js
//      https://github.com/npm/node-semver/blob/master/test/fixtures/version-not-gt-range.js
//      https://github.com/npm/node-semver/blob/master/test/fixtures/version-not-lt-range.js
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace McSherry.SemanticVersioning.Ranges
{
    /// <summary>
    /// Provides tests for <see cref="VersionRange.CompareTo(SemanticVersion)"/> using
    /// the test cases that <c>node-semver</c> provides.
    /// </summary>
    [TestClass]
    public class NodeRangeOperatorTests
    {
        private const string Category = "Version Range Operators (Node Tests)";

        [DataRow("~1.2.2",          "1.3.0")]
        [DataRow("~0.6.1-1",        "0.7.1-1")]
        [DataRow("1.0.0 - 2.0.0",   "2.0.1")]
        [DataRow("1.0.0",           "1.0.1-beta1")]
        [DataRow("1.0.0",           "2.0.0")]
        [DataRow("<=2.0.0",         "2.1.1")]
        [DataRow("<=2.0.0",         "3.2.9")]
        [DataRow("<2.0.0",          "2.0.0")]
        [DataRow("0.1.20 || 1.2.4", "1.2.5")]
        [DataRow("2.x.x",           "3.0.0")]
        [DataRow("1.2.x",           "1.3.0")]
        [DataRow("1.2.x || 2.x",    "3.0.0")]
        [DataRow("2.*.*",           "5.0.1")]
        [DataRow("1.2.*",           "1.3.3")]
        [DataRow("1.2.* || 2.*",    "4.0.0")]
        [DataRow("2",               "3.0.0")]
        [DataRow("2.3",             "2.4.2")]
        [DataRow("~2.4",            "2.5.0")]
        [DataRow("~2.4",            "2.5.5")]
        [DataRow("~1",              "2.2.3")]
        [DataRow("~ 1.0",           "1.1.0")]
        [DataRow("~1.0",            "1.1.2")]
        [DataRow("<1.2",            "1.2.0")]
        [DataRow("< 1.2",           "1.2.1")]
        [DataRow("~v0.5.4-pre",     "0.6.0")]
        [DataRow("~v0.5.4-pre",     "0.6.1-pre")]
        [DataRow("=0.7.x",          "0.8.0")]
        [DataRow("=0.7.x",          "0.8.0-asdf")]
        [DataRow("<0.7.x",          "0.7.0")]
        [DataRow("~1.2.2",          "1.3.0")]
        [DataRow("1.0.0 - 2.0.0",   "2.2.3")]
        [DataRow("1.0.0",           "1.0.1")]
        [DataRow("<=2.0.0",         "3.0.0")]
        [DataRow("<=2.0.0",         "2.9999.9999")]
        [DataRow("<=2.0.0",         "2.2.9")]
        [DataRow("<2.0.0",          "2.2.9")]
        [DataRow("2.x.x",           "3.1.3")]
        [DataRow("1.2.x",           "1.3.3")]
        [DataRow("1.2.x || 2.x",    "3.1.3")]
        [DataRow("2.*.*",           "3.1.3")]
        [DataRow("2",               "3.1.2")]
        [DataRow("2.3",             "2.4.1")]
        [DataRow("~2.4",            "2.5.0")]
        [DataRow("~1",              "2.2.3")]
        [DataRow("~1.0",            "1.1.0")]
        [DataRow("<1",              "1.0.0")]
        [DataRow("=0.7.x",          "0.8.2")]
        [DataRow("<0.7.x",          "0.8.2")]
        [DataRow("0.7.x",           "0.7.2-beta")]
        [DataTestMethod, TestCategory(Category)]
        public void Node_CompareTo_Greater(string range, string version)
        {
            var r = new VersionRange(range);
            var v = (SemanticVersion)version;

            var res = r.CompareTo(v);

            Assert.IsTrue(res > 0, $"Result: {res}");
        }

        [DataRow("~1.2.2",          "1.2.1")]
        [DataRow("~0.6.1-1",        "0.6.1-0")]
        [DataRow("1.0.0 - 2.0.0",   "0.0.1")]
        [DataRow("1.0.0-beta.2",    "1.0.0-beta.1")]
        [DataRow("1.0.0",           "0.0.0")]
        [DataRow(">=2.0.0",         "1.1.1")]
        [DataRow(">=2.0.0",         "1.2.9")]
        [DataRow(">2.0.0",          "2.0.0")]
        [DataRow("0.1.20 || 1.2.4", "0.1.5")]
        [DataRow("2.x.x",           "1.0.0")]
        [DataRow("1.2.x",           "1.1.0")]
        [DataRow("1.2.x || 2.x",    "1.0.0")]
        [DataRow("2.*.*",           "1.0.1")]
        [DataRow("1.2.*",           "1.1.3")]
        [DataRow("1.2.* || 2.*",    "1.1.9999")]
        [DataRow("2",               "1.0.0")]
        [DataRow("2.3",             "2.2.2")]
        [DataRow("~2.4",            "2.3.0")]
        [DataRow("~2.4",            "2.3.5")]
        [DataRow("~1",              "0.2.3")]
        [DataRow("~1.0",            "0.1.2")]
        [DataRow("~ 1.0",           "0.1.0")]
        [DataRow(">1.2",            "1.2.0")]
        [DataRow("> 1.2",           "1.2.1")]
        [DataRow("=0.7.x",          "0.6.0")]
        [DataRow("=0.7.x",          "0.7.0-asdf")]
        [DataRow(">=0.7.x",         "0.6.0")]
        [DataRow("~1.2.2",          "1.2.1")]
        [DataRow("1.0.0 - 2.0.0",   "0.2.3")]
        [DataRow("1.0.0",           "0.0.1")]
        [DataRow(">=2.0.0",         "1.0.0")]
        [DataRow(">=2.0.0",         "1.9999.9999")]
        [DataRow(">=2.0.0",         "1.2.9")]
        [DataRow("2.x.x",           "1.1.3")]
        [DataRow("1.2.x",           "1.1.3")]
        [DataRow("1.2.x || 2.x",    "1.1.3")]
        [DataRow("2.*.*",           "1.1.3")]
        [DataRow("1.2.*",           "1.1.3")]
        [DataRow("1.2.* || 2.*",    "1.1.3")]
        [DataRow("2",               "1.9999.9999")]
        [DataRow("2.3",             "2.2.1")]
        [DataRow("~2.4",            "2.3.0")]
        [DataRow("~1",              "0.2.3")]
        [DataRow("~1.0",            "0.0.0")]
        [DataRow(">1",              "1.0.0")]
        [DataRow("=0.7.x",          "0.6.2")]
        [DataRow("=0.7.x",          "0.7.0-asdf")]
        [DataRow("^1",              "1.0.0-0")]
        [DataRow(">=0.7.x",         "0.7.0-asdf")]
        [DataRow(">=0.7.x",         "0.6.2")]
        [DataRow(">1.2.3",          "1.3.0-alpha")]
        [DataTestMethod, TestCategory(Category)]
        public void Node_CompareTo_Lesser(string range, string version)
        {
            var r = new VersionRange(range);
            var v = (SemanticVersion)version;

            var res = r.CompareTo(v);

            Assert.IsTrue(res < 0, $"Result: {res}");
        }

        [DataRow("~0.6.1-1",            "0.6.1-1")]
        [DataRow("1.0.0 - 2.0.0",       "1.2.3")]
        [DataRow("1.0.0 - 2.0.0",       "1.0.0")]
        [DataRow(">=*",                 "0.2.4")]
        [DataRow("*",                   "1.2.3")]
        [DataRow("*",                   "v1.2.3-foo")]
        [DataRow(">=1.0.0",             "1.0.0")]
        [DataRow(">=1.0.0",             "1.0.1")]
        [DataRow(">=1.0.0",             "1.1.0")]
        [DataRow(">1.0.0",              "1.0.1")]
        [DataRow(">1.0.0",              "1.1.0")]
        [DataRow("<=2.0.0",             "2.0.0")]
        [DataRow("<=2.0.0",             "1.9999.9999")]
        [DataRow("<=2.0.0",             "0.2.9")]
        [DataRow(">= 1.0.0",            "1.0.0")]
        [DataRow(">=  1.0.0",           "1.0.1")]
        [DataRow(">=   1.0.0",          "1.1.0")]
        [DataRow("> 1.0.0",             "1.0.1")]
        [DataRow(">  1.0.0",            "1.1.0")]
        [DataRow("<=   2.0.0",          "2.0.0")]
        [DataRow("<= 2.0.0",            "1.9999.9999")]
        [DataRow("<=  2.0.0",           "0.2.9")]
        [DataRow("<    2.0.0",          "1.9999.9999")]
        [DataRow("<\t2.0.0",            "0.2.9")]
        [DataRow(">=0.1.97",            "v0.1.97")]
        [DataRow(">=0.1.97",            "v0.1.97")]
        [DataRow("0.1.20 || 1.2.4",     "1.2.4")]
        [DataRow("0.1.20 || >1.2.4",    "1.2.4")]
        [DataRow("0.1.20 || 1.2.4",     "1.2.3")]
        [DataRow("0.1.20 || 1.2.4",     "0.1.20")]
        [DataRow(">=0.2.3 || <0.0.1",   "0.0.0")]
        [DataRow(">=0.2.3 || <0.0.1",   "0.2.3")]
        [DataRow(">=0.2.3 || <0.0.1",   "0.2.4")]
        [DataRow("2.x.x",               "2.1.3")]
        [DataRow("1.2.x",               "1.2.3")]
        [DataRow("1.2.x || 2.x",        "2.1.3")]
        [DataRow("1.2.x || 2.x",        "1.2.3")]
        [DataRow("x",                   "1.2.3")]
        [DataRow("2.*.*",               "2.1.3")]
        [DataRow("1.2.*",               "1.2.3")]
        [DataRow("1.2.* || 2.*",        "2.1.3")]
        [DataRow("1.2.* || 2.*",        "1.2.3")]
        [DataRow("*",                   "1.2.3")]
        [DataRow("2",                   "2.1.2")]
        [DataRow("2.3",                 "2.3.1")]
        [DataRow("~2.4",                "2.4.0")]
        [DataRow("~2.4",                "2.4.5")]
        [DataRow("~1",                  "1.2.3")]
        [DataRow("~1.0",                "1.0.2")]
        [DataRow("~ 1.0",               "1.0.2")]
        [DataRow(">=1",                 "1.0.0")]
        [DataRow(">= 1",                "1.0.0")]
        [DataRow("<1.2",                "1.1.1")]
        [DataRow("< 1.2",               "1.1.1")]
        [DataRow("~v0.5.4-pre",         "0.5.5")]
        [DataRow("~v0.5.4-pre",         "0.5.4")]
        [DataRow("=0.7.x",              "0.7.2")]
        [DataRow(">=0.7.x",             "0.7.2")]
        [DataRow("=0.7.x",              "0.7.0-asdf")]
        [DataRow(">=0.7.x",             "0.7.0-asdf")]
        [DataRow("<=0.7.x",             "0.6.2")]
        [DataRow(">0.2.3 >0.2.4 <=0.2.5",   "0.2.5")]
        [DataRow(">=0.2.3 <=0.2.4",     "0.2.4")]
        [DataRow("1.0.0 - 2.0.0",       "2.0.0")]
        [DataRow("^1",                  "0.0.0-0")]
        [DataRow("^3.0.0",              "2.0.0")]
        [DataRow("^1.0.0 || ~2.0.1",    "2.0.0")]
        [DataRow("^0.1.0 || ~3.0.1 || 5.0.0",   "3.2.0")]
        [DataRow("^0.1.0 || ~3.0.1 || >4 <=5.0.0",  "3.5.0")]
        [DataRow("~ 1.0",               "1.1.0")]
        [DataTestMethod, TestCategory(Category)]
        public void Node_CompareTo_Zero(string range, string version)
        {
            var r = new VersionRange(range);
            var v = (SemanticVersion)version;

            var res = r.CompareTo(v);

            Assert.IsTrue(res == 0, $"Result: {res}");
        }
    }
}
