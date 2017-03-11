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
using System.Linq;
using System.Text;

using McSherry.SemanticVersioning.Internals.Shims;
using static McSherry.SemanticVersioning.Ranges.VersionRange.ParseResultType;

using DD = System.Diagnostics.DebuggerDisplayAttribute;

namespace McSherry.SemanticVersioning.Ranges
{
    using ResultSet = IEnumerable<IEnumerable<VersionRange.ComparatorToken>>;

    // Documentation and attributes are in 'Ranges\VersionRange.cs'
    public sealed partial class VersionRange
    {
        /// <summary>
        /// <para>
        /// Represents the operators that the <see cref="VersionRange"/>
        /// parser recognises.
        /// </para>
        /// </summary>
        internal enum Operator
        {
            /// <summary>
            /// <para>
            /// Used to check version equality. Assumed when no operator
            /// is specified.
            /// </para>
            /// </summary>
            Equal,
            /// <summary>
            /// <para>
            /// Used in checking that a <see cref="SemanticVersion"/> has
            /// lesser precedence than the specified version.
            /// </para>
            /// </summary>
            LessThan,
            /// <summary>
            /// <para>
            /// Used in checking that a <see cref="SemanticVersion"/> has
            /// greater precedence than the specified version.
            /// </para>
            /// </summary>
            GreaterThan,
            /// <summary>
            /// <para>
            /// Used in checking that a <see cref="SemanticVersion"/> is
            /// equal or has lesser precedence than the specified version.
            /// </para>
            /// </summary>
            LessThanOrEqual,
            /// <summary>
            /// <para>
            /// Used in checking that a <see cref="SemanticVersion"/> is
            /// equal or has greater precedence than the specified version.
            /// </para>
            /// </summary>
            GreaterThanOrEqual,
        }

        /// <summary>
        /// <para>
        /// Represents an <see cref="Operator"/> and <see cref="SemanticVersion"/>
        /// grouping identified by the parser.
        /// </para>
        /// </summary>
        internal struct ComparatorToken
        {
            private readonly Operator _op;
            private readonly SemanticVersion _sv;
            private readonly bool _valid;

            /// <summary>
            /// <para>
            /// Ensures that the the current instance has been
            /// correctly constructed and is valid.
            /// </para>
            /// </summary>
            /// <typeparam name="T">
            /// The type of the value to pass through on valid
            /// construction.
            /// </typeparam>
            /// <param name="passthrough">
            /// A value to be passed through if the current
            /// instance is valid.
            /// </param>
            /// <returns>
            /// If the current instance is valid, returns the
            /// value given in <paramref name="passthrough"/>.
            /// Otherwise, throws an exception.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when the current instance is not valid.
            /// </exception>
            private T Validate<T>(T passthrough)
            {
                if (_valid)
                    return passthrough;

                throw new InvalidOperationException(
                    "An attempt was made to use a Comparator that was " +
                    "not correctly constructed."
                    );
            }

            /// <summary>
            /// <para>
            /// Creates a new <see cref="ComparatorToken"/> instance with
            /// the specified operator and semantic version.
            /// </para>
            /// </summary>
            /// <param name="op">
            /// The <see cref="VersionRange.Operator"/> to use.
            /// </param>
            /// <param name="semver">
            /// The <see cref="SemanticVersion"/> to use.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="semver"/> is null.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="op"/> is not a recognised
            /// <see cref="VersionRange.Operator"/>.
            /// </exception>
            public ComparatorToken(Operator op, SemanticVersion semver)
            {
                if (semver == null)
                {
                    throw new ArgumentNullException(
                        message:    "The specified version cannot be null.",
                        paramName:  nameof(semver)
                        );
                }

                if (!Enum.IsDefined(typeof(Operator), op))
                {
                    throw new ArgumentException(
                        message:    "The specified operator is not valid.",
                        paramName:  nameof(op)
                        );
                }

                _op = op;
                _sv = semver;
                _valid = true;
            }

            /// <summary>
            /// <para>
            /// The operator used during comparison.
            /// </para>
            /// </summary>
            public Operator Operator => Validate(_op);
            /// <summary>
            /// <para>
            /// The semantic version that is compared against.
            /// </para>
            /// </summary>
            public SemanticVersion Version => Validate(_sv);
        }

