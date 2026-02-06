using System.Text;

namespace CourseWork.Utility
{
    public static class KeyboardLayoutConverter
    {
        // En -> Uk/Ru mapping
        private static readonly Dictionary<char, char> EnToUk = new Dictionary<char, char>
        {
            {'q', 'й'}, {'w', 'ц'}, {'e', 'у'}, {'r', 'к'}, {'t', 'е'}, {'y', 'н'}, {'u', 'г'}, {'i', 'ш'}, {'o', 'щ'}, {'p', 'з'}, {'[', 'х'}, {']', 'ї'},
            {'a', 'ф'}, {'s', 'і'}, {'d', 'в'}, {'f', 'а'}, {'g', 'п'}, {'h', 'р'}, {'j', 'о'}, {'k', 'л'}, {'l', 'д'}, {';', 'ж'}, {'\'', 'є'},
            {'z', 'я'}, {'x', 'ч'}, {'c', 'с'}, {'v', 'м'}, {'b', 'и'}, {'n', 'т'}, {'m', 'ь'}, {',', 'б'}, {'.', 'ю'}, {'/', '.'}
        };

        // Ru/Uk -> En mapping (Reverse)
        private static readonly Dictionary<char, char> UkToEn;

        static KeyboardLayoutConverter()
        {
            UkToEn = new Dictionary<char, char>();
            foreach (var pair in EnToUk)
            {
                if (!UkToEn.ContainsKey(pair.Value))
                {
                    UkToEn[pair.Value] = pair.Key;
                }
            }
            // Add Russian specific overrides/additions if needed, currently reusing Uk standard layout which mostly overlaps
            // 'ы' is 's' on Ru layout but 'і' on Uk. 
            // The mapping above uses 's' -> 'і' which is standard Ukrainian.
            // Let's add explicit Russian char support for reverse mapping if user types on Ru layout
            if (!UkToEn.ContainsKey('ы')) UkToEn['ы'] = 's';
            if (!UkToEn.ContainsKey('э')) UkToEn['э'] = '\'';
            if (!UkToEn.ContainsKey('ъ')) UkToEn['ъ'] = ']';
            if (!UkToEn.ContainsKey('ё')) UkToEn['ё'] = '`';
        }

        public static string FixLayout(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var result = new StringBuilder(input.Length);
            bool isEnglishConfig = IsLikelyEnglish(input);

            foreach (var c in input.ToLower())
            {
                if (isEnglishConfig)
                {
                    if (EnToUk.TryGetValue(c, out char mapped)) result.Append(mapped);
                    else result.Append(c);
                }
                else
                {
                    if (UkToEn.TryGetValue(c, out char mapped)) result.Append(mapped);
                    else result.Append(c);
                }
            }

            return result.ToString();
        }

        private static bool IsLikelyEnglish(string input)
        {
            int enCount = 0;
            foreach (var c in input.ToLower())
            {
                if (EnToUk.ContainsKey(c)) enCount++;
            }
            return enCount > input.Length / 2;
        }
    }
}
