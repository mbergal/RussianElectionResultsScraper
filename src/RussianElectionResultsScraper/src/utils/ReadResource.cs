using System.IO;
using System.Reflection;

namespace RussianElectionResultsScraper
    {
    public static class AssemblyEx
        {
        public static string LoadTextResource( this Assembly assembly, string resourceName )
            {
            return new StreamReader( Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName) ).ReadToEnd();
            }
        }
    }
