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

namespace McSherry.SemanticVersioning.Ranges
{
    using ComponentState = SemanticVersion.ComponentState;

    // Documentation and attributes are in 'Ranges\VersionRange.cs'
    public sealed partial class VersionRange
    {
        /// <summary>
        /// <para>
        /// Represents an implementation of a comparator.
        /// </para>
        /// </summary>
        internal interface IComparator
            : IComparable<SemanticVersion>
        {
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

            /// <summary>
            /// Whether a <see cref="SemanticVersion"/> with the pre-release
            /// identifers of the specified version can satisfy the comparator.
            /// </summary>
            /// <param name="comparand">
            /// The <see cref="SemanticVersion"/> of the kind to test.
            /// </param>
            /// <returns>
            /// True if, in line with version range comparison rules for
            /// versions with pre-release identifiers, the comparand is capable
            /// of satisfying the comparator. False if otherwise.
            /// </returns>
            bool ComparableTo(SemanticVersion comparand);
        }

        /// <summary>
        /// Provides a simple comparator which covers a single contiguous range
        /// of <see cref="SemanticVersion"/>s between a maximum and minimum.
        /// </summary>
        internal abstract class ContiguousComparator
            : IComparator
        {
            public ContiguousComparator(SemanticVersion min, SemanticVersion max)
            {
                this.MinimumVersion = min;
                this.MaximumVersion = max;
            }


            public SemanticVersion MaximumVersion { get; }

            public SemanticVersion MinimumVersion { get; }

            public bool SatisfiedBy(SemanticVersion comparand)
            {
                return (comparand >= this.MinimumVersion) && (comparand <= this.MaximumVersion);
            }

            public int CompareTo(SemanticVersion semver)
            {
                var sum = semver.CompareTo(this.MinimumVersion) +
                          semver.CompareTo(this.MaximumVersion);

                // If the sum of the two comparisons is less than two, irrespective
                // of sign, then at least one equality existed, which means that
                // the provided version falls within the represented range.
                return Math.Abs(sum) < 2 ? 0 : sum;
            }

            public abstract bool ComparableTo(SemanticVersion comparand);
        }

        /// <summary>
        /// <para>
        /// Provides the <see cref="Parser"/> with implementations for unary
        /// operators.
        /// </para>
        /// </summary>
        internal sealed class UnaryComparator : ContiguousComparator
        {
            /// <summary>
            /// <para>
            /// Represents a method capable of generating a delegate which
            /// implements a unary comparator.
            /// </para>
            /// </summary>
            /// <param name="comparator">
            /// The <see cref="SemanticVersion"/> that the version being checked
            /// is compared against. This is the version that is extracted from
            /// the range string by the parser.
            /// </param>
            /// <returns>
            /// A delegate which implements the comparison.
            /// </returns>
            private delegate UnaryComparator ComparatorFactory(SemanticVersion comparator);

            private static readonly IReadOnlyDictionary<Operator, ComparatorFactory>
                ComparatorFactories;


            /*
                The [VersionRange] class handles special rules for comparing
                against versions with pre-release identifiers, so these
                implementations need not be concerned with it.
            */

            private static UnaryComparator OpEqualFactory(SemanticVersion comparator)
            {
                return new UnaryComparator(comparator, comparator);
            }

            private static UnaryComparator OpLTEQFactory(SemanticVersion comparator)
            {
                return new UnaryComparator(SemanticVersion.MinValue, comparator);
            }

            private static UnaryComparator OpGTEQFactory(SemanticVersion comparator)
            {
                return new UnaryComparator(comparator, SemanticVersion.MaxValue);
            }


            private static UnaryComparator OpCaretFactory(SemanticVersion comparator)
            {
                SemanticVersion upper;

                // The caret operator allows changes that don't modify the
                // leftmost non-zero version component, so we need to identify
                // which non-zero component is leftmost.
                if (comparator.Major != 0)
                {
                    upper = new SemanticVersion(
                        major: comparator.Major,
                        minor: Int32.MaxValue,
                        patch: Int32.MaxValue
                        );
                }
                else if (comparator.Minor != 0)
                {
                    upper = new SemanticVersion(
                        major: 0,
                        minor: comparator.Minor,
                        patch: Int32.MaxValue
                        );
                }
                else
                {
                    upper = new SemanticVersion(
                        major: 0,
                        minor: 0,
                        patch: comparator.Patch
                        );
                }

                return new UnaryComparator(comparator, upper);
            }

