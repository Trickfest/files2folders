using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using static System.Console;

namespace files2folders
{
    class Program
    {
        public const string MOVE = "move";
        public const string COPY = "copy";

        static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption("-h|--help");

            var optionSource = app.Option("-s|--source <source-directory>", "The directory containing the source files. Defaults to current directory", CommandOptionType.SingleValue);

            var optionDest = app.Option("-d|--dest <destination-directory>", "The directory that will receive the folders of files. Defaults to current directory.", CommandOptionType.SingleValue);

            var optionOperation = app.Option("-o|--operation <move-or-copy>", "Move or copy the files.  Defaults to copy.", CommandOptionType.SingleValue);
            optionOperation.Validators.Add(new MustBeMoveOrCopyValidator());

            app.OnExecute(() =>
            {
                var source = optionSource.HasValue() ?
                    optionSource.Value().Trim() :
                    ".";

                var dest = optionDest.HasValue() ?
                    optionDest.Value().Trim() :
                    ".";

                var operation = optionOperation.HasValue() ?
                    optionOperation.Value().Trim() :
                    COPY;

                WriteLine($"Args: {source} {dest} {operation}");
            });

            return app.Execute(args);
        }
    }

    class MustBeMoveOrCopyValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            // This validator only runs if there is a value
            if (!option.HasValue()) return ValidationResult.Success;

            var val = option.Value().Trim();

            if (val != Program.MOVE && val != Program.COPY)
            {
                return new ValidationResult($"The value for --{option.LongName} must be {Program.MOVE} or {Program.COPY}.");
            }

            return ValidationResult.Success;
        }
    }
}