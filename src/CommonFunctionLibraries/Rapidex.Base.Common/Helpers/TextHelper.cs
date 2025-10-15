using Rapidex;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rapidex
{
    public static class TextHelper
    {
        private static CultureInfo CultureTr = new CultureInfo("tr-TR", false);

        public static string Clear(this string str, params char[] chars)
        {
            if (str.IsNullOrEmpty())
                return str;

            if (chars.IsNullOrEmpty())
                return str;

            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if (chars.Contains(c))
                    continue;

                sb.Append(c);
            }

            return sb.ToString();
        }

        public static string ClearSpecials(this string str, params char[] includedChars)
        {
            if (str.IsNullOrEmpty())
                return str;

            var sb = new StringBuilder();
            foreach (var c in str)
            {
                if (c > 31 || includedChars.Contains(c))
                    sb.Append(c);
            }

            string text = sb.ToString().Trim();
            return text;
        }

        public static char ToInvariant(this char ch)
        {
            if (ch < 128) // karakter ascii içeriğe sahiptir
                return ch;

            switch (ch)
            {
                case (char)8216: ch = '\''; break; //'‘'
                case (char)8217: ch = '\''; break; //'’'
                case (char)8220: ch = '"'; break;  //'“'
                case (char)8221: ch = '"'; break;  //'”'
                case (char)8223: ch = '"'; break;  //'‟'
                case (char)8211: ch = '-'; break;  //'–'
                case 'ı': ch = 'i'; break;

            }

            string b = string.Join("", ch.ToString().Normalize(NormalizationForm.FormD).Where(k => char.GetUnicodeCategory(k) != System.Globalization.UnicodeCategory.NonSpacingMark));
            if (b == string.Empty)
                return '_';

            ch = Convert.ToChar(b);
            return ch;
        }

        /// <summary>
        /// Verilen metindeki Türkçe karakterleri ingilizce (invariant)
        /// karşılıkları ile değiştirir
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToInvariant(this string text)
        {
            if (text == null)
                text = string.Empty;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                ch = ch.ToInvariant();
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static string ToFriendly(this string text, params char[] include)
        {
            if (text.IsNullOrEmpty())
                return text;

            text = text.ToInvariant();

            string ftext = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    ftext += c;
                else
                {
                    if (include.Contains(c))
                        ftext += c;
                }
            }
            return ftext;
        }

        public static string ToNavigationName(this string text)
        {
            if (text.IsNullOrEmpty())
                return text;
            return text.ToFriendly().CamelCase();
        }

        public static void ValidateInvariantName(this string name)
        {
            if (name.IsNullOrEmpty())
                throw new BaseVariableRequiredException("name");

            string _name = name.ToFriendly();
            if (_name != name)
                throw new BaseValidationException($"name '{name}' is invalid. Names must contain invariant characters and must not contain any special + whitespace characters");
        }

        public static string Capitalize(this string str)
        {
            if (str.IsNullOrEmpty())
                return str;

            TextInfo textInfo = CultureTr.TextInfo;
            return textInfo.ToTitleCase(str.ToLower(CultureTr));
        }

        public static string CamelCase(this string str)
        {
            if (str.IsNullOrEmpty())
                return str;

            return System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(str);
        }

        public static string FirstLetterUpcase(this string str)
        {
            if (str.IsNullOrEmpty())
                return str;

            string lower = str.ToLower(CultureTr);
            string result = char.ToUpper(lower[0], CultureTr) + lower.Remove(0, 1);
            return result;
        }

        /// <summary>
        /// Verilen kelimelerin ilk harflerini birleştirerek kısaltma haline getirir. 
        /// Örn: Türkiye Büyük Millet Mecisi -> TBMM
        /// </summary>
        /// <returns></returns>
        public static string AbbrFromFirstLetters(this string str)
        {
            if (str.IsNullOrEmpty())
                return str;

            string[] parts = str.Split(' ').TrimElements();
            string abbr = string.Empty;

            foreach (string part in parts)
                abbr = abbr + char.ToUpper(part[0], CultureTr);

            return abbr;
        }

        public static string Crop(this string str, int size, bool placeDots = false)
        {
            if (str.IsNullOrEmpty())
                return str;

            if (str.Length < size)
                return str;

            if (placeDots)
                size = size - 3;

            string cropped = str.Substring(0, size);
            return placeDots ? cropped + "..." : cropped;
        }

        public static string[] CropLastPart(this string text, char seperator)
        {
            if (text.IsNullOrEmpty())
                return new string[0];

            string[] parts = text.Split(seperator);
            if (parts.Length == 1)
                return new string[0];

            string[] _result = new string[parts.Length - 1];
            Array.Copy(parts, _result, parts.Length - 1);
            return _result;
        }

        public static string Retry(this char ch, int count)
        {
            string str = string.Empty;
            for (int i = 0; i < count; i++)
            {
                str = string.Concat(str, ch);
            }
            return str;
        }

        public static string AlignRight(this string str, int totalWidth, char paddingChar = ' ')
        {
            if (str.IsNullOrEmpty())
                return str;

            if (str.Length >= totalWidth)
                return str;

            return str.PadLeft(totalWidth, paddingChar);
        }

        public static string AppendIsNotExists(this string str, string endfix)
        {
            if (!str.EndsWith(endfix))
                str = str + endfix;

            return str;
        }

        //Check if the string is base64 encoded
        public static bool IsBase64(this string base64)
        {
            if (string.IsNullOrEmpty(base64) || base64.Length % 4 != 0)
            {
                return false;
            }

            //https://stackoverflow.com/questions/6309379/how-to-check-for-a-valid-base64-encoded-string#:~:text=It's%20pretty%20easy%20to%20recognize,the%20exception%2C%20if%20it%20occurs.
            bool isBase64 = Convert.TryFromBase64String(base64.PadRight(base64.Length / 4 * 4 + (base64.Length % 4 == 0 ? 0 : 4), '='), new Span<byte>(new byte[base64.Length]), out _);
            return isBase64;
        }

        //Create join method for string dictionary
        public static string Join(this IDictionary<string, string> dictionary, string separatorForKeyValues, string seperatorForCouples, bool encodeValuesForUrl = false)
        {
            if (dictionary.IsNullOrEmpty())
                return string.Empty;

            List<string> strings = new List<string>();

            foreach (var item in dictionary)
            {
                strings.Add($"{item.Key}{separatorForKeyValues}{(encodeValuesForUrl ? item.Value.EncodeForUrl() : item.Value)}");
            }

            return string.Join(seperatorForCouples, strings);
        }

        public static string EncodeForUrl(this string str)
        {
            //Uri.EscapeDataString
            if (str.IsNullOrEmpty())
                return str;
            return Uri.EscapeDataString(str);
        }

    }
}
