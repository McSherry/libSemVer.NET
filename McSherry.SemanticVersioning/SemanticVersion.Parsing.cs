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

using McSherry.SemanticVersioning.Internals.Shims;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// <para>
    /// Represents the parsing modes that the <see cref="SemanticVersion"/>
    /// parser may be configured to use.
    /// </para>
    /// </summary>
    [Flags]
    [CLSCompliant(true)]
    public enum ParseMode : int
    {
        /// <summary>
        /// <para>
        /// The default parser behaviour, with no set flags. This forces
        /// specification compliance.
        /// </para>
        /// </summary>
        Strict          = 0x00000000,
        /// <summary>
        /// <para>
        /// The opposite of <see cref="Strict"/>, with all parser flags
        /// set.
        /// </para>
        /// </summary>
        Lenient         = ~0,

        /// <summary>
        /// <para>
        /// The parser will accept a version prefixed with "v" or "V".
        /// </para>
        /// </summary>
        AllowPrefix     = 1 << 0,
        /// <summary>
        /// <para>
        /// The parser will accept versions with the 
        /// <see cref="SemanticVersion.Patch"/> version component omitted.
        /// </para>
        /// </summary>
        OptionalPatch   = 1 << 1,
    }

    // Documentation/attributes/interfaces/etc are in the main
    // implementation file, "SemanticVersion.cs"
    public sealed partial class SemanticVersion
    {
        /// <summary>
        /// <para>
        /// Represents the possible results of the <see cref="SemanticVersion"/>
        /// parser.
        /// </para>
        /// </summary>
        internal enum ParseResultType
        {
            /// <summary>
            /// <para>
            /// Parsing completed with no issues.
            /// </para>
            /// </summary>
            Success,

            /// <summary>
            /// <para>
            /// The string to parse was null or empty.
            /// </para>
            /// </summary>
            NullString,

            /// <summary>
            /// <para>
            /// An invalid character is present before the
            /// major-minor-patch trio.
            /// </para>
            /// </summary>
            PreTrioInvalidChar,

            /// <summary>
            /// <para>
            /// One or more items in the major-minor-patch
            /// trio contains an invalid character.
            /// </para>
            /// </summary>
            TrioInvalidChar,
            /// <summary>
            /// <para>
            /// One or more of the items in the major-minor-patch
            /// trio has a leading zero.
            /// </para>
            /// </summary>
            TrioItemLeadingZero,
            /// <summary>
            /// <para>
            /// One or more of the items in the major-minor-patch
            /// trio is missing.
            /// </para>
            /// </summary>
            TrioItemMissing,
            /// <summary>
            /// <para>
            /// One of the major, minor, or patch version components
            /// represents a number greater than <see cref="int.MaxValue"/>.
            /// </para>
            /// </summary>
            TrioItemOverflow,

            /// <summary>
            /// <para>
            /// An pre-release identifier was expected but not 
            /// found.
            /// </para>
            /// </summary>
            IdentifierMissing,
            /// <summary>
            /// <para>
            /// The version contains one or more invalid
            /// pre-release identifiers.
            /// </para>
            /// </summary>
            IdentifierInvalid,

            /// <summary>
            /// <para>
            /// A build metadata item was expected but
            /// not found.
            /// </para>
            /// </summary>
            MetadataMissing,
            /// <summary>
            /// <para>
            /// The version contains one or more invalid 
            /// build metadata items.
            /// </para>
            /// </summary>
            MetadataInvalid,
        }

        /// <summary>
        /// <para>
        /// Represents the result produced by the <see cref="SemanticVersion"/>
        /// parser.
        /// </para>
        /// </summary>
        internal struct ParseResult
        {
            // This will be false if we're default-constructed.
            private readonly bool _successfulCreation;
            // Backing fields for properties
            private readonly ParseResultType _type;
            private readonly SemanticVersion _version;

            /// <summary>
            /// <para>
            /// Checks the <see cref="_successfulCreation"/> field,
            /// and throws if it is false.
            /// </para>
            /// </summary>
            /// <typeparam name="T">
            /// The type of the value to be returned on success.
            /// </typeparam>
            /// <param name="passthrough">
            /// A value to be returned from the function if the
            /// <see cref="ParseResult"/> was successfully constructed.
            /// </param>
            /// <returns>
            /// If <see cref="_successfulCreation"/> is true, returns
            /// <paramref name="passthrough"/>.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="_successfulCreation"/> is false.
            /// </exception>
            private T VerifyResult<T>(T passthrough)
            {
                if (_successfulCreation)
                    return passthrough;

                throw new InvalidOperationException(
                    "Attempt to use a [SemanticVersion.ParseResult] that " +
                    "was default-constructed.");
            }

            /// <summary>
            /// <para>
            /// Creates a new successful <see cref="ParseResult"/> with
            /// the specified <see cref="SemanticVersion"/> as its payload.
            /// </para>
            /// </summary>
            /// <param name="version">
            /// The <see cref="SemanticVersion"/> to provide as the result.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="version"/> is null.
            /// </exception>
            public ParseResult(SemanticVersion version)
            {
                if (version == null)
                {
                    throw new ArgumentNullException(
                        paramName:  nameof(version),
                        message:    "The provided SemanticVersion cannot be null."
                        );
                }

                _successfulCreation = true;
                _type = ParseResultType.Success;
                _version = version;
            }
            /// <summary>
            /// <para>
            /// Creates a new failure-state <see cref="ParseResult"/> with
            /// the specified <see cref="ParseResultType"/> as its payload.
            /// </para>
            /// </summary>
            /// <param name="error">
            /// The error code to provide to consuming code.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="error"/> is equal to
            /// <see cref="ParseResultType.Success"/>, or is unrecognised.
            /// </exception>
            public ParseResult(ParseResultType error)
            {
                if (!Enum.IsDefined(typeof(ParseResultType), error))
                {
                    throw new ArgumentException(
                        message:    $"The parse result code {(int)error:X8} is " +
                                     "not recognised.",
                        paramName:  nameof(error));
                }

                if (error == ParseResultType.Success)
                {
                    throw new ArgumentException(
                        message:    "A failure-state SV.ParseResult cannot be " +
                                    "created with the [Success] status.",
                        paramName:  nameof(error));
                }

                _type = error;
                _version = null;
                _successfulCreation = true;
            }

            /// <summary>
            /// <para>
            /// The result code describing the parse result.
            /// </para>
            /// </summary>
            public ParseResultType Type => VerifyResult(_type);
            /// <summary>
            /// <para>
            /// The produced <see cref="SemanticVersion"/>, if
            /// the parsing was successful.
            /// </para>
            /// </summary>
            public SemanticVersion Version => VerifyResult(_version);

            /// <summary>
            /// <para>
            /// Creates the appropriate exception for the current
            /// instance's <see cref="Type"/> value.
            /// </para>
            /// </summary>
            /// <returns>
            /// An <see cref="Exception"/> appropriate for the value
            /// of the current instance's <see cref="Type"/> field.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="Type"/> has the value
            /// <see cref="ParseResultType.Success"/>.
            /// </exception>
            public Exception CreateException()
            {
                // REMEMBER: If changing the exceptions here, update the
                //           documentation for the [Parse] method(s).

                if (this.Type == ParseResultType.Success)
                {
                    throw new InvalidOperationException(
                        "Cannot create exception: operation was successful.");
                }

                var msg = this.GetErrorMessage();

                switch (this.Type)
                {
                    // [ArgumentNullException] thrown when the version string
                    // is empty.
                    case ParseResultType.NullString:
                    return new ArgumentNullException(message: msg, 
                                                     innerException: null);

                    // [ArgumentException] thrown when an expected component is
                    // not present.
                    case ParseResultType.TrioItemMissing:
                    case ParseResultType.IdentifierMissing:
                    case ParseResultType.MetadataMissing:
                    return new ArgumentException(message: msg);

                    // [FormatException] thrown when an invalid character is
                    // encountered.
                    case ParseResultType.PreTrioInvalidChar:
                    case ParseResultType.TrioInvalidChar:
                    case ParseResultType.TrioItemLeadingZero:
                    case ParseResultType.IdentifierInvalid:
                    case ParseResultType.MetadataInvalid:
                    return new FormatException(message: msg);

                    // [OverflowException] thrown when an integer overflow is
                    // encountered.
                    case ParseResultType.TrioItemOverflow:
                    return new OverflowException(message: msg);

                    default:
                    {
                        // Not using [ArgumentException] here so any time this
                        // is thrown (which should be never) won't accidentally
                        // be caught by user code catching [ArgumentException].
                        throw new NotSupportedException(
                            $"Unrecognised result code {(int)Type:X8} provided.");
                    }
                }
            }
            /// <summary>
            /// <para>
            /// Retrieves a human-friendly error message describing the
            /// value of <see cref="Type"/>.
            /// </para>
            /// </summary>
            /// <returns>
            /// A human-friendly error message describing the value of
            /// <see cref="Type"/>.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="Type"/> has the value
            /// <see cref="ParseResultType.Success"/>.
            /// </exception>
            public string GetErrorMessage()
            {
                switch (this.Type)
                {
                    case ParseResultType.NullString:
                    return "The provided version string was null or empty.";

                    case ParseResultType.PreTrioInvalidChar:
                    case ParseResultType.TrioInvalidChar:
                    return "The version string contained an invalid character.";

                    case ParseResultType.TrioItemLeadingZero:
                    return "The major, minor, and patch versions may not have " +
                           "leading zeroes.";

                    case ParseResultType.TrioItemMissing:
                    return "The version string was missing a version component.";

                    case ParseResultType.TrioItemOverflow:
                    return "One or more of the major, minor, or patch " +
                           "versions represented a number greater than the " +
                           "supported maximum.";

                    case ParseResultType.IdentifierMissing:
                    return "A pre-release identifier was not found where one " +
                           "was expected.";

                    case ParseResultType.IdentifierInvalid:
                    return "One or more pre-release identifiers were invalid.";

                    case ParseResultType.MetadataMissing:
                    return "A build metadata item was not found where one " +
                           "was expected";

                    case ParseResultType.MetadataInvalid:
                    return "One or more build metadata items were invalid.";

                    case ParseResultType.Success:
                    {
                        throw new InvalidOperationException(
                            "Cannot retrieve message: operation was successful.");
                    };
                    default:
                    {
                        throw new NotSupportedException(
                            $"Unrecognised result code {(int)Type:X8} provided.");
                    };
                }
            }
        }

        /// <summary>
        /// <para>
        /// Encapsulates the parser for the <see cref="SemanticVersion"/> class.
        /// </para>
        /// </summary>
        internal static class Parser
        {
            /// <summary>
            /// <para>
            /// Represents the states of the <see cref="SemanticVersion"/>
            /// parser.
            /// </para>
            /// </summary>
            private enum State
            {
                Major,
                Minor,
                Patch,
                Identifiers,
                Metadata
            }

            /// <summary>
            /// <para>
            /// The character used to start the sequence of pre-release
            /// identifiers (a hyphen).
            /// </para>
            /// </summary>
            private const char IdentifierStart = '-';
            /// <summary>
            /// <para>
            /// The character used to start the sequence of build metadata
            /// items (a plus sign).
            /// </para>
            /// </summary>
            private const char MetadataStart = '+';
            /// <summary>
            /// <para>
            /// The character used to separate various version
            /// compoents (a period).
            /// </para>
            /// </summary>
            private const char ComponentSeparator = '.';

            private static readonly object _lockbox = new object();

            /// <summary>
            /// <para>
            /// Attempts to normalise the version string provided as input
            /// to the parser.
            /// </para>
            /// </summary>
            /// <param name="input">
            /// The version string to normalise.
            /// </param>
            /// <param name="mode">
            /// Modes augmenting how parsing is performed.
            /// </param>
            /// <returns>
            /// A <see cref="ParseResult"/> indicating whether normalisation
            /// was successful or, if it wasn't successful, why it failed.
            /// </returns>
            internal static ParseResultType Normalise(ref string input, 
                                                      ParseMode mode)
            {
                // Normalisation should make our attempt at caching the result
                // of the parser (and so speeding up programs that use a lot of
                // parsed semantic versions) more effective.
                //
                // Once normalised, strings like ["v1.2.0"] and ["1.2.0"] would
                // appear identical, and leading and trailing whitespace is not
                // preserved.
                //
                // Normalisation is required for this because we're going to be
                // feeding the string representation into an [IDictionary<>] to
                // make a map of string representations to class instances.

                // We can't normalise a string with no content, so if it's
                // null, empty, or whitespace, indicate that it is null.
                if (String.IsNullOrWhiteSpace(input))
                    return ParseResultType.NullString;

                // Removing whitespace is the first step of normalisation. Other
                // [Parse] methods (such as on [Int32]) do it, so to maintain the
                // Principle of Least Astonishment we're doing it too.
                input = input.Trim();

                // Next, we're going to remove a prefixing "v" or "V" if it is
                // present and the flags we've been passed ([mode]) allow us to
                // do so.
                //
                // We should be safe to use the indexer because we've already
                // verified that the string is not entirely whitespace, so 
                // trimming should give us at least a single character.
                if (!Helper.IsNumber(input[0]))
                {
                    // If the first character isn't a number and we haven't
                    // been passed the [AllowPrefix] flag, there's no point in
                    // checking what character it is because it's definitely
                    // invalid.
                    //
                    // However, if we have been passed the [AllowPrefix] flag
                    // and the character is neither "v" nor "V", we have to
                    // return the same result because it's invalid.
                    if (!mode.HasFlag(ParseMode.AllowPrefix))
                        return ParseResultType.PreTrioInvalidChar;
                    else if (input[0] != 'v' && input[0] != 'V')
                        return ParseResultType.PreTrioInvalidChar;

                    // If we end up here, we know that we've been passed the
                    // [AllowPrefix] flag and that the first character is a
                    // "v" or "V". We don't want to pass the first character
                    // to the parser (because it isn't meaningful), so we need
                    // to substring the string.
                    input = input.Substring(startIndex: 1);
                }

                // If we haven't returned yet, it means everything should be
                // good and we can indicate success to the caller.
                return ParseResultType.Success;
            }

            /// <summary>
            /// <para>
            /// Implements <see cref="SemanticVersion"/> parsing.
            /// </para>
            /// </summary>
            /// <param name="input">
            /// The string representing the semantic version.
            /// </param>
            /// <param name="mode">
            /// Modes augmenting how parsing is performed.
            /// </param>
            /// <returns>
            /// A <see cref="ParseResult"/> describing whether the
            /// parse succeeded.
            /// </returns>
            private static ParseResult _parseVersion(string input, 
                                                     ParseMode mode)
            {
                // This could all probably be accomplished with a single
                // (probably ugly) regular expression, but we probably
                // wouldn't be able to provide as-specific error messages
                // if we did that.
                //
                // I'll take slower parsing for better error reporting
                // any day of the week.

                // The major version is the first thing we're expecting,
                // so the [Major] state seems like a good pick for initial
                // state.
                var state = State.Major;
                var sb = new StringBuilder();

                // We're converting to a list of nullable characters to
                // make handling the end of the string easier.
                //
                // Instead of having some duplicate code outside of the
                // loop, we can now detect null as appropriate.
                var chars = new List<char?>(input.ToCharArray()
                                                 .Select(c => (char?)c));
                chars.Add(null);

                // Negative values will cause an exception if we pass them
                // to the [SemanticVersion] constructor.
                int major = -1, minor = -1, patch = -1;
                // Null values will too, as well as causing an NRE if we try
                // to use them before we've given them a proper value.
                ICollection<string> identifiers = new List<string>(),
                                    metadata = new List<string>();

                foreach (char? c in chars)
                {
                    switch (state)
                    {
                        // [Major], [Minor], and [Patch] state handling is mostly
                        // the same. If you're looking at [Minor]/[Patch] and
                        // are wondering why some bits are sparsely-commented,
                        // look at the [Major] state to see comments.
                        //
                        // Similarly, [Identifiers] has more comments than
                        // [Metadata].

                        #region Major
                        case State.Major:
                        {
                            // No value means we've hit the end of the
                            // string. This isn't allowed in the major
                            // version, so we raise an error.
                            if (!c.HasValue)
                            {
                                return new ParseResult(
                                    error: ParseResultType.TrioItemMissing);
                            }
                            // If it has a value, we want to check if it's
                            // a number before we add it to the builder.
                            else if (Helper.IsNumber(c.Value))
                            {
                                // We're leaving leading-zero checks until
                                // we hit the end of the major version as
                                // doing it that way simplifies the check.
                                sb.Append(c.Value);
                            }
                            // If it's not a number, it might be a period,
                            // which would indicate that we're now moving
                            // on to the minor version.
                            else if (c.Value == ComponentSeparator)
                            {
                                // If there's nothing in the [StringBuilder],
                                // it means we didn't have a major version, so
                                // we need to report a missing component.
                                if (sb.Length == 0)
                                    return new ParseResult(
                                        error: ParseResultType.TrioItemMissing);

                                // Major versions can't have leading zeroes, so
                                // if the major version is more than a single
                                // digit long and its first digit is a leading
                                // zero, we need to raise an error, too.
                                if (sb.Length > 1 && sb[0] == '0')
                                    return new ParseResult(
                                        ParseResultType.TrioItemLeadingZero);

                                // Next step, we need to convert the numeric
                                // major version component string to an integer
                                // so we can have it in our [SemanticVersion]
                                // class. We know that the string is a valid
                                // number (i.e. composed of characters 0-9),
                                // so the only reason this call should fail is
                                // if the number is too long and causes an
                                // overflow.
                                if (!Int32.TryParse(sb.ToString(), out major))
                                    return new ParseResult(
                                        ParseResultType.TrioItemOverflow);

                                // If we're here, we've got our major version
                                // parsed and now we need to handle the minor
                                // version.
                                state = State.Minor;
                                // Remember to clear the [StringBuilder] so
                                // the next iteration doesn't get any of our
                                // leftover state.
                                sb.Clear();
                            }
                            // These characters have special significance in
                            // that they are used to indicate the start of
                            // identifiers or metadata after the major, minor,
                            // and patch versions.
                            //
                            // If we hit one in the [Major] version state, it
                            // means that both the minor and patch version
                            // components are missing.
                            else if (c == IdentifierStart || c == MetadataStart)
                            {
                                return new ParseResult(
                                    error: ParseResultType.TrioItemMissing);
                            }
                            // If it's none of the above, it's the character
                            // is invalid so we have to raise an error.
                            else
                            {
                                return new ParseResult(
                                    error: ParseResultType.TrioInvalidChar);
                            }
                        }
                        break;
                        #endregion
                        #region Minor
                        case State.Minor:
                        {
                            if (!c.HasValue ||
                                c.Value == ComponentSeparator ||
                                c.Value == IdentifierStart    ||
                                c.Value == MetadataStart)
                            {
                                if (sb.Length == 0)
                                    return new ParseResult(
                                        error: ParseResultType.TrioItemMissing);

                                if (sb.Length > 1 && sb[0] == '0')
                                    return new ParseResult(
                                        ParseResultType.TrioItemLeadingZero);

                                if (!Int32.TryParse(sb.ToString(), out minor))
                                    return new ParseResult(
                                        ParseResultType.TrioItemOverflow);

                                if (!c.HasValue)
                                {
                                    // If the [OptionalPatch] flag is present, 
                                    // then we're going to allow the version 
                                    // string to end in the [Minor] state.
                                    if (mode.HasFlag(ParseMode.OptionalPatch))
                                    {
                                        return new ParseResult(
                                            new SemanticVersion(
                                                major: major,
                                                minor: minor
                                            ));
                                    }

                                    // If it isn't, then this is an invalid 
                                    // version and we want to return an error.
                                    return new ParseResult(
                                        error: ParseResultType.TrioItemMissing);
                                }
                                else if (c.Value == ComponentSeparator)
                                    state = State.Patch;
                                // Here's the bit where [Minor] differs from
                                // [Major] in how it's handled. The [ParseMode]
                                // flag has a flag, [OptionalPatch], that allows
                                // the caller to omit the patch version.
                                //
                                // If the character is a hyphen or plus sign, we
                                // are going to test for that flag.
                                else if (c.Value == MetadataStart ||
                                         c.Value == IdentifierStart)
                                {
                                    // If the flag isn't present, it means that
                                    // the version string is trying to omit the
                                    // patch version without enabling the ability
                                    // to do so, which is an error.
                                    if (!mode.HasFlag(ParseMode.OptionalPatch))
                                        return new ParseResult(
                                            ParseResultType.TrioItemMissing);
                                    
                                    // We've been passed the correct flag and, if
                                    // we're here, there is no patch version
                                    // present, so we want to default it to zero.
                                    patch = 0;

                                    // If the flag is set, then we need to make
                                    // a decision on the state to transition to
                                    // based on the character we're on.
                                    //
                                    // Hyphen for identifiers, plus for metadata.
                                    if (c.Value == IdentifierStart)
                                        state = State.Identifiers;
                                    else
                                        state = State.Metadata;
                                }

                                sb.Clear();
                            }
                            else if (Helper.IsNumber(c.Value))
                            {
                                sb.Append(c.Value);
                            }
                            else
                            {
                                return new ParseResult(
                                    error: ParseResultType.TrioInvalidChar);
                            }
                        }
                        break;
                        #endregion
                        #region Patch
                        case State.Patch:
                        {
                            // The [Patch] state differs slightly from [Major] and
                            // [Minor] because it is acceptable for the end of the
                            // string to occur in the [Patch] state.
                            //
                            // We're going to share the end-of-string code with
                            // the identifier-start/metadata-start code so we
                            // don't repeat ourselves more than we already have.
                            if (!c.HasValue ||
                                c == IdentifierStart || c == MetadataStart)
                            {
                                // All the "try to turn this into an actual
                                // number" code from the other states.
                                if (sb.Length == 0)
                                    return new ParseResult(
                                        error: ParseResultType.TrioItemMissing);

                                if (sb.Length > 1 && sb[0] == '0')
                                    return new ParseResult(
                                        ParseResultType.TrioItemLeadingZero);

                                if (!Int32.TryParse(sb.ToString(), out patch))
                                    return new ParseResult(
                                        ParseResultType.TrioItemOverflow);

                                // If we hit a hyphen, we need to transition to
                                // the identifier-parsing state.
                                if (c == IdentifierStart)
                                    state = State.Identifiers;
                                // Similarly, if we hit a plus we need to move
                                // into the metadata-parsing state.
                                else if (c == MetadataStart)
                                    state = State.Metadata;
                                // If it isn't either, then it's the end of the
                                // string and we want to indicate success by
                                // returning the parsed [SemanticVersion].
                                else
                                    return new ParseResult(new SemanticVersion(
                                        major: major,
                                        minor: minor,
                                        patch: patch
                                        ));

                                // If we're transitioning into a new state, we
                                // want the [StringBuilder] to be clear so it
                                // doesn't end up with any of our data.
                                sb.Clear();
                            }
                            else if (Helper.IsNumber(c.Value))
                            {
                                sb.Append(c);
                            }
                            else
                            {
                                return new ParseResult(
                                    error: ParseResultType.TrioInvalidChar);
                            }
                        }
                        break;
                        #endregion
                        #region Identifiers
                        case State.Identifiers:
                        {
                            // When we get here, the hyphen will have already
                            // been consumed by the previous state's handler, so
                            // we can launch straight in to parsing identifiers.

                            // What we do when we hit the end of the string or a
                            // component separator (period) is much the same.
                            if (!c.HasValue || c.Value == ComponentSeparator ||
                                c == MetadataStart)
                            {
                                // First, we check to see whether we actually
                                // picked up any content that could be an
                                // identifier.
                                //
                                // If we didn't, it means an identifier is missing
                                // where one is expected, and that's an error.
                                if (sb.Length == 0)
                                {
                                    return new ParseResult(
                                        error: ParseResultType.IdentifierMissing);
                                }

                                // If there is content, then we want to check to
                                // make sure that it's a valid identifier.
                                //
                                // If it isn't, that's also an error so we need
                                // to report it.
                                if (!Helper.IsValidIdentifier(sb.ToString()))
                                {
                                    return new ParseResult(
                                        error: ParseResultType.IdentifierInvalid);
                                }

                                // And finally, if it passes the above checks, we
                                // can add it to our list of identifiers and clear
                                // the string builder for the next iteration.
                                identifiers.Add(sb.ToString());
                                sb.Clear();

                                // Now we check to see whether this is the end of
                                // the string. If it is, we want to return the
                                // created [SemanticVersion].
                                if (!c.HasValue)
                                {
                                    return new ParseResult(new SemanticVersion(
                                        major:          major,
                                        minor:          minor,
                                        patch:          patch,
                                        identifiers:    identifiers
                                        ));
                                }
                                // If it isn't the end of the string, it might be
                                // the start of the metadata. If it is, we want to
                                // transition to the metadata-parsing state.
                                else if (c == MetadataStart)
                                {
                                    state = State.Metadata;
                                }
                            }
                            // If it isn't any of the characters we've previously
                            // checked for, we add it to the [StringBuilder]. We
                            // don't care about whether it's valid, because that
                            // will be checked in future.
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        break;
                        #endregion
                        #region Metadata
                        case State.Metadata:
                        {
                            // The [Metadata] state is much the same as the
                            // [Identifiers] state, except for the validation
                            // method called and it not being able to switch
                            // to other states (metadata is always last).
                            //
                            // Like with [Identifiers] and hyphens, we don't
                            // need to handle a leading plus sign because it
                            // will have been consumed by the previous state.

                            if (!c.HasValue || c == ComponentSeparator)
                            {
                                if (sb.Length == 0)
                                {
                                    return new ParseResult(
                                        error: ParseResultType.MetadataMissing);
                                }

                                if (!Helper.IsValidMetadata(sb.ToString()))
                                {
                                    return new ParseResult(
                                        error: ParseResultType.MetadataInvalid);
                                }

                                metadata.Add(sb.ToString());
                                sb.Clear();

                                if (!c.HasValue)
                                {
                                    return new ParseResult(new SemanticVersion(
                                        major:          major,
                                        minor:          minor,
                                        patch:          patch,
                                        identifiers:    identifiers,
                                        metadata:       metadata
                                        ));
                                }
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        break;
                        #endregion
                    }
                }

                // We should never be able to enter this state, because we should
                // never break out of the switch statement.
                //
                // If we do hit this state, we want to immediately throw an
                // exception because something must have gone horribly, horribly
                // wrong and we're probably not recoverable.
                throw new InvalidOperationException(
                    "Invalid state entered: SemanticVersion parser error."
                    );
            }

            /// <summary>
            /// <para>
            /// Implements <see cref="SemanticVersion"/> parsing and
            /// input preprocessing.
            /// </para>
            /// </summary>
            /// <param name="input">
            /// The string representing the semantic version.
            /// </param>
            /// <param name="mode">
            /// Modes augmenting how parsing is performed.
            /// </param>
            /// <returns>
            /// A <see cref="ParseResult"/> describing whether the
            /// parse succeeded.
            /// </returns>
            public static ParseResult Parse(string input, ParseMode mode)
            {
                // First thing we do is normalise the string. This gives us
                // the string in a format we can understand properly.
                var normaliseResult = Parser.Normalise(ref input, mode);

                // If normalisation was not successful, then we pass on the
                // error code it returned to the caller.
                if (normaliseResult != ParseResultType.Success)
                    return new ParseResult(normaliseResult);

                SemanticVersion cacheResult = null;
                ParseResult result;

                // We don't know what the memoization agent actually is, and
                // whether a user intends on accessing it. By locking on the
                // property, it is possible for the parser and the user (who 
                // may be doing anything with their cache) to safely access
                // the cache.
                //
                // However, [MemoizationAgent] won't always have a value, so
                // we have a private object to lock on when this is the case.
                var lockObj = MemoizationAgent ?? _lockbox;
                lock (lockObj)
                {
                    // Whoever's using the library may or may not have configured
                    // a memoization agent for us. If they have, we're going to
                    // see if the input string is in the cache.
                    //
                    // If they haven't configured an agent, we'll go straight to
                    // parsing. We'll also do this if the input isn't in the
                    // cache.
                    if (MemoizationAgent?.TryGetValue(input, out cacheResult) 
                            != true)
                    {
                        // There is no cache, or the item isn't in the cache.
                        // This means we have to try and parse the input.
                        result = _parseVersion(input, mode);

                        // If parsing was successful, and if there is a cache
                        // configured, add the result to the cache.
                        if (result.Type == ParseResultType.Success)
                            MemoizationAgent?.Add(input, result.Version);
                    }
                    // The item was in our cache. Our result is what we've
                    // retrieved from our cache.
                    else
                    {
                        result = new ParseResult(cacheResult);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// <para>
        /// Converts a version string to a <see cref="SemanticVersion"/>,
        /// taking into account a set of flags.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The version string to be converted to a <see cref="SemanticVersion"/>.
        /// </param>
        /// <param name="mode">
        /// A set of flags that augment how the version string is parsed.
        /// </param>
        /// <returns>
        /// A <see cref="SemanticVersion"/> equivalent to the provided version
        /// string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="version"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a component in the version string was expected but not
        /// found (for example, a missing minor or patch version).
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when an invalid character or character sequence is encountered.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when an attempt to convert the major, minor, or patch version
        /// into an <see cref="int"/> resulted in an overflow.
        /// </exception>
        public static SemanticVersion Parse(string version, ParseMode mode)
        {
            var result = Parser.Parse(version, mode);

            // If the parsing was successful, return the created version.
            if (result.Type == ParseResultType.Success)
                return result.Version;

            // If it wasn't, create and throw the appropriate exception.
            throw result.CreateException();
        }
        /// <summary>
        /// <para>
        /// Converts a version string to a <see cref="SemanticVersion"/>, only
        /// accepting the format given in the Semantic Versioning specification.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The version string to be converted to a <see cref="SemanticVersion"/>.
        /// </param>
        /// <returns>
        /// A <see cref="SemanticVersion"/> equivalent to the provided version
        /// string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="version"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a component in the version string was expected but not
        /// found (for example, a missing minor or patch version).
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when an invalid character or character sequence is encountered.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when an attempt to convert the major, minor, or patch version
        /// into an <see cref="int"/> resulted in an overflow.
        /// </exception>
        /// <remarks>
        /// This method is equivalent to calling 
        /// <see cref="Parse(string, ParseMode)"/> and passing the value
        /// <see cref="ParseMode.Strict"/>.
        /// </remarks>
        public static SemanticVersion Parse(string version)
        {
            return SemanticVersion.Parse(version, ParseMode.Strict);
        }

        /// <summary>
        /// <para>
        /// Attempts to convert a version string to a 
        /// <see cref="SemanticVersion"/>, taking into account a set of flags.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The version string to be converted to a <see cref="SemanticVersion"/>.
        /// </param>
        /// <param name="mode">
        /// A set of flags that augment how the version string is parsed.
        /// </param>
        /// <param name="semver">
        /// When the method returns, this parameter is either set to the created
        /// <see cref="SemanticVersion"/> (if parsing was sucessful), or is given
        /// an undefined value (if parsing was unsuccessful).
        /// </param>
        /// <returns>
        /// True if parsing succeeded, false if otherwise.
        /// </returns>
        public static bool TryParse(string version, ParseMode mode,
                                    out SemanticVersion semver)
        {
            var result = Parser.Parse(version, mode);
            
            // We don't need to perform any checks here. Either we had
            // a success and [Version] has a value, or we didn't and it's
            // null.
            semver = result.Version;

            // Only [ParseResultType.Success] indicates a successful parsing
            // and the producing of a [SemanticVersion] instance.
            return result.Type == ParseResultType.Success;
        }
        /// <summary>
        /// <para>
        /// Attempts to convert a version string to a 
        /// <see cref="SemanticVersion"/>.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The version string to be converted to a <see cref="SemanticVersion"/>.
        /// </param>
        /// <param name="semver">
        /// When the method returns, this parameter is either set to the created
        /// <see cref="SemanticVersion"/> (if parsing was sucessful), or is given
        /// an undefined value (if parsing was unsuccessful).
        /// </param>
        /// <returns>
        /// True if parsing succeeded, false if otherwise.
        /// </returns>
        /// <remarks>
        /// This method is equivalent to calling
        /// <see cref="TryParse(string, ParseMode, out SemanticVersion)"/> and
        /// passing the value <see cref="ParseMode.Strict"/>.
        /// </remarks>
        public static bool TryParse(string version, out SemanticVersion semver)
        {
            return SemanticVersion.TryParse(version, ParseMode.Strict, out semver);
        }


        /// <summary>
        /// <para>
        /// The cache to use to memoize the results of the 
        /// <see cref="SemanticVersion"/> parsing methods.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Assign <c>null</c> to disable memoization. The value of this
        /// property is <c>null</c> by default.
        /// </para>
        /// <para>
        /// Accesses by the parser to the memoization agent are surrounded
        /// by <c>lock (MemoizationAgent)</c>.
        /// </para>
        /// </remarks>
        public static IDictionary<string, SemanticVersion> MemoizationAgent
        {
            get;
            set;
        }
    }
}
