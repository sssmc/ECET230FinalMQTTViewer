using System;

namespace ChecksumCalculator
{

    /// <summary>
    /// Class <c>ChecksumCalculator</c> is used to calculate the checksum of a string.
    /// </summary>
    public static class ChecksumCalculator
    {
        /// <summary>
        /// Calculates the checksum of a packet.
        /// </summary>
        /// <param name="packet">The data to calculate the checksum for</param>
        public static int CalculateChecksum(string packet)
        {
            int checksum = 0;

            //Creates a char array of the packet
            char[] chars = new char[packet.Length];
            chars = packet.ToCharArray();

            //Adds the ASCII value of each char to the checksum
            foreach (char c in chars)
            {
                checksum += (int)c;
            }

            //Modulo 10000 to get the last 4 digits
            checksum = checksum % 10000;

            return checksum;
        }
    }
}