        /// <summary>
        /// <para>
        /// Represents the various possible return statuses of the
        /// <see cref="VersionRange"/> parser.
        /// </para>
        /// </summary>
        internal enum ParseResultType
        {
            /// <summary>
            /// <para>
            /// Parsing completed without issue.
            /// </para>
            /// </summary>
            Success,

            /// <summary>
            /// <para>
            /// The version range string is null, empty, or contains
            /// only whitespace characters.
            /// </para>
            /// </summary>
            NullString,
            /// <summary>
            /// <para>
            /// There are one or more invalid characters in the
            /// version range string.
            /// </para>
            /// </summary>
            InvalidCharacter,

            /// <summary>
            /// <para>
            /// One or more comparator sets contain no comparators.
            /// </para>
            /// </summary>
            EmptySet,
            /// <summary>
            /// <para>
            /// An operator is present with no attached version.
            /// </para>
            /// </summary>
            OrphanedOperator,
            /// <summary>
            /// <para>
            /// The version range string contains an invalid
            /// semantic version string.
            /// </para>
            /// </summary>
            InvalidVersion,
        }

        /// <summary>
        /// <para>
        /// Represents the result produced by the <see cref="VersionRange"/>
        /// parser.
        /// </para>
        /// </summary>
        [DD("Type={Type}")]
        internal struct ParseResult
        {
            // Certain codes are required to be provided with an exception. 
            // This hashset will contain those specific codes so we can 
            // check for them.
            private static readonly HashSet<ParseResultType> _exCodes;

            static ParseResult()
            {
                _exCodes = new HashSet<ParseResultType>
                {
                    // [InvalidVersion] needs an exception so that the exact
                    // reason for the version string being invalid can be relayed.
                    InvalidVersion,
                };
            }

            // This will be false if we're default-constructed.
            private readonly bool _successfulCreation;
            // Property backing fields.
            private readonly ParseResultType _type;
            private readonly ResultSet _results;
            private readonly Lazy<Exception> _innerEx;

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
                    "Attempt to use a [VersionRange.ParseResult] that was " +
                    "default-constructed.");
            }

