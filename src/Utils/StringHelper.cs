namespace LyraBot.Utils
{
    public static class StringHelper
    {
        public static string Truncate(this string value, int length)
            => (value != null && value.Length > length) ? value.Substring(0, length) : value;
    }
}
