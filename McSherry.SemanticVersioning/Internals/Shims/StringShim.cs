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
using System.Collections.Generic;

namespace McSherry.SemanticVersioning.Internals.Shims
{
#if USE_SHIMS
    /// <summary>
    /// <para>
    /// Provides shims that make <see cref="string"/> work as expected.
    /// </para>
    /// </summary>
    internal static class StringShim
    {
        /// <summary>
        /// <para>
        /// Provides an <see cref="IEnumerator{T}"/> that enumerates over
        /// the characters in a string.
        /// </para>
        /// </summary>
        /// <param name="s">
        /// The string containing the characters to enumerate over.
        /// </param>
        /// <returns>
        /// An enumerator that enumerates over the provided string's
        /// characters.
        /// </returns>
        public static IEnumerator<char> GetEnumerator(this string s)
        {
            foreach (var c in s)
            {
                yield return c;
            }
        }
    }
#endif
}
