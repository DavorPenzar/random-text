#if NETSTANDARD2_0
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#endif // NETSTANDARD2_0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml.Serialization;

namespace MagicText
{
    /// <summary>Defines the options for the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods and for the extension methods from <see cref="TokeniserExtensions" />.</summary>
    /// <seealso cref="ITokeniser" />
    /// <seealso cref="TokeniserExtensions" />
    [CLSCompliant(true)]
    [Serializable]
    public class ShatteringOptions : Object, IEquatable<ShatteringOptions>, ICloneable
    {
        protected const string SerialisationInfoNullErrorMessage = "Serialisation info cannot be null.";
        private const string OtherNullErrorMessage = "Shattering options to copy cannot be null.";
        protected const string StringComparerNullErrorMessage = "String comparer cannot be null.";
#if !NETSTANDARD2_0
        protected const string StringComparisonNotSupportedErrorMessage = "The string comparison type passed in is currently not supported.";
#endif // NETSTANDARD2_0

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are equal to the <c><paramref name="right" /></c> or not.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="stringComparer">The <see cref="IEqualityComparer{T}" /> of <see cref="String" />s used for comparing <see cref="String" />s for equality and for retrieving <see cref="String" />s' hash codes.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="stringComparer" /></c> is <c>null</c>.</exception>
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?, StringComparison)" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public static Boolean Equals(ShatteringOptions? left, ShatteringOptions? right, IEqualityComparer<String?> stringComparer) =>
            stringComparer is null ? throw new ArgumentNullException(nameof(stringComparer), StringComparerNullErrorMessage) :
                left is null || right is null ? Object.ReferenceEquals(left, right) : left.Equals(right, stringComparer);

#if !NETSTANDARD2_0
        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are equal to the <c><paramref name="right" /></c> or not.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="stringComparison">One of the enumeration values that specifies how <see cref="String" />s should be compared.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">The parameter <c><paramref name="stringComparison" /></c> is not a <see cref="StringComparison" /> value.</exception>
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(Object?, StringComparison)"/>
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public static Boolean Equals(ShatteringOptions? left, ShatteringOptions? right, StringComparison stringComparison)
        {
            try
            {
                return Equals(left, right, StringComparer.FromComparison(stringComparison));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(StringComparisonNotSupportedErrorMessage, nameof(stringComparison), ex);
            }
        }
#endif // NETSTANDARD2_0

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are equal to the <c><paramref name="right" /></c> or not.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?, StringComparison)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public static Boolean Equals(ShatteringOptions? left, ShatteringOptions? right) =>
            Equals(left, right, EqualityComparer<String?>.Default);

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are equal to the <c><paramref name="right" /></c> or not.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?, StringComparison)" />
        public static Boolean operator ==(ShatteringOptions? left, ShatteringOptions? right) =>
            Equals(left, right);

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are not equal to the <c><paramref name="right" /></c>.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> are not equal, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?, StringComparison)" />
        public static Boolean operator !=(ShatteringOptions? left, ShatteringOptions? right) =>
            !Equals(left, right);

        /// <summary>Gets the default <see cref="ShatteringOptions" /> to use when no options are set.</summary>
        /// <returns>The default <see cref="ShatteringOptions" />.</returns>
        /// <remarks>
        ///     <para>Retrieving the value of this property is <strong>exactly the same</strong> as creating new <see cref="ShatteringOptions" /> using the default <see cref="ShatteringOptions()" /> constructor. The property is provided merely to enable a more readable and explicit code when handling <c>null</c>-options in the implementations of the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods.</para>
        ///     <para>The property always returns a new reference (to the newly constructed default <see cref="ShatteringOptions" />). Hence changes made to one <see cref="ShatteringOptions" /> returned by the property do not affect the others. Also, <c><see cref="Object" />.ReferenceEquals(<see cref="Default" />, <see cref="Default" />)</c> never evaluates to <c>true</c>, although all <c><see cref="Default" /> == <see cref="Default" /></c>, <c><see cref="ShatteringOptions" />.Equals(<see cref="Default" />, <see cref="Default" />)</c> and <c><see cref="Default" />.Equals(<see cref="Default" />)</c> evaluate to <c>true</c> because of overloads of methods and operators.</para>
        /// </remarks>
        public static ShatteringOptions Default => new ShatteringOptions();

        [XmlIgnore]
        [JsonIgnore]
        private Boolean ignoreEmptyTokens;

        [XmlIgnore]
        [JsonIgnore]
        private Boolean ignoreLineEnds;

        [XmlIgnore]
        [JsonIgnore]
        private Boolean ignoreEmptyLines;

        [XmlIgnore]
        [JsonIgnore]
        private String? lineEndToken;

        [XmlIgnore]
        [JsonIgnore]
        private String? emptyLineToken;

        /// <summary>Gets or sets the policy of ignoring empty tokens: <c>true</c> if ignoring, <c>false</c> otherwise. The default is <c>false</c>.</summary>
        /// <returns>If empty tokens should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>The new ignoring empty tokens policy value.</value>
        /// <remarks>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly an <em>empty</em> token means, but usually this would be a <c>null</c> or a <see cref="String" /> yielding <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> or <see cref="String.IsNullOrWhiteSpace(String)" /> method calls.</para>
        /// </remarks>
        [DisplayName("Ignore empty tokens")]
        [Display(Name = "Ignore empty tokens", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "Empty tokens", Order = 1)]
        [Editable(true, AllowInitialValue = true)]
        [DefaultValue(false)]
        public Boolean IgnoreEmptyTokens
        {
            get => ignoreEmptyTokens;
            set
            {
                ignoreEmptyTokens = value;
            }
        }

        /// <summary>Gets or sets the policy of ignoring line ends: <c>true</c> if ignoring, <c>false</c> otherwise. The default is <c>false</c>.</summary>
        /// <returns>If line ends should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>The new ignoring line ends policy value.</value>
        /// <remarks>
        ///     <para>If the value is <c>false</c>, then line ends should be copied or represented by <see cref="LineEndToken" />s.</para>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly a <em>line end</em> means, but usually this would be the new line characters and sequences of characters (CR, LF and CRLF) and/or the end of the input.</para>
        /// </remarks>
        /// <seealso cref="IgnoreEmptyLines" />
        /// <seealso cref="LineEndToken" />
        /// <seealso cref="EmptyLineToken" />
        [DisplayName("Ignore line ends")]
        [Display(Name = "Ignore line ends", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "Line ends", Order = 2)]
        [Editable(true, AllowInitialValue = true)]
        [DefaultValue(false)]
        public Boolean IgnoreLineEnds
        {
            get => ignoreLineEnds;
            set
            {
                ignoreLineEnds = value;
            }
        }

        /// <summary>Gets or sets the policy of ignoring empty lines: <c>true</c> if ignoring, <c>false</c> otherwise. The default is <c>false</c>.</summary>
        /// <returns>If empty lines should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>The new ignoring empty lines policy value.</value>
        /// <remarks>
        ///     <para>If the value is <c>true</c>, empty lines might not produce even <see cref="LineEndToken" />s (it depends on the <see cref="ITokeniser" /> interface implementation used); if <c>false</c>, they should be represented by <see cref="EmptyLineToken" />s.</para>
        ///     <para>A <em>line</em> should be considered a <see cref="String" /> of text spanned between two consecutive line ends, whereas the definition of a <em>line end</em> is left to the actual implementation of the <see cref="ITokeniser" /> interface (v. <see cref="IgnoreLineEnds" /> and <see cref="LineEndToken" />). Of course, a line's border may also be the beginning and/or the end of the text.</para>
        ///     <para>Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering the empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.</para>
        /// </remarks>
        /// <seealso cref="IgnoreLineEnds" />
        /// <seealso cref="LineEndToken" />
        /// <seealso cref="EmptyLineToken" />
        [DisplayName("Ignore empty lines")]
        [Display(Name = "Ignore empty lines", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "Empty lines", Order = 3)]
        [Editable(true, AllowInitialValue = true)]
        [DefaultValue(false)]
        public Boolean IgnoreEmptyLines
        {
            get => ignoreEmptyLines;
            set
            {
                ignoreEmptyLines = value;
            }
        }

        /// <summary>Gets or sets the token to represent a line end. The default is the <see cref="Environment.NewLine" />.</summary>
        /// <returns>The line end token.</returns>
        /// <value>The new line end token value.</value>
        /// <remarks>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly a <em>line end</em> means, but usually this would be the new line characters and sequences of characters (CR, LF and CRLF) and/or the end of the input. Furthermore, an actual implementation of the <see cref="ITokeniser" /> interface at hand may as well choose to merely copy the line end, and not replace it with a <see cref="LineEndToken" />. The property is given only to allow the standardisation of line ends when shattering text, but not to force it.</para>
        ///     <para>If a line is discarded as empty (when <see cref="IgnoreEmptyLines" /> is <c>true</c>), it might not produce <see cref="LineEndToken" />—it depends on the <see cref="ITokeniser" /> interface implementation used.</para>
        /// </remarks>
        /// <seealso cref="IgnoreLineEnds" />
        /// <seealso cref="IgnoreEmptyLines" />
        /// <seealso cref="EmptyLineToken" />
        [DataType(DataType.MultilineText)]
        [DisplayName("Line end token")]
        [Display(Name = "Line end token", Description = "Token to represent a line end.", GroupName = "Tokens", ShortName = "Line end", Order = 4)]
        [DisplayFormat(ConvertEmptyStringToNull = false, HtmlEncode = true)]
        [Editable(true, AllowInitialValue = true)]
        [DefaultValue("\n")] // <-- this may be different from the `System.Environment.NewLine`
        public String? LineEndToken
        {
            get => lineEndToken;
            set
            {
                lineEndToken = value;
            }
        }

        /// <summary>Gets or sets the token to represent an empty line. The default is the <see cref="String.Empty" />.</summary>
        /// <returns>The empty line token.</returns>
        /// <value>The new empty line token value.</value>
        /// <remarks>
        ///     <para>If a line is empty but should not be discarded (if <see cref="IgnoreEmptyLines" /> is <c>false</c>), it might be be represented by a <see cref="EmptyLineToken" />. If empty lines are substituted by <see cref="EmptyLineToken" />s, it should be done even if the <see cref="EmptyLineToken" /> would be considered an empty token and empty tokens should be ignored (if <see cref="IgnoreEmptyTokens" /> is <c>true</c>). However, an actual implementation of the <see cref="ITokeniser" /> interface at hand may choose to simply yield 0 tokens for an empty line instead of the <see cref="EmptyLineToken" />. The property is given only to allow using empty lines as special parts of text (such as paragraph breaks), but not to force it.</para>
        ///     <para>A <em>line</em> should be considered a <see cref="String" /> of text between two consecutive line ends, whereas the definition of a <em>line end</em> is left to the actual implementation of the <see cref="ITokeniser" /> interface (v. <see cref="IgnoreLineEnds" /> and <see cref="LineEndToken" />). Of course, a line's border may also be the beginning and/or the end of the text.</para>
        ///     <para>Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering the empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.</para>
        /// </remarks>
        /// <seealso cref="IgnoreLineEnds" />
        /// <seealso cref="IgnoreEmptyLines" />
        /// <seealso cref="LineEndToken" />
        [DataType(DataType.MultilineText)]
        [DisplayName("Empty line token")]
        [Display(Name = "Empty line token", Description = "Token to represent an empty line.", GroupName = "Tokens", ShortName = "Empty line", Order = 5)]
        [DisplayFormat(ConvertEmptyStringToNull = false, HtmlEncode = true)]
        [Editable(true, AllowInitialValue = true)]
        [DefaultValue("")]
        public String? EmptyLineToken
        {
            get => emptyLineToken;
            set
            {
                emptyLineToken = value;
            }
        }

        /// <summary>Creates options.</summary>
        /// <param name="ignoreEmptyTokens">The indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">The indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">The indicator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">The token to represent a line end.</param>
        /// <param name="emptyLineToken">The token to represent an empty line.</param>
        public ShatteringOptions(Boolean ignoreEmptyTokens, Boolean ignoreLineEnds, Boolean ignoreEmptyLines, String? lineEndToken, String? emptyLineToken) : base()
        {
            this.ignoreEmptyTokens = ignoreEmptyTokens;
            this.ignoreLineEnds = ignoreLineEnds;
            this.ignoreEmptyLines = ignoreEmptyLines;
            this.lineEndToken = lineEndToken;
            this.emptyLineToken = emptyLineToken;
        }

        /// <summary>Creates default options.</summary>
        public ShatteringOptions() : this(false, false, false, Environment.NewLine, String.Empty)
        {
        }

        /// <summary>Copies the <c><paramref name="other" /></c> options.</summary>
        /// <param name="other">The <see cref="ShatteringOptions" /> to copy.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="other" /></c> is <c>null</c>.</exception>
        public ShatteringOptions(ShatteringOptions other) :
            this(
                other is null ? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage) : other.IgnoreEmptyTokens,
                other is null ? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage) : other.IgnoreLineEnds,
                other is null ? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage) : other.IgnoreEmptyLines,
                other?.LineEndToken,
                other?.EmptyLineToken
            )
        {
        }

        /// <summary>Deconstructs the current options.</summary>
        /// <param name="ignoreEmptyTokens">The indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">The indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">The indicator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">The token to represent a line end.</param>
        /// <param name="emptyLineToken">The token to represent an empty line.</param>
        public virtual void Deconstruct(out Boolean ignoreEmptyTokens, out Boolean ignoreLineEnds, out Boolean ignoreEmptyLines, out String? lineEndToken, out String? emptyLineToken)
        {
            ignoreEmptyTokens = IgnoreEmptyTokens;
            ignoreLineEnds = IgnoreLineEnds;
            ignoreEmptyLines = IgnoreEmptyLines;
            lineEndToken = LineEndToken;
            emptyLineToken = EmptyLineToken;
        }

        /// <summary>Returns the hash code of the current options.</summary>
        /// <param name="stringComparer">The <see cref="IEqualityComparer{T}" /> of <see cref="String" />s used for comparing <see cref="String" />s for equality and for retrieving <see cref="String" />s' hash codes.</param>
        /// <returns>The hash code of the current <see cref="ShatteringOptions" /> according to the <c><paramref name="stringComparer" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="stringComparer" /></c> is <c>null</c>.</exception>
        /// <seealso cref="GetHashCode(StringComparison)" />
        /// <seealso cref="GetHashCode()" />
        public virtual Int32 GetHashCode(IEqualityComparer<String?> stringComparer)
        {
            if (stringComparer is null)
            {
                throw new ArgumentNullException(nameof(stringComparer), StringComparerNullErrorMessage);
            }

            Int32 hashCode = 7;

            hashCode = 31 * hashCode + IgnoreEmptyTokens.GetHashCode();
            hashCode = 31 * hashCode + IgnoreLineEnds.GetHashCode();
            hashCode = 31 * hashCode + IgnoreEmptyLines.GetHashCode();
            try
            {
                hashCode = 31 * hashCode + stringComparer.GetHashCode(LineEndToken);
            }
            catch (ArgumentNullException) when (LineEndToken is null)
            {
                hashCode *= 31;
            }
            try
            {
                hashCode = 31 * hashCode + stringComparer.GetHashCode(EmptyLineToken);
            }
            catch (ArgumentNullException) when (EmptyLineToken is null)
            {
                hashCode *= 31;
            }

            return hashCode;
        }

