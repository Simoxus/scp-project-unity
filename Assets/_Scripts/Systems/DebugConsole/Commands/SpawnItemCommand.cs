using UnityEngine;

namespace Console.Commands
{
    public class SpawnItemCommand : BaseConsole
    {
        public override string CommandWord => "spawnitem";
        public override string Description => "Spawns an item near the player using name or ID.";
        protected override string RawUsage => "spawnitem <itemID|itemName> <quantity>";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            if (Core.FacilityManager == null)
            {
                return;
            }

            // Parse quantity
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

            if (itemData.worldPrefab == null)
            {
                ConsoleManager.LogToConsole($"Item '{itemData.itemID}' has no prefab.".AsError());
                return;
            }

            SpawnItems(itemData, quantity);
            string itemName = string.IsNullOrEmpty(itemData.GetItemName()) ? itemData.itemID : itemData.GetItemName();
            ConsoleManager.LogToConsole($"Spawned {quantity}x {itemName}".AsSuccess());
        }

        private void SpawnItems(ItemData itemData, int quantity)
        {
            if (Core.Player?.Controller == null) return;

            Transform playerTransform = Core.Player.Controller.transform;
            Vector3 spawnPosition = playerTransform.position + playerTransform.forward * 2f;

            for (int i = 0; i < quantity; i++)
            {
                Vector3 finalPosition = spawnPosition;

                if (quantity > 1)
                {
                    float angle = (360f / quantity) * i * Mathf.Deg2Rad;
                    float radius = 0.5f + (quantity * 0.1f);
                    finalPosition += new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                }

                if (Physics.Raycast(finalPosition + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
                {
                    finalPosition = hit.point + Vector3.up * 0.1f;
                }

                GameObject spawnedItem = Object.Instantiate(itemData.worldPrefab, finalPosition, Quaternion.identity);
                spawnedItem.name = $"{itemData.localizedName}_{i}";
            }
        }
    }
}