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
using System.Text;

namespace McSherry.SemanticVersioning
{
    using SVF = SemanticVersionFormat;

    /// <summary>
    /// <para>
    /// Lists the format identifiers accepted by the 
    /// <see cref="SemanticVersion"/> class's implementation of
    /// <see cref="IFormattable"/>.
    /// </para>
    /// </summary>
    [CLSCompliant(true)]
    public static class SemanticVersionFormat
    {
        /// <summary>
        /// <para>
        /// The default way to format a semantic version.
        /// </para>
        /// </summary>
        public static string Default            => "G";
        /// <summary>
        /// <para>
        /// The default way to format a semantic version, prefixed
        /// with a letter "v".
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// For details on how this option formats a semantic version,
        /// see <see cref="Default"/>.
        /// </para>
        /// </remarks>
        public static string PrefixedDefault    => "g";

        /// <summary>
        /// <para>
        /// A way to concisely format a semantic version. Omits metadata
        /// and only includes the <see cref="SemanticVersion.Patch"/> version 
        /// if it is non-zero.
        /// </para>
        /// </summary>
        public static string Concise            => "C";
        /// <summary>
        /// <para>
        /// A concise way to format a semantic version, prefixed with a
        /// letter "v".
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// For details on how this option formats a semantic version, see
        /// <see cref="Concise"/>.
        /// </para>
        /// </remarks>
        public static string PrefixedConcise    => "c";

        /// <summary>
        /// <para>
        /// The format used for monotonic version strings. Aliases
        /// <see cref="Concise"/>.
        /// </para>
        /// </summary>
        public static string Monotonic          => Concise;
        /// <summary>
        /// <para>
        /// The format used for monotonic version strings, prefixed
        /// with a letter "v". Aliases <see cref="PrefixedConcise"/>.
        /// </para>
        /// </summary>
        public static string PrefixedMonotonic  => PrefixedConcise;
    }

    // Documentation/attributes/interfaces/etc are in the main
    // implementation file, "SemanticVersion.cs"
    public sealed partial class SemanticVersion
    {
        /// <summary>
        /// <para>
        /// Encapsulates the formatting routines provided by the
        /// <see cref="SemanticVersion"/> class.
        /// </para>
        /// </summary>
        private static class Formatter
        {
            private delegate string FmtRoutine(SemanticVersion semver);
            private static readonly IReadOnlyDictionary<string, FmtRoutine> Fmtrs;

            private static string Default(SemanticVersion semver)
            {
                var sb = new StringBuilder();

                // The three numeric version components are always 
                // present, so we can add them to the builder without
                // any checks.
                sb.Append($"{semver.Major}.{semver.Minor}.{semver.Patch}");

                // Pre-release identifiers always come before metadata,
                // but we need to make sure there are identifiers to add
                // first.
                if (semver.Identifiers.Any())
                {
                    // Identifiers are separated from the three-part 
                    // version by a hyphen character.
                    sb.Append('-');

                    // Identifiers are separated from each other by
                    // periods.
                    //
                    // We concatenate them in this way to avoid an
                    // extra step at the end. If we concatenated using
                    // the format string [$"{id}."], we'd need to get
                    // rid of an extra period at the end.
                    semver.Identifiers.Skip(1).Aggregate(
                        seed: sb.Append(semver.Identifiers.First()),
                        func: (bdr, id) => bdr.Append($".{id}"));
                }

                // Like with the pre-release identifiers, we want to make sure
                // there is metadata to add before we attempt to add it.
                if (semver.Metadata.Any())
                {
                    // Metadata is separated from the three-part version/pre-
                    // -release identifiers by a plus character.
                    sb.Append('+');

                    // Identifiers are separated from each other by
                    // periods.
                    //
                    // We concatenate them in this way to avoid an
                    // extra step at the end. If we concatenated using
                    // the format string [$"{id}."], we'd need to get
                    // rid of an extra period at the end.
                    semver.Metadata.Skip(1).Aggregate(
                        seed: sb.Append(semver.Metadata.First()),
                        func: (bdr, md) => bdr.Append($".{md}"));
                }

                return sb.ToString();
            }

            private static string Concise(SemanticVersion semver)
            {
                var sb = new StringBuilder();
                
                // Major-Minor is always included.
                sb.Append($"{semver.Major}.{semver.Minor}");

                // The patch version must be greater than zero
                // to be included.
                if (semver.Patch > 0)
                    sb.Append($".{semver.Patch}");

                // If there are any identifiers, include them in
                // the version.
                if (semver.Identifiers.Any())
                {
                    // Identifiers are separated from the maj/min
                    // by a hyphen.
                    sb.Append("-");

                    // Identifiers are separated from each other by
                    // periods.
                    //
                    // We concatenate them in this way to avoid an
                    // extra step at the end. If we concatenated using
                    // the format string [$"{id}."], we'd need to get
                    // rid of an extra period at the end.
                    semver.Identifiers.Skip(1).Aggregate(
                        seed: sb.Append(semver.Identifiers.First()),
                        func: (bdr, id) => bdr.Append($".{id}"));
                }

                return sb.ToString();
            }

