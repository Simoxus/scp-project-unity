using Cysharp.Threading.Tasks;
using System;

namespace Console.Commands
{
    public class ModsCommand : BaseConsole
    {
        public override string CommandWord => "mods";
        public override string Description => "Manage Lua mods.";
        protected override string RawUsage => "mods <list|reload> [id]";

        public override void Execute(string[] args)
        {
            if (args.Length == 0 || args[0].ToLower() == "list")
            {
                ListMods();
                return;
            }

            // "reload" command
            if (args[0].ToLower() == "reload")
            {
                if (args.Length == 1)
                {
                    ReloadAllMods();
                }
                else
                {
                    string modId = args[1];
                    ReloadMod(modId);
                }
                return;
            }

            ConsoleManager.LogToConsole(Usage.AsError());
        }

        private void ListMods()
        {
            var mods = Core.ModManager.GetAllModInfo();

            if (mods.Count == 0)
            {
                ConsoleManager.LogToConsole("No mods loaded.".AsError());
                return;
            }

            ConsoleManager.LogToConsole($"--- Loaded Mods ({mods.Count} mods) ---".AsHeader());

            foreach (var mod in mods)
            {
                var modInstance = Core.ModManager.GetMod(mod.id);
                string status = modInstance != null && modInstance.IsEnabled ? "Enabled".AsSuccess() : "Disabled".AsError();

                ConsoleManager.LogToConsole($"<u><b>[{mod.id}] {mod.name} v{mod.version}</b></u> - <b>{status}</b>".AsInfo());
                ConsoleManager.LogToConsole($"  <b>Author</b>: {mod.author}");

                if (!string.IsNullOrEmpty(mod.description))
                {
                    ConsoleManager.LogToConsole($"  <b>Description</b>: {mod.description}");
                }
            }
        }

        private void ReloadMod(string modId)
        {
            var mod = Core.ModManager.GetMod(modId);
            if (mod == null)
            {
                ConsoleManager.LogToConsole($"Mod '{modId}' not found.".AsError());
                return;
            }

            ReloadModAsync(modId).Forget();
        }

        private async UniTaskVoid ReloadModAsync(string modId)
        {
            try
            {
                ConsoleManager.LogToConsole($"Reloading mod '{modId}'...".AsInfo());
                await Core.ModManager.ReloadMod(modId);
                ConsoleManager.LogToConsole($"Mod '{modId}' reloaded successfully.".AsSuccess());
            }
            catch (Exception ex)
            {
                Log.Exception(ex, message: ex.Message);
                ConsoleManager.LogToConsole($"Failed to reload mod '{modId}': {ex.Message}".AsError());
            }
        }

        private void ReloadAllMods()
        {
            ReloadAllModsAsync().Forget();
        }

        private async UniTaskVoid ReloadAllModsAsync()
        {
            try
            {
                ConsoleManager.LogToConsole("Reloading all mods...".AsInfo());
                await Core.ModManager.ReloadAllMods();
                ConsoleManager.LogToConsole("All mods reloaded successfully.".AsSuccess());
            }
            catch (Exception ex)
            {
                Log.Exception(ex, message: ex.Message);
                ConsoleManager.LogToConsole($"Failed to reload mods: {ex.Message}".AsError());
            }
        }
    }
}