            /// <summary>
            /// <para>
            /// Creates a parse result indicating success and
            /// containing the specified results.
            /// </para>
            /// </summary>
            /// <param name="results">
            /// The results for the <see cref="ParseResult"/>
            /// to contain.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="results"/> or
            /// any of its items are null.
            /// </exception>
            public ParseResult(ResultSet results)
            {
                if (results?.Any(cs => cs == null) != false)
                {
                    throw new ArgumentNullException(
                        paramName:  nameof(results),
                        message:    "The result set cannot be null, and " +
                                    "cannot contain null items."
                        );
                }

                _type = ParseResultType.Success;
                _results = results;
                _innerEx = null;
                _successfulCreation = true;
            }
            /// <summary>
            /// <para>
            /// Creates a parse result indicating failure and
            /// with the specified result code.
            /// </para>
            /// </summary>
            /// <param name="error">
            /// The result code for the <see cref="ParseResult"/>
            /// to report.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="error"/> is invalid
            /// or unrecognised.
            /// </exception>
            public ParseResult(ParseResultType error)
            {
                if (error == Success)
                {
                    throw new ArgumentException(
                        message:    "A failure-state [VR.ParseResult] cannot " +
                                    "be created with the [Success] status.",
                        paramName:  nameof(error));
                }

                if (!Enum.IsDefined(typeof(ParseResultType), error))
                {
                    throw new ArgumentException(
                        message:    $"The parse result code {(int)error:X8} is " +
                                     "not recognised.",
                        paramName:  nameof(error));
                }

                // Some status codes must be provided with an exception. We
                // need to check for these in this constructor, as this ctor
                // doesn't take an exception argument.
                if (_exCodes.Contains(error))
                {
                    throw new ArgumentException(
                        message:    $"The parse result code {(int)error:X8} is " +
                                     "required to be passed with an exception.",
                        paramName:  nameof(error));
                }

                _type = error;
                _results = null;
                _innerEx = new Lazy<Exception>(() => null);
                _successfulCreation = true;
            }
            /// <summary>
            /// <para>
            /// Creates a parse result indicating failure and with
            /// the specified error code and inner exception.
            /// </para>
            /// </summary>
            /// <param name="error">
            /// The result code for the <see cref="ParseResult"/>
            /// to report.
            /// </param>
            /// <param name="innerException">
            /// The inner exception to store in the parse result.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="innerException"/> is null.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="error"/> is invalid
            /// or unrecognised.
            /// </exception>
            public ParseResult(ParseResultType error, 
                               Lazy<Exception> innerException)
            {
                if (error == Success)
                {
                    throw new ArgumentException(
                        message:    "A failure-state [VR.ParseResult] cannot " +
                                    "be created with the [Success] status.",
                        paramName:  nameof(error));
                }

                if (innerException == null)
                {
                    throw new ArgumentNullException(
                        paramName:  nameof(innerException),
                        message:    "The provided inner exception cannot " +
                                    "be null."
                        );
                }

                if (!Enum.IsDefined(typeof(ParseResultType), error))
                {
                    throw new ArgumentException(
                        message:    $"The parse result code {(int)error:X8} is " +
                                     "not recognised.",
                        paramName:  nameof(error));
                }

                // We want to make sure that the code we've been passed is one of
                // the ones that has to be passed with an exception. If it isn't,
                // then we want to throw an exception.
                if (!_exCodes.Contains(error))
                {
                    throw new ArgumentException(
                        message:   $"The parse result code {(int)error:X8} must" +
                                    " not be passed with an exception.",
                        paramName: nameof(error));
                }

                _type = error;
                _results = null;
                _innerEx = innerException;
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
            /// The collection of comparator sets produced by the
            /// parser.
            /// </para>
            /// </summary>
            public ResultSet ComparatorSets => VerifyResult(_results);
            /// <summary>
            /// <para>
            /// The exception that is stored in the parse result to
            /// provide additional error information.
            /// </para>
            /// </summary>
            private Exception InnerException => VerifyResult(_innerEx.Value);

            /// <summary>
            /// <para>
            /// Retrieves a human-friendly error message describing the
            /// error represented by the current <see cref="ParseResult"/>.
            /// </para>
            /// </summary>
            /// <returns>
            /// A string representing the error represented by the
            /// current <see cref="ParseResult"/>.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="ParseResult.Type"/> has the
            /// value <see cref="ParseResultType.Success"/>.
            /// </exception>
            public string GetErrorMessage()
            {
                if (this.Type == Success)
                {
                    throw new InvalidOperationException(
                        "Cannot retrieve error message: operation was " +
                        "successful."
                        );
                }

                switch (this.Type)
                {
                    case NullString:
                    return "The version range string cannot be null, empty, or " +
                           "composed entirely of whitespace characters.";

                    case InvalidCharacter:
                    return "The version range string contains one or more " +
                           "invalid characters.";

                    case EmptySet:
                    return "A version range string cannot contain a set with " +
                           "no comparators.";

                    case OrphanedOperator:
                    return "The version range string contains an operator " +
                           "with no associated version (check whitespace).";

                    case InvalidVersion:
                    return "The version range string contains one or more " +
                           "invalid semantic version strings.";

                    default:
                    {
                        throw new NotSupportedException(
                            $"Unrecognised result code {(int)Type:X8} provided."
                            );
                    }
                }
            }
            /// <summary>
            /// <para>
            /// Creates an <see cref="Exception"/> instance appropriate
            /// for the status represented by the current instance.
            /// </para>
            /// </summary>
            /// <returns>
            /// An <see cref="Exception"/> instance appropriate for the
            /// status represented by the current instance.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="ParseResult.Type"/> has the
            /// value <see cref="ParseResultType.Success"/>.
            /// </exception>
            public Exception CreateException()
            {
                if (this.Type == Success)
                {
                    throw new InvalidOperationException(
                        "Cannot retrieve exception: operation was successful."
                        );
                }

                var msg = this.GetErrorMessage();

                switch (this.Type)
                {
                    // [ArgumentNullException] for an empty version range string.
                    case NullString:
                    return new ArgumentNullException(msg, this.InnerException);

                    // [ArgumentException] for anything that means the version
                    // range string is invalid, but which is unrelated an invalid
                    // version string being present.
                    case InvalidCharacter:
                    case EmptySet:
                    case OrphanedOperator:
                    return new ArgumentException(msg, this.InnerException);

                    // [FormatException] for anything related to the parsing of a
                    // semantic version string failing.
                    case InvalidVersion:
                    return new FormatException(msg, this.InnerException);

                    default:
                    {
                        throw new NotSupportedException(
                            $"Unrecognised result code {(int)Type:X8} provided."
                            );
                    }
                }
            }
        }

