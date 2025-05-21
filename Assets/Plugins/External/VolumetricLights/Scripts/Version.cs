namespace VLB
{
    public static class Version
    {
        public const int Current = 20203;

        public static string CurrentAsString => GetVersionAsString(Current);

        static string GetVersionAsString(int version)
        {
            const int n1Mult = 10000;
            const int n2Mult = 100;
            const int n3Mult = 1;

            int n1 = (int)(version / n1Mult);
            int n2 = (int)((version - n1 * n1Mult) / n2Mult);
            int n3 = (int)((version - n1 * n1Mult - n2 * n2Mult) / n3Mult);

            return string.Format("{0}.{1}.{2}", n1, n2, n3);
        }
    }
}
