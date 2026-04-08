using System;

namespace Facility.Generation
{
    public static class FG_SeedUtility
    {
        public static int ConvertToNumericSeed(string seedString)
        {
            if (string.IsNullOrEmpty(seedString))
            {
                Log.Warning("Empty seed string provided, using default seed");
                return 0;
            }

            char[] seedChars = seedString.ToCharArray();
            int tmp = 0;
            int shift = 0;

            foreach (char c in seedChars)
            {
                tmp = tmp ^ (c << shift);
                shift = (shift + 1) % 24;
            }

            return tmp;
        }

        public static int ConvertToNumericSeedFlexible(string seedString)
        {
            if (string.IsNullOrEmpty(seedString))
            {
                Log.VerboseWarning("Empty seed string provided, using default seed 0");
                return 0;
            }

            if (int.TryParse(seedString, out int directSeed))
            {
                return directSeed;
            }

            return ConvertToNumericSeed(seedString);
        }

        public static int ConvertToNumericSeedHash(string seedString)
        {
            if (string.IsNullOrEmpty(seedString))
            {
                Log.Warning("Empty seed string provided, using default seed 0");
                return 0;
            }

            return Math.Abs(seedString.GetHashCode());
        }

        public static bool IsValidSeedString(string seedString)
        {
            return !string.IsNullOrEmpty(seedString);
        }

        public static string GenerateRandomSeedString(int length = 9)
        {
            if (length <= 0)
            {
                Log.Warning("Invalid seed length, using default length of 9");
                length = 9;
            }

            System.Random random = new System.Random(Environment.TickCount);
            char[] seedChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                seedChars[i] = (char)random.Next(48, 58);
            }

            return new string(seedChars);
        }

        public static int GenerateRandomNumericSeed() => Environment.TickCount;
    }
}