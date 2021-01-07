using System;
using Spectre.Console;

namespace Db.Deploy.Cli.Commands
{
    public class Logger
    {
        public static void Information(string message)
        {
            var t = $"{message.Replace("[", "[[").Replace("]", "]]")}";
            AnsiConsole.MarkupLine(t);
        }

        public static void Error(string message)
        {
            var t = $"[red]{message.Replace("[", "[[").Replace("]", "]]")}[/]";
            AnsiConsole.MarkupLine(t);
        }
    }
}