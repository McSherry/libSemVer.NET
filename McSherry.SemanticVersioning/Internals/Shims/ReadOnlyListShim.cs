// Copyright (c) 2015-17 Liam McSherry
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
using System.Collections;
using System.Collections.Generic;

namespace McSherry.SemanticVersioning.Internals.Shims
{
#if USE_SHIMS
    /// <summary>
    /// <para>
    /// Provides methods for working with read-only lists under .NET Standard.
    /// </para>
    /// </summary>
    internal static class ReadOnlyListShim
    {
        /// <summary>
        /// <para>
        /// Produces a read-only view of a list.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements of the list.
        /// </typeparam>
        /// <param name="list">
        /// The list to create a read-only view of.
        /// </param>
        /// <returns>
        /// A read-only view of the specified list.
        /// </returns>
        public static IReadOnlyList<T> AsReadOnly<T>(this List<T> list)
        {
            return new ReadOnlyList<T>(list);
        }
    }

    /// <summary>
    /// <para>
    /// Provides an implementation of <see cref="IReadOnlyList{T}"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// The type of the items in the list.
    /// </typeparam>
    internal sealed class ReadOnlyList<T>
        : IReadOnlyList<T>
    {
        private readonly IList<T> _src;

        /// <summary>
        /// <para>
        /// Creates a new <see cref="ReadOnlyList{T}"/> from a source
        /// <see cref="IList{T}"/>.
        /// </para>
        /// </summary>
        /// <param name="source">
        /// The source to create from.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the source is null.
        /// </exception>
        public ReadOnlyList(IList<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(
                    paramName:  nameof(source),
                    message:    "The list from which to create a read-only " +
                                "list cannot be null."
                    );
            }

            _src = source;
        }

        public T this[int index] => _src[index];

        int IReadOnlyCollection<T>.Count => _src.Count;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _src.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_src).GetEnumerator();
    }
#endif
}
