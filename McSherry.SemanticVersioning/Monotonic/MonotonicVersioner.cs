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

using McSherry.SemanticVersioning.Internals.Shims;

namespace McSherry.SemanticVersioning.Monotonic
{
    /// <summary>
    /// <para>
    /// The types of change which may be made to a monotonic-versioned
    /// software package.
    /// </para>
    /// </summary>
    [CLSCompliant(true)]
    public enum MonotonicChange
    {
        /// <summary>
        /// <para>
        /// A backwards-compatible change, where the change would not
        /// break existing uses of the software package's API.
        /// </para>
        /// </summary>
        Compatible,
        /// <summary>
        /// <para>
        /// A breaking change, where the change will break existing uses
        /// of the software package's API.
        /// </para>
        /// </summary>
        Breaking,
    }

    /// <summary>
    /// <para>
    /// Provides a method of working with <see cref="SemanticVersion"/>s as
    /// monotonic versions.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Monotonic Versioning is a simplified versioning scheme that
    /// is compatible with Semantic Versioning 2.0.0. The scheme uses
    /// two components: compatibility, indicating a "line of compatibility"
    /// where all versions with the same component are compatible; and
    /// release, which is incremented with every release, regardless of
    /// the line of compatibility.
    /// </para>
    /// <para>
    /// For example, a first release would be "1.0". A backwards-compatible
    /// update to this would be "1.1". If a breaking change was made, that
    /// release would be "2.2". However, if the first line of compatibility
    /// was updated again, it would be "1.3".
    /// </para>
    /// <para>
    /// The full manifesto is available from the Applied Computer Science
    /// Lab website. This class is based on the 1.2 manifesto.
    /// </para>
    /// </remarks>
    [CLSCompliant(true)]
    public sealed partial class MonotonicVersioner
    {
        // Checks that a collection of metadata is valid and throws if it isn't.
        private static void _verifyMetadataColl(IEnumerable<string> metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(
                    message: "The specified metadata items cannot be null.",
                    paramName: nameof(metadata)
                    );
            }

            if (metadata.Contains(null))
            {
                throw new ArgumentNullException(
                    message: "The specified collection of metadata items " +
                                "cannot contain a null item.",
                    paramName: nameof(metadata)
                    );
            }

            if (!metadata.All(Helper.IsValidMetadata))
            {
                throw new ArgumentException(
                    message: "One or more of the specified metadata items " +
                                "is not a valid metadata item.",
                    paramName: nameof(metadata)
                    );
            }
        }

        // Checks that a sequence is an ordered sequence of contiguous integers.
        private static bool _isOrderedCtgsIntSeq(IEnumerable<int> seq)
        {
            var last = seq.First();
            seq = seq.Skip(1);

            foreach (var n in seq)
            {
                if (last + 1 != n)
                    return false;

                last = n;
            }

            return true;
        }

        // The last version that this instance created.
        private SemanticVersion _latest;
        // A map of compatibility lines to the latest versions
        // within those lines.
        private readonly IDictionary<int, SemanticVersion> _latestVers;
        // An explicit FIFO collection (like a [Queue<T>]) would make the
        // most sense, but unfortunately [ReadOnlyCollection<T>] will only
        // accept an [IList<T>].
        private readonly List<SemanticVersion> _chronology;

        // Adds a [SemanticVersion] to this instance. No error checking is
        // performed.
        private SemanticVersion _add(SemanticVersion sv)
        {
            _latest = sv;
            _latestVers[sv.Major] = sv;
            _chronology.Add(sv);

            return sv;
        }
        // Creates all the various read-only copies used by the instance.
        private void _createReadOnlies()
        {
            this.LatestVersions = _latestVers.AsReadOnly();
            this.Chronology = _chronology.AsReadOnly();
        }

