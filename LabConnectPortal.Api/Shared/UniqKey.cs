namespace LabConnectPortal.Api.Shared
{
    public static class UniqKey
    {
        private static long _lastTimestamp = 0;
        private static int _counter = 0;
        private static readonly object _lock = new();

        public static string Generate()
        {
            lock (_lock)
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (timestamp == _lastTimestamp)
                {
                    _counter++;
                }
                else
                {
                    _counter = 0;
                    _lastTimestamp = timestamp;
                }

                return (timestamp * 1000 + _counter).ToString(); // allows 1000 ids per ms
            }
        }
    }
}
