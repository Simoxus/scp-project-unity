using UnityEngine;

namespace Console.Commands
{
    public class GiveItemCommand : BaseConsole
    {
        public override string CommandWord => "giveitem";
        public override string Description => "Gives an item to the player's inventory using name or ID.";
        protected override string RawUsage => "giveitem <itemID|itemName> [quantity]";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            if (Core.FacilityManager == null || Core.Player.Inventory == null) return;

            int quantity = 1;
            if (args.Length >= 2 && int.TryParse(args[args.Length - 1], out int parsed))
            {
                quantity = Mathf.Clamp(parsed, 1, 100);
            }

            int nameArgCount = (args.Length >= 2 && int.TryParse(args[args.Length - 1], out _)) ? args.Length - 1 : args.Length;
            string searchTerm = string.Join(" ", args, 0, nameArgCount);

            ItemData itemData = Core.FacilityManager.FindItem(searchTerm);
            if (itemData == null)
            {
                ConsoleManager.LogToConsole($"Item '{searchTerm}' not found.".AsError());
                return;
            }

            int added = 0;
            for (int i = 0; i < quantity; i++)
            {
                if (Core.Player.Inventory.AddItem(itemData))
                    added++;
                else
                    break;
            }

            if (added > 0)
            {
                string itemName = string.IsNullOrEmpty(itemData.GetItemName()) ? itemData.itemID : itemData.GetItemName();
                if (added == quantity)
                    ConsoleManager.LogToConsole($"Gave {added}x {itemName}".AsSuccess());
                else
                    ConsoleManager.LogToConsole($"Gave {added}x {itemName} (inventory full)".AsWarning());
            }
            else
            {
                ConsoleManager.LogToConsole("Inventory is full.".AsError());
            }
        }
    }
}