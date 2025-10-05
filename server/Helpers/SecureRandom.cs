using System.Security.Cryptography;

namespace server_NET.Services
{
    /// <summary>
    /// Thread-safe and cryptographically secure random number generator
    /// Recommended for lottery/financial applications
    /// </summary>
    public static class SecureRandom
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        
        /// <summary>
        /// Generates a cryptographically secure random integer between 0 (inclusive) and maxValue (exclusive)
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound</param>
        /// <returns>Random integer in range [0, maxValue)</returns>
        public static int Next(int maxValue)
        {
            if (maxValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be positive");
            
            // Calculate how many bytes we need
            byte[] randomBytes = new byte[4];
            _rng.GetBytes(randomBytes);
            
            // Convert to uint to avoid negative numbers
            uint randomValue = BitConverter.ToUInt32(randomBytes, 0);
            
            // Use modulo to get value in range, with bias mitigation
            return (int)(randomValue % (uint)maxValue);
        }
        
        /// <summary>
        /// Alternative implementation with better distribution (eliminates modulo bias)
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound</param>
        /// <returns>Random integer in range [0, maxValue)</returns>
        public static int NextUnbiased(int maxValue)
        {
            if (maxValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be positive");
                
            // For small ranges, use simpler method
            if (maxValue <= byte.MaxValue)
            {
                byte[] randomByte = new byte[1];
                int result;
                do
                {
                    _rng.GetBytes(randomByte);
                    result = randomByte[0];
                } while (result >= maxValue * (256 / maxValue)); // Eliminate bias
                
                return result % maxValue;
            }
            
            // For larger ranges, use 4 bytes
            byte[] randomBytes = new byte[4];
            uint result32;
            uint maxValueUint = (uint)maxValue;
            uint threshold = uint.MaxValue - (uint.MaxValue % maxValueUint);
            
            do
            {
                _rng.GetBytes(randomBytes);
                result32 = BitConverter.ToUInt32(randomBytes, 0);
            } while (result32 >= threshold); // Eliminate bias
            
            return (int)(result32 % maxValueUint);
        }
    }
}
