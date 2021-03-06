using MagicText.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>Implements an <see cref="ITokeniser" /> which shatters lines of text one by one.</summary>
    /// <remarks>
    ///     <para>By default, empty tokens (that are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens that yield <c>true</c> when checked via the <see cref="DefaultIsEmptyTokenPredicate" /> method. Derived classes may override this behaviour.</para>
    ///     <para>Shattering methods read and process text <em>line-by-line</em> with all CR, LF and CRLF line breaks treated the same. These line breaks and the end of the input are considered line ends when shattering text, and are therefore substituted by a <see cref="ShatteringOptions.LineEndToken" /> if <see cref="ShatteringOptions.IgnoreLineEnds" /> is <c>false</c>. This behaviour may not be overridden by a derived class.</para>
    ///     <para>The empty lines are substituted by a <see cref="ShatteringOptions.EmptyLineToken" /> if <see cref="ShatteringOptions.IgnoreEmptyLines" /> is <c>false</c>. This behaviour may also not be overridden by a derived class.</para>
    ///
    ///     <h3>Notes to Implementers</h3>
    ///     <para>A derived class must minimally implement <see cref="ShatterLine(String)" /> method to make a useful instance of <see cref="LineByLineTokeniser" />.</para>
    ///     <para>No thread safety mechanism is implemented nor assumed by the class. If the function for checking emptiness of tokens (<see cref="IsEmptyToken" />) should be thread-safe, lock the tokeniser during the complete <see cref="Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method calls to ensure consistent behaviour of the function over a single shattering process.</para>
    /// </remarks>
    /// <seealso cref="ITokeniser" />
    [CLSCompliant(true)]
    public abstract class LineByLineTokeniser : Object, ITokeniser
    {
        protected const string IsEmptyTokenNullErrorMessage = "Token emptiness checking function cannot be null.";
        private const string InputNullErrorMessage = "Input reader cannot be null.";
        private const string LineTokensNullErrorMessage = "Line tokens cannot be null.";
        protected const string LineNullErrorMessage = "Line string cannot be null.";

        /// <summary>Always indicates the <c><paramref name="_" /></c> as non-empty.</summary>
        /// <param name="_">The token to check. This parameter is unused.</param>
        /// <returns>Always <c>false</c>.</returns>
        /// <remarks>
        ///     <para>This function (predicate) may be used by a <see cref="LineByLineTokeniser" /> for <em>checking</em> if a token is empty in cases in which no token should be considered empty (not even <c>null</c>s if <c>null</c>-tokens are possible).</para>
        /// </remarks>
        protected static Boolean IsEmptyTokenAlwaysFalse(String? _) =>
            false;

        private static readonly Func<String?, Boolean> _defaultIsEmptTokenPredicate;

        /// <summary>Gets the default function (predicate) for checking if a token is empty.</summary>
        /// <returns>The default function for checking if a token is empty.</returns>
        /// <remarks>
        ///     <para>This function is equivalent to the <see cref="String.IsNullOrEmpty(String)" /> method.</para>
        /// </remarks>
        public static Func<String?, Boolean> DefaultIsEmptyTokenPredicate => _defaultIsEmptTokenPredicate;

        /// <summary>Initialises static fields.</summary>
        static LineByLineTokeniser()
        {
            _defaultIsEmptTokenPredicate = String.IsNullOrEmpty;
        }

        private readonly Func<String?, Boolean> _isEmptyToken;
        private readonly Func<String?, Boolean> _isNonEmptyToken;

        /// <summary>Gets the function (predicate) to check if a token is empty: it returns <c>true</c> if and only if the token to check is empty.</summary>
        /// <returns>The token emptiness checking function (predicate).</returns>
        protected Func<String?, Boolean> IsEmptyToken => _isEmptyToken;

        /// <summary>Gets the function (predicate) to check if a token is non-empty: it returns <c>true</c> if and only if the token to check is non-empty.</summary>
        /// <returns>The token non-emptiness checking function (predicate).</returns>
        protected Func<String?, Boolean> IsNonEmptyToken => _isNonEmptyToken;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="isEmptyToken">The function (predicate) for checking if a token is empty.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="isEmptyToken" /></c> is <c>null</c>.</exception>
        protected LineByLineTokeniser(Func<String?, Boolean> isEmptyToken) : base()
        {
            _isEmptyToken = isEmptyToken ?? throw new ArgumentNullException(nameof(isEmptyToken), IsEmptyTokenNullErrorMessage);
            _isNonEmptyToken = (new NegativePredicateWrapper<String?>(IsEmptyToken)).EvaluateNegation;
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>The function (predicate) <see cref="DefaultIsEmptyTokenPredicate" /> is used for checking if a token is empty.</para>
        /// </remarks>
        public LineByLineTokeniser() : this(DefaultIsEmptyTokenPredicate)
        {
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <remarks>
        ///     <h3>Notes to Implementers</h3>
        ///     <para>The method <strong>should not</strong> produce an <see cref="ShatteringOptions.EmptyLineToken" /> to represent an empty line or a <see cref="ShatteringOptions.LineEndToken" /> at the <c><paramref name="line" /></c>'s end. Also, the method <strong>should not</strong> manually filter out empty tokens. Hence no <see cref="ShatteringOptions" /> are available to the method. The result of an empty line should be an empty enumerable. Empty tokens, empty lines and the line ends are treated within the scope of the <see cref="LineByLineTokeniser" /> parent class and its methods.</para>
        ///     <para>It is guaranteed that, when called from the <see cref="LineByLineTokeniser" />'s non-overridable methods, the <c><paramref name="line" /></c> shall be a non-<c>null</c> <see cref="String" /> not containing a line end (CR, LF or CRLF). Nonetheless, when calling from a derived class, its programmer may call the method however they wish.</para>
        /// </remarks>
        protected abstract IEnumerable<String?> ShatterLine(String line);

        /// <summary>Shatters the text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="input">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="NullReferenceException">The method <see cref="ShatterLine(String)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after disposing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="TextReader.ReadLine()" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        /// <seealso cref="TokeniserExtensions" />
        public IEnumerable<String?> Shatter(TextReader input, ShatteringOptions? options = null)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            options ??= ShatteringOptions.Default;

            // Declare:
            Boolean addLineEnd = false; // the indicator that a line end should be added

            // Shatter text from the `input` line-by-line.
            while (true)
            {
                // Return the `options.LineEndToken` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    yield return options.LineEndToken;
                }

                // Read and shatter the next line.

                String? line = input.ReadLine();
                if (line is null)
                {
                    yield break;
                }

                IEnumerable<String?> lineTokens = ShatterLine(line) ?? throw new NullReferenceException(LineTokensNullErrorMessage);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(IsNonEmptyToken);
                }

                // Return the appropriate tokens and update `addLineEnd`.
                {
                    Int32 i;

                    using (IEnumerator<String?> en = lineTokens.GetEnumerator())
                    {
                        for (i = 0; en.MoveNext(); ++i)
                        {
                            yield return en.Current;
                        }
                    }

                    do
                    {
                        if (i == 0) // <-- no new tokens were returned (the line is empty)
                        {
                            if (options.IgnoreEmptyLines)
                            {
                                addLineEnd = false;

                                break;
                            }
                            else
                            {
                                yield return options.EmptyLineToken;
                            }
                        }

                        addLineEnd = true;
                    }
                    while (false);
                }
            }
        }

        /// <summary>Shatters the text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="input">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) should be used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="NullReferenceException">The method <see cref="ShatterLine(String)" /> call returns <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, if the <c><paramref name="input" /></c> is a <see cref="StreamReader" />, data having already been read from the underlying <see cref="Stream" /> may be irrecoverable when cancelling the operation.</para>
        ///     <para>The enumeration and, consequently, the shattering operation may be cancelled via the <see cref="TaskAsyncEnumerableExtensions.WithCancellation{T}(IAsyncEnumerable{T}, CancellationToken)" /> extension method as the parameter <c><paramref name="cancellationToken" /></c> is set with the <see cref="EnumeratorCancellationAttribute" /> attribute.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after disposing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="TextReader.ReadLineAsync()" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions" />
        /// <seealso cref="ShatteringOptions" />
        public async IAsyncEnumerable<String?> ShatterAsync(TextReader input, ShatteringOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            options ??= ShatteringOptions.Default;

            // Declare:
            Boolean addLineEnd = false; // the indicator that a line end should be added

            // Shatter text from the `input` line-by-line.
            while (true)
            {
                // Return the `options.LineEndToken` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return options.LineEndToken;
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Read and shatter the next line.

                String? line = await input.ReadLineAsync().ConfigureAwait(continueTasksOnCapturedContext);
                if (line is null)
                {
                    yield break;
                }

                IEnumerable<String?> lineTokens = ShatterLine(line) ?? throw new NullReferenceException(LineTokensNullErrorMessage);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(IsNonEmptyToken);
                }

                // Return the appropriate tokens and update `addLineEnd`.
                {
                    Int32 i;

                    using (IEnumerator<String?> en = lineTokens.GetEnumerator())
                    {
                        for (i = 0; en.MoveNext(); ++i)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            yield return en.Current;
                        }
                    }

                    do
                    {
                        if (i == 0) // <-- no new tokens were returned (the line is empty)
                        {
                            if (options.IgnoreEmptyLines)
                            {
                                addLineEnd = false;

                                break;
                            }
                            else
                            {
                                yield return options.EmptyLineToken;
                            }
                        }

                        addLineEnd = true;
                    }
                    while (false);
                }
            }
        }
    }
}
