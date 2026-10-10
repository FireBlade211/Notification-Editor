namespace FireBlade.NotificationEditor.Helpers
{
    public static class StringHelper
    {
        public static string OrDefault(this string s, string def) => string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s) ? def : s;
    }
}
