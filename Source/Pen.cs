using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace MagicText
{
    /// <summary>Random text generator.</summary>
    /// <remarks>
    ///     <para>
    ///         If the pen should choose from tokens from multiple sources, the tokens should be concatenated into a single enumerable <c>context</c> passed to the constructor. To prevent overflowing from one source to another (e. g. if the last token from the first source is not a contextual predecessor of the first token from the second source), an ending token (<see cref="SentinelToken" />) should be put between the sources' tokens in the final enumerable <c>tokens</c>. Choosing an ending token in <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, Random, Nullable{Int32})" /> or <see cref="Render(Int32, Nullable{Int32})" /> methods shall cause the rendering to stop—the same as when a successor of the last entry in tokens is chosen.
    ///     </para>
    ///
    ///     <para>
    ///         A complete deep copy of enumerable <c>context</c> (passed to the constructor) is created and stored by the pen. Memory errors may occur if the number of tokens in the enumerable is too large. To reduce memory usage and even time consumption, <c>true</c> may be passed as parameter <c>intern</c> to constructor(s); however, be aware of other side effects of string interning via <see cref="String.Intern(String)" /> method.
    ///     </para>
    ///
    ///     <para>
    ///         Changing any of the properties—public or protected—breaks the functionality of the pen. By doing so, behaviour of <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, Random, Nullable{Int32})" /> and <see cref="Render(Int32, Nullable{Int32})" /> methods is unexpected and no longer guaranteed.
    ///     </para>
    /// </remarks>
    public class Pen : Object
    {
        protected const string ComparerNullErrorMessage = "String comparer may not be `null`.";
        private const string TokensNullErrorMessage = "Token list may not be `null`.";
        private const string IndexNullErrorMessage = "Index may not be `null`.";
        private const string SampleNullErrorMessage = "Sample tokens may not be `null`.";
        protected const string ContextNullErrorMessage = "Context token enumerable may not be `null`.";
        protected const string PickerNullErrorMessage = @"""Random"" number generator function (picker function) may not be `null`.";
        private const string RandomNullErrorMessage = "(Pseudo-)Random number generator may not be `null`.";
        protected const string RelevantTokensOutOfRangeErrorMessage = "Number of relevant tokens must be non-negative (greater than or equal to 0).";
        protected const string FromPositionOutOfRangeErrorMessage = "Index of the first token must be between 0 and the total number of tokens inclusively.";
        protected const string PickOutOfRangeErrorMessage = "Picker function must return an integer from [0, n) including 0 (even if `n == 0`).";

        /// <summary>Auxiliary <see cref="Int32" /> comparer for sorting indices of tokens in <see cref="Pen.Context" />.</summary>
        /// <remarks>
        ///     <para>
        ///         Only the reference of the list <c>tokens</c> passed to the constructor is stored by the sorter in <see cref="Tokens" /> property. Changing the content of the enumerable externally, or even its order, results in unconsitent behaviour of comparison via <see cref="IComparer{T}.Compare(T, T)" /> method.
        ///     </para>
        /// </remarks>
        private class IndexComparer : Object, IEqualityComparer<Int32>, IComparer<Int32>
        {
            private const string ComparerNullErrorMessage = "String comparer may not be `null`.";
            private const string TokensNullErrorMessage = "Token list may not be `null`.";

            private readonly StringComparer _comparer;
            private readonly IReadOnlyList<String?> _tokens;

            /// <summary>String comparer used by the sorter for comparing tokens.</summary>
            /// <returns>Internal string comparer.</returns>
            protected StringComparer Comparer => _comparer;

            /// <summary>Reference tokens used by the sorter for comparing indices.</summary>
            /// <returns>Reference tokens.</returns>
            /// <remarks>
            ///     <para>
            ///         If integers <c>x</c>, <c>y</c> are legal indices of <see cref="Tokens" />, they are compared by comparing <c><see cref="Tokens" />[x]</c>, <c><see cref="Tokens" />[y]</c> using <see cref="Comparer" /> in <see cref="IComparer{T}.Compare(T, T)" /> method. Ties are resolved by comparing <c><see cref="Tokens" />[x + 1]</c>, <c><see cref="Tokens" />[y + 1]</c> and so on; the first index to reach the end of <see cref="Tokens" /> is considered less if all tokens compared equal.
            ///     </para>
            /// </remarks>
            /// <seealso cref="Comparer" />
            public IReadOnlyList<String?> Tokens => _tokens;

            /// <summary>Create an index comparer with provided options.</summary>
            /// <param name="comparer">String comparer. Tokens shall be compared (e. g. for equality) by <paramref name="comparer" />.</param>
            /// <param name="tokens">Reference tokens. Indices shall be compared by comparing elements of <paramref name="tokens" />.</param>
            /// <exception cref="ArgumentNullException">Parameter <paramref name="comparer" /> is <c>null</c>. Parameter <paramref name="tokens" /> is <c>null</c>.</exception>
            public IndexComparer(StringComparer comparer, IReadOnlyList<String?> tokens) : base()
            {
                _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer), ComparerNullErrorMessage);
                _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens), TokensNullErrorMessage);
            }

            /// <summary>Get the hash code of <paramref name="obj" />.</summary>
            /// <param name="obj">Index of which the hash code is seeked.</param>
            /// <returns>Hash code of <paramref name="obj" />.</returns>
            Int32 IEqualityComparer<Int32>.GetHashCode(Int32 obj) =>
                obj.GetHashCode();

            /// <summary>Compare <paramref name="x" /> and <paramref name="y" /> for equality.</summary>
            /// <param name="x">Left index to compare.</param>
            /// <param name="y">Right index tot compare.</param>
            /// <returns>If <paramref name="x" /> and <paramref name="y" /> compare equal, <c>true</c>; <c>false</c> otherwise.</returns>
            Boolean IEqualityComparer<Int32>.Equals(Int32 x, Int32 y) =>
                (x == y);

            /// <summary>Compare <paramref name="x" /> and <paramref name="y" />.</summary>
            /// <param name="x">Left index to compare.</param>
            /// <param name="y">Right index tot compare.</param>
            /// <returns>A strictly negative, i. e. less than 0, value if <paramref name="x" /> compares (strictly) less than <paramref name="y" />; a strictly positive, i. e. greater than 0, value if <paramref name="x" /> compares (strictly) less than <paramref name="y" />; 0 otherwise (<paramref name="x" /> and <paramref name="y" /> compare equal).</returns>
            /// <remarks>
            ///     <para>
            ///         See <see cref="Tokens" /> to understand how (legal) indices are compared. If any of <paramref name="x" /> and <paramref name="y" /> is out of range as an index of <see cref="Tokens" />, simply <see cref="Int32.CompareTo(Int32)" /> method is used for comparison.
            ///     </para>
            /// </remarks>
            /// <seealso cref="Tokens" />
            Int32 IComparer<Int32>.Compare(Int32 x, Int32 y)
            {
                // Compare indices. If not equal, compare tokens (if possible).
                int c = y.CompareTo(x);
                if (c != 0 && x >= 0 && y >= 0)
                {
                    while (x < Tokens.Count && y < Tokens.Count)
                    {
                        // Extract current tokens.
                        String? t1 = Tokens[x];
                        String? t2 = Tokens[y];

                        // Compare tokens. If not equal, return the result.
                        {
                            int ct = Comparer.Compare(t1, t2);
                            if (ct != 0)
                            {
                                c = ct;

                                break;
                            }
                        }

                        // Proceed to next tokens.
                        ++x;
                        ++y;
                    }
                }

                // Return comparison results.  If all tokens compared equal, the greater index has reached the end of `Context` first, implying the shorter (sub)sequence.
                return c;
            }
        }

        /// <summary>Auxiliary index finder for finding indices of tokens in <see cref="Pen.Context" />.</summary>
        /// <remarks>
        ///     <para>
        ///         Only the reference of the enumerables <c>index</c> and <c>ignoreTokens</c> passed to the constructor is stored by the finder in <see cref="Index" /> and <see cref="IgnoreTokens" /> properties respectively. Changing the content of the enumerables externally, or even their orders, results in unconsitent behaviour of search via <see cref="FindIndex(String?, Int32)" /> method.
        ///     </para>
        /// </remarks>
        private class IndexFinder : Object
        {
            private const string IndexNullErrorMessage = "Index may not be `null`.";
            private const string IgnoreTokensNullErrorMessage = "Ignored token list may not be `null`.";

            /// <summary>Check if <paramref name="index" /> is valid.</summary>
            /// <param name="index">Index to check.</param>
            /// <returns>If <paramref name="index" /> is greater than or equal to 0, <c>true</c>; false otherwise.</returns>
            public static Boolean IsValidIndex(Int32 index) =>
                index >= 0;

            private readonly IReadOnlyList<Int32> _index;
            private readonly IReadOnlyCollection<String?> _ignoreTokens;

            /// <summary>Index of reference tokens customly sorted (their sorting positions) used by the finder for finding indices.</summary>
            /// <returns>Sorting positions of reference tokens.</returns>
            protected IReadOnlyList<Int32> Index => _index;

            /// <summary>Collection of tokens to ignore used by the finder.</summary>
            /// <returns>Tokens to ignore.</returns>
            /// <remarks>
            ///     <para>
            ///         If a token is found in <see cref="IgnoreTokens" /> via <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)" /> extension method, the search in <see cref="FindIndex(String?, Int32)" /> is immediately terminated and -1 is returned. Note that <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)" /> extension method actually calls <see cref="ICollection{T}.Contains(T)" /> method if the source enumerable implements <see cref="ICollection{T}" /> interface, which may be useful if a special string comparison should be used when checking if a token should be ignored (for instance, pass <see cref="HashSet{T}" /> of strings constructed with a desired <see cref="StringComparer" />).
            ///     </para>
            /// </remarks>
            public IReadOnlyCollection<String?> IgnoreTokens => _ignoreTokens;

            /// <summary>Create an index finder with provided options.</summary>
            /// <param name="positions">Sorting index of tokens.</param>
            /// <param name="ignoreTokens">Tokens to ignore.</param>
            public IndexFinder(IReadOnlyList<Int32> positions, IReadOnlyCollection<String?> ignoreTokens) : base()
            {
                _index = positions ?? throw new ArgumentNullException(nameof(positions), IndexNullErrorMessage);
                _ignoreTokens = ignoreTokens ?? throw new ArgumentNullException(nameof(ignoreTokens), IgnoreTokensNullErrorMessage);
            }

            /// <summary>Find the position of <paramref name="token" />'s <paramref name="index" /> in <see cref="Index" />.</summary>
            /// <param name="token">Token whose <paramref name="index" /> should be found.</param>
            /// <param name="index">Index of <paramref name="token" /> (potential element in <see cref="Index" />).</param>
            /// <returns>If <paramref name="token" /> should be ignored (if <see cref="IgnoreTokens" /> contains it), -1; otherwise minimal index <c>i</c> such that <c><see cref="Index" />[i] == <paramref name="index" /></c>. If <paramref name="index" /> is not found, -1 is returned as well.</returns>
            public Int32 FindIndex(String? token, Int32 index) =>
                IgnoreTokens.Contains(token) ? -1 : Index.IndexOf(index);
        }

        private static readonly Object _locker;
        private static Int32 randomSeed;

        [ThreadStatic]
        private static System.Random? mutableThreadStaticRandom;

        /// <summary>Internal locking object of <see cref="Pen" /> class.</summary>
        /// <returns>Internal locking object.</returns>
        private static Object Locker => _locker;

        /// <summary>Seed for the internal (pseudo-)random number generator (<see cref="Random" />).</summary>
        /// <returns>Current seed.</returns>
        /// <value>New seed. If <c>value</c> is less than or equal to 0, new seed is set to 1.</value>
        /// <remarks>
        ///     <para>
        ///         The current value does not necessarily indicate the seeding value or the random state of the internal number generator. The dissonance may appear if the number generator has already been instantiated and used or if the value of <see cref="RandomSeed" /> has changed.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Random" />
        private static Int32 RandomSeed
        {
            get
            {
                lock (Locker)
                {
                    return randomSeed;
                }
            }

            set
            {
                lock (Locker)
                {
                    randomSeed = Math.Max(value, 1);
                }
            }
        }

        /// <summary>Internal (pseudo-)random number generator.</summary>
        /// <returns>Current thread's (pseudo-)random number generator.</returns>
        /// <remarks>
        ///     <para>
        ///         The number generator is thread safe (actually, each thread has its own instance) and instances across multiple threads are seeded differently. However, instancess across multiple processes initiated at approximately the same time could be seeded with the same value. Therefore the main purpose of the number generator is to provide a virtually unpredictable (to an unconcerned human user) implementation of <see cref="Render(Int32, Nullable{Int32})" /> for a single process without having to provide a custom number generator (a <see cref="Func{T, TResult}" /> function or a <see cref="System.Random" /> object); no other properties are guaranteed.
        ///     </para>
        ///
        ///     <para>
        ///         If the code is expected to spread over multiple threads (either explicitly by starting <see cref="Thread" />s or implicitly by programming asynchronously), reference returned by <see cref="Random" /> property should not be stored in an outside variable and used later. Reference property <see cref="Random" /> directly in such scenarios.
        ///     </para>
        /// </remarks>
        protected static System.Random Random
        {
            get
            {
                lock (Locker)
                {
                    if (mutableThreadStaticRandom is null)
                    {
                        unchecked
                        {
                            mutableThreadStaticRandom = new Random(RandomSeed++);
                        }
                    }

                    return mutableThreadStaticRandom;
                }
            }
        }

        /// <summary>Initialise static fields.</summary>
        static Pen()
        {
            _locker = new Object();

            lock (Locker)
            {
                unchecked
                {
                    randomSeed = Math.Max((Int32)((1073741827L * DateTime.UtcNow.Ticks + 1073741789L) & (Int64)Int32.MaxValue), 1);
                }
            }
        }

        /// <summary>Retrieve the system's reference to the specified nullable string.</summary>
        /// <param name="str">Nullable string to search for in the intern pool.</param>
        /// <returns>If <paramref name="str" /> is <c>null</c>, <c>null</c> is returned as well. Otherwise the system's reference to <paramref name="str" /> is returned if it is interned (if not, a new reference to a string with the value of <paramref name="str" />, interned in the system's intern pool, is returned).</returns>
        /// <remarks>
        ///     <para>
        ///         Unlike <see cref="String.Intern(String)" /> method, this method does not throw <see cref="ArgumentNullException" /> if the argument is <c>null</c>; instead, <c>null</c> is simply returned. However, <see cref="String.Intern(String)" /> method is indeed called for non-<c>null</c> arguments and therefore strings interned via this method are interned in the same system's intern pool of strings as strings interned via <see cref="String.Intern(String)" /> method (and vice versa).
        ///     </para>
        /// </remarks>
        protected static String? InternNullableString(String? str) =>
            str is null ? str : String.Intern(str);

        /// <summary>Generate non-negative (pseudo-)random integer using internal (pseudo-)random number generator (<see cref="Random" />).</summary>
        /// <returns>(Pseudo-)random integer that is greater than or equal to 0 but less than <see cref="Int32.MaxValue" />.</returns>
        protected static Int32 RandomNext() =>
            Random.Next();

        /// <summary>Generate non-negative (pseudo-)random integer using internal (pseudo-)random number generator (<see cref="Random" />).</summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. The value must be greater than or equal to 0.</param>
        /// <returns>(Pseudo-)random integer that is greater than or equal to 0 but less than <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0, 0 is returned.</returns>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <see cref="System.Random.Next(Int32)" /> method (notably <see cref="ArgumentOutOfRangeException" />) are not caught.
        ///     </para>
        /// </remarks>
        protected static Int32 RandomNext(Int32 maxValue) =>
            Random.Next(maxValue);

        /// <summary>Generate non-negative (pseudo-)random integer using internal (pseudo-)random number generator (<see cref="Random" />).</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. The value must be greater than or equal to <paramref name="minValue" />.</param>
        /// <returns>(Pseudo-)random integer that is greater than or equal to <paramref name="minValue" /> and less than <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals <paramref name="minValue" />, the value is returned.</returns>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <see cref="System.Random.Next(Int32, Int32)" /> method (notably <see cref="ArgumentOutOfRangeException" />) are not caught.
        ///     </para>
        /// </remarks>
        protected static Int32 RandomNext(Int32 minValue, Int32 maxValue) =>
            Random.Next(minValue, maxValue);

        /// <summary>Compare a subrange of <paramref name="tokens" /> with a sample of tokens <paramref name="sampleCycle" /> in respect of <paramref name="comparer" />.</summary>
        /// <param name="comparer">String comparer used for comparing.</param>
        /// <param name="tokens">List of tokens whose subrange is compared to <paramref name="sampleCycle" />.</param>
        /// <param name="sampleCycle">Cyclical sample list of tokens. The list represents range <c>{ <paramref name="sampleCycle" />[<paramref name="cycleStart" />], <paramref name="sampleCycle" />[<paramref name="cycleStart" /> + 1], ..., <paramref name="sampleCycle" />[<paramref name="sampleCycle" />.Count - 1], <paramref name="sampleCycle" />[0], ..., <paramref name="sampleCycle" />[<paramref name="cycleStart" /> - 1] }</c>.</param>
        /// <param name="i">Starting index of the subrange from <paramref name="tokens" /> to compare. The subrange <c>{ <paramref name="tokens" />[i], <paramref name="tokens" />[i + 1], ..., <paramref name="tokens" />[min(i + <paramref name="sampleCycle" />.Count - 1, <paramref name="tokens" />.Count - 1)] }</c> is used.</param>
        /// <param name="cycleStart">Starting index of the cycle in <paramref name="sampleCycle" />.</param>
        /// <returns>A signed integer that indicates the relative values of subrange from <paramref name="tokens" /> starting from <paramref name="i" /> and cyclical sample <paramref name="sampleCycle" />.</returns>
        /// <remarks>
        ///     <para>
        ///         Values from the subrange of <paramref name="tokens" /> and <paramref name="sampleCycle" /> are compared in order by calling <see cref="StringComparer.Compare(String, String)" /> method on <paramref name="comparer" />. If a comparison yields a non-zero value, it is returned. If the subrange from <paramref name="tokens" /> is shorter (in the number of tokens) than <paramref name="sampleCycle" /> but all of its tokens compare equal to corresponding tokens from the beginning of <paramref name="sampleCycle" />, a negative number is returned. If all tokens compare equal and the subrange from <paramref name="tokens" /> is the same length (in the number of tokens) as <paramref name="sampleCycle" />, 0 is returned.
        ///     </para>
        /// </remarks>
        private static Int32 CompareRange(StringComparer comparer, IReadOnlyList<String?> tokens, IReadOnlyList<String?> sampleCycle, Int32 i, Int32 cycleStart)
        {
            int c = 0;

            {
                int j;

                // Compare tokens.
                for (/* [`i` is set in function call,] */ j = 0; i < tokens.Count && j < sampleCycle.Count; ++i, ++j)
                {
                    c = comparer.Compare(tokens[i], sampleCycle[(cycleStart + j) % sampleCycle.Count]);
                    if (c != 0)
                    {
                        break;
                    }
                }

                // If `tokens` has reached the end, but `sample` has not, consider the subrange of `tokens` less.
                if (c == 0 && i == tokens.Count && j < sampleCycle.Count)
                {
                    c = -1;
                }
            }

            return c;
        }

        /// <summary>Find the first index and the number of occurances of <paramref name="sampleCycle" /> in <paramref name="tokens" /> sorted by <paramref name="index" />.</summary>
        /// <param name="comparer">String comparer used for comparing.</param>
        /// <param name="tokens">List of tokens amongst which <paramref name="sampleCycle" /> should be found.</param>
        /// <param name="index">Positional ordering of <paramref name="tokens" /> in respect of <paramref name="comparer" />.</param>
        /// <param name="sampleCycle">Cyclical sample list of tokens to find. The list represents range <c>{ <paramref name="sampleCycle" />[<paramref name="cycleStart" />], <paramref name="sampleCycle" />[<paramref name="cycleStart" /> + 1], ..., <paramref name="sampleCycle" />[<paramref name="sampleCycle" />.Count - 1], <paramref name="sampleCycle" />[0], ..., <paramref name="sampleCycle" />[<paramref name="cycleStart" /> - 1] }</c>.</param>
        /// <param name="cycleStart">Starting index of the cycle in <paramref name="sampleCycle" />.</param>
        /// <returns>The minimal index <c>i</c> such that an occurance of <paramref name="sampleCycle" /> begins at <c><paramref name="tokens" />[<paramref name="index" />[i]]</c> and the total number of its occurances amongst <paramref name="tokens" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="comparer" /> is <c>null</c>. Parameter <paramref name="tokens" /> is <c>null</c>. Parameter <paramref name="index" /> is <c>null</c>. Parameter <paramref name="sampleCycle" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Because of performance reasons, the implementation of the method <em>assumes</em> the following without checking:
        ///     </para>
        ///
        ///     <list type="bullet">
        ///         <listheader>
        ///             <term>
        ///                 assumption
        ///             </term>
        ///             <description>
        ///                 description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 <paramref name="index" /> legality
        ///             </term>
        ///             <description>
        ///                 parameter <paramref name="index" /> is of the same length as <paramref name="tokens" />, all of its values are legal indices for <paramref name="tokens" /> and each of its values appears only once (in short, <paramref name="index" /> is a permutation of the sequence <c>{ 0, 1, ..., <paramref name="tokens" />.Count - 1 }</c>),
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <paramref name="cycleStart" /> legality
        ///             </term>
        ///             <description>
        ///                 parameter <paramref name="cycleStart" /> is a legal index for <paramref name="sampleCycle"/>,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <paramref name="index" /> validity
        ///             </term>
        ///             <description>
        ///                 parameter <paramref name="index" /> indeed sorts <paramref name="tokens" /> ascendingly in respect of <paramref name="comparer" />,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <paramref name="sampleCycle" /> existence
        ///             </term>
        ///             <description>
        ///                 parameter <paramref name="sampleCycle" /> exists (as compared by <paramref name="comparer" />) amongst <paramref name="tokens" />.
        ///             </description>
        ///         </item>
        ///     </list>
        ///
        ///     <para>
        ///         If any of the first three assumptions is incorrect, the behaviour of the method is undefined (even <see cref="ArgumentOutOfRangeException" /> might be thrown and not caught when calling <see cref="IReadOnlyList{T}.this[Int32]" />). If the last assumption is incorrect, the returned index shall point to the position at which <paramref name="sampleCycle" />'s position should be inserted to retain the sorted order and the number of occurances shall be 0.
        ///     </para>
        /// </remarks>
        protected static ValueTuple<Int32, Int32> FindPositionIndexAndCount(StringComparer comparer, IReadOnlyList<String?> tokens, IReadOnlyList<Int32> index, IReadOnlyList<String?> sampleCycle, Int32 cycleStart)
        {
            if (comparer is null)
            {
                throw new ArgumentNullException(nameof(comparer), ComparerNullErrorMessage);
            }
            if (tokens is null)
            {
                throw new ArgumentNullException(nameof(tokens), TokensNullErrorMessage);
            }
            if (index is null)
            {
                throw new ArgumentNullException(nameof(index), IndexNullErrorMessage);
            }
            if (sampleCycle is null)
            {
                throw new ArgumentNullException(nameof(sampleCycle), SampleNullErrorMessage);
            }

            // Binary search...

            // Initialise lower, upper and middle positions.
            int l = 0;
            int h = tokens.Count;
            int m = h >> 1;

            // Loop until found.
            {
                while (l < h)
                {
                    // Compare ranges.
                    int c = CompareRange(comparer, tokens, sampleCycle, index[m], cycleStart);

                    // Break the loop or update positions.
                    if (c == 0)
                    {
                        l = m;
                        h = m + 1;

                        break;
                    }
                    else if (c < 0)
                    {
                        l = m;
                    }
                    else if (c > 0)
                    {
                        h = m;
                    }
                    m = (l + h) >> 1;
                }
            }

            // Find the minimal position index `l` and the maximal index `h` of occurances of `sample` amongst `tokens`.
            while (l > 0 && CompareRange(comparer, tokens, sampleCycle, index[l - 1], cycleStart) == 0)
            {
                --l;
            }
            while (h < tokens.Count && CompareRange(comparer, tokens, sampleCycle, index[h], cycleStart) == 0)
            {
                ++h;
            }

            // Return the computed values.
            return ValueTuple.Create(l, h - l);
        }

        /// <summary>Find the first index and the number of occurances of <paramref name="sample" /> in <paramref name="tokens" /> sorted by <paramref name="index" />.</summary>
        /// <param name="comparer">String comparer used for comparing.</param>
        /// <param name="tokens">List of tokens amongst which <paramref name="sample" /> should be found.</param>
        /// <param name="index">Positional ordering of <paramref name="tokens" /> in respect of <paramref name="comparer" />.</param>
        /// <param name="sample">Sample of tokens to find.</param>
        /// <returns>The minimal index <c>i</c> such that an occurance of <paramref name="sample" /> begins at <c><paramref name="tokens" />[<paramref name="index" />[i]]</c> and the total number of its occurances amongst <paramref name="tokens" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="comparer" /> is <c>null</c>. Parameter <paramref name="tokens" /> is <c>null</c>. Parameter <paramref name="index" /> is <c>null</c>. Parameter <paramref name="sample" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Because of performance reasons, the implementation of the method <em>assumes</em> the following without checking:
        ///     </para>
        ///
        ///     <list type="bullet">
        ///         <listheader>
        ///             <term>
        ///                 assumption
        ///             </term>
        ///             <description>
        ///                 description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 <paramref name="index" /> legality
        ///             </term>
        ///             <description>
        ///                 parameter <paramref name="index" /> is of the same length as <paramref name="tokens" />, all of its values are legal indices for <paramref name="tokens" /> and each of its values appears only once (in short, <paramref name="index" /> is a permutation of the sequence <c>{ 0, 1, ..., <paramref name="tokens" />.Count - 1 }</c>),
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <paramref name="index" /> validity
        ///             </term>
        ///             <description>
        ///                 parameter <paramref name="index" /> indeed sorts <paramref name="tokens" /> ascendingly in respect of <paramref name="comparer" />,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 <paramref name="sample" /> existence
        ///             </term>
        ///             <description>
        ///                 parameter <paramref name="sample" /> exists (as compared by <paramref name="comparer" />) amongst <paramref name="tokens" />.
        ///             </description>
        ///         </item>
        ///     </list>
        ///
        ///     <para>
        ///         If any of the first two assumptions is incorrect, the behaviour of the method is undefined (even <see cref="ArgumentOutOfRangeException" /> might be thrown and not caught when calling <see cref="IReadOnlyList{T}.this[Int32]" />). If the last assumption is incorrect, the returned index shall point to the position at which <paramref name="sample" />'s position should be inserted to retain the sorted order and the number of occurances shall be 0.
        ///     </para>
        /// </remarks>
        protected static ValueTuple<Int32, Int32> FindPositionIndexAndCount(StringComparer comparer, IReadOnlyList<String?> tokens, IReadOnlyList<Int32> index, IEnumerable<String?> sample) =>
            FindPositionIndexAndCount(
                comparer,
                tokens,
                index,
                sample switch
                {
                    null => null!,
                    IReadOnlyList<String?> sampleReadOnlyList => sampleReadOnlyList,
                    IList<String?> sampleList => new ReadOnlyCollection<String?>(sampleList),
                    _ => new List<String?>(sample)
                },
                0
            );

        private readonly StringComparer _comparer;
        private readonly IReadOnlyList<String?> _context;
        private readonly IReadOnlyList<Int32> _index;
        private readonly String? _sentinelToken;
        private readonly Int32 _firstIndexPosition;
        private readonly Boolean _allSentinels;

        /// <summary>String comparer used by the pen for comparing tokens.</summary>
        /// <returns>Internal string comparer.</returns>
        protected StringComparer Comparer => _comparer;

        /// <summary>Index of entries in <see cref="Context" /> sorted ascendingly (their sorting positions): if <c>i &lt; j</c>, then <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Index" />[i]], <see cref="Context" />[<see cref="Index" />[j]]) &lt;= 0</c>.</summary>
        /// <returns>Sorting positions of tokens in <see cref="Context" />.</returns>
        /// <remarks>
        ///     <para>
        ///         The order is actually determined by the complete sequence of tokens, and not just by single tokens. For instance, if <c>i != j</c>, but <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Index" />[i]], <see cref="Context" />[<see cref="Index" />[j]]) == 0</c> (<c><see cref="Comparer" />.Equals(<see cref="Context" />[<see cref="Index" />[i]], <see cref="Context" />[<see cref="Index" />[j]])</c>), then the result of <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Index" />[i] + 1], <see cref="Context" />[<see cref="Index" />[j] + 1])</c> is used; if it also evaluates to 0, <c><see cref="Index" />[i] + 2</c> and <c><see cref="Index" />[j] + 2</c> are checked, and so on. The first position to reach the end (when <c>max(<see cref="Index" />[i] + n, <see cref="Index" />[j] + n) == <see cref="Context" />.Count</c> for a non-negative integer <c>n</c>), if all previous positions compared equal, is considered less. Hence <see cref="Index" /> defines a total (linear) lexicographic ordering of <see cref="Context" /> in respect of <see cref="Comparer" />, which may be useful in search algorithms, such as binary search, for finding the position of any non-empty finite sequence of tokens.
        ///     </para>
        ///
        ///     <para>
        ///         The position(s) of ending tokens' (<see cref="SentinelToken" />) indices, if there are any in <see cref="Context" />, are not fixed nor guaranteed (for instance, they are not necessarily at the beginning or the end of <see cref="Index" />). If the ending token is <c>null</c> or an empty string (<see cref="String.Empty" />), then ending tokens shall be compared less than any other tokens and their indices shall be <em>pushed</em> to the beginning, provided there are no <c>null</c>s in <see cref="Context" /> in the latter case. But, generally speaking, ending tokens' indices are determined by <see cref="Comparer" /> and the values of other tokens in <see cref="Context" />.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Context" />
        /// <seealso cref="Comparer" />
        protected IReadOnlyList<Int32> Index => _index;

        /// <summary>Position of the first non-ending token's (<see cref="SentinelToken" />) index in <see cref="Context" /> (index of <see cref="Index" />). If such a token does not exist, <see cref="FirstIndexPosition" /> evaluates to the total number of elements in <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property).</summary>
        /// <returns>The first non-ending token's (<see cref="SentinelToken" />) index.</returns>
        /// <remarks>
        ///     <para>
        ///         This index position points to the index of the <strong>actual</strong> first non-ending token (<see cref="SentinelToken" />) in <see cref="Context" />, even though there may exist other tokens comparing equal to it in respect of <see cref="Comparer" />. Hence <c>{ <see cref="Index" />[<see cref="FirstIndexPosition" />], <see cref="Index" />[<see cref="FirstIndexPosition" />] + 1, ..., <see cref="Index" />[<see cref="FirstIndexPosition" />] + n, ... }</c> enumerates <see cref="Context" /> from the beginning by ignoring potential initial ending tokens.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Context" />
        /// <seealso cref="Comparer" />
        /// <seealso cref="SentinelToken" />
        /// <seealso cref="Index" />
        protected Int32 FirstIndexPosition => _firstIndexPosition;

        /// <summary>Reference token context used by the pen.</summary>
        /// <returns>Reference context.</returns>
        /// <remarks>
        ///     <para>
        ///         The order of tokens is kept as provided in the constructor.
        ///     </para>
        /// </remarks>
        public IReadOnlyList<String?> Context => _context;

        /// <summary>Ending token of the pen (see <em>Remarks</em> of <see cref="Pen" />).</summary>
        /// <returns>Ending token.</returns>
        /// <remarks>
        ///     <para>
        ///         The ending token (<see cref="SentinelToken" />), or any other comparing equal to it by <see cref="Comparer" />, shall never be rendered.
        ///     </para>
        ///
        ///     <para>
        ///         If the ending token is <c>null</c>, it <strong>does not</strong> mean that no token is considered an ending token. It simply means that <c>null</c>s are considered ending tokens. Moreover, the ending token <strong>cannot</strong> be ignored (not used)—if no token should be considered an ending token, set the ending token to a value not appearing in <see cref="Context" /> (which can be found via an adaptation of <a href="http://en.wikipedia.org/wiki/Cantor%27s_diagonal_argument">Cantor's diagonal method</a>). Note, however, that comparing long strings is expensive in time resources, and therefore <em>short</em> ending tokens should be preferred: <c>null</c>, <c><see cref="String.Empty" /></c>, <c>"\0"</c> etc. (depending on the values in <see cref="Context" />).
        ///     </para>
        /// </remarks>
        /// <seealso cref="Context" />
        /// <seealso cref="Comparer" />
        public String? SentinelToken => _sentinelToken;

        /// <summary>Indicator of all tokens in <see cref="Context" /> being equal to the ending token (<see cref="SentinelToken" />) (as compared by <see cref="Comparer" />): <c>true</c> if equal, <c>false</c> otherwise.</summary>
        /// <returns>If all tokens in <see cref="Context" /> are ending tokens (<see cref="SentinelToken" />), <c>true</c>; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         If <see cref="Context" /> is empty, <see cref="AllSentinels" /> is <c>true</c>. This coincides with mathematical logic of empty sets.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Context" />
        /// <seealso cref="SentinelToken" />
        /// <seealso cref="Comparer" />
        public Boolean AllSentinels => _allSentinels;

        /// <summary>Create a pen with provided options.</summary>
        /// <param name="context">Input tokens. Random text shall be generated based on <paramref name="context" />: both by picking only from <paramref name="context" /> and by using the order of tokens in <paramref name="context" />.</param>
        /// <param name="sentinelToken">Ending token. See <em>Remarks</em> of <see cref="Pen" /> for clarification.</param>
        /// <param name="comparer">String comparer. Tokens shall be compared (e. g. for equality) by <paramref name="comparer" />. If <c>null</c>, <see cref="StringComparer.Ordinal" /> is used.</param>
        /// <param name="intern">If <c>true</c>, tokens from <paramref name="context" /> shall be interned (via <see cref="String.Intern(String)" /> method) when being copied into the internal pen's container (<see cref="Context" />).</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="context" /> is <c>null</c>.</exception>
        public Pen(IEnumerable<String?> context, String? sentinelToken = null, StringComparer? comparer = null, Boolean intern = false) : base()
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context), ContextNullErrorMessage);
            }

            // Copy comparer and ending token.
            _comparer = comparer ?? StringComparer.Ordinal;
            _sentinelToken = intern ? InternNullableString(sentinelToken) : sentinelToken;

            // Copy context.
            {
                List<String?> contextList = new List<String?>(intern ? context.Select(InternNullableString) : context);
                contextList.TrimExcess();
                _context = contextList.AsReadOnly();
            }

            // Find sorting positions of tokens in context.
            {
                List<Int32> positionsList = new List<Int32>(Enumerable.Range(0, Context.Count));
                positionsList.Sort(new IndexComparer(Comparer, Context));
                positionsList.TrimExcess();
                _index = positionsList.AsReadOnly();
            }

            // Find the position of the first (non-ending) token.
            {
                int firstPositionIndexVar;
                try
                {
                    firstPositionIndexVar = Context.Select((new IndexFinder(Index, new HashSet<String?>(1, comparer) { SentinelToken })).FindIndex).Where(IndexFinder.IsValidIndex).First();
                }
                catch (InvalidOperationException)
                {
                    firstPositionIndexVar = Context.Count;
                }
                _firstIndexPosition = firstPositionIndexVar;
            }

            // Check if all tokens are ending tokens.
            _allSentinels = (FirstIndexPosition == Context.Count);
        }

        /// <summary>Render (generate) a block of text from <see cref="Context" />.</summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens. The value must be greater than or equal to 0.</param>
        /// <param name="picker">Random number generator. When passed an integer <c>n</c> (greater than or equal to 0) as the argument, it should return an integer from range [0, max(<c>n</c>, 1)), i. e. greater than or equal to 0 but (strictly) less than max(<c>n</c>, 1).</param>
        /// <param name="fromPosition">If set, the first max(<paramref name="relevantTokens" />, 1) tokens are chosen as <c>{ <see cref="Context" />[<paramref name="fromPosition" />], <see cref="Context" />[<paramref name="fromPosition" /> + 1], ..., <see cref="Context" />[<paramref name="fromPosition" /> + max(<paramref name="relevantTokens" />, 1) - 1] }</c> (or fewer if the index exceeds its limitation or an ending token is reached) without calling <paramref name="picker" /> function; otherwise the first token is chosen by immediately calling <paramref name="picker" /> function. The value must be greater than or equal to 0 and less than or equal to the total number of tokens in <see cref="Context" />.</param>
        /// <returns>Query for rendering tokens.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="picker" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Parameter <paramref name="relevantTokens" /> is (strictly) negative, i. e. less than 0. Parameter <paramref name="fromPosition" /> is (strictly) negative or (strictly) greater than the total number of tokens in <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property). Function <paramref name="picker" /> returns a value outside of the legal range.</exception>
        /// <remarks>
        ///     <para>
        ///         If <paramref name="fromPosition" /> is set, the first <c>max(<paramref name="relevantTokens" />, 1)</c> tokens are chosen accordingly; otherwise they are chosen by calling <paramref name="picker" /> function. Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <paramref name="picker" /> function. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="SentinelToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        ///
        ///     <para>
        ///         An extra copy of <paramref name="relevantTokens"/> tokens is kept when generating new tokens. Memory errors may occur if <paramref name="relevantTokens"/> is too large.
        ///     </para>
        ///
        ///     <para>
        ///         The query returned is not run until enumerating it, such as via explicit calls to <see cref="IEnumerable{T}.GetEnumerator" /> method, a <c>foreach</c> loop, a call to <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc. (<em>deferred execution</em>). If <paramref name="picker" /> is not a deterministic function, two distinct enumerators over the query may return different results.
        ///     </para>
        ///
        ///     <para>
        ///         It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as <see cref="List{T}" />, or concatenated together into a string to avoid memory errors. This may be done by calling <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.
        ///     </para>
        ///
        ///     <para>
        ///         The query shall immediately stop, without rendering any tokens, if:
        ///     </para>
        ///
        ///     <list type="number">
        ///         <listheader>
        ///             <term>
        ///                 case
        ///             </term>
        ///             <description>
        ///                 description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 no tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an empty enumerable of tokens,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 all ending tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case),
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 by choice
        ///             </term>
        ///             <description>
        ///                 a <em>successor</em> of the last token or an ending token is picked first.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <seealso cref="Render(Int32, Random, Nullable{Int32})" />
        /// <seealso cref="Render(Int32, Nullable{Int32})" />
        public virtual IEnumerable<String?> Render(Int32 relevantTokens, Func<Int32, Int32> picker, Nullable<Int32> fromPosition = default)
        {
            if (picker is null)
            {
                throw new ArgumentNullException(nameof(picker), PickerNullErrorMessage);
            }
            if (relevantTokens < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relevantTokens), relevantTokens, RelevantTokensOutOfRangeErrorMessage);
            }

            // Initialise the list of `relevantTokens` most recent tokens and its first position (the list will be cyclical after rendering `relevantTokens` tokens).
            List<String?> text = new List<String?>(Math.Max(relevantTokens, 1));
            int c = 0;

            // Render the first token, or the first `relevantTokens` if needed.
            if (fromPosition.HasValue)
            {
                if (fromPosition.Value < 0 || fromPosition.Value > Context.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(fromPosition), fromPosition.Value, FromPositionOutOfRangeErrorMessage);
                }

                int next;
                int i;
                for ((next, i) = ValueTuple.Create(fromPosition.Value, 0); i < text.Capacity; ++next, ++i)
                {
                    String? nextToken = next < Context.Count ? Context[next] : SentinelToken;
                    if (Comparer.Equals(nextToken, SentinelToken))
                    {
                        yield break;
                    }

                    yield return nextToken;
                    text.Add(nextToken);
                }
            }
            else
            {
                int pick = picker(Context.Count + 1);
                if (pick < 0 || pick > Context.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(picker), pick, PickOutOfRangeErrorMessage);
                }

                int first = pick < Context.Count ? Index[pick] : Context.Count;
                String? firstToken = first < Context.Count ? Context[first] : SentinelToken;
                if (Comparer.Equals(firstToken, SentinelToken))
                {
                    yield break;
                }

                yield return firstToken;
                text.Add(firstToken);
            }

            // Loop rendering until over.
            while (true)
            {
                // Declare:
                int p; // the first position (index of `Positions`) of the most recent `relevantTokens` tokens rendered;                 0 if `relevantTokens == 0`
                int n; // the number of the most recent `relevantTokens` tokens rendered occurances in `Tokens`;                         `Tokens.Count` + 1 if `relevantTokens == 0`
                int d; // the distance (in number of tokens) between the first relevant token and the next to render (`relevantTokens`); 0 if `relevantTokens == 0`
                    // until `relevantTokens` tokens have not yet been rendered, `text.Count` is used as the number of relevant tokens, i. e. all rendered tokens are relevant

                // Find the values according to `relevantTokens`.
                if (relevantTokens == 0)
                {
                    p = 0;
                    n = Context.Count + 1;
                    d = 0;
                }
                else
                {
                    (p, n) = FindPositionIndexAndCount(Comparer, Context, Index, text, c);
                    d = text.Count; // note that `text` shall never hold more than `relevantTokens` tokens
                }

                // Render the next token.

                int pick = picker(n);
                if (pick < 0 || pick >= Math.Max(n, 1)) // actually, `n` should never be 0
                {
                    throw new ArgumentOutOfRangeException(nameof(picker), pick, PickOutOfRangeErrorMessage);
                }

                int next = Index[p + pick] + d;
                String? nextToken = next < Context.Count ? Context[next] : SentinelToken;
                if (Comparer.Equals(nextToken, SentinelToken))
                {
                    yield break;
                }

                yield return nextToken;
                if (text.Count < text.Capacity)
                {
                    text.Add(nextToken); 
                }
                else
                {
                    text[c] = nextToken;
                    c = (c + 1) % text.Count; // <-- cyclicity of list `text`
                }
            }
        }

        /// <summary>Render (generate) a block of text from <see cref="Context" />.</summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="random">(Pseudo-)Random number generator.</param>
        /// <param name="fromPosition">If set, the first max(<paramref name="relevantTokens" />, 1) tokens are chosen as <c>{ <see cref="Context" />[<paramref name="fromPosition" />], <see cref="Context" />[<paramref name="fromPosition" /> + 1], ..., <see cref="Context" />[<paramref name="fromPosition" /> + max(<paramref name="relevantTokens" />, 1) - 1] }</c> (or fewer if the index exceeds its limitation or an ending token is reached) without using <paramref name="random" /> parameter; otherwise the first token is chosen by immediately calling <see cref="System.Random.Next(Int32)"/> method on <paramref name="random" />. The value must be greater than or equal to 0 and less than or equal to the total number of tokens in <see cref="Context" />.</param>
        /// <returns>Query for rendering tokens.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="random" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Parameter <paramref name="relevantTokens" /> is (strictly) negative, i. e. less than 0. Parameter <paramref name="fromPosition" /> is (strictly) negative or (strictly) greater than the total number of tokens in <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property).</exception>
        /// <remarks>
        ///     <para>
        ///         If <paramref name="fromPosition" /> is set, the first <c>max(<paramref name="relevantTokens" />, 1)</c> tokens are chosen accordingly; otherwise they are chosen by calling <see cref="System.Random.Next(Int32)" /> method of <paramref name="random" />. Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <see cref="System.Random.Next(Int32)" /> method of <paramref name="random" />. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="SentinelToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        ///
        ///     <para>
        ///         An extra copy of <paramref name="relevantTokens"/> tokens is kept when generating new tokens. Memory errors may occur if <paramref name="relevantTokens"/> is too large.
        ///     </para>
        ///
        ///     <para>
        ///         If no specific <see cref="System.Random" /> object or seed should be used, <see cref="Render(Int32, Nullable{Int32})" /> method may be called instead.
        ///     </para>
        ///
        ///     <para>
        ///         The query returned is not run until enumerating it, such as via explicit calls to <see cref="IEnumerable{T}.GetEnumerator" /> method, a <c>foreach</c> loop, a call to <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc. (<em>deferred execution</em>). Since the point of <see cref="System.Random" /> class is to provide a (pseudo-)random number generator, two distinct enumerators over the query may return different results.
        ///     </para>
        ///
        ///     <para>
        ///         It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as a <see cref="List{T}" />, or concatenated together into a string to avoid memory errors. This may be done by calling <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.
        ///     </para>
        ///
        ///     <para>
        ///         The query shall immediately stop, without rendering any tokens, if:
        ///     </para>
        ///
        ///     <list type="number">
        ///         <listheader>
        ///             <term>
        ///                 case
        ///             </term>
        ///             <description>
        ///                 description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 no tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an empty enumerable of tokens,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 all ending tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case),
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 by choice
        ///             </term>
        ///             <description>
        ///                 a <em>successor</em> of the last token or an ending token is picked first.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <seealso cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />
        /// <seealso cref="Render(Int32, Nullable{Int32})" />
        public IEnumerable<String?> Render(Int32 relevantTokens, System.Random random, Nullable<Int32> fromPosition = default) =>
            random is null ? throw new ArgumentNullException(nameof(random), RandomNullErrorMessage) : Render(relevantTokens, random.Next, fromPosition);

        /// <summary>Render (generate) a block of text from <see cref="Context" />.</summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="fromPosition">If set, the first max(<paramref name="relevantTokens" />, 1) tokens are chosen as <c>{ <see cref="Context" />[<paramref name="fromPosition" />], <see cref="Context" />[<paramref name="fromPosition" /> + 1], ..., <see cref="Context" />[<paramref name="fromPosition" /> + max(<paramref name="relevantTokens" />, 1) - 1] }</c> (or fewer if the index exceeds its limitation or an ending token is reached); otherwise the first token is chosen by immediately calling <see cref="System.Random.Next(Int32)"/> method on the internal (pseudo-)random number generator (<see cref="Random" />). The value must be greater than or equal to 0 and less than or equal to the total number of tokens in <see cref="Context" />.</param>
        /// <returns>Query for rendering tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter <paramref name="relevantTokens" /> is (strictly) negative, i. e. less than 0. Parameter <paramref name="fromPosition" /> is (strictly) negative or (strictly) greater than the total number of tokens in <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property).</exception>
        /// <remarks>
        ///     <para>
        ///         If <paramref name="fromPosition" /> is set, the first <c>max(<paramref name="relevantTokens" />, 1)</c> tokens are chosen accordingly; otherwise they are chosen by calling <see cref="System.Random.Next(Int32)" /> method of an internal <see cref="System.Random" /> object (<see cref="System.Random" />). Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <see cref="System.Random.Next(Int32)" /> method of the internal <see cref="System.Random" /> object. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="SentinelToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        ///
        ///     <para>
        ///         An extra copy of <paramref name="relevantTokens"/> tokens is kept when generating new tokens. Memory errors may occur if <paramref name="relevantTokens"/> is too large.
        ///     </para>
        ///
        ///     <para>
        ///         Calling this method is essentially the same (reproducibility aside) as calling <see cref="Render(Int32, Random, Nullable{Int32})" /> by providing an internal <see cref="System.Random" /> object (<see cref="System.Random" />) as the parameter <c>random</c>. If no specific <see cref="System.Random" /> object or seed should be used, this method will suffice.
        ///     </para>
        ///
        ///     <para>
        ///         The query returned is not run until enumerating it, such as via explicit calls to <see cref="IEnumerable{T}.GetEnumerator" /> method, a <c>foreach</c> loop, a call to <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc. (<em>deferred execution</em>). Since the point of <see cref="System.Random" /> class is to provide a (pseudo-)random number generator, two distinct enumerators over the query may return different results.
        ///     </para>
        ///
        ///     <para>
        ///         It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as a <see cref="List{T}" />, or concatenated together into a string to avoid memory errors. This may be done by calling <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.
        ///     </para>
        ///
        ///     <para>
        ///         The query shall immediately stop, without rendering any tokens, if:
        ///     </para>
        ///
        ///     <list type="number">
        ///         <listheader>
        ///             <term>
        ///                 case
        ///             </term>
        ///             <description>
        ///                 description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 no tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an empty enumerable of tokens,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 all ending tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case),
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 by choice
        ///             </term>
        ///             <description>
        ///                 a <em>successor</em> of the last token or an ending token is picked first.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <seealso cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />
        /// <seealso cref="Render(Int32, Random, Nullable{Int32})" />
        public IEnumerable<String?> Render(Int32 relevantTokens, Nullable<Int32> fromPosition = default) =>
            Render(relevantTokens, RandomNext, fromPosition); // not `Render(relevantTokens, Random, fromPosition)` to avoid accessing the thread-static (pseudo-)random number generator (`Pen.Random`) from multiple threads if the returned query (enumerable) is enumerated from multiple threads
    }
}
