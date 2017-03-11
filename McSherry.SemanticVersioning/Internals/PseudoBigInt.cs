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
using System.Linq;

using McSherry.SemanticVersioning.Internals.Shims;

namespace McSherry.SemanticVersioning.Internals
{
    /*
     * As there's no limit on the length of an integer in the labels of a
     * pre-release identifier/metadata item, we were using System.Numerics and
     * its [BigInteger] struct before. It seems a bit silly to reference that
     * library for a single use, so this class is intended to replace it.
     * 
     * This class takes two strings and compares them based on the integer
     * values they represent, but doesn't provide any other number-related
     * functionality (like arithmetic).
     */

    /// <summary>
    /// <para>
    /// Compares two strings based on the positive integer values they represent.
    /// </para>
    /// </summary>
    internal static class PseudoBigInt
    {
        /// <summary>
        /// <para>
        /// Compares two strings based on the positive integer values they represent.
        /// </para>
        /// </summary>
        /// <param name="subject">
        /// The string to compare to <paramref name="against"/>.
        /// </param>
        /// <param name="against">
        /// The string against which <paramref name="subject"/> is compared.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if <paramref name="subject"/> is greater than
        /// <paramref name="against"/>, <c>null</c> if the two are equal, and
        /// <c>false</c> if <paramref name="subject"/> is lesser.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either the parameter is null.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown if either parameter is not a valid positive decimal integer
        /// without leading zeroes.
        /// </exception>
        public static bool? Compare(string subject, string against)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(
                    paramName:  nameof(subject),
                    message:    $@"The parameter ""{nameof(subject)}"" " +
                                  "cannot be null."
                    );
            }

            

            if (against == null)
            {
                throw new ArgumentNullException(
                    paramName:  nameof(against),
                    message:    $@"The parameter ""{nameof(against)}"" " +
                                  "cannot be null."
                    );
            }

            if (subject.Length == 0 || against.Length == 0)
            {
                throw new FormatException(
                    "A numeric string cannot be zero-length."
                    );
            }

            // Leading zeroes aren't permitted in the strings. If the length
            // greater than one digit, then the first digit cannot be a zero
            // without that digit being a leading zero.
            //
            // We don't need to worry about Unicode stuff because no permitted
            // characters can be more than a single UTF-16 codepoint long, and
            // so will be rejected in later checks.
            if ( (subject.Length > 1 && subject[0] == '0') ||
                 (against.Length > 1 && against[0] == '0') )
            {
                throw new FormatException(
                    "A numeric string cannot contain leading zeroes."
                    );
            }

            // The strings must only contain numeric characters (0..9).
            if (subject.ToCharArray().Any(c => c < '0' || c > '9') ||
                against.ToCharArray().Any(c => c < '0' || c > '9'))
            {
                throw new FormatException(
                    "A numeric string may only contain the characters 0 to 9."
                    );
            }

            // As strings cannot have leading digits or fractional components,
            // any string with more digits must have a greater numeric value
            // than a string with fewer digits.
            if (subject.Length > against.Length)
            {
                return true;
            }
            else if (subject.Length < against.Length)
            {
                return false;
            }

            // If they're equal length, then the digits must be compared to
            // determine which is greater.
            //
            // Luckily, this is made easy by two factors:
            //  (1) Decimal numbers are written with the most-significant digit
            //      first, so we can just walk through the string from the
            //      start and compare the values of each digit.
            //
            //  (2) The Unicode code-points values for digits are in ascending
            //      order ('0' < '1' < '2' ...), so the code-points can be
            //      directly compared.
            using (var subEn = subject.GetEnumerator())
            using (var agnEn = against.GetEnumerator())
            {
                // They're the same length, so both will be true at the same
                // time. This is just a handy trick to enumerate both at the
                // same time.
                while (subEn.MoveNext() & agnEn.MoveNext())
                {
                    // If they're equal, we don't care.
                    if (subEn.Current == agnEn.Current)
                        continue;
                    // But if they aren't, we have to determine which has
                    // a greater value.
                    else if (subEn.Current > agnEn.Current)
                        return true;
                    else
                        return false;
                }

                // If we end up here, we enumerated through all the chars and
                // didn't find a difference, so the two strings are the same.
                return null;
            }
        }
    }
}