        /// <summary>
        /// <para>
        /// Implements parsing for version ranges.
        /// </para>
        /// </summary>
        internal static class Parser
        {
            private enum State
            {
                /// <summary>
                /// <para>
                /// The starting state, and the state used between
                /// parsing comparators.
                /// </para>
                /// </summary>
                Start,

                /// <summary>
                /// <para>
                /// Entered when a left chevron (<see cref="LeftChevron"/>)
                /// is encountered while in the <see cref="Start"/> state.
                /// </para>
                /// </summary>
                FoundChevL,
                /// <summary>
                /// <para>
                /// Entered when a right chevron (<see cref="RightChevron"/>)
                /// is encountered while in the <see cref="Start"/> state.
                /// </para>
                /// </summary>
                FoundChevR,
                /// <summary>
                /// <para>
                /// Entered when an equals sign (<see cref="EqualSign"/>) is
                /// encountered while in the <see cref="Start"/> state.
                /// </para>
                /// </summary>
                FoundEquals,
                /// <summary>
                /// <para>
                /// Entered when a vertical bar (<see cref="VerticalBar"/> is
                /// encountered while in the <see cref="Start"/> state.
                /// </para>
                /// </summary>
                FoundVBar,

                /// <summary>
                /// <para>
                /// Entered to start version string parsing and collect
                /// the characters that make up a version string.
                /// </para>
                /// </summary>
                VersionCollect,
                /// <summary>
                /// <para>
                /// Entered to finalise version parsing and attempt to
                /// turn all collected characters into a
                /// <see cref="SemanticVersion"/>.
                /// </para>
                /// </summary>
                VersionFinalise,

                /// <summary>
                /// <para>
                /// Entered when the end of the version range string
                /// is encountered.
                /// </para>
                /// </summary>
                EndOfString,
            }

            // Rather than directly typing the literals in the parser,
            // we're going to use a set of constants with appropriate
            // names.
            private const char  LeftChevron     = '<',
                                RightChevron    = '>',
                                EqualSign       = '=',
                                VerticalBar     = '|';

