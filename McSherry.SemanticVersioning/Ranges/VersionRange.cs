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
using System.Runtime.CompilerServices;
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
    /// written for the v6.0.0 release of <c>node-semver</c>.
    /// </para>
    /// <para>
    /// The full basic and advanced range syntaxes are supported, but
    /// there are minor differences in how 'X-ranges' are handled. Unlike
    /// with <c>node-semver</c>, this class will reject ranges where the
    /// wildcard (<c>x</c>, <c>X</c>, or <c>*</c> character) is followed
    /// by another version component or by pre-release identifiers or
    /// metadata. As <c>node-semver</c> appears to ignore anything that
    /// follows a wildcard, this has no real impact on functionality.
    /// </para>
    /// <para>
    /// In addition, for backwards compatibility, an empty version range
    /// will be considered invalid. <c>node-semver</c> treats this as
    /// equivalent to <c>*</c>.
    /// </para>
    /// </remarks>
    [Serializable]
    [CLSCompliant(true)]
    public sealed partial class VersionRange
        : VersionRange.IComparator, IComparable<SemanticVersion>
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

            // If it was successful, return the comparator sets that the parser
            // produced from the range string.
            return parseResult.ComparatorSets;
        }


        // The comparators that make up the version range. Each [IComparator]
        // is an individual comparator, so we need two [IEnumerable<T>] wraps:
        // one for comparator sets, and one to contain the multiple comparator
        // sets that make up the version range.
        private readonly IEnumerable<IEnumerable<IComparator>> _comparators;

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
        /// The cache to use to memoize the results of <see cref="VersionRange"/>
        /// satisfaction methods.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Assign <see langword="null"/> to disable memoization. The value of this
        /// property is <see langword="null"/> by default.
        /// </para>
        /// <para>
        /// Accesses by <see cref="VersionRange"/> to the memoization agent
        /// are surrounded by <c><see langword="lock"/> (<see cref="SynchronizationObject"/>)</c>.
        /// </para>
        /// </remarks>
        public IDictionary<SemanticVersion, bool> MemoizationAgent
        {
            get; set;
        }
        /// <summary>
        /// The object used in synchronising accesses to the
        /// <see cref="MemoizationAgent"/>.
        /// </summary>
        public object SynchronizationObject { get; } = new object();

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
            // If we've cached a comparison to this semantic version, return
            // the result we cached.
            lock (this.SynchronizationObject)
            {
                if (this.MemoizationAgent != null &&
                    this.MemoizationAgent.TryGetValue(semver, out var res))
                {
                    return res;
                }
            }


            bool result = _comparators
                // Sets which can be satisfied by the specified version (needed
                // for correct handling of pre-release versions)
                .Where(set => set.Any(cmp => cmp.ComparableTo(semver)))
                // In at least one of the comparator sets, all comparators are
                // satisfied by the specified version
                .Any(set => set.All(cmp => cmp.SatisfiedBy(semver)));

            // If we have a cache, add the result of the comparison to it.
            lock (this.SynchronizationObject)
            {
                this.MemoizationAgent?.Add(semver, result);
            }

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

        /// <summary>
        /// Determines whether the specified <see cref="SemanticVersion"/> is
        /// outside the bounds of the current version range.
        /// </summary>
        /// <param name="semver">
        /// The <see cref="SemanticVersion"/> to compare to the current version
        /// range.
        /// </param>
        /// <returns>
        /// <para>
        /// If <paramref name="semver"/> has higher precedence than all versions
        /// that satisfy the current version range, an integer greater than zero.
        /// </para>
        /// <para>
        /// If <paramref name="semver"/> has lower precedence than all versions
        /// that satisfy the current version range, an integer less than zero.
        /// </para>
        /// <para>
        /// If <paramref name="semver"/> satisfies the current version range, if it
        /// is neither greater than nor less than all versions that satisfy the
        /// current version range, or if it is <see langword="null"/>, zero.
        /// </para>
        /// </returns>
        public int CompareTo(SemanticVersion semver)
        {
            //const int GREATER = +1;
            const int EQUAL   =  0;
            //const int LESSER  = -1;

            // To determine relative precedence, we have to determine whether
            // the provided version is either greater or less than all versions
            // that the current range accepts.
            //
            // To start with, we know that neither can be true if any of the
            // comparator sets we have is satisfied by the current version, as
            // that means it falls within their range.
            if (_comparators.Any(set => set.All(cmp => cmp.SatisfiedBy(semver))))
                return EQUAL;

            // Once we know that none of the comparator sets are satisfied, we
            // need to verify that the version is greater or lesser than all of
            // the sets individually, with the same relation for all sets. 
            //
            // Version ranges can be noncontiguous, e.g. '>1.7 <1.8 || 2.0.0' is
            // not satisfied by 'v1.9.2', but it clearly isn't greater than the
            // maximum the range matches (which is '2.0.0') and nor is it less
            // than the minimum version matched by the range ('1.7.1'). If the
            // result--greater or lesser--is the same for all sets, then we know
            // it doesn't fall into a gap in a noncontiguous range.
            //
            // As we do for comparators in each set, here we take the first set
            // as a seed and verify that all others match.
            var setSeed = CompareToSet(_comparators.First());

            // And, in the same manner as for comparators, if all the results
            // match we're able to return the seed.
            if (_comparators.Skip(1).All(set => CompareToSet(set) == setSeed))
                return setSeed;
            // And if they don't, it's neither greater nor lesser than the range.
            else
                return EQUAL;


            int CompareToSet(IEnumerable<IComparator> set)
            {
                // A set will likely have multiple comparators setting its bounds,
                // which means that some comparators will match. If all comparators
                // but one matches, we can use the mismatch as our final result.
                var lastMismatch = 0;

                foreach (var cmp in set)
                {
                    var curr = cmp.CompareTo(semver);

                    // If we haven't yet found a mismatch, then we can store it
                    // and carry on until we run out of comparators or find another.
                    if (lastMismatch == 0)
                    {
                        lastMismatch = curr;
                    }
                    // If we have found a mismatch but the current comparison is a
                    // satisfaction, there's no conflict and we can carry on.
                    //
                    // Alternatively, a mismatch of the same type could indicate a
                    // meaningless but still valid range (e.g. '<1.1 <1.2' would
                    // produce two 'greater than' results for '1.3').
                    else if (curr == 0 || lastMismatch == curr)
                    {
                        continue;
                    }
                    // However, if we've found a mismatch of a different type, we
                    // have a noncontiguous range where the provided version falls
                    // within one of the range's gaps.
                    else
                    {
                        return EQUAL;
                    }
                }

                return lastMismatch;

                // We take the first comparator in the set as a seed, and we'll
                // compare all subsequent comparators against this. It doesn't
                // matter which we start with, this is just easiest.
                //var cmpSeed = set.First().CompareTo(semver);

                //// If all subsequent comparators match, we can return the seed.
                //if (set.Skip(1).All(cmp => cmp.CompareTo(semver) == cmpSeed))
                //    return cmpSeed;
                //// Otherwise, we have to indicate it's neither greater nor lesser.
                //else
                //    return EQUAL;
            }
        }


        bool IComparator.ComparableTo(SemanticVersion comparand)
        {
            return _comparators.Any(
                set => set.Any(cmp => cmp.ComparableTo(comparand))
                );
        }
    }
}
