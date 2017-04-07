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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// <para>
    /// An internal class providing helper methods.
    /// </para>
    /// </summary>
    internal static class Helper
    {
        private static readonly Regex _metaRegex;

        static Helper()
        {
            // We're going to save ourselves some trouble and just use a
            // regular expression to check each individual build metadata
            // item.
            _metaRegex = new Regex("^[0-9A-Za-z-]+$");
        }

        /// <summary>
        /// <para>
        /// Checks whether a provided <see cref="string"/> is a valid
        /// pre-release identifier.
        /// </para>
        /// </summary>
        /// <param name="identifier">
        /// The pre-release identifier to check.
        /// </param>
        /// <returns>
        /// True if <paramref name="identifier"/> is a valid pre-release
        /// identifier, false if otherwise.
        /// </returns>
        public static bool IsValidIdentifier(string identifier)
        {
            // The rules for identifiers are a superset of the rules
            // for metadata items, so if it isn't a build metadata
            // item the it can't be an identifier.
            if (!IsValidMetadata(identifier))
                return false;

            // If the identifier is numeric (i.e. entirely numbers), then
            // it cannot have a leading zero.
            //
            // We have to check the length is greater than [1] because a
            // single ['0'] character as an identifier is valid.
            if (identifier.Length > 1 && identifier[0] == '0' &&
                identifier.ToCharArray().All(IsNumber))
                return false;

            // It's passed the tests, so it's a valid identifier.
            return true;
        }
        /// <summary>
        /// <para>
        /// Checks whether a provided <see cref="string"/> is a valid
        /// build metadata item.
        /// </para>
        /// </summary>
        /// <param name="metadata">
        /// The build metadata item to check.
        /// </param>
        /// <returns>
        /// True if <paramref name="metadata"/> is a valid pre-release
        /// identifier, false if otherwise.
        /// </returns>
        public static bool IsValidMetadata(string metadata)
        {
            // Metadata items cannot be empty, and cannot contain
            // whitespace.
            if (String.IsNullOrWhiteSpace(metadata))
                return false;

            // A metadata item may only contain characters that are
            // alphanumeric or the hyphen character.
            if (!_metaRegex.IsMatch(metadata))
                return false;

            // It's passed the tests, so it's a valid metadata item.
            return true;
        }

        /// <summary>
        /// <para>
        /// Determines whether a given character is a number.
        /// </para>
        /// </summary>
        /// <param name="c">
        /// The character to check.
        /// </param>
        /// <returns>
        /// True if <paramref name="c"/> is a number, false if
        /// otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method differs from <see cref="Char.IsNumber(char)"/> in
        /// that it only considers numbers between 0 and 9 valid.
        /// </para>
        /// </remarks>
        public static bool IsNumber(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// <para>
        /// Returns a read-only <see cref="IReadOnlyDictionary{TKey, TValue}"/>
        /// wrapper of the specified <see cref="IDictionary{TKey, TValue}"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="K">
        /// The type of the dictionary's keys.
        /// </typeparam>
        /// <typeparam name="V">
        /// The type of the dictionary's values.
        /// </typeparam>
        /// <param name="dictionary">
        /// The <see cref="IDictionary{TKey, TValue}"/> to wrap.
        /// </param>
        /// <returns>
        /// A read-only wrapper of <paramref name="dictionary"/>.
        /// </returns>
        public static IReadOnlyDictionary<K, V> AsReadOnly<K, V>(
            this IDictionary<K, V> dictionary
            )
        {
            return new ReadOnlyDictionary<K, V>(dictionary);
        }
    }
}