        /// <summary>
        /// <para>
        /// Creates a new <see cref="MonotonicVersioner"/> instance.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The <see cref="Compatibility"/> number sequence produced
        /// by an instance which was created using this constructor
        /// starts at one. If a zero-based sequence is required, use
        /// <see cref="MonotonicVersioner(bool)"/>.
        /// </remarks>
        public MonotonicVersioner()
            : this(startAtOne: true)
        {

        }
        /// <summary>
        /// <para>
        /// Creates a new <see cref="MonotonicVersioner"/> instance
        /// with the specified initial compatibility line.
        /// </para>
        /// </summary>
        /// <param name="startAtOne">
        /// If true, the produced <see cref="Compatibility"/> number
        /// sequence starts at one. If false, zero.
        /// </param>
        /// <remarks>
        /// <para>
        /// The Monotonic Versioning Manifesto 1.2 does not specify
        /// whether the <see cref="Compatibility"/> component of versions 
        /// are to start at one or zero. It is assumed that either is valid 
        /// as neither is specifically recommended nor prohibited.
        /// </para>
        /// <para>
        /// If the <see cref="Compatibility"/> components are to start at
        /// one, <see cref="MonotonicVersioner()"/> may be used.
        /// </para>
        /// </remarks>
        public MonotonicVersioner(bool startAtOne)
            : this(startAtOne, Enumerable.Empty<string>())
        {

        }
        /// <summary>
        /// <para>
        /// Creates a new <see cref="MonotonicVersioner"/> instance with
        /// the specified initial compatibility line and metadata.
        /// </para>
        /// </summary>
        /// <param name="startAtOne">
        /// If true, the produced <see cref="Compatibility"/> number
        /// sequence starts at one. If false, zero.
        /// </param>
        /// <param name="metadata">
        /// Any metadata items to be included as part of the
        /// initial version number.
        /// </param>
        /// <remarks>
        /// <para>
        /// The Monotonic Versioning Manifesto 1.2 does not specify
        /// whether the <see cref="Compatibility"/> component of versions 
        /// are to start at one or zero. It is assumed that either is valid 
        /// as neither is specifically recommended nor prohibited.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="metadata"/> or an item thereof is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or more of the items in <paramref name="metadata"/> is not
        /// a valid metadata item.
        /// </exception>
        public MonotonicVersioner(bool startAtOne, IEnumerable<string> metadata)
        {
            _verifyMetadataColl(metadata);

            _latest = new SemanticVersion(
                major:          startAtOne ? 1 : 0,
                minor:          0,
                patch:          0,
                identifiers:    Enumerable.Empty<string>(),
                metadata:       metadata
                );

            _latestVers = new Dictionary<int, SemanticVersion>
            {
                { _latest.Major, _latest }
            };
            
            _chronology = new List<SemanticVersion>() { _latest };


            _createReadOnlies();
        }
        /// <summary>
        /// <para>
        /// Creates a new <see cref="MonotonicVersioner"/> with the
        /// specified version number history.
        /// </para>
        /// </summary>
        /// <param name="chronology">
        /// A collection of version numbers providing the version
        /// history to use for this instance.
        /// </param>
        /// <remarks>
        /// <paramref name="chronology"/> is not required to be
        /// in order.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="chronology"/> or an item thereof is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="chronology"/> contains a version which is
        /// not a valid monotonic version.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>
        /// <paramref name="chronology"/> provides an incomplete
        /// version history. The chronology may:
        /// </para>
        /// <list type="bullet">
        ///     <item>
        ///     <description>
        ///     Not provide a contiguous sequence of <see cref="Compatibility"/>
        ///     numbers;
        ///     </description>
        ///     </item>
        ///     <item>
        ///     <description>
        ///     Not provide a contiguous sequence of <see cref="Release"/>
        ///     numbers;
        ///     </description>
        ///     </item>
        ///     <item>
        ///     <description>
        ///     Not contain a <see cref="Compatibility"/> starting at either
        ///     zero or one; or
        ///     </description>
        ///     </item>
        ///     <item>
        ///     <description>
        ///     Be empty.
        ///     </description>
        ///     </item>
        /// </list>
        /// </exception>
        public MonotonicVersioner(IEnumerable<SemanticVersion> chronology)
        {
            if (chronology == null)
            {
                throw new ArgumentNullException(
                    message:    "The specified chronology cannot be null.",
                    paramName:  nameof(chronology)
                    );
            }

            if (chronology.Contains(null))
            {
                throw new ArgumentNullException(
                    message:    "The specified chronology cannot contain a " +
                                "null version.",
                    paramName:  nameof(chronology)
                    );
            }

            if (!chronology.All(MonotonicExtensions.IsMonotonic))
            {
                throw new ArgumentOutOfRangeException(
                    message:    "The specified chronology contains a version " +
                                "that is not a valid monotonic version.",
                    paramName:  nameof(chronology)
                    );
            }

            if (!chronology.Any())
            {
                throw new ArgumentException(
                    message:    "The specified chronology is empty.",
                    paramName:  nameof(chronology)
                    );
            }

            // Order the versions we've provided by release number.
            var orderedChron = chronology.OrderBy(v => v.Minor);

            // Is the set of lines of compatibility without gaps?
            var linesOfCompat = orderedChron.Select(v => v.Major).Distinct();
            if (!_isOrderedCtgsIntSeq(linesOfCompat))
            {
                throw new ArgumentException(
                    message:    "The specified chronology does not provide " +
                                "a contiguous set of lines of compatibility.",
                    paramName:  nameof(chronology)
                    );
            }

            // Is the set of release numbers ordered without any gaps.
            if (!_isOrderedCtgsIntSeq(orderedChron.Select(v => v.Minor)))
            {
                throw new ArgumentException(
                    message:    "The specified chronology does not provide " +
                                "a contiguous sequence of release numbers.",
                    paramName:  nameof(chronology)
                    );
            }

            // Is the first line of compatibility a value other than 0 or 1?
            if (orderedChron.First().Major >= 2)
            {
                throw new ArgumentException(
                    message:    "The set of lines of compatibility in the " +
                                "specified chronology does not start at " +
                                $"one or zero ({chronology.First().Major}).",
                    paramName:  nameof(chronology)
                    );
            }

            _latest = orderedChron.Last();

            _latestVers = new Dictionary<int, SemanticVersion>();
            foreach (var line in linesOfCompat)
            {
                _latestVers[line] = orderedChron.Where(v => v.Major == line)
                                                .Last();
            }

            _chronology = new List<SemanticVersion>(orderedChron);

            _createReadOnlies();
        }

