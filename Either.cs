using System;

namespace Shapez2Multiplayer
{
    public readonly struct Either<TLeft, TRight>
    {
        private readonly TLeft _left;
        private readonly TRight _right;
        private readonly bool _isRight;

        private Either(TLeft left)
        {
            _left = left;
            _right = default!;
            _isRight = false;
        }

        private Either(TRight right)
        {
            _left = default!;
            _right = right;
            _isRight = true;
        }

        public static implicit operator Either<TLeft, TRight>(TLeft left) => new Either<TLeft, TRight>(left);
        public static implicit operator Either<TLeft, TRight>(TRight right) => new Either<TLeft, TRight>(right);

        public TResult Match<TResult>(Func<TLeft, TResult> leftFunc, Func<TRight, TResult> rightFunc)
        {
            return _isRight ? rightFunc(_right) : leftFunc(_left);
        }
    }
}