            /// <summary>
            /// <para>
            /// Parses a set of comparator sets from a version range string.
            /// </para>
            /// </summary>
            /// <param name="rangeString">
            /// The string representing one or more comparator sets.
            /// </param>
            /// <returns>
            /// An <see cref="ParseResult"/> indicating whether parsing
            /// was successful and containing the result if it was.
            /// </returns>
            private static ParseResult _parseString(string rangeString)
            {
                if (String.IsNullOrWhiteSpace(rangeString))
                    return new ParseResult(NullString);

                // This is where we push each comparator set once we've collected
                // each comparator in it.
                var setOfSets = new List<IEnumerable<ComparatorToken>>();
                // And this is the list we use to build each comparator set before
                // we push it into [setOfSets].
                var cmpSet = new List<ComparatorToken>();

                // We're using [Nullable<char>]s so we can put a null value at the
                // end of the string to make it easy to detect the last character
                // in the string.
                var chars = rangeString.ToCharArray()
                                       .Select(c => new Nullable<char>(c))
                                       .Concat(new Nullable<char>[] { null });

                var state = State.Start;
                var bdr = new StringBuilder();
                // We need to store the operator while we're parsing the
                // semantic version. We give it the value [-1] to make any
                // checks for a correct operator throw.
                Operator op = (Operator)(-1);

                foreach (var c in chars)
                {
                    switch (state)
                    {
                        #region Start
                        // The [Start] state is used when we have not yet reached
                        // the start of a new comparator or comparator set, either
                        // because we've just started parsing the string, or
                        // because we've just finished parsing one of them.
                        case State.Start:
                        {
                            // No value means the end of our string. We use the
                            // 'goto case' construct so we can skip straight there
                            // without another iteration.
                            if (!c.HasValue)
                                goto case State.EndOfString;
                            // If we hit whitespace, just continue because we
                            // don't really care about it other than when we need
                            // to find the end of a comparator.
                            else if (Char.IsWhiteSpace(c.Value))
                                continue;
                            // If the character isn't any of the above cases,
                            // we're going to see whether it matches any of the
                            // operators we recognise.
                            else switch (c.Value)
                            {
                                case LeftChevron:   state = State.FoundChevL;
                                    continue;

                                case RightChevron:  state = State.FoundChevR;
                                    continue;

                                case EqualSign:     state = State.FoundEquals;
                                    continue;

                                case VerticalBar:   state = State.FoundVBar;
                                    continue;
                            };

                            // If we don't recognise the character, we're going
                            // to end up here. To prevent the character being
                            // lost, we're going to jump straight to the
                            // collection case rather than go through another
                            // iteration.
                            state = State.VersionCollect;
                            goto case State.VersionCollect;
                        };
                        #endregion

                        #region FoundChevL
                        // This state is entered when we find a left chevron in
                        // the string.
                        case State.FoundChevL:
                        {
                            // We're expecting the next character to have a value,
                            // since an operator without an adjacent version is of
                            // no use.
                            //
                            // If the character is whitespace, it isn't associated
                            // with a version so, like we would with no value, we
                            // have to report an orphaned operator.
                            if (!c.HasValue || Char.IsWhiteSpace(c.Value))
                                return new ParseResult(OrphanedOperator);

                            // If the character has a value, then we need to check
                            // that value. The left chevron on its own is an
                            // operator, but there are two-character operators
                            // that include the left chevron as their first
                            // character.
                            switch (c.Value)
                            {
                                // An equals sign means that we're on the operator
                                // "less than or equal to," so we need to set the
                                // current operator to that and move on to parsing
                                // a version string.
                                case EqualSign:
                                {
                                    op = Operator.LessThanOrEqual;
                                    state = State.VersionCollect;
                                }
                                break;

                                // If the character isn't any of the recognised
                                // characters, then it might be part of a version
                                // string.
                                //
                                // We need to add it to the [StringBuilder], set
                                // the operator to the "less than" operator, and
                                // switch into the version-parsing state.
                                default:
                                {
                                    op = Operator.LessThan;
                                    bdr.Append(c);
                                    state = State.VersionCollect;
                                }
                                break;
                            }
                        }
                        break;
                        #endregion
                        #region FoundChevR
                        // The code here is much the same as in the [FoundChevL]
                        // state, so only code that significantly differs will be
                        // commented. For comments that are mostly applicable in
                        // this state, refer to the [FoundChevL] state.
                        case State.FoundChevR:
                        {
                            if (!c.HasValue || Char.IsWhiteSpace(c.Value))
                                return new ParseResult(OrphanedOperator);

                            switch (c.Value)
                            {
                                case EqualSign:
                                {
                                    op = Operator.GreaterThanOrEqual;
                                    state = State.VersionCollect;
                                }
                                break;

                                default:
                                {
                                    op = Operator.GreaterThan;
                                    bdr.Append(c);
                                    state = State.VersionCollect;
                                }
                                break;
                            }
                        }
                        break;
                        #endregion
                        #region FoundEquals
                        // Another operator-identifying state, like [FoundChevL],
                        // although at the time of writing the equals sign op is
                        // not the first character of any multi-character
                        // operators.
                        case State.FoundEquals:
                        {
                            op = Operator.Equal;
                            state = State.VersionCollect;
                            goto case State.VersionCollect;
                        };
                        #endregion
                        #region FoundVBar
                        // An operator-identifying state, but different from the
                        // others because the double vertical bar operator (||)
                        // is used to separate comparator sets, rather than being
                        // an operator that is applied to a version.
                        case State.FoundVBar:
                        {
                            // If the current character is another vertical bar,
                            // then we've got the end of the current comparator
                            // set.
                            if (c == VerticalBar)
                            {
                                // Before we do anything, we need to make sure
                                // that our comparator set has contents. An
                                // empty comparator set is an error.
                                if (!cmpSet.Any())
                                    return new ParseResult(EmptySet);

                                // We have to add the current comparator set to
                                // our set of sets, then create a new list and
                                // make [cmpSet] a reference to the new list.
                                setOfSets.Add(cmpSet.AsReadOnly());
                                cmpSet = new List<ComparatorToken>();

                                // Now we need to go back to our starting state
                                // so we can parse the next comparator set.
                                state = State.Start;
                            }
                            // If the character is anything else, then we've
                            // hit an invalid character and need to return.
                            else return new ParseResult(InvalidCharacter);
                        }
                        break;
                        #endregion
                         
                        #region VersionCollect
                        // This is the state we enter when we're about to try
                        // to parse a version string.
                        case State.VersionCollect:
                        {
                            // If we hit a character with no value, or if we hit
                            // a whitespace character, it means we've hit the end
                            // of both the version range string and the current
                            // version string.
                            //
                            // As we're at the end of the version range string,
                            // we want to attempt to turn the collected characters
                            // into a [SemanticVersion] instance, so we switch to
                            // the [VersionFinalise] state.
                            //
                            // We use 'goto case' so that we don't advance to the
                            // next character, as this might not yet be the end of
                            // the string.
                            if (!c.HasValue || Char.IsWhiteSpace(c.Value))
                                goto case State.VersionFinalise;
                            // If it's not a character indicating the end of the
                            // version range string, then we just add it to the
                            // [StringBuilder] and carry on.
                            else
                            {
                                bdr.Append(c);
                            }
                        }
                        break;
                        #endregion
                        #region VersionFinalise
                        // This is the state we enter when we want to turn any
                        // collected characters into a [SemanticVersion].
                        case State.VersionFinalise:
                        {
                            // If we get here and there's nothing in the
                            // [StringBuilder], it means we encountered an
                            // operator on its own without an immediately
                            // adjacent version.
                            if (bdr.Length == 0)
                                return new ParseResult(OrphanedOperator);

                            // If the operator is still using the invalid value,
                            // then default it to the equality operator.
                            if (op == (Operator)(-1))
                                op = Operator.Equal;

                            // Attempt to parse the version string we have in
                            // our [StringBuilder]. We pass the [AllowPrefix]
                            // option because 'node-semver' allows versions to
                            // be prefixed with 'v'.
                            var verParse = SemanticVersion.Parser.Parse(
                                input:  bdr.ToString(),
                                mode:   ParseMode.AllowPrefix);

                            // If parsing was not successful, then we need to
                            // return a [ParseResult] indicating as much with
                            // the appropriate inner exception.
                            if (verParse.Type != SemanticVersion.ParseResultType
                                                    .Success)
                            {
                                return new ParseResult(
                                    InvalidVersion,
                                    new Lazy<Exception>(verParse.CreateException)
                                    );
                            }

                            // If parsing was successful, then we need to add the
                            // version we just parsed to our comparator set with
                            // the operator it's included with.
                            cmpSet.Add(new ComparatorToken(op, verParse.Version));

                            // If the current character has no value, then this is
                            // the end of the string. There are no more characters
                            // to iterate over, so we can swap directly to the
                            // termination state without going through another
                            // iteration.
                            if (!c.HasValue)
                                goto case State.EndOfString;

                            // If it does have a value, then we're not at the end
                            // of our version range string and we need to prepare
                            // for future iterations.
                            //
                            // To do this, we first clear all the state we set.
                            bdr.Clear();
                            op = (Operator)(-1);

                            // We then make it so the next state we enter is the
                            // [Start] state.
                            state = State.Start;
                        }
                        break;
                        #endregion

                        #region EndOfString
                        // This case handles the end of the string so that other
                        // states don't need to duplicate the code. It also ends
                        // the loop.
                        case State.EndOfString:
                        {
                            // If we hit the end of our string and there are
                            // no items in our comparator set, then it means
                            // we have an empty set, which we're choosing to
                            // disallow.
                            if (cmpSet.Count == 0)
                                return new ParseResult(EmptySet);

                            // If it does have items, then we can move on to
                            // pushing those items into our set of sets. We
                            // call [AsReadOnly] to prevent a casting and
                            // modification down the line.
                            setOfSets.Add(cmpSet.AsReadOnly());

                            goto terminate;
                        };
                        #endregion
                        #region Default
                        default:
                        {
                            // If we end up here, something Very Bad(TM) is
                            // happening and we probably can't recover.
                            throw new InvalidOperationException(
                                message:    "Version range parser in invalid " +
                                            "state."
                                );
                        };
                        #endregion
                    }

                    continue;

                // Yes, using labels and gotos is usually bad practice, but
                // it seems like the best solution here.
                //
                // IMO, there's too much state for the loop to be placed in
                // its own method, and adding a 'keepLooping' flag just
                // clutters the scope.
                //
                // Plus, doing this lets us use a 'foreach' loop instead of
                // a 'for' loop because we don't need to fiddle the counter
                // variable to force a loop exit within a switch.
                terminate:
                    break;
                }

                // Okay, we've parsed our version range string, and we've ended
                // up here so there've been no errors, so all we have left to do
                // is return what we produced.
                //
                // We return a read-only wrapper to prevent any users of the class
                // from modifying the data once we've returned it.
                return new ParseResult(setOfSets.AsReadOnly());
            }

