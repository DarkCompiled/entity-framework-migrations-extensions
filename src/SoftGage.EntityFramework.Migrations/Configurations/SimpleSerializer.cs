using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SoftGage.EntityFramework.Migrations.Configurations
{
    internal static class SimpleSerializer
    {
        #region Constants
        public const char Separator = '|';
        public static readonly Regex SpliRegex = new Regex(@"(?<!\\)\|", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        public static readonly char[] SeparatorArray = { '|' };
        #endregion

        #region Public methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Serialize(string first, string second)
        {
            return (first != null ? Escape(first) : string.Empty) + Separator + (second != null ? Escape(second) : string.Empty);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Serialize(string first, int second)
        {
            return first + Separator + second;
        }
        public static void Deserialize(string serialized, out string first, out string second)
        {
            var split = SpliRegex.Split(serialized);
            if (split.Length > 1)
            {
                var name = Unescape(split[0]);
                first = name != string.Empty ? name : null;

                var value = Unescape(split[1]);
                second = value != string.Empty ? value : null;

                return;
            }

            first = null;
            second = null;
        }
        public static void Deserialize(string serialized, out string first, out int second)
        {
            if (serialized[0] == Separator)
            {
                first = null;
                second = int.Parse(serialized.Substring(1));
                return;
            }

            var n = serialized.Length - 2;
            var index = serialized.LastIndexOfAny(SeparatorArray, n, n);
            first = serialized.Substring(0, index);
            second = int.Parse(serialized.Substring(index + 1));
        }
        #endregion

        #region Private methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Escape(string text)
        {
            return text.Replace(@"|", @"\|");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Unescape(string text)
        {
            return text.Replace(@"\|", @"|");
        }
        #endregion
    }
}
