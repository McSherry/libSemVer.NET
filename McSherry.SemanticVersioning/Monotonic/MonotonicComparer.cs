// Copyright (c) 2015-16 Liam McSherry
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

namespace McSherry.SemanticVersioning.Monotonic
{
    /// <summary>
    /// <para>
    /// Represents a semantic version comparison operation using
    /// monotonic versioning comparison rules.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This class compares monotonic versions as set out by the
    /// Monotonic Versioning Manifesto 1.2.
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class MonotonicComparer
        : IComparer<SemanticVersion>
    {
        private static readonly MonotonicComparer 
            _standard = new MonotonicComparer();

        /// <summary>
        /// <para>
        /// A <see cref="MonotonicComparer"/> which compares using the
        /// standard rules for monotonic versions.
        /// </para>
        /// </summary>
        public static MonotonicComparer Standard => _standard;

        private MonotonicComparer()
        {

        }

        /// <summary>
        /// <para>
        /// Compares two monotonic versions and returns an indication
        /// of their relative order.
        /// </para>
        /// </summary>
        /// <param name="x">
        /// A monotonic version to compare to <paramref name="y"/>.
        /// </param>
        /// <param name="y">
        /// A monotonic version to compare to <paramref name="x"/>.
        /// </param>
        /// <returns>
        /// <para>
        /// A value less than zero if <paramref name="x"/> precedes
        /// <paramref name="y"/> in sort order, or if
        /// <paramref name="x"/> is null and <paramref name="y"/>
        /// is not null.
        /// </para>
        /// <para>
        /// Zero if <paramref name="x"/> is equal to <paramref name="y"/>,
        /// including if both are null.
        /// </para>
        /// <para>
        /// A value greater than zero if <paramref name="x"/> follows
        /// <paramref name="y"/> in sort order, or if <paramref name="y"/>
        /// is null and <paramref name="x"/> is not null.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="x"/> or <paramref name="y"/> is not a valid
        /// monotonic version.
        /// </exception>
        public int Compare(SemanticVersion x, SemanticVersion y)
        {
            const string NonMonMsg =
                "A non-monotonic version cannot be compared using " +
                nameof(MonotonicComparer) + ".";

            const int XGreater = +1;
            const int Equal    =  0;
            const int YGreater = -1;

            if (x != null && !x.IsMonotonic())
            {
                throw new ArgumentException(
                    message:    NonMonMsg,
                    paramName:  nameof(x)
                    );
            }

            if (y != null && !y.IsMonotonic())
            {
                throw new ArgumentException(
                    message:    NonMonMsg,
                    paramName:  nameof(y)
                    );
            }

            // If only one of the parameters is null, the null
            // one will have precedence.
            if (x == null ^ y == null)
                return x == null ? XGreater : YGreater;

            // If both are equal (including if both are null), then
            // the parameters have the same precedence.
            if (x == y)
                return Equal;

            // If the compatibility components differ, the numerically
            // larger one has precedence.
            if (x.Major > y.Major)
                return XGreater;
            else if (x.Major < y.Major)
                return YGreater;

            // If the release components differ, the numerically larger
            // one takes precedence.
            if (x.Minor > y.Minor)
                return XGreater;
            else if (x.Minor < y.Minor)
                return YGreater;

            // Metadata is compared lexically, and the later-sorted
            // version takes precedence. We'll assume that "lexically"
            // means "with ASCII sort order" like in the Semantic
            // Versioning specification, since that's what Monotonic
            // Versioning is based on and the manifesto does not make
            // any more-specific requirements.

            // If the metadata collections are of equal length, check
            // to see whether they contain the same items. If they do,
            // the versions have equal precedence.
            if (x.Metadata.Count == y.Metadata.Count &&
                x.Metadata.SequenceEqual(y.Metadata))
                return Equal;

            using (var xe = x.Metadata.GetEnumerator())
            using (var ye = y.Metadata.GetEnumerator())
            {
                // Iterate through both metadata collections at once.
                while (xe.MoveNext() & ye.MoveNext())
                {
                    // We're looking for the first difference, so
                    // we continue to the next iteration if the items
                    // are equal.
                    if (xe.Current == ye.Current)
                        continue;

                    // If we're here, we've found a difference.
                    //
                    // The manifesto specifies lexical ordering, which
                    // we'll take to mean "ASCII sort order" since it
                    // isn't any more specific and that's what Semantic
                    // Versioning uses.
                    //
                    // The later-sorted one has precedence.
                    return String.CompareOrdinal(xe.Current, ye.Current);
                }

                // If we end up here, one collection is longer than the other.
                // We also know that the larger collection is a superset of
                // the smaller one, otherwise we wouldn't have ended up here.
                //
                // We're going to use the lengths of the collections to
                // determine precedence, sorting the shorter collection before
                // the longer one due to the requirement that the lexically-
                // later-sorted version takes precedence.
                if (x.Metadata.Count < y.Metadata.Count)
                    return XGreater;
                // We already know that the collections are not equal, so
                // we don't need to check again here.
                else
                    return YGreater;
            }
        }
    }
}
