using System;
using System.Linq;
using System.Threading.Tasks;
//using log4net;

namespace Utils
{
    public static class RetryHelper
    {
        //I am using log4net but swap in your favourite
        //private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task RetryOnExceptionAsync(
            int times, Func<Task> operation)
        {
            await RetryOnExceptionAsync<Exception>(times, operation);
        }

        public static async Task RetryOnExceptionAsync<TException>(
            int times, Func<Task> operation) where TException : Exception
        {
            if (times <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(times));
            }


            var attempts = 0;
            do
            {
                try
                {
                    attempts++;
                    await operation();
                    break;
                }
                catch (TException ex)
                {
                    if (attempts == times)
                    {
                        throw;
                    };

                    await CreateDelayForException(times, attempts, ex);
                }
            } while (true);
        }

        private static Task CreateDelayForException(int times, int attempts, Exception ex)
        {
            var delay = IncreasingDelayInSeconds(attempts);
            Console.WriteLine($"Exception on attempt {attempts} of {times}. Will retry after sleeping for {delay}.");
            Console.WriteLine(ex.Message);

            return Task.Delay(delay);
        }

        internal static int[] DelayPerAttemptInSeconds =
        {
            (int) TimeSpan.FromSeconds(2).TotalMilliseconds,
            (int) TimeSpan.FromSeconds(30).TotalMilliseconds,
            (int) TimeSpan.FromMinutes(2).TotalMilliseconds,
            (int) TimeSpan.FromMinutes(10).TotalMilliseconds,
            (int) TimeSpan.FromMinutes(30).TotalMilliseconds
        };

        static int IncreasingDelayInSeconds(int failedAttempts)
        {
            return failedAttempts <= 0
                ? throw new ArgumentOutOfRangeException()
                : failedAttempts > DelayPerAttemptInSeconds.Length ? DelayPerAttemptInSeconds.Last() : DelayPerAttemptInSeconds[failedAttempts];
        }
    }
}