            private static UnaryComparator OpTildeFactory(SemanticVersion comparator)
            {
                SemanticVersion lower, upper;

                // The behaviour of tilde-operator comparators varies depending
                // on how the provided semantic version was written, so we must
                // rely on parse metadata to produce the correct bounds.
                //
                // If no minor version is specified, the tilde operator allows
                // minor- and patch-level changes.
                if (comparator.ParseInfo.MinorState == ComponentState.Omitted)
                {
                    lower = new SemanticVersion(
                        major:       comparator.Major,
                        minor:       0,
                        patch:       0,
                        identifiers: comparator.Identifiers,
                        metadata:    comparator.Metadata
                        );

                    upper = new SemanticVersion(
                        major:       lower.Major,
                        minor:       Int32.MaxValue,
                        patch:       Int32.MaxValue
                        );
                }
                // If one is specified, the tilde operator allows only changes
                // at the patch level.
                else
                {
                    // We want to maintain a patch version if we have it, or
                    // reset it to zero if we don't.
                    var patch = comparator.ParseInfo.PatchState == ComponentState.Omitted
                        ? 0 
                        : comparator.Patch;

                    lower = new SemanticVersion(
                        major:       comparator.Major,
                        minor:       comparator.Minor,
                        patch:       patch,
                        identifiers: comparator.Identifiers,
                        metadata:    comparator.Metadata
                        );

                    upper = new SemanticVersion(
                        major:       lower.Major,
                        minor:       lower.Minor,
                        patch:       Int32.MaxValue
                        );
                }

                return new UnaryComparator(lower, upper);
            }

            private static UnaryComparator OpWildcardFactory(SemanticVersion comparator)
            {
                SemanticVersion lower, upper;

                // The wildcard operator only accepts changes at the same level
                // as the wildcard or at a level subordinate it to it, so our
                // behaviour changes depending on which component is a wildcard.
                //
                // A wildcard major version allows changes at the major version
                // level and below. That's the same as allowing any change, so
                // this will always return true (barring any non-comparability
                // due to pre-release identifiers, but we leave handling that
                // to [VersionRange]).
                if (comparator.ParseInfo.MajorState == ComponentState.Wildcard)
                {
                    return new UnaryComparator(
                        min: SemanticVersion.MinValue,
                        max: SemanticVersion.MaxValue
                        );
                }
                else if (comparator.ParseInfo.MinorState == ComponentState.Wildcard)
                {
                    // Wildcard parameters should default to zero, so we should
                    // be safe in assigning this here.
                    lower = comparator;

                    // Wildcard versions can't have pre-release identifiers, so
                    // we won't bother copying them over.
                    upper = new SemanticVersion(
                        major: lower.Major,
                        minor: Int32.MaxValue,
                        patch: Int32.MaxValue
                        );
                }
                else
                {
                    // Same as above, except with patch-level changes
                    lower = comparator;

                    upper = new SemanticVersion(
                        major: lower.Major,
                        minor: lower.Minor,
                        patch: Int32.MaxValue
                        );
                }

                return new UnaryComparator(lower, upper);
            }

            static UnaryComparator()
            {
                ComparatorFactories = new Dictionary<Operator, ComparatorFactory>
                {
                    // Simple comparators
                    [Operator.Equal]                = OpEqualFactory,
                    [Operator.LessThanOrEqual]      = OpLTEQFactory,
                    [Operator.GreaterThanOrEqual]   = OpGTEQFactory,

                    // Complex comparators
                    [Operator.Caret]                = OpCaretFactory,
                    [Operator.Tilde]                = OpTildeFactory,
                    [Operator.Wildcard]             = OpWildcardFactory,
                }.AsReadOnly();
            }

