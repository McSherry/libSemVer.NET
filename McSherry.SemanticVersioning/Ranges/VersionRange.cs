// Copyright (c) 2015 Liam McSherry
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

using McSherry.SemanticVersioning.Internals.Shims;

namespace McSherry.SemanticVersioning.Ranges
{

    /*
        The implementation of this class is split across
        multiple files.

        Ranges\VersionRange.cs:
            Main implementation.

        Ranges\VersionRange.Parsing.cs:
            Parsing ranges, comparators, etc, from strings.

        Ranges\VersionRange.Comparison.cs:
            Implementation of comparators and comparisons, works
            with the output of the parser to provide usable methods.
    */

    /// <summary>
    /// <para>
    /// Represents a range of acceptable versions. This class cannot
    /// be inherited.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// A version range specifies a set of semantic versions that are
    /// acceptable, and is used to check that a given semantic version
    /// fits within this set.
    /// </para>
    /// <para>
    /// Version ranges use the <c>node-semver</c> syntax for ranges.
    /// Specifically, ranges are based on the specification as it was
    /// written for the v5.0.0 release of <c>node-semver</c>.
    /// </para>
    /// <para>
    /// Presently, only the basic range syntax is supported.
    /// </para>
    /// </remarks>
    [Serializable]
    [CLSCompliant(true)]
    public sealed partial class VersionRange
    {
        // Used as a passthrough method so that the constructor taking a string
        // argument can use constructor chaining to avoid duplicate code.
        private static IEnumerable<IEnumerable<IComparator>> _ctorPassthrough(
            string range
            )
        {
            // First step, pass the string through the parser so we can
            // get a result to work with.
            var parseResult = Parser.Parse(range);

            // If the parsing was not successful, then we want to create an
            // exception and throw it.
            if (parseResult.Type != ParseResultType.Success)
                throw parseResult.CreateException();

            // If it was successful, then we want to transform the parser
            // output into something that we can return to our caller.
            //
            // The parser returns [ComparatorToken]s, which represent
            // individual comparators in a comparator set, but have no
            // actual implementation. We need to hand these tokens off
            // to our [Comparator] class, which will provide us with an
            // [IComparator] instance we can use.
            return parseResult.ComparatorSets.Select(
                tokenSet => tokenSet.Select(token => Comparator.Create(token))
                );
        }

        // The comparators that make up the version range. Each [IComparator]
        // is an individual comparator, so we need two [IEnumerable<T>] wraps:
        // one for comparator sets, and one to contain the multiple comparator
        // sets that make up the version range.
        private readonly IEnumerable<IEnumerable<IComparator>> _comparators;
        // A cache of versions that match and that don't match so that
        // many repeated comparisons might be sped up a bit.
        private readonly ISet<SemanticVersion> _matches, _mismatches;

