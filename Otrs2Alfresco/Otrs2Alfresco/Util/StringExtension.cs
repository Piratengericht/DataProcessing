namespace System
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return (value == null) || (value == string.Empty);
        }
    }
}