        /// <summary>
        /// <para>
        /// Returns the next version number when a specified change is
        /// made to the latest version.
        /// </para>
        /// </summary>
        /// <param name="change">
        /// The type of change being made to the latest version.
        /// </param>
        /// <returns>
        /// <para>
        /// The next version number produced when the specified change
        /// is made to the latest version.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Compatible"/>, the release number
        /// is incremented but the compatibility number remains the same.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Breaking"/>, both the release and
        /// compatibility numbers are incremented.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// <paramref name="change"/> is not a recognised type of change.
        /// </para>
        /// </exception>
        public SemanticVersion Next(MonotonicChange change)
        {
            return this.Next(change, Enumerable.Empty<string>());
        }
        /// <summary>
        /// <para>
        /// Returns the next version number when a specified change is
        /// made to the latest version.
        /// </para>
        /// </summary>
        /// <param name="change">
        /// The type of change being made to the latest version.
        /// </param>
        /// <param name="metadata">
        /// The metadata to be included with the new version.
        /// </param>
        /// <returns>
        /// <para>
        /// The next version number produced when the specified change
        /// is made to the latest version, with the specified metadata.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Compatible"/>, the release number
        /// is incremented but the compatibility number remains the same.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Breaking"/>, both the release and
        /// compatibility numbers are incremented.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// <paramref name="change"/> is not a recognised type of change.
        /// </para>
        /// </exception>        
        /// <exception cref="ArgumentNullException">
        /// <paramref name="metadata"/> or an item therein is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or more items within <paramref name="metadata"/> is not a
        /// valid metadata string.
        /// </exception>
        public SemanticVersion Next(
            MonotonicChange change, IEnumerable<string> metadata
            )
        {
            return this.Next(this.Latest.Major, change, metadata);
        }
        /// <summary>
        /// <para>
        /// Returns the next version number when a specified change is
        /// made to a given line of compatibility.
        /// </para>
        /// </summary>
        /// <param name="line">
        /// The line of compatibility to which the change is being
        /// made.
        /// </param>
        /// <param name="change">
        /// The type of change being made to <paramref name="line"/>.
        /// </param>
        /// <returns>
        /// <para>
        /// The next version number produced when the specified change
        /// is made to the specified line of compatibility.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Compatible"/>, the release number
        /// is incremented but the compatibility number remains the same.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Breaking"/>, both the release and
        /// compatibility numbers are incremented.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// <paramref name="line"/> is negative.
        /// </para>
        /// <para>
        /// <paramref name="change"/> is not a recognised type of change.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="line"/> is not a current line of compatibility.
        /// </exception>
        public SemanticVersion Next(int line, MonotonicChange change)
        {
            return this.Next(line, change, Enumerable.Empty<string>());
        }
        /// <summary>
        /// <para>
        /// Returns the next version number when a specified change is
        /// made to a given line of compatibility with the specified
        /// metadata.
        /// </para>
        /// </summary>
        /// <param name="line">
        /// The line of compatibility to which the change is being
        /// made.
        /// </param>
        /// <param name="change">
        /// The type of change being made to <paramref name="line"/>.
        /// </param>
        /// <param name="metadata">
        /// The metadata to be included with the new version.
        /// </param>
        /// <returns>
        /// <para>
        /// The next version number produced when the specified change
        /// is made to the specified line of compatibility.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Compatible"/>, the release number
        /// is incremented but the compatibility number remains the same.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Breaking"/>, both the release and
        /// compatibility numbers are incremented.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// <paramref name="line"/> is negative.
        /// </para>
        /// <para>
        /// <paramref name="change"/> is not a recognised type of change.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="metadata"/> or an item therein is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>
        /// One or more items within <paramref name="metadata"/> is not a
        /// valid metadata string.
        /// </para>
        /// <para>
        /// <paramref name="line"/> is not a current line of compatibility.
        /// </para>
        /// </exception>
        public SemanticVersion Next(
            int line,
            MonotonicChange change,
            IEnumerable<string> metadata
            )
        {
            if (line < 0)
            {
                throw new ArgumentOutOfRangeException(
                    message:    "The specified line of compatibility cannot " +
                                "be negative.",
                    paramName:  nameof(line)
                    );
            }

            if (!Enum.IsDefined(typeof(MonotonicChange), change))
            {
                throw new ArgumentOutOfRangeException(
                    message:    "The specified change was unrecognised " +
                                "or invalid.",
                    paramName:  nameof(change)
                    );
            }

            _verifyMetadataColl(metadata);

            if (!_latestVers.Keys.Contains(line))
            {
                throw new ArgumentException(
                    message:    "The specified line of compatibility is not " +
                                "recognised for this versioner.",
                    paramName:  nameof(line)
                    );
            }

            // If it's a breaking change, we are free to ignore the change
            // because a breaking change will produce a new line of compatibility.
            if (change == MonotonicChange.Breaking)
            {
                return _add(new SemanticVersion(
                    major:       _latestVers.Keys.Max() + 1,
                    minor:       _latest.Minor + 1,
                    patch:       0,
                    identifiers: Enumerable.Empty<string>(),
                    metadata:    metadata
                    ));
            }
            // Otherwise, we use the line of compatibility specified.
            else if (change == MonotonicChange.Compatible)
            {
                return _add(new SemanticVersion(
                    major:       line,
                    minor:       _latest.Minor + 1,
                    patch:       0,
                    identifiers: Enumerable.Empty<string>(),
                    metadata:    metadata
                    ));
            }

            throw new Exception("Invalid [MonotonicVersioner.Next] state.");
        }

