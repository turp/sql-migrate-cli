using Spectre.Console;
using Spectre.Console.Cli;

namespace Db.Deploy.Cli.Commands
{
    public class EmptyCommand : Command<EmptyCommand.Settings>
    {
        public class Settings : CommandSettings
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