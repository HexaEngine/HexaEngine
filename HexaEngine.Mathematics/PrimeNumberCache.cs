namespace HexaEngine.Mathematics
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides functionality to check whether a number is prime and maintains a cache of prime numbers.
    /// </summary>
    public static class PrimeNumberCache
    {
        private static readonly HashSet<long> primes = [];
        private static long lastNumber;

        /// <summary>
        /// Initializes the PrimeNumberCache with a warm-up of prime numbers up to a specified limit.
        /// </summary>
        static PrimeNumberCache()
        {
            Warmup(1000000);
        }

        /// <summary>
        /// Checks if a given number is prime.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>Returns true if the number is prime, otherwise false.</returns>
        public static bool IsPrime(long number)
        {
            if (number < 0)
                return false;

            if (number > lastNumber)
            {
                Warmup(number);
            }

            return primes.Contains(number);
        }

        /// <summary>
        /// Checks if a given number is prime.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>Returns true if the number is prime, otherwise false.</returns>
        public static bool IsPrime(sbyte number)
        {
            return IsPrime((long)number);
        }

        /// <summary>
        /// Checks if a given number is prime.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>Returns true if the number is prime, otherwise false.</returns>
        public static bool IsPrime(byte number)
        {
            return IsPrime((long)number);
        }

        /// <summary>
        /// Checks if a given number is prime.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>Returns true if the number is prime, otherwise false.</returns>
        public static bool IsPrime(short number)
        {
            return IsPrime((long)number);
        }

        /// <summary>
        /// Checks if a given number is prime.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>Returns true if the number is prime, otherwise false.</returns>
        public static bool IsPrime(ushort number)
        {
            return IsPrime((long)number);
        }

        /// <summary>
        /// Checks if a given number is prime.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>Returns true if the number is prime, otherwise false.</returns>
        public static bool IsPrime(int number)
        {
            return IsPrime((long)number);
        }

        /// <summary>
        /// Checks if a given number is prime.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>Returns true if the number is prime, otherwise false.</returns>
        public static bool IsPrime(uint number)
        {
            return IsPrime((long)number);
        }

        /// <summary>
        /// Checks if a given number is prime.
        /// </summary>
        /// <param name="number">The number to check for primality.</param>
        /// <returns>Returns true if the number is prime, otherwise false.</returns>
        public static bool IsPrime(ulong number)
        {
            return IsPrime((long)number);
        }

        /// <summary>
        /// Warms up the cache with prime numbers up to a specified limit.
        /// </summary>
        /// <param name="number">The limit up to which to calculate prime numbers.</param>
        public static void Warmup(long number)
        {
            Dispatch(lastNumber + number);
        }

        /// <summary>
        /// Dispatches the calculation of prime numbers in parallel for a specified range.
        /// </summary>
        private static void Dispatch(long number)
        {
            Parallel.For(lastNumber + 1, number + 1, DispatchVoid);
            lastNumber = number;
        }

        /// <summary>
        /// Calculates and adds prime numbers to the cache within a specified range.
        /// </summary>
        private static void DispatchVoid(long number)
        {
            bool result = IsPrimeCheck(number);
            if (result)
            {
                lock (primes)
                {
                    primes.Add(number);
                }
            }
        }

        /// <summary>
        /// Checks if a given number is prime using a simple algorithm. Doesn't use the cache be warned!
        /// </summary>
        public static bool IsPrimeCheck(long number)
        {
            if (number < 2) return false;
            if (number % 2 == 0) return true;

            long boundary = (long)Math.Sqrt(number);

            for (long i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}