#if !NETSTANDARD2_0
        /// <summary>Returns the hash code of the current options.</summary>
        /// <param name="stringComparison">One of the enumeration values that specifies how <see cref="String" />s should be compared.</param>
        /// <returns>The hash code of the current <see cref="ShatteringOptions" /> according to the <see cref="StringComparer" /> specified by the <c><paramref name="stringComparison" /></c>.</returns>
        /// <exception cref="ArgumentException">The parameter <c><paramref name="stringComparison" /></c> is not a <see cref="StringComparison" /> value.</exception>
        /// <seealso cref="GetHashCode(IEqualityComparer{String?})" />
        /// <seealso cref="GetHashCode()" />
        public virtual Int32 GetHashCode(StringComparison stringComparison)
        {
            try
            {
                return GetHashCode(StringComparer.FromComparison(stringComparison));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(StringComparisonNotSupportedErrorMessage, nameof(stringComparison), ex);
            }
        }
#endif // NETSTANDARD2_0

        /// <summary>Returns the hash code of the current options.</summary>
        /// <returns>The hash code of the current <see cref="ShatteringOptions" />.</returns>
        /// <seealso cref="GetHashCode(IEqualityComparer{String?})" />
        /// <seealso cref="GetHashCode(StringComparison)" />
        public override Int32 GetHashCode() =>
            GetHashCode(EqualityComparer<String?>.Default);

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="other" /></c> options or not.</summary>
        /// <param name="other">The other <see cref="ShatteringOptions" /> to compare with these options.</param>
        /// <param name="stringComparer">The <see cref="IEqualityComparer{T}" /> of <see cref="String" />s used for comparing <see cref="String" />s for equality and for retrieving <see cref="String" />s' hash codes.</param>
        /// <returns>If the current <see cref="ShatteringOptions" /> are equal to the <c><paramref name="other" /></c> according to the <c><paramref name="stringComparer" /></c>, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="stringComparer" /></c> is <c>null</c>.</exception>
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?, StringComparison)" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public virtual Boolean Equals(ShatteringOptions? other, IEqualityComparer<String?> stringComparer) =>
            stringComparer is null ?
                throw new ArgumentNullException(nameof(stringComparer), StringComparerNullErrorMessage) :
                Object.ReferenceEquals(this, other) ||
                    (
                        !(other is null) &&
                        IgnoreEmptyTokens == other.IgnoreEmptyTokens &&
                        IgnoreLineEnds == other.IgnoreLineEnds &&
                        IgnoreEmptyLines == other.IgnoreEmptyLines &&
                        stringComparer.Equals(LineEndToken, other.LineEndToken) &&
                        stringComparer.Equals(EmptyLineToken, other.EmptyLineToken)
                    );

