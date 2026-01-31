using Facility.Generation;

namespace Console.Commands
{
    public class FacilityCommand : BaseConsole
    {
        public override string CommandWord => "facility";
        public override string Description => "Controls facility generation.";
        public override string[] Aliases => new string[] { "fac", "facgen" };
        protected override string RawUsage => "facgen <generate|clear|genrandom|seed[random]|save|load>";

        private FacilityGenerator _generator => Core.FacilityGenerator;

        public override void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            if (_generator == null)
            {
                return;
            }

            string subcommand = args[0].ToLower();

            switch (subcommand)
            {
                case "generate":
                case "gen":
                case "regen":
                    ExecuteGenerate();
                    break;

                case "clear":
                    ExecuteClear();
                    break;

                case "randomgen":
                case "genrandom":
                    ExecuteGenerateRandom();
                    break;

                case "seed":
                    ExecuteSeed(args);
                    break;

                case "save":
                    ExecuteSave();
                    break;

                case "load":
                    ExecuteLoad();
                    break;

                default:
                    ConsoleManager.LogToConsole(Usage.AsError());
                    break;
            }
        }

        private void ExecuteGenerate()
        {
            ConsoleManager.LogToConsole($"Generating facility with seed '{_generator.CurrentSeed}'...".AsWarning());
            _generator.GenerateFacility();
            ConsoleManager.LogToConsole("Generated facility! (probably)".AsSuccess());
        }

        private void ExecuteClear()
        {
            _generator.ClearFacility();
            ConsoleManager.LogToConsole("Facility cleared successfully.".AsSuccess());
        }

        private void ExecuteGenerateRandom()
        {
            int randomSeed = FG_SeedUtility.GenerateRandomNumericSeed();
            _generator.SetNumericSeed(randomSeed);
            ExecuteGenerate();
        }

        private void ExecuteSeed(string[] args)
        {
            if (args.Length < 2)
            {
                ConsoleManager.LogToConsole($"Numeric seed: {_generator.CurrentSeed}".AsInfo());
                ConsoleManager.LogToConsole($"String seed: '{_generator.CurrentSeedString}'".AsInfo());
                return;
            }

            string seedArg = args[1].ToLower();

            if (seedArg == "random" || seedArg == "rand")
            {
                int randomSeed = FG_SeedUtility.GenerateRandomNumericSeed();
                _generator.SetNumericSeed(randomSeed);
                ConsoleManager.LogToConsole($"Random seed set to '{randomSeed}'".AsSuccess());
                return;
            }

            // Try parsing as numeric seed
            if (int.TryParse(seedArg, out int numericSeed))
            {
                _generator.SetNumericSeed(numericSeed);
                ConsoleManager.LogToConsole($"Numeric seed set to '{numericSeed}'".AsSuccess());
            }
            else
            {
                // Use as string seed
                _generator.SetSeedFromString(seedArg);
                ConsoleManager.LogToConsole($"String seed set to '{seedArg}' (numeric: {_generator.CurrentSeed})".AsSuccess());
            }
        }

        private void ExecuteSave()
        {
            if (!_generator.IsGenerated)
            {
                ConsoleManager.LogToConsole("No facility to save.".AsError());
                return;
            }

            ConsoleManager.LogToConsole("Saving facility...".AsInfo());
            _generator.QuickSave();
            ConsoleManager.LogToConsole("Saved facility! (probably)".AsSuccess());
        }

        private void ExecuteLoad()
        {
            ConsoleManager.LogToConsole("Loading facility...".AsInfo());
            _generator.QuickLoad();
            ConsoleManager.LogToConsole("Loaded facility! (probably)".AsSuccess());
        }
    }
}