            /// <summary>
            /// <para>
            /// Creates a new <see cref="IComparator"/> using the specified
            /// version and unary operator.
            /// </para>
            /// </summary>
            /// <param name="op">
            /// The operator to create an equivalent <see cref="IComparator"/>
            /// for.
            /// </param>
            /// <param name="semver">
            /// The <see cref="SemanticVersion"/> the operator is associated with.
            /// </param>
            /// <returns>
            /// An <see cref="IComparator"/> instance that implements the
            /// comparison function represented by the specified
            /// <see cref="Operator"/>.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="op"/> is not a recognised
            /// <see cref="Operator"/> value, or is not a unary operator.
            /// </exception>
            public static IComparator Create(Operator op, SemanticVersion semver)
            {
                // We have to handle 'less than' and 'greater than' comparisons
                // separately because they can't feasibly be represented as an
                // inclusive range of versions. As we only guarantee that we provide
                // an [IComparator], the different type returned still fulfils
                // the contract.
                if (op == Operator.LessThan)
                {
                    return new ComparatorShell(
                        satisfiedBy: sv => sv < semver,
                        compareTo: sv =>
                        {
                            var cmp = sv.CompareTo(semver);

                            // If the version provided is less than the version
                            // in the comparator, then it matches the comparator
                            // and so cannot be less than all versions that the
                            // comparator matches.
                            //return cmp < 0 ? 0 : -1;

                            // In a less-than comparison, the operated-on version
                            // is the minimum value that cannot be accepted. If the
                            // comparand version is equal or greater, then it is
                            // higher than all the versions matched.
                            return cmp == -1 ? 0 : 1;
                        },
                        comparableTo: semver.ComparableTo
                        );
                }
                else if (op == Operator.GreaterThan)
                {
                    // This is the same as for [LessThan], reversed appropriately.

                    return new ComparatorShell(
                        satisfiedBy: sv => sv > semver,
                        compareTo: sv =>
                        {
                            var cmp = sv.CompareTo(semver);

                            return cmp == 1 ? 0 : -1;
                        },
                        comparableTo: semver.ComparableTo
                        );
                }
                // Anything else, we can defer to another method to build our
                // comparator instance for us.
                if (ComparatorFactories.TryGetValue(op, out var fact))
                {
                    var cmp = fact(semver);

                    cmp.Operator = op;
                    cmp.Version = semver;

                    return cmp;
                }
                else
                {
                    throw new ArgumentException(
                        message: $"Unrecognised unary operator (value: {op:N}).",
                        paramName: nameof(op)
                        );
                }
            }


            private UnaryComparator(SemanticVersion min, SemanticVersion max)
                : base(min, max)
            {

            }

            /// <summary>
            /// The operator the comparator implements.
            /// </summary>
            public Operator Operator
            {
                get;
                private set;
            }
            /// <summary>
            /// <para>
            /// The version against which the comparator compares.
            /// </para>
            /// </summary>
            public SemanticVersion Version
            {
                get;
                private set;
            }

            public override bool ComparableTo(SemanticVersion comparand)
            {
                return this.Version.ComparableTo(comparand);
            }
        }

        /// <summary>
        /// <para>
        /// Provides the parser with implementations for binary operators.
        /// </para>
        /// </summary>
        internal sealed class BinaryComparator : ContiguousComparator
        {
            /// <summary>
            /// <para>
            /// Represents a method capable of generating a delegate which
            /// implements a binary comparator.
            /// </para>
            /// </summary>
            /// <param name="lhs">
            /// The <see cref="SemanticVersion"/> which appears on the left
            /// hand side of the operator.
            /// </param>
            /// <param name="rhs">
            /// The <see cref="SemanticVersion"/> which appears on the right
            /// hand side of the operator.
            /// </param>
            /// <returns></returns>
            private delegate BinaryComparator Factory(SemanticVersion lhs, SemanticVersion rhs);

            private static readonly IReadOnlyDictionary<Operator, Factory>
                ComparerFactories;


