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

namespace McSherry.SemanticVersioning.Ranges
{
    // Documentation and attributes are in 'Ranges\VersionRange.cs'
    public sealed partial class VersionRange
    {
        /// <summary>
        /// <para>
        /// Represents an implementation of a comparator.
        /// </para>
        /// </summary>
        private interface IComparator
        {
            /*
                So, why bother with this interface if it's private and
                we've only got one class implementing it, and that class
                is little more than a shell?

                Answer: future proofing.

                Currently, we only support the basic features of version
                ranges specified by the 'node-semver' docs, but in future
                we're likely to support the advanced features, and the
                advanced features all decompose into a set of the basic
                features.

                By having this interface here from the start, we don't need
                to do a large amount of rework, we just need to make the
                [VersionRange] class implement the interface, we need to 
                add the appropriate [Operator] values for the advanced features,
                and we then make [Comparator.Create] return [VersionRange]s
                for advanced features, and that's that. It'll also be entirely
                transparent to the [VersionRange] consuming it, which is a big
                plus.
            */

            /// <summary>
            /// <para>
            /// Determines whether the comparator is satisfied
            /// by a specified <see cref="SemanticVersion"/>.
            /// </para>
            /// </summary>
            /// <param name="comparand">
            /// The <see cref="SemanticVersion"/> to check
            /// against the comparator.
            /// </param>
            /// <returns>
            /// True if the comparator is satisfied by the
            /// specified semantic version, false if otherwise.
            /// </returns>
            bool SatisfiedBy(SemanticVersion comparand);
        }

        /// <summary>
        /// <para>
        /// Implements comparison using the <see cref="ComparatorToken"/>
        /// instances produced by the <see cref="Parser"/>.
        /// </para>
        /// </summary>
        private sealed class Comparator : IComparator
        {
            /// <summary>
            /// <para>
            /// Represents a method implementing a comparator.
            /// </para>
            /// </summary>
            /// <param name="comparand">
            /// The <see cref="SemanticVersion"/> that is being compared
            /// against the version specified in the <see cref="ComparatorToken"/>.
            /// </param>
            /// <param name="comparator">
            /// The <see cref="SemanticVersion"/> that the version being
            /// checked is compared against. This is the version that is
            /// specified in the <see cref="ComparatorToken"/>.
            /// </param>
            /// <returns>
            /// True if the comparator is satisfied, false if otherwise.
            /// </returns>
            private delegate bool ComparatorImpl(SemanticVersion comparand,
                                                 SemanticVersion comparator);

            private static readonly IReadOnlyDictionary<Operator, ComparatorImpl>
                Comparers;

            private static bool OpEqual(SemanticVersion arg, 
                                        SemanticVersion comparator)
            {
                // We don't care about metadata when we're comparing with
                // version ranges, so we use [EquivalentTo] so that those
                // items are ignored.
                return arg.EquivalentTo(comparator);
            }

            private static bool OpLess(SemanticVersion arg,
                                       SemanticVersion comparator)
            {
                // This one's slightly trickier since it isn't a direct
                // translation to one of the [SemanticVersion] operators or
                // methods.

                // Generally, a "less than" comparison will be a simple
                // precedence comparison, but in certain situations we need
                // to do some extra work.
                //
                // If the comparator version ([ca]) has pre-release identifiers,
                // then we need to check whether the comparand ([c]) has them as
                // well. If it does, [c] and [ca] must have the same 
                // major -minor-patch trio for [c] to satisfy the comparator.
                // However, if [c] does not have identifiers, then a simple
                // precedence comparison will work.
                //
                // Example:
                //
                //      Comparator:     <1.2.3-alpha.5
                //
                //      +---------------+------------+
                //      | Comparand     | Satisfies? |
                //      +---------------+------------+
                //      | 1.2.3         | False      |
                //      | 1.2.3-alpha.4 | True       |
                //      | 1.2.3-alpha.6 | False      |
                //      | 1.2.2         | True       |
                //      | 1.2.2-alpha.1 | False      |
                //      | 2.3.4         | False      |
                //      | 2.3.4-alpha.1 | False      |
                //      +---------------+------------+
                //
                // The 'node-semver' documentation justifies this behaviour by
                // saying that a user, by specifying a comparator with pre-release
                // identifiers, has stated that they are okay with pre-releases of
                // the same version, but not of any other version.


                // If both versions have identifiers, they must have the same
                // major-minor-patch trio. If not, then we must return false.
                if (arg.Identifiers.Any() && comparator.Identifiers.Any())
                {
                    if (comparator.Major != arg.Major ||
                        comparator.Minor != arg.Minor ||
                        comparator.Patch != arg.Patch)
                        return false;
                }
                // If the comparator doesn't have pre-release identifiers but
                // the comparand does, then we want to return false.
                else if (arg.Identifiers.Any())
                {
                    return false;
                }

                return arg < comparator;
            }

