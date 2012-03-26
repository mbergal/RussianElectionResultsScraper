namespace RussianElectionResultsScraper.Commons
    {
    public static class StringEx
        {
        public static string AppendIfNotThere( this string str, string ending )
            {
            return str.EndsWith( ending )
                       ? str
                       : str + ending;
            }
        }
    }