            static Formatter()
            {
                // REMEMBER:    When updating the formatters, add a property to
                //              [SemVerFormat] and information in the doc comments
                //              for [ToString(string, IFormatProvider] on the
                //              [SemanticVersion] class.

                Fmtrs = new Dictionary<string, FmtRoutine>
                {
                    [SVF.Default]           = Default,
                    [SVF.PrefixedDefault]   = (sv) => $"v{Default(sv)}",

                    [SVF.Concise]           = Concise,
                    [SVF.PrefixedConcise]   = (sv) => $"v{Concise(sv)}",
                }.AsReadOnly();
            }

            /// <summary>
            /// <para>
            /// Exposes the formatter for <see cref="SemanticVersion"/>
            /// instances.
            /// </para>
            /// </summary>
            /// <param name="semver">
            /// The <see cref="SemanticVersion"/> to be formatted.
            /// </param>
            /// <param name="format">
            /// The format string specifying how the <see cref="SemanticVersion"/>
            /// should be formatted.
            /// </param>
            /// <returns></returns>
            public static string Format(SemanticVersion semver, string format)
            {
                // Null should be treated as equivalent to the default format.
                if (format == null)
                    format = SVF.Default;

                // Attempt to retrieve the formatter from our collection of
                // formatters and, if we can retrieve it, call it and return
                // what it produces.
                if (Fmtrs.TryGetValue(format, out var fmt))
                    return fmt(semver);

                // If we couldn't retrieve a formatter, throw an exception.
                throw new FormatException(
                    $@"Unrecognised format specifier ""{format}"".");
            }
        }

        // [ToString(string, IFormatProvider)] is implemented explicitly because
        // the [IFormatProvider] parameter is always ignored, so there isn't much 
        // sense in exposing it to the user, who will never use the parameter.

        /// <summary>
        /// <para>
        /// Formats the value of the current <see cref="SemanticVersion"/>
        /// as specified.
        /// </para>
        /// </summary>
        /// <param name="format">
        /// The format to use, or null for the default format.
        /// </param>
        /// <param name="provider">
        /// The format provider to use, or null for the default provider. 
        /// This parameter is ignored.
        /// </param>
        /// <returns>
        /// A string representation of the current <see cref="SemanticVersion"/>,
        /// formatted as specified.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown when the format specifier given in <paramref name="format"/>
        /// is not recognised or is invalid.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The format of a Semantic Version is not dependent on culture
        /// information, and so the value of <paramref name="provider"/>
        /// is ignored.
        /// </para>
        /// <para>
        /// The value of <paramref name="format"/> should contain one of
        /// the below-listed format specifiers. Custom format patterns
        /// are not supported. If <paramref name="format"/> is null, the
        /// default format specifier, "G", is used in its place.
        /// </para>
        /// <para>
        /// The list of recognised format specifiers is given in the
        /// below table.
        /// </para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Format Specifier</term>
        ///         <term>Description</term>
        ///         <term>Example</term>
        ///     </listheader>
        /// 
        ///     <item>
        ///         <term><c>"c"</c></term>
        ///         <term>
        ///             Prefixed concise format. Identical
        ///             to the concise format (<c>"C"</c>),
        ///             except prefixed with a lowercase "v".
        ///         </term>
        ///         <term>
        ///             <para>v1.8</para>
        ///             <para>v1.15.1</para>
        ///             <para>v2.1-beta.3</para>
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term><c>"C"</c></term>
        ///         <term>
        ///             Concise format. Omits metadata items,
        ///             and only includes the <see cref="Patch"/>
        ///             version if it is non-zero.
        ///         </term>
        ///         <term>
        ///             <para>1.8</para>
        ///             <para>1.15.1</para>
        ///             <para>2.1-beta.3</para>
        ///         </term>
        ///     </item>
        /// 
        ///     <item>
        ///         <term><c>"g"</c></term>
        ///         <term>
        ///             Prefixed default format. Identical to
        ///             the default format (<c>"G"</c>), except
        ///             prefixed with a lowercase "v".
        ///         </term>
        ///         <term>
        ///             <para>v1.7.0-alpha.2+20150925.f8f2cb1a</para>
        ///             <para>v1.2.5</para>
        ///             <para>v2.0.1-rc.1</para>
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term><c>"G"</c>, <c>null</c></term>
        ///         <term>
        ///             The default format, as given by the
        ///             Semantic Versioning 2.0.0 specification.
        ///         </term>
        ///         <term>
        ///             <para>1.7.0-alpha.2+20150925.f8f2cb1a</para>
        ///             <para>1.2.5</para>
        ///             <para>2.0.1-rc.1</para>
        ///         </term>
        ///     </item>
        /// </list>
        /// </remarks>
        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return this.ToString(format);
        }
        /// <summary>
        /// <para>
        /// Formats the value of the current <see cref="SemanticVersion"/>
        /// as specified.
        /// </para>
        /// </summary>
        /// <param name="format">
        /// The format to use, or null for the default format.
        /// </param>
        /// <returns>
        /// A string representation of the current <see cref="SemanticVersion"/>,
        /// formatted as specified.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown when the format specifier given in <paramref name="format"/>
        /// is not recognised or is invalid.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For information on the acceptable format specifiers, see the
        /// Remarks section of the 
        /// <see cref="IFormattable.ToString(string, IFormatProvider)"/>
        /// implementation for <see cref="SemanticVersion"/>.
        /// </para>
        /// </remarks>
        public string ToString(string format)
        {
            return Formatter.Format(this, format);
        }
    }
}
