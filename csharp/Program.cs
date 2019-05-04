using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
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

                WriteLine($"Starting execution - source: {source} | dest: {dest} | operation: {operation}");

                ProcessFiles(source, dest, operation == MOVE);
            });

            return app.Execute(args);
        }

        private static bool ProcessFiles(string sourcePath, string destPath, bool moveFiles = false)
        {
            var files = Directory.GetFiles(sourcePath);
            int fileCounter = 0;

            foreach (string filename in files)
            {
                // don't copy system files like .DS_Store
                if (Path.GetFileName(filename).StartsWith('.'))
                {
                    continue;
                }
                
                var fileCreationTime = Directory.GetCreationTime(filename);

                fileCounter++;

                WriteLine($"{fileCounter}: Processing {Path.GetFileName(filename)} created on {fileCreationTime}");

                var destFolder = GetDestFolder(destPath, fileCreationTime);

                Directory.CreateDirectory(destFolder);

                var destFilename = Path.Combine(destFolder, Path.GetFileName(filename));

                if (moveFiles)
                {
                    File.Move(filename, Path.Combine(destFolder, destFilename));
                }
                else
                {
                    File.Copy(filename, Path.Combine(destFolder, destFilename));
                }

                // may or may not be necessary depending on the file system and OS
                // but it doesn't hurt
                File.SetCreationTime(destFilename, fileCreationTime);
            }
            return true;
        }

        private static string GetDestFolder(string destPath, DateTime fileCreationTime)
        {
            return Path.Combine(destPath,
                fileCreationTime.Year.ToString("0000"),
                fileCreationTime.Month.ToString("00"));
        }
    }

    class MustBeMoveOrCopyValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
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