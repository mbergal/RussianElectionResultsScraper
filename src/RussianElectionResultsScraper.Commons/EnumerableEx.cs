namespace RussianElectionResultsScraper.Commons
    {
    using System;
    using System.Collections.Generic;

    public static class EnumerableEx
        {
        public static void ForEach<T>( this IEnumerable<T> seq, Action<T> block )
            {
            foreach ( var e in seq )
                block( e );
            }
        }
    }