        /// <summary>
        /// <para>
        /// Returns a <see cref="MonotonicVersioner"/> with an identical
        /// chronology, but which can advance its versions separately.
        /// </para>
        /// </summary>
        /// <returns>
        /// A <see cref="MonotonicVersioner"/> with an identical chronology
        /// up to the moment this method was called, but which is able
        /// to separately advance its version numbers.
        /// </returns>
        public MonotonicVersioner Clone()
        {
            return new MonotonicVersioner(this.Chronology);
        }


        /// <summary>
        /// <para>
        /// The chronologically-latest version number in this versioning
        /// sequence.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// The chronologically-latest version is the version with the
        /// greatest value as its release component, regardless of the
        /// line of compatibility.
        /// </para>
        /// </remarks>
        public SemanticVersion Latest => _latest;
        /// <summary>
        /// <para>
        /// The latest versions in each line of compatibility, where the
        /// key is the line of compatibility.
        /// </para>
        /// </summary>
        public IReadOnlyDictionary<int, SemanticVersion> LatestVersions
        {
            get;
            private set;
        }

        /// <summary>
        /// <para>
        /// The highest compatibility number. This component indicates which
        /// releases are compatible with each other.
        /// </para>
        /// </summary>
        public int Compatibility => this.LatestVersions.Keys.Max();
        /// <summary>
        /// <para>
        /// The current release number. This component indicates when a release
        /// was made relative to other releases.
        /// </para>
        /// </summary>
        public int Release => _latest.Minor;

        /// <summary>
        /// <para>
        /// The monotonic versions this instance has produced, in 
        /// chronological order.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// For monotonic versions, chronological order means that the
        /// versions are ordered by ascending release number.
        /// </para>
        /// </remarks>
        public IEnumerable<SemanticVersion> Chronology
        {
            get;
            private set;
        }
    }
}
