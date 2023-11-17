using System;

namespace ChecksumCalculator
{
    public static class ChecksumCalculator
    {
        public static int CalculateChecksum(string packet)
        {
            int checksum = 0;

            char[] chars = new char[packet.Length];

            chars = packet.ToCharArray();

            foreach (char c in chars)
            {
                checksum += (int)c;
            }

            checksum = checksum % 10000;

            return checksum;
        }
    }
}
