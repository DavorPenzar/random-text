using System;

namespace MagicText.Internal
{
    /// <summary>Exposes the negation of a simple (one argument) predicate.</summary>
    /// <typeparam name="T">The type of the argument of the predicate.</typeparam>
    internal class NegativePredicateWrapper<T> : Object
    {
        private const string PositivePredicateNullErrorMessage = "Positive predicate cannot be null.";

        private readonly Func<T, Boolean> _positivePredicate;

        /// <summary>Gets the predicate that is negated through the <see cref="EvaluateNegation(T)" /> method.</summary>
        /// <returns>The internal wrapped predicate.</returns>
        public Func<T, Boolean> PositivePredicate => _positivePredicate;

        /// <summary>Creates a negative wrapper of the <c><paramref name="positivePredicate" /></c>.</summary>
        /// <param name="positivePredicate">The predicate that is negated through the <see cref="EvaluateNegation(T)" /> method.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="positivePredicate" /></c> is <c>null</c>.</exception>
        public NegativePredicateWrapper(Func<T, Boolean> positivePredicate) : base()
        {
            _positivePredicate = positivePredicate ?? throw new ArgumentNullException(nameof(positivePredicate), PositivePredicateNullErrorMessage);
        }

        /// <summary>Negates the evaluation of the <c><paramref name="arg" /></c> via the <see cref="PositivePredicate" /> delegate.</summary>
        /// <param name="arg">The parameter to evaluate.</param>
        /// <returns>The <see cref="Boolean" /> negation (<c>true</c> to <c>false</c> and vice versa) of the evaluation of the <c><paramref name="arg" /></c> via the <see cref="PositivePredicate" /> delegate. Simply put, the method returns <c>!<see cref="PositivePredicate" />(<paramref name="arg" />)</c>.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="PositivePredicate" /> delegate call are not caught.</para>
        /// </remarks>
        public Boolean EvaluateNegation(T arg) =>
            !PositivePredicate(arg);
    }
}