            /// <summary>
            /// Implements <see cref="VersionRange"/> parsing.
            /// </summary>
            /// <param name="rangeString">
            /// The string representing the version range to be parsed.
            /// </param>
            /// <returns>
            /// A <see cref="ParseResult"/> indicating whether parsing
            /// was successful and, if it was, containing a 
            /// <see cref="VersionRange"/> equivalent to the value of
            /// <paramref name="rangeString"/>.
            /// </returns>
            public static ParseResult Parse(string rangeString)
            {
                // Nothing here yet, but we might want to have some intermediary
                // code in future. Who knows.
                return _parseString(rangeString);
            }
        }

        /// <summary>
        /// <para>
        /// Parses a version range from a string.
        /// </para>
        /// </summary>
        /// <param name="range">
        /// The string representing the version range.
        /// </param>
        /// <returns>
        /// A <see cref="VersionRange"/> equivalent to the
        /// value of <paramref name="range"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="range"/> is null, empty,
        /// or contains only whitespace characters.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="range"/> is invalid for any
        /// reason unrelated to an invalid semantic version string.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when <paramref name="range"/> contains an invalid
        /// semantic version string.
        /// </exception>
        public static VersionRange Parse(string range)
        {
            // This constructor does all the same stuff as this method would
            // do, so we might as well just call it instead of reimplementing.
            return new VersionRange(range);
        }
        /// <summary>
        /// <para>
        /// Attempts to parse a version range from a string.
        /// </para>
        /// </summary>
        /// <param name="range">
        /// The string representing the version range.
        /// </param>
        /// <param name="result">
        /// The <see cref="VersionRange"/> that, on success, is
        /// given a value equivalent to <paramref name="range"/>.
        /// </param>
        /// <returns>
        /// True on success, false on failure.
        /// </returns>
        public static bool TryParse(string range, out VersionRange result)
        {
            var parseResult = Parser.Parse(range);

            // If the parsing wasn't successful, set [result] to null and
            // return false to indicate failure.
            if (parseResult.Type != Success)
            {
                result = null;
                return false;
            }

            // If parsing was successful, then exchange all of the comparator
            // tokens we were given for [IComparator] instances that we can
            // pass to a constructor.
            var comparators = parseResult.ComparatorSets.Select(
                tokenSet => tokenSet.Select(
                    token => Comparator.Create(token)
                    )
                );

            result = new VersionRange(comparators);
            return true;
        }
    }
}
