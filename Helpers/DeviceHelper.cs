namespace VisitorTracker.Helpers
{
    public static class DeviceHelper
    {
        public static string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            if (userAgent.Contains("Mobi") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                return "Mobile";

            return "Desktop";
        }
    }
}