        /// <summary>
        /// <para>
        /// Creates a new <see cref="VersionRange"/> instance from a set
        /// of comparator sets.
        /// </para>
        /// </summary>
        /// <param name="cmps">
        /// The set of comparator sets to create an instance from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="cmps"/> or any of its members are
        /// null or contain null values.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="cmps"/> is empty, or contains an
        /// empty set.
        /// </exception>
        private VersionRange(IEnumerable<IEnumerable<IComparator>> cmps)
        {
            // A one-liner for checking that the set of sets is not null, that
            // it does not contain any null sets, and that those sets do not
            // contain any null items.
            //
            // This probably warrants some explanation, since it can be a bit
            // obtuse on first reading.
            //
            // [set?.Any(cmp => cmp == null)] will return a [bool?]. If [set]
            // is null, then [null] is returned. Otherwise, the result of the
            // predicate is returned. If the returned [bool?] is [true] or
            // [null], it means that there is a null item. To make this into
            // a workable condition, we instead check that it isn't false.
            //
            // The same applies for the outer predicate, except this time it's
            // over a set of sets rather than a set of comparators.
            if (cmps?.Any(set => set?.Any(cmp => cmp == null) != false) != false)
            {
                throw new ArgumentNullException(
                    paramName:  nameof(cmps),
                    message:    "The provided set of comparator sets cannot be " +
                                "null, cannot contain null sets, and cannot " +
                                "have a set containing a null item."
                    );
            }

            // Make sure that the set of comparator sets is not empty, and does
            // not contain any empty comparator sets.
            if (!cmps.Any() || cmps.Any(set => !set.Any()))
            {
                throw new ArgumentException(
                    message:    "The provided set of comparator sets cannot be " +
                                "empty, and cannot contain any empty sets.",
                    paramName:  nameof(cmps)
                    );
            }

            // We've checked the provided comparators, so we're free to add them
            // to our field for comparators. We're going to assume that whoever
            // passed us the collection isn't going to modify it so we can avoid
            // copying the contents.
            _comparators = cmps;
            // We're using [HashSet<T>]s for our caches because they're fast and
            // because we only need to know whether a [SemanticVersion] is in
            // there.
            _matches = new HashSet<SemanticVersion>();
            _mismatches = new HashSet<SemanticVersion>();
        }
        /// <summary>
        /// <para>
        /// Creates a version range from a string representing the range.
        /// </para>
        /// </summary>
        /// <param name="range">
        /// The version range string from which to create an instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="range"/> is null, empty,
        /// or contains only whitespace characters.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="range"/> is invalid for any
        /// reason unrelated to an invalid semantic version string.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when <paramref name="range"/> contains an invalid
        /// semantic version string.
        /// </exception>
        public VersionRange(string range)
            : this(_ctorPassthrough(range))
        {
            // See implementation of [_ctorPassthrough].
        }

        /// <summary>
        /// <para>
        /// Determines whether the current version range is
        /// satisfied by a specified <see cref="SemanticVersion"/>.
        /// </para>
        /// </summary>
        /// <param name="semver">
        /// The <see cref="SemanticVersion"/> to check against the
        /// current version range.
        /// </param>
        /// <returns>
        /// True if the current version range is satisfied by
        /// <paramref name="semver"/>, false if otherwise.
        /// </returns>
        public bool SatisfiedBy(SemanticVersion semver)
        {
            // If this is a known match, return true.
            if (_matches.Contains(semver))
                return true;

            // If it's a known non-match, return false.
            if (_mismatches.Contains(semver))
                return false;

            // If it's not in the cache, run it against our comparators
            // to check whether it's a match or not.
            //
            // The [_comparators] variable contains all of the comparator
            // sets that were parsed from the string, and only one of the
            // comparator sets has to be satisfied for the range to be
            // satisfied.
            var result = _comparators.Any(
                // Each comparator set has a number of comparators in it,
                // and for a comparator set to be satisfied all of the
                // comparators in that set must be satisfied.
                set => set.All(cmp => cmp.SatisfiedBy(semver))
                );

            // We know whether it matches or not now, so we add it to the
            // appropriate cache so that it is known for future checks.
            (result ? _matches : _mismatches).Add(semver);

            // Result cached, we can now return that result to our caller.
            return result;
        }
        /// <summary>
        /// <para>
        /// Determines whether the current version range is satisfied
        /// by all specified <see cref="SemanticVersion"/> instances.
        /// </para>
        /// </summary>
        /// <param name="semvers">
        /// The <see cref="SemanticVersion"/> instances to check
        /// against the current version range.
        /// </param>
        /// <returns>
        /// True if all <see cref="SemanticVersion"/> instances in
        /// <paramref name="semvers"/> satisfy the current version
        /// range, false if otherwise.
        /// </returns>
        public bool SatisfiedBy(params SemanticVersion[] semvers)
        {
            return semvers.All(this.SatisfiedBy);
        }
        /// <summary>
        /// <para>
        /// Determines whether the current version range is satisfied
        /// by all specified <see cref="SemanticVersion"/> instances.
        /// </para>
        /// </summary>
        /// <param name="semvers">
        /// The <see cref="SemanticVersion"/> instances to check
        /// against the current version range.
        /// </param>
        /// <returns>
        /// True if all <see cref="SemanticVersion"/> instances in
        /// <paramref name="semvers"/> satisfy the current version
        /// range, false if otherwise.
        /// </returns>
        public bool SatisfiedBy(IEnumerable<SemanticVersion> semvers)
        {
            return semvers.All(this.SatisfiedBy);
        }
    }
}
