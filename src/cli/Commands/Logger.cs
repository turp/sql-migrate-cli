using System;
using System.Runtime.CompilerServices;
using Spectre.Console;

namespace Db.Deploy.Cli.Commands
{
    public class Logger
    {
        public static void Information(string message)
        {
            AnsiConsole.MarkupLine(Clean(message));
        }

        private static string Clean(string message)
        {
            return message
                .Replace("[", "[[")
                .Replace("]", "]]");
        }

        public static void Error(string message)
        {
            AnsiConsole.MarkupLine($"[red]{Clean(message)}[/]");
        }

        public static void Warning(string message)
        {
            AnsiConsole.MarkupLine($"[yellow]{Clean(message)}[/]");
        }
    }
}