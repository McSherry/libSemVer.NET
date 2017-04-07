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

namespace McSherry.SemanticVersioning.Monotonic
{
    /// <summary>
    /// <para>
    /// Provides extension methods related to treating 
    /// <see cref="SemanticVersion"/>s as monotonic versions.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The methods this class provides are based on the
    /// Monotonic Versioning Manifesto 1.2.
    /// </remarks>
    [CLSCompliant(true)]
    public static class MonotonicExtensions
    {
        /// <summary>
        /// <para>
        /// Determines whether a <see cref="SemanticVersion"/> is a valid
        /// monotonic version.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The <see cref="SemanticVersion"/> to be checked.
        /// </param>
        /// <returns>
        /// True if <paramref name="version"/> is a valid monotonic version,
        /// false if otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// A <see cref="SemanticVersion"/> is considered a valid monotonic
        /// version if it: has no <see cref="SemanticVersion.Patch"/> component;
        /// and has no <see cref="SemanticVersion.Identifiers"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="version"/> is null.
        /// </exception>
        public static bool IsMonotonic(this SemanticVersion version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(
                    message:    "The specified version cannot be null.",
                    paramName:  nameof(version)
                    );
            }

            return version.Patch == 0 &&
                   version.Identifiers.Count == 0;
        }
    }
}
