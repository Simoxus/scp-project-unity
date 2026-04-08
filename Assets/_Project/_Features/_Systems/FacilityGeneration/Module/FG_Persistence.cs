using Cysharp.Threading.Tasks;
using Facility.Persistence.Types;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facility.Generation
{
    public class FG_Persistence
    {
        private readonly FacilityGeneratorSettings _settings;

        public FG_Persistence(FacilityGeneratorSettings settings)
        {
            _settings = settings;
        }

        public FacilityPersistData SaveFacilityToData(
            GridCell[,] grid,
            List<GridCell> occupiedCells,
            GridCell startCell,
            int seed,
            string seedString)
        {
            return SerializeFacility(grid, occupiedCells, startCell, seed, seedString);
        }

        public (GridCell[,] grid, List<GridCell> occupiedCells, GridCell startCell) DeserializeFacility(
            FacilityPersistData persistData)
        {
            if (persistData == null)
            {
                Log.Error("Cannot deserialize null persist data");
                return (null, null, null);
            }

            var grid = new GridCell[persistData.gridWidth, persistData.gridHeight];
            var occupiedCells = new List<GridCell>();
            GridCell startCell = null;

            foreach (var cellData in persistData.cells)
            {
                GridCell cell = new GridCell(cellData.position);
                cellData.ApplyToCell(cell, _settings);
                grid[cell.position.x, cell.position.y] = cell;
                occupiedCells.Add(cell);

                if (cell.position == persistData.startCellPosition)
                {
                    startCell = cell;
                }
            }

            if (startCell == null)
            {
                Log.Error("Start room cell not found in persist data");
            }

            Log.Info($"Deserialized facility: {occupiedCells.Count} cells restored");
            return (grid, occupiedCells, startCell);
        }

        public NavLinksPersistData SaveNavLinksToData(List<NavLinkData> navLinkData)
        {
            if (navLinkData == null || navLinkData.Count == 0)
            {
                return null;
            }

            var navLinksData = new NavLinksPersistData();
            navLinksData.links.AddRange(navLinkData);

            Log.Info($"Saved {navLinkData.Count} navigation links to data");
            return navLinksData;
        }

        public DoorStatesPersistData SaveDoorStatesToData(List<GameObject> doorInstances)
        {
            if (doorInstances == null || doorInstances.Count == 0)
            {
                return null;
            }

            var doorStatesData = new DoorStatesPersistData();

            foreach (var doorObj in doorInstances)
            {
                if (doorObj == null) continue;

                var roomDoor = doorObj.GetComponent<RoomDoor>();
                if (roomDoor != null)
                {
                    var stateData = roomDoor.GetDoorStateData();
                    if (stateData != null)
                    {
                        doorStatesData.doorStates.Add(stateData);
                    }
                }
            }

            Log.Info($"Saved {doorStatesData.doorStates.Count} door states");
            return doorStatesData;
        }

        public void LoadDoorStates(DoorStatesPersistData doorStatesData, List<GameObject> doorInstances)
        {
            if (doorStatesData == null || doorStatesData.doorStates == null || doorStatesData.doorStates.Count == 0)
            {
                Log.VerboseInfo("No door states to load");
                return;
            }

            if (doorInstances == null || doorInstances.Count == 0)
            {
                Log.Warning("No door instances found to apply states to");
                return;
            }

            // Create a lookup dictionary for faster access
            var doorStatesByID = new Dictionary<string, DoorStateData>();
            foreach (var stateData in doorStatesData.doorStates)
            {
                if (!string.IsNullOrEmpty(stateData.doorID))
                {
                    doorStatesByID[stateData.doorID] = stateData;
                }
            }

            int loadedCount = 0;
            foreach (var doorObj in doorInstances)
            {
                if (doorObj == null) continue;

                var roomDoor = doorObj.GetComponent<RoomDoor>();
                if (roomDoor != null && doorStatesByID.TryGetValue(roomDoor.DoorID, out var stateData))
                {
                    roomDoor.LoadDoorState(stateData);
                    loadedCount++;
                }
            }

            Log.Info($"Loaded {loadedCount} door states from save data");
        }

        public async UniTask<(FacilityPersistData facilityData, NavLinksPersistData navLinksData, DoorStatesPersistData doorStatesData)> TryLoadFromExistingSeed(
            string seedString)
        {
            string siteName = seedString;

            if (!Core.ProgressManager.SiteExists(siteName))
            {
                return (null, null, null);
            }

            try
            {
                string sitePath = System.IO.Path.Combine(
                    Core.ProgressManager.GetSavesFolderPath(),
                    Core.ProgressManager.SanitizeFolderName(siteName)
                );

                string facilityPath = System.IO.Path.Combine(sitePath, "facility.json");
                if (!System.IO.File.Exists(facilityPath))
                {
                    Log.Info($"Site '{siteName}' exists but has no facility data, generating new facility");
                    return (null, null, null);
                }

                Log.Info($"Found existing facility data for seed '{seedString}', loading from site root...");

                var facilityData = await Core.ProgressManager.LoadDataFromPath<FacilityPersistData>(sitePath);
                var navLinksData = await Core.ProgressManager.LoadDataFromPath<NavLinksPersistData>(sitePath);
                var doorStatesData = await Core.ProgressManager.LoadDataFromPath<DoorStatesPersistData>(sitePath);

                return (facilityData, navLinksData, doorStatesData);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, message: ex.Message);
                Log.Warning($"Failed to load from existing seed");
                return (null, null, null);
            }
        }

        private FacilityPersistData SerializeFacility(
            GridCell[,] grid,
            List<GridCell> occupiedCells,
            GridCell startCell,
            int seed,
            string seedString)
        {
            if (grid == null || occupiedCells == null || startCell == null)
            {
                Log.Error("Cannot serialize null facility data");
                return null;
            }

            var persistData = new FacilityPersistData
            {
                seed = seed,
                seedString = seedString,
                gridWidth = _settings.GridWidth,
                gridHeight = _settings.GridHeight,
                startCellPosition = startCell.position
            };

            foreach (var cell in occupiedCells)
            {
                if (cell != null)
                {
                    persistData.cells.Add(new GridCellData(cell));
                }
            }

            Log.Info($"Serialized facility: {persistData.cells.Count} cells, seed {seed}");
            return persistData;
        }
    }
}