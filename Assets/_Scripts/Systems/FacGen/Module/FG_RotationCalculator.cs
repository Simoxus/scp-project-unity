namespace Facility.Generation
{
    public static class FG_RotationCalculator
    {
        public static int CalculateRotation(GridCell cell, RoomData roomData)
        {
            if (cell == null || roomData == null) return 0;

            bool[] defaultExits = roomData.GetDefaultExitPattern();
            bool[] requiredExits = cell.exits;

            for (int rotation = 0; rotation < 4; rotation++)
            {
                if (ExitsMatchAfterRotation(defaultExits, requiredExits, rotation))
                {
                    return rotation;
                }
            }

            int bestRotation = FindBestPartialMatch(defaultExits, requiredExits);
            return bestRotation;
        }

        private static bool ExitsMatchAfterRotation(bool[] defaultExits, bool[] requiredExits, int rotation)
        {
            for (int i = 0; i < 4; i++)
            {
                // Which direction the exit maps to after rotation
                int rotatedIndex = (i + rotation) % 4;

                // If rotated exit matches what's required
                if (defaultExits[i] != requiredExits[rotatedIndex])
                {
                    return false;
                }
            }
            return true;
        }

        private static int FindBestPartialMatch(bool[] defaultExits, bool[] requiredExits)
        {
            int bestRotation = 0;
            int bestMatchCount = 0;

            for (int rotation = 0; rotation < 4; rotation++)
            {
                int matchCount = 0;

                for (int i = 0; i < 4; i++)
                {
                    int rotatedIndex = (i + rotation) % 4;
                    if (defaultExits[i] == requiredExits[rotatedIndex])
                    {
                        matchCount++;
                    }
                }

                if (matchCount > bestMatchCount)
                {
                    bestMatchCount = matchCount;
                    bestRotation = rotation;
                }
            }

            return bestRotation;
        }

        public static float RotationToDegrees(int rotation)
        {
            return (rotation % 4) * 90f;
        }
    }
}