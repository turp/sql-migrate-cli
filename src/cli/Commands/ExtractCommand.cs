using System.ComponentModel;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    [Description("Extracts procedures, functions, views and triggers from given server and database")]
    public class ExtractCommand : Command<ExtractCommand.Settings>
    {
        public sealed class Settings : BaseSettings
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            SettingsDumper.Dump(settings);
            return 0;
        }
    }
}