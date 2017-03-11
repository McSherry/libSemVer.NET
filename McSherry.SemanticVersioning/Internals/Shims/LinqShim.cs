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
    /// Provides reimplementations of LINQ methods for compiling with
    /// .NET Standard or other platforms that don't provide them.
    /// </para>
    /// </summary>
    internal static class LinqShim
    {
        /// <summary>
        /// <para>
        /// Checks whether any characters in a string satisfy a predicate.
        /// </para>
        /// </summary>
        /// <param name="s">
        /// The string containing the characters to check.
        /// </param>
        /// <param name="pred">
        /// The predicate to check against.
        /// </param>
        /// <returns>
        /// True if any characters in the string satisfy the predicate.
        /// </returns>
        public static bool Any(this string s, Predicate<char> pred)
        {
            foreach (var c in s)
            {
                if (pred(c))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// <para>
        /// Checks whether all characters in a string satisfy a predicate.
        /// </para>
        /// </summary>
        /// <param name="s">
        /// The string containing the characters to check.
        /// </param>
        /// <param name="pred">
        /// The predicate to check against.
        /// </param>
        /// <returns>
        /// True if all characters in the string satisfy the predicate.
        /// </returns>
        public static bool All(this string s, Predicate<char> pred)
        {
            foreach (var c in s)
            {
                if (!pred(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// <para>
        /// Applies a transform to each character of a string.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of the output of the transform.
        /// </typeparam>
        /// <param name="s">
        /// The string providing the characters to be transformed.
        /// </param>
        /// <param name="selector">
        /// The transform function.
        /// </param>
        /// <returns>
        /// A set containing the result of the transform applied to
        /// each character.
        /// </returns>
        public static IEnumerable<T> Select<T>(this string s, Func<char, T> selector)
        {
            var selectees = new List<T>();

            foreach (var c in s)
            {
                selectees.Add(selector(c));
            }

            return selectees;
        }
    }
#endif
}
