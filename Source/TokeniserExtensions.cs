using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>Provides auxiliary extension methods for the instances of the <see cref="ITokeniser" /> interface.</summary>
    /// <seealso cref="ITokeniser" />
    [CLSCompliant(true)]
    public static class TokeniserExtensions
    {
        private const int DefaultBufferSize = 1024;
        private const bool DefaultDetectEncodingFromByteOrderMarks = true;

        private const string TokeniserNullErrorMessage = "Tokeniser cannot be null.";
        private const string TextNullErrorMessage = "Input string cannot be null.";
        private const string InputNullErrorMessage = "Input stream cannot be null.";
        private const string InvalidStreamErrorMessage = "Cannot read from the input stream.";

        /// <summary>Gets the default <see cref="Encoding" /> for reading and writing <see cref="Stream" />s as text resources, i. e. <see cref="Encoding.UTF8" />.</summary>
        /// <returns>The default <see cref="Encoding" />.</returns>
        private static Encoding DefaultEncoding => Encoding.UTF8;

        /// <summary>Creates a <see cref="StreamReader" /> for reading <c><paramref name="stream" /></c>.</summary>
        /// <param name="stream">The <see cref="Stream" /> from which to read data.</param>
        /// <returns>A <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c> with default settings.</returns>
        /// <remarks>
        ///     <para>The method is intended for the internal use only and therefore does not make unnecessary checks of the parameters.</para>
        ///     <para>The <see cref="TokeniserExtensions" />' internal default settings are used for construction of the <see cref="StreamReader" />. These settings should coincide with the actual defaults of the <see cref="StreamReader" /> class.</para>
        ///     <para>Disposing of the <see cref="StreamReader" /> will neither dispose nor close the <c><paramref name="stream" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor are not caught.</para>
        /// </remarks>
        private static StreamReader CreateDefaultStreamReader(Stream stream) =>
            new StreamReader(stream: stream, encoding: DefaultEncoding, detectEncodingFromByteOrderMarks: DefaultDetectEncodingFromByteOrderMarks, bufferSize: DefaultBufferSize, leaveOpen: true);

        /// <summary>Shatters <c><paramref name="text" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="text" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="text" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, String text, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), TextNullErrorMessage);
            }

            using TextReader textReader = new StringReader(text);
            foreach (String? token in tokeniser.Shatter(textReader, options))
            {
                yield return token;
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with default settings (regarding the <see cref="Encoding" /> and the buffer size). To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            TextReader inputReader;
            try
            {
                inputReader = CreateDefaultStreamReader(input);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(InvalidStreamErrorMessage, nameof(input), ex);
            }

            foreach (String? token in tokeniser.Shatter(inputReader, options))
            {
                yield return token;
            }

            inputReader.Dispose();
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The list of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null.</c></exception>
        /// <remarks>
        ///     <para>The returned enumerable is a fully-built container and is therefore safe to enumerate even after disposing the <c><paramref name="input" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IList<String?> ShatterToList(this ITokeniser tokeniser, TextReader input, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>(tokeniser.Shatter(input, options));
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters <c><paramref name="text" /></c> into a token list.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The list of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="text" /></c> is <c>null.</c></exception>
        /// <remarks>
        ///     <para>The returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="Shatter(ITokeniser, String, ShatteringOptions?)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IList<String?> ShatterToList(this ITokeniser tokeniser, String text, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>(tokeniser.Shatter(text, options));
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The list of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null.</c></exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with default settings (regarding the <see cref="Encoding" /> and the buffer size). To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IList<String?> ShatterToList(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>(tokeniser.Shatter(input, options));
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters <c><paramref name="text" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="text" /></c> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Since <see cref="String" />s are immutable and the encapsulated <see cref="StringReader" /> is not available outside of the method, the <c><paramref name="cancellationToken" /></c> may be used to cancel the shattering process without extra caution.</para>
        ///     <para>The parameter <c><paramref name="continueTasksOnCapturedContext" /></c> should always be set to <c>false</c> as every context has reading access to all <see cref="String" />s, including <c><paramref name="text" /></c>. Providing <c>true</c> as <c><paramref name="continueTasksOnCapturedContext" /></c> indeed passes the value to the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call, which may result in negative consequences. The parameter is exposed only to maintain consistency of method signatures and calls.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="text" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), TextNullErrorMessage);
            }

            using TextReader textReader = new StringReader(text);
            await foreach (String? token in tokeniser.ShatterAsync(textReader, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                yield return token;
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with default settings (regarding the <see cref="Encoding" /> and the buffer size). To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, data having already been read from the <see cref="Stream" /> <c><paramref name="input" /></c> may be irrecoverable when cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), TextNullErrorMessage);
            }

            TextReader inputReader;
            try
            {
                inputReader = CreateDefaultStreamReader(input);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(InvalidStreamErrorMessage, nameof(input), ex);
            }

            await foreach (String? token in tokeniser.ShatterAsync(inputReader, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                yield return token;
            }

            inputReader.Dispose();
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method).</param>
        /// <returns>The task that represents the asynchronous shattering operation. Its value of the <see cref="Task{TResult}.Result" /> property is the list of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null.</c></exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, if the <c><paramref name="input" /></c> is a <see cref="StreamReader" />, data having already been read from the underlying <see cref="Stream" /> may be irrecoverable when cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The ultimately returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IList<String?>> ShatterToListAsync(this ITokeniser tokeniser, TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>();
            await foreach (String? token in tokeniser.ShatterAsync(input, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                tokens.Add(token);
            }
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters <c><paramref name="text" /></c> into a token list asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method).</param>
        /// <returns>The task that represents the asynchronous shattering operation. Its value of the <see cref="Task{TResult}.Result" /> property is the list of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="text" /></c> is <c>null.</c></exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Since <see cref="String" />s are immutable and the encapsulated <see cref="StringReader" /> is not available outside of the method, the <c><paramref name="cancellationToken" /></c> may be used to cancel the shattering process without extra caution.</para>
        ///     <para>The parameter <c><paramref name="continueTasksOnCapturedContext" /></c> should always be set to <c>false</c> as every context has reading access to all <see cref="String" />s, including <c><paramref name="text" /></c>. Providing <c>true</c> as <c><paramref name="continueTasksOnCapturedContext" /></c> indeed passes the value to the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call, which may result in negative consequences. The parameter is exposed only to maintain consistency of method signatures and calls.</para>
        ///     <para>The ultimately returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IList<String?>> ShatterToListAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>();
            await foreach (String? token in tokeniser.ShatterAsync(text, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                tokens.Add(token);
            }
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with default settings (regarding the <see cref="Encoding" /> and the buffer size). To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, data having already been read from the <see cref="Stream" /> <c><paramref name="input" /></c> may be irrecoverable when cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The ultimately returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IList<String?>> ShatterToListAsync(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>();
            await foreach (String? token in tokeniser.ShatterAsync(input, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                tokens.Add(token);
            }
            tokens.TrimExcess();

            return tokens;
        }
    }
}