            private static bool OpGreater(SemanticVersion arg, 
                                          SemanticVersion comparator)
            {
                // This function is going to be mostly the same as [OpLess], just
                // using a greater-than precedence comparison instead of a
                // less-than comparison.
                //
                // As a result, any comments that would be practical duplicates
                // are omitted. See the body of [OpLess] for roughly equivalent
                // comments.

                if (arg.Identifiers.Any() && comparator.Identifiers.Any())
                {
                    if (comparator.Major != arg.Major ||
                        comparator.Minor != arg.Minor ||
                        comparator.Patch != arg.Patch)
                        return false;
                }

                return arg > comparator;
            }

            private static bool OpLTEQ(SemanticVersion arg,
                                       SemanticVersion comparator)
            {
                return OpEqual(arg, comparator) || OpLess(arg, comparator);
            }

            private static bool OpGTEQ(SemanticVersion arg,
                                       SemanticVersion comparator)
            {
                return OpEqual(arg, comparator) || OpGreater(arg, comparator);
            }

            static Comparator()
            {
                Comparers = new Dictionary<Operator, ComparatorImpl>
                {
                    [Operator.Equal]                = OpEqual,
                    [Operator.LessThan]             = OpLess,
                    [Operator.GreaterThan]          = OpGreater,
                    [Operator.LessThanOrEqual]      = OpLTEQ,
                    [Operator.GreaterThanOrEqual]   = OpGTEQ,
                }.AsReadOnly();
            }

            /// <summary>
            /// <para>
            /// Creates a new <see cref="IComparator"/> using the specified
            /// <see cref="ComparatorToken"/>.
            /// </para>
            /// </summary>
            /// <param name="cmp">
            /// The <see cref="ComparatorToken"/> to create an equivalent
            /// <see cref="IComparator"/> for.
            /// </param>
            /// <returns>
            /// An <see cref="IComparator"/> instance that implements the
            /// comparison function represented by the specified
            /// <see cref="ComparatorToken"/>.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="cmp"/> has an unrecognised
            /// <see cref="ComparatorToken.Operator"/> value.
            /// </exception>
            public static IComparator Create(ComparatorToken cmp)
            {
                if (!Comparers.TryGetValue(cmp.Operator, out var cmpImpl))
                {
                    throw new ArgumentException(
                        message: "Unrecognised operator.",
                        paramName: nameof(cmp));
                }

                return new Comparator(sv => cmpImpl(sv, cmp.Version));
            }

            private readonly Predicate<SemanticVersion> _cmpFn;

            /// <summary>
            /// <para>
            /// Creates a new <see cref="Comparator"/> with the specified
            /// function as its comparison function.
            /// </para>
            /// </summary>
            /// <param name="impl">
            /// The function to use as the comparison function.
            /// </param>
            private Comparator(Predicate<SemanticVersion> impl)
            {
                if (impl == null)
                {
                    throw new ArgumentNullException(
                        paramName: nameof(impl),
                        message: "Comparator implementation cannot " +
                                    "be null."
                        );
                }

                _cmpFn = impl;
            }

            bool IComparator.SatisfiedBy(SemanticVersion comparand)
            {
                return _cmpFn(comparand);
            }
        }
    }
}
