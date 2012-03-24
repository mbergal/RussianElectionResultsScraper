using System.Diagnostics.CodeAnalysis;

namespace RussianElectionResultsScraper.Model
    {
    [SuppressMessage( "StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter",
        Justification = "Reviewed. Suppression is OK here." )] 
    public class Counters
        {
        public const string _1 = "1";
        public const string _2 = "2";
        public const string _3 = "3";
        public const string _4 = "4";
        public const string _5 = "5";
        public const string _6 = "6";
        public const string а = "а";
        public const string б = "б";

        public const string в = "в";
        public const string г = "г";
        public const string д = "д";
        public const string е = "е";
        public const string ж = "ж";
        public const string з = "з";
        public const string И = "и";
        public const string К = "к";
        public const string Л = "л";
        public const string М = "м";

        public static readonly string[] Hierarchical = new[] {а, б, в, г, д, е, ж, з, И, К, Л, М};
        }
    }