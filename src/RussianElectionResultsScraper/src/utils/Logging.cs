using System;
using System.Collections;
using System.IO;
using log4net;
using log4net.Core;
using log4net.Layout.Pattern;
using log4net.Util;

namespace RussianElectionResultsScraper
{
    static class LoggingExtensions
        {
        public static void Info(this ILog log, string message, Action block)
            {
            log.Info( message );
            if (log4net.ThreadContext.Properties["Level"] == null)
                log4net.ThreadContext.Properties["Level"] = 0;
            log4net.ThreadContext.Properties["Level"] = (int)log4net.ThreadContext.Properties["Level"] + 1;
            log4net.ThreadContext.Properties["Indent"] = new string( ' ', (int)log4net.ThreadContext.Properties["Level"]*4 );
            try {
                block();
                }
            finally 
                {
                log4net.ThreadContext.Properties["Level"] = (int)log4net.ThreadContext.Properties["Level"] - 1;
                }
            }

        public static T Info<T>(this ILog log, string message, Func<T> block)
            {
            log.Info( message );
            if (log4net.ThreadContext.Properties["Level"] == null)
                log4net.ThreadContext.Properties["Level"] = 0;
            log4net.ThreadContext.Properties["Level"] = (int)log4net.ThreadContext.Properties["Level"] + 1;
            log4net.ThreadContext.Properties["Indent"] = new string(' ', (int)log4net.ThreadContext.Properties["Level"] * 4);
            try
            {
                return block();
            }
            finally
            {
                log4net.ThreadContext.Properties["Level"] = (int)log4net.ThreadContext.Properties["Level"] - 1;
            }
            }
        }

    internal sealed class MessagePatternConverter : PatternLayoutConverter
    {
        public const string startMarker = "#$%@#$";
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            var a = new StringWriter();
            a.Write(startMarker);
            loggingEvent.WriteRenderedMessage(a);
            writer.Write(a.ToString());
        }
    }


    public class PatternLayout : log4net.Layout.PatternLayout
    {
        protected override log4net.Util.PatternParser CreatePatternParser(string pattern)
        {
            var parser = base.CreatePatternParser(pattern);
            parser.PatternConverters.Add("indentedmessage", new ConverterInfo { Name = "indentedmessage", Type = typeof(MessagePatternConverter) });
            return parser;
        }
        public override void Format(System.IO.TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
        {
            var a = new StringWriter();
            base.Format(a, loggingEvent);
            string formatted = a.ToString();
            string indented = IndentMessage(formatted);
            writer.Write(indented);
        }

        private string IndentMessage(string formatted)
        {
            var lines = split_lines(formatted);
            string indent = (string)log4net.ThreadContext.Properties["Indent"] ?? "";
            var messageStartPos = lines[0].IndexOf(MessagePatternConverter.startMarker);
            if (lines.Length > 1)
            {
                lines[0] = lines[0].Remove(messageStartPos, MessagePatternConverter.startMarker.Length).Insert(messageStartPos, indent);
                var nextLinesIndent = new string(' ', messageStartPos) + indent;
                for (int i = 1; i < lines.Length; ++i)
                    lines[i] = lines[i].Insert(0, nextLinesIndent);
                return string.Join("\n", lines) + "\n";
            }
            else
                return formatted.Remove(messageStartPos, MessagePatternConverter.startMarker.Length).Insert(messageStartPos, indent);
        }

        public static string[] split_lines(string data)
        {
            // Whole thing has been stolen from Python 2.4.1
            int i;
            int j;
            int len = data.Length;
            int keepends = 0;
            ArrayList list = new ArrayList();

            for (i = j = 0; i < len; )
            {
                int eol;

                /* Find a line and append it */
                while (i < len && data[i] != '\n' && data[i] != '\r')
                    i++;

                /* Skip the line break reading CRLF as one line break */
                eol = i;
                if (i < len)
                {
                    if (data[i] == '\r' && i + 1 < len &&
                        data[i + 1] == '\n')
                        i += 2;
                    else
                        i++;
                    if (keepends != 0)
                        eol = i;
                }
                list.Add(data.Substring(j, eol - j));
                j = i;
            }
            if (j < len)
            {
                list.Add(data.Substring(j, len - j));
            }

            return (string[])list.ToArray(typeof(string));
        }


    }


}