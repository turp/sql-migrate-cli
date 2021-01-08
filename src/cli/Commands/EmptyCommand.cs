using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    [Description("Empty Command")]
    public class EmptyCommand : Command<EmptyCommand.Settings>
    {
        public class Settings : BaseSettings
        {
        }

        public override ValidationResult Validate(CommandContext context, Settings settings)
        {
            return base.Validate(context, settings);
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            return 0;
        }
    }
}