#if !NETSTANDARD2_0
        /// <summary>Indicates whether the current options are equal to the <c><paramref name="other" /></c> options or not.</summary>
        /// <param name="other">The other <see cref="ShatteringOptions" /> to compare with these options.</param>
        /// <param name="stringComparison">One of the enumeration values that specifies how <see cref="String" />s should be compared.</param>
        /// <returns>If the current <see cref="ShatteringOptions" /> are equal to the <c><paramref name="other" /></c> according to the <see cref="StringComparer" /> specified by the <c><paramref name="stringComparison" /></c>, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">The parameter <c><paramref name="stringComparison" /></c> is not a <see cref="StringComparison" /> value.</exception>
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(Object?, StringComparison)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public virtual Boolean Equals(ShatteringOptions? other, StringComparison stringComparison)
        {
            try
            {
                return Equals(other, StringComparer.FromComparison(stringComparison));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(StringComparisonNotSupportedErrorMessage, nameof(stringComparison), ex);
            }
        }
#endif // NETSTANDARD2_0

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="other" /></c> options or not.</summary>
        /// <param name="other">The other <see cref="ShatteringOptions" /> to compare with these options.</param>
        /// <returns>If the current <see cref="ShatteringOptions" /> are equal to the <c><paramref name="other" /></c>, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public virtual Boolean Equals(ShatteringOptions? other) =>
            Equals(other, EqualityComparer<String?>.Default);

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="obj" /></c> or not.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare with these <see cref="ShatteringOptions" />.</param>
        /// <param name="stringComparer">The <see cref="IEqualityComparer{T}" /> of <see cref="String" />s used for comparing <see cref="String" />s for equality and for retrieving <see cref="String" />s' hash codes.</param>
        /// <returns>If the <c><paramref name="obj" /></c> is also <see cref="ShatteringOptions" /> or it may be cast to <see cref="ShatteringOptions" /> and the current shattering options are equal to it according to the <c><paramref name="stringComparer" /></c>, <c>true</c>; <c>false</c>otherwise.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="stringComparer" /></c> is <c>null</c>.</exception>
        /// <seealso cref="Equals(Object?, StringComparison)" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public virtual Boolean Equals(Object? obj, IEqualityComparer<String?> stringComparer)
        {
            try
            {
                return !(obj is null) && Equals((ShatteringOptions)obj, stringComparer);
            }
            catch (InvalidCastException)
            {
            }

            return false;
        }

#if !NETSTANDARD2_0
        /// <summary>Indicates whether the current options are equal to the <c><paramref name="obj" /></c> or not.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare with these <see cref="ShatteringOptions" />.</param>
        /// <param name="stringComparison">One of the enumeration values that specifies how <see cref="String" />s should be compared.</param>
        /// <returns>If the <c><paramref name="obj" /></c> is also <see cref="ShatteringOptions" /> or it may be cast to <see cref="ShatteringOptions" /> and the current shattering options are equal to it according to the <see cref="StringComparer" /> specified by the <c><paramref name="stringComparison" /></c>, <c>true</c>; <c>false</c>otherwise.</returns>
        /// <exception cref="ArgumentException">The parameter <c><paramref name="stringComparison" /></c> is not a <see cref="StringComparison" /> value.</exception>
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public virtual Boolean Equals(Object? obj, StringComparison stringComparison)
        {
            try
            {
                return Equals(obj, StringComparer.FromComparison(stringComparison));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(StringComparisonNotSupportedErrorMessage, nameof(stringComparison), ex);
            }
        }
#endif // NETSTANDARD2_0

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="obj" /></c> or not.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare with these <see cref="ShatteringOptions" />.</param>
        /// <returns>If the <c><paramref name="obj" /></c> is also <see cref="ShatteringOptions" /> or it may be cast to <see cref="ShatteringOptions" /> and the current shattering options are equal to it, <c>true</c>; <c>false</c>otherwise.</returns>
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, StringComparison)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, ShatteringOptions?, StringComparison)" />
        /// <seealso cref="operator ==(ShatteringOptions?, ShatteringOptions?)" />
        /// <seealso cref="operator !=(ShatteringOptions?, ShatteringOptions?)" />
        public override Boolean Equals(Object? obj) =>
            Equals(obj, EqualityComparer<String?>.Default);

        /// <summary>Creates a new <see cref="Object" /> that is a copy of the current options.</summary>
        /// <returns>The new <see cref="ShatteringOptions" /> with the same values.</returns>
        /// <seealso cref="ShatteringOptions(ShatteringOptions)" />
        public virtual Object Clone() =>
            new ShatteringOptions(this);
    }
}

#if NETSTANDARD2_0
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#endif // NETSTANDARD2_0
