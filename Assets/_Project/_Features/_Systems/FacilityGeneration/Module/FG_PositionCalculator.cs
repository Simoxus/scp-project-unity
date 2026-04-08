using UnityEngine;

namespace Facility.Generation
{
    public static class FG_PositionCalculator
    {
        public static Vector3 CalculatePosition(Vector3 basePosition, RoomData roomData, int rotation, float cellSize)
        {
            Vector3 finalPosition = basePosition;

            // Apply custom offset if present
            if (roomData.HasCustomOffset)
            {
                Vector3 rotatedOffset = RotateOffset(roomData.RoomOffset, rotation);
                finalPosition += rotatedOffset;
            }

            return finalPosition;
        }

        public static float CalculateRotation(int baseRotation, RoomData roomData)
        {
            float baseAngle = baseRotation * 90f;

            if (!roomData.HasCustomOffset)
            {
                return baseAngle;
            }

            float finalAngle = baseAngle + roomData.RotationOffset;
            return finalAngle;
        }

        private static Vector3 RotateOffset(Vector3 offset, int rotation)
        {
            float angle = rotation * 90f;
            Quaternion rotationQuat = Quaternion.Euler(0, angle, 0);
            return rotationQuat * offset;
        }

        public static bool ValidateOffset(Vector3 basePosition, Vector3 offset, float cellSize, int gridWidth, int gridHeight)
        {
            Vector3 finalPos = basePosition + offset;
            float maxX = gridWidth * cellSize;
            float maxZ = gridHeight * cellSize;

            if (finalPos.x < 0 || finalPos.x > maxX || finalPos.z < 0 || finalPos.z > maxZ)
            {
                return false;
            }

            return true;
        }
    }
}