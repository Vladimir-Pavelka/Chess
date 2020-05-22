namespace ChessBoard
{
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {
        public static IEnumerable<TRes> Select<TSrc, TRes>(this TSrc[,] source, Func<TSrc, (int, int), TRes> selector)
        {
            for (var i = 0; i < source.GetLength(0); i++)
                for (var j = 0; j < source.GetLength(1); j++)
                    yield return selector(source[i, j], (i, j));
        }
    }
}