            private static BinaryComparator OpHyphenFactory(
                SemanticVersion lhs, SemanticVersion rhs)
            {
                // Anything omitted in the left-hand version resets to zero.
                var lower = new SemanticVersion(
                    major:       lhs.Major,
                    minor:       lhs.ParseInfo.MinorState == ComponentState.Omitted ? 0 : lhs.Minor,
                    patch:       lhs.ParseInfo.PatchState == ComponentState.Omitted ? 0 : lhs.Patch,
                    identifiers: lhs.Identifiers,
                    metadata:    lhs.Metadata
                    );

                // In the right-hand version, behaviour depends on which of
                // the components is omitted.
                //
                // If the minor version is omitted, the major is incremented by
                // one, the others are reset to zero, and it's a less-than
                // comparison only.
                if (rhs.ParseInfo.MinorState == ComponentState.Omitted)
                {
                    var upper = new SemanticVersion(
                        major: rhs.Major,
                        minor: Int32.MaxValue,
                        patch: Int32.MaxValue,
                        identifiers: rhs.Identifiers,
                        metadata: rhs.Metadata
                        );

                    return new BinaryComparator(lower, upper);
                }
                // If the patch version is omitted it's similar, but the minor
                // version is incremented by one instead.
                else if (rhs.ParseInfo.PatchState == ComponentState.Omitted)
                {
                    var upper = new SemanticVersion(
                        major:       rhs.Major,
                        minor:       rhs.Minor,
                        patch:       Int32.MaxValue,
                        identifiers: rhs.Identifiers,
                        metadata:    rhs.Metadata
                        );

                    return new BinaryComparator(lower, upper);
                }
                // And if nothing is omitted, then it's a simple inclusive
                // range comparison against the right-hand version
                else
                {
                    return new BinaryComparator(lower, rhs);
                }
            }


            static BinaryComparator()
            {
                ComparerFactories = new Dictionary<Operator, Factory>
                {
                    [Operator.Hyphen]   = OpHyphenFactory,
                }.AsReadOnly();
            }


            /// <summary>
            /// <para>
            /// Creates a new <see cref="IComparator"/> using the specified
            /// versions and binary operator.
            /// </para>
            /// </summary>
            /// <param name="op">
            /// The operator to create an equivalent <see cref="IComparator"/>
            /// for.
            /// </param>
            /// <param name="lhs">
            /// The <see cref="SemanticVersion"/> that appears on the left-hand
            /// side of the operator.
            /// </param>
            /// <param name="rhs">
            /// The <see cref="SemanticVersion"/> that appears on the right-hand
            /// side of the operator.
            /// </param>
            /// <returns>
            /// An <see cref="IComparator"/> that implements the comparison
            /// function represented by <paramref name="op"/>.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="op"/> is not a recognised
            /// <see cref="Operator"/> value, or is not a binary operator.
            /// </exception>
            public static IComparator Create(
                Operator op, SemanticVersion lhs, SemanticVersion rhs
                )
            {
                if (ComparerFactories.TryGetValue(op, out var fact))
                {
                    var cmp = fact(lhs, rhs);

                    cmp.LeftVersion = lhs;
                    cmp.RightVersion = rhs;

                    return cmp;
                }
                else
                {
                    throw new ArgumentException(
                        message: $"Unrecognised binary operator (value: {op:N}).",
                        paramName: nameof(op)
                        );
                }
            }


            private BinaryComparator(SemanticVersion min, SemanticVersion max)
                : base(min, max)
            {

            }


            /// <summary>
            /// <para>
            /// The <see cref="SemanticVersion"/> on the left-hand side of the
            /// operator.
            /// </para>
            /// </summary>
            public SemanticVersion LeftVersion
            {
                get;
                private set;
            }
            /// <summary>
            /// <para>
            /// The <see cref="SemanticVersion"/> on the right-hand side of the
            /// operator.
            /// </para>
            /// </summary>
            public SemanticVersion RightVersion
            {
                get;
                private set;
            }


            public override bool ComparableTo(SemanticVersion comparand)
            {
                return this.LeftVersion.ComparableTo(comparand) ||
                       this.RightVersion.ComparableTo(comparand);
            }
        }

        /// <summary>
        /// Provides a shell of a comparator which takes a <see cref="Predicate{T}"/>
        /// to provide its <see cref="IComparator.SatisfiedBy(SemanticVersion)"/>
        /// implementation.
        /// </summary>
        private sealed class ComparatorShell
            : IComparator
        {
            private readonly Predicate<SemanticVersion> _satis, _comparable;
            private readonly Func<SemanticVersion, int> _compareTo;

            public ComparatorShell(
                Predicate<SemanticVersion> satisfiedBy,
                Func<SemanticVersion, int> compareTo,
                Predicate<SemanticVersion> comparableTo
                )
            {
                _satis = satisfiedBy;
                _compareTo = compareTo;
                _comparable = comparableTo;
            }


            public bool SatisfiedBy(SemanticVersion semver) => _satis(semver);

            public int CompareTo(SemanticVersion semver) => _compareTo(semver);

            public bool ComparableTo(SemanticVersion semver) => _comparable(semver);
        }
    }
}
