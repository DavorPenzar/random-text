using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Tokeniser which shatters text shattering its lines one by one.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A derived class must minimally implement <see cref="ShatterLine(String)" /> method to make a useful (non-abstract) instance of <see cref="LineByLineTokeniser" />.
    ///     </para>
    ///
    ///     <para>
    ///         By default, empty tokens are considered those tokens that yield <c>true</c> when checked via <see cref="String.IsNullOrEmpty(String)" /> method. Derived classes may override this behaviour.
    ///     </para>
    ///
    ///     <para>
    ///         Shattering methods read and process text <em>line-by-line</em> with all CR, LF and CRLF line breaks treated the same.
    ///     </para>
    ///
    ///     <para>
    ///         No thread safety mechanism is implemented nor assumed by the class. If the function for checking emptiness of tokens (<see cref="IsEmptyToken" />) should be thread-safe, lock the tokeniser during complete <see cref="Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method calls to ensure consistent behaviour of the function over a single shattering process.
    ///     </para>
    /// </remarks>
    public abstract class LineByLineTokeniser : ITokeniser
    {
        private const string IsEmptyTokenNullErrorMessage = "Function for checking emptiness of tokens may not be `null`.";
        private const string InputNullErrorMessage = "Input stream reader may not be `null`.";
        private const string LineTokensNullErrorMessage = "Line tokens may not be `null`.";
        protected const string LineNullErrorMessage = "Line string may not be `null`.";

        /// <summary>
        ///     <para>
        ///         Auxiliary predicate wrapper that exposes a predicate's negation.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of the parameter of the predicate that this class encapsulates.</typeparam>
        private class NegativePredicateWrapper<T>
        {
            private const string PositivePredicateNullErrorMessage = "Positive predicate may not be `null`.";

            private readonly Func<T, Boolean> _positivePredicate;

            /// <returns>Predicate that is negated through <see cref="Evaluate(T)" /> method.</returns>
            public Func<T, Boolean> PositivePredicate => _positivePredicate;

            /// <summary>
            ///     <para>
            ///         Create a negative wrapper of a predicate.
            ///     </para>
            /// </summary>
            /// <param name="positivePredicate">Predicate that is negated through <see cref="Evaluate(T)" /> method.</param>
            /// <exception cref="ArgumentNullException">Parameter <paramref name="positivePredicate" /> is <c>null</c>.</exception>
            public NegativePredicateWrapper(Func<T, Boolean> positivePredicate)
            {
                _positivePredicate = positivePredicate ?? throw new ArgumentNullException(nameof(positivePredicate), PositivePredicateNullErrorMessage);
            }

            /// <summary>
            ///     <para>
            ///         Check negation of the evaluation of the argument via encapsulated predicate (<see cref="PositivePredicate" />).
            ///     </para>
            /// </summary>
            /// <param name="arg">The parameter to check.</param>
            /// <returns>Boolean negation (<c>true</c> to <c>false</c> and vice versa) of the evaluation of <paramref name="arg" /> via encapsulated predicate (<see cref="PositivePredicate" />). Simply put, the method returns <c>!<see cref="PositivePredicate" />(<paramref name="arg" />)</c>.</returns>
            /// <remarks>
            ///     <para>
            ///         Exceptions thrown by the encapsuplated predicate (<see cref="PositivePredicate" />) are not caught.
            ///     </para>
            /// </remarks>
            public Boolean Evaluate(T arg) =>
                !PositivePredicate(arg);
        }

        /// <summary>
        ///     <para>
        ///         Always indicate the token as non-empty.
        ///     </para>
        /// </summary>
        /// <param name="token">Token to check.</param>
        /// <returns><c>false</c></returns>
        protected static Boolean IsEmptyTokenAlwaysFalse(String? token) =>
            false;

        private readonly Func<String?, Boolean> _isEmptyToken;
        private readonly Func<String?, Boolean> _isNonEmptyToken;

        /// <returns>Function to check if a token is empty: it returns <c>true</c> if and only if the token to check is empty.</returns>
        protected Func<String?, Boolean> IsEmptyToken => _isEmptyToken;

        /// <returns>Function to check if a token is non-empty: it returns <c>true</c> if and only if the token to check is non-empty.</returns>
        protected Func<String?, Boolean> IsNonEmptyToken => _isNonEmptyToken;

        /// <summary>
        ///     <para>
        ///         Initialise a tokeniser.
        ///     </para>
        /// </summary>
        public LineByLineTokeniser() : this(String.IsNullOrEmpty)
        {
        }

        /// <summary>
        ///     <para>
        ///         Initialise a tokeniser with provided options.
        ///     </para>
        /// </summary>
        /// <param name="isEmptyToken">Function to check if a token is empty.</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="isEmptyToken" /> is <c>null</c>.</exception>
        protected LineByLineTokeniser(Func<String?, Boolean> isEmptyToken)
        {
            _isEmptyToken = isEmptyToken ?? throw new ArgumentNullException(nameof(isEmptyToken), IsEmptyTokenNullErrorMessage);
            _isNonEmptyToken = (new NegativePredicateWrapper<String?>(IsEmptyToken)).Evaluate;
        }

        /// <summary>
        ///     <para>
        ///         Shatter a single line into tokens.
        ///     </para>
        /// </summary>
        /// <param name="line">Line of text to shatter.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="line" />.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="line" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         The method <strong>should not</strong> produce <see cref="ShatteringOptions.EmptyLineToken" />s to represent empty lines and <see cref="ShatteringOptions.LineEndToken" />s for line ends. Also, the method <strong>should not</strong> manually filter out empty tokens. Hence no <see cref="ShatteringOptions" /> are available to the method. The result of an empty line should be an empty enumerable, while empty tokens, empty lines and line ends are treated within the scope of <see cref="LineByLineTokeniser" /> parent class and its methods.
        ///     </para>
        ///
        ///     <para>
        ///         It is guaranteed that, when called from <see cref="LineByLineTokeniser" />'s non-overridable methods, <paramref name="line" /> shall be a non-<c>null</c> string not containing a line end (CR, LF or CRLF). Nonetheless, when calling from a derived class, its programmer may call the method however they wish, but this is beyond the original programmer's scope.
        ///     </para>
        /// </remarks>
        protected abstract IEnumerable<String?> ShatterLine(String line);

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <returns>Query to enumerate tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <exception cref="NullReferenceException">Method <see cref="ShatterLine(String)" /> returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Returned enumerable is merely a query for enumerating tokens to allow simultaneously reading and enumerating tokens from <paramref name="input" />. If a fully built container is needed, consider using <see cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" /> extension method instead to improve performance and avoid accidentally enumerating the query after disposing <paramref name="input" />.
        ///     </para>
        ///
        ///     <para>
        ///         The method returns the equivalent enumeration of tokens as <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public IEnumerable<String?> Shatter(TextReader input, ShatteringOptions? options = null)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            if (options is null)
            {
                options = new ShatteringOptions();
            }

            // Declare:
            bool addLineEnd = false; // indicator that a line end should be added

            // Shatter text from `input` line-by-line.
            while (true)
            {
                // Add `options.LineEndToken` to `tokens` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    yield return options.LineEndToken;
                }

                // Read and shatter next line.

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

                // Return appropriate tokens and update `addLineEnd`.
                {
                    int i = 0;

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

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (<see cref="TextReader.ReadLineAsync" /> method calls) is marshalled back to the original context (via <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method). See <em>Remarks</em> for additional information.</param>
        /// <returns>Query to asynchronously enumerate tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <exception cref="NullReferenceException">Method <see cref="ShatterLine(String)" /> returns <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">Operation is cancelled via <paramref name="cancellationToken" />.</exception>
        /// <remarks>
        ///     <para>
        ///         Although the method accepts <paramref name="cancellationToken" /> to support cancelling the operation, this should be used with caution. For instance, if <paramref name="input" /> is <see cref="StreamReader" />, data having already been read from underlying <see cref="Stream" /> may be irrecoverable when cancelling the operation.
        ///     </para>
        ///
        ///     <para>
        ///         The enumeration may be cancelled via <see cref="TaskAsyncEnumerableExtensions.WithCancellation{T}(IAsyncEnumerable{T}, CancellationToken)" /> extension method as parameter <paramref name="cancellationToken" /> is set with <see cref="EnumeratorCancellationAttribute" /> attribute.
        ///     </para>
        ///
        ///     <para>
        ///         Usually the default <c>false</c> value of <paramref name="continueTasksOnCapturedContext" /> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by <paramref name="input" />, and thus <paramref name="continueTasksOnCapturedContext" /> should be set to <c>true</c> to avoid errors.
        ///     </para>
        ///
        ///     <para>
        ///         Returned enumerable is merely a query for enumerating tokens to allow simultaneously reading and enumerating tokens from <paramref name="input" />. If a fully built container is needed, consider using <see cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> extension method instead to improve performance and avoid accidentally enumerating the query after disposing <paramref name="input" />.
        ///     </para>
        ///
        ///     <para>
        ///         The method ultimately returns the equivalent enumeration of tokens as <see cref="Shatter(TextReader, ShatteringOptions?)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public async IAsyncEnumerable<String?> ShatterAsync(TextReader input, ShatteringOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            if (options is null)
            {
                options = new ShatteringOptions();
            }

            // Declare:
            bool addLineEnd = false; // indicator that a line end should be added

            // Shatter text from `input` line-by-line.
            while (true)
            {
                // Add `options.LineEndToken` to `tokens` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    yield return options.LineEndToken;
                }

                // Read and shatter next line if not cancelled.

                cancellationToken.ThrowIfCancellationRequested();

                String? line = await input.ReadLineAsync().ConfigureAwait(continueTasksOnCapturedContext);
                if (line is null)
                {
                    break;
                }

                IEnumerable<String?> lineTokens = ShatterLine(line) ?? throw new NullReferenceException(LineTokensNullErrorMessage);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(IsNonEmptyToken);
                }

                // Return appropriate tokens and update `addLineEnd`.
                {
                    int i = 0;

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
    }
}
