using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Rapidex
{
    public static class RandomHelper //From ProCore
    {
        private static string CHARS_UPPERCASE = "QWERTYUIOPASDFGHJKLZXCVBNM";
        private static string CHARS_LOWERCASE = "qwertyuiopasdfghjklzxcvbnm";

        private static string DIGITS = "0123456789";
        private static string NONALPHA = "!,.*-_()+%&=?"; //{}[] alt gr kullanılan karakterler kullanıcıları zorlayabilir.

        private static string ALL_CHARS = CHARS_UPPERCASE + CHARS_LOWERCASE;
        private static string ALL_CHARS_AND_DIGITS = CHARS_UPPERCASE + CHARS_LOWERCASE + DIGITS;
        private static string ALL = RandomHelper.ALL_CHARS_AND_DIGITS + RandomHelper.NONALPHA;
        private static RNGCryptoServiceProvider seedProvider = new RNGCryptoServiceProvider();


        //public static Bogus.DataSets.Lorem Lorem = new Bogus.DataSets.Lorem();

        public static string CreateRandomPassword(int lenght = 6)
        {
            string pass = RandomSelect(lenght, ALL_CHARS_AND_DIGITS);
            return pass;
        }

        public static string CreateValidationCode()
        {
            Random random = new Random(DateTimeOffset.Now.Millisecond);
            return random.Next(1000, 9000).ToString();
        }

        public static int GenerateSeed()
        {
            byte[] randomBytes = new byte[4];
            seedProvider.GetBytes(randomBytes);

            int seed = (randomBytes[0] & 0x7f) << 24 |
                        randomBytes[1] << 16 |
                        randomBytes[2] << 8 |
                        randomBytes[3];

            return seed;
        }

        public static Random SeededRandom()
        {
            return new Random(RandomHelper.GenerateSeed());
        }


        /// <summary>
        /// Verilen diziden belirtilen miktarda rasgele eleman seçer.
        /// 
        /// NOT1: count parametresi diziden büyük eşit ise, diziyi karıştırarak geri döner. Yani shuffle için kullanılabilir.
        /// NOT2: count sıfırdan küçükse hata fırlatır.
        /// </summary>
        /// <typeparam name="T">Kaynak dizi tipi</typeparam>
        /// <param name="source">Kaynak dizi</param>
        /// <param name="count">İstenen miktar</param>
        /// <returns></returns>
        public static IList<T> RandomElement<T>(this IList<T> source, int count, Func<T, bool> validator = null)
        {
            if (count < 0)
                throw new ArgumentException("'count' parameter must be positive.");

            if (source.Count < count)
                count = source.Count;

            int totalIteration = 0;

            T[] result = new T[count];
            Random random = RandomHelper.SeededRandom();
            for (int i = 0; i < count; i++)
            {
                var item = source[random.Next(source.Count)];

                bool isValid = validator == null || validator(item);

                if (isValid)
                    result[i] = item;
                else
                    count--;

                totalIteration++;

                if (totalIteration > 10000)
                    break;
            }
            return result;
        }

        public static IList<T> RandomElement<T>(int count, Func<T, bool> validator = null) where T : Enum
        {
            Type tType = typeof(T);
            Array values = Enum.GetValues(tType);
            IList<T> listValues = values.Cast<T>().ToList();
            return listValues.RandomElement(count, validator);
        }

        public static T RandomElement<T>(Func<T, bool> validator = null) where T : Enum
        {
            return RandomElement<T>(1, validator).First();
        }

        /// <summary>
        /// Verilen diziden rasgele bir adet eleman seçer.  Not: Dizi boş ise, tipin varsayılanını döner.
        /// </summary>
        /// <typeparam name="T">Kaynak dizi tipi</typeparam>
        /// <param name="source">Kaynak dizi</param>
        /// <returns></returns>
        public static T RandomElement<T>(this IList<T> source, Func<T, bool> validator = null)
        {
            if (source.Count == 0)
                return default(T);

            return RandomHelper.RandomElement<T>(source, 1, validator)[0];
        }

        /// <summary>
        /// Verilen dizi elemanlarının yerlerini karıştırır.
        /// </summary>
        /// <typeparam name="T">Kaynak dizi tipi</typeparam>
        /// <param name="source">Kaynak dizi</param>
        /// <returns></returns>
        public static IList<T> Shuffle<T>(this IList<T> source)
        {
            return RandomHelper.RandomElement<T>(source, source.Count);
        }

        /// <summary>
        /// IList alan metodun array'ler ile kolay kullanımı için.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T[] Shuffle<T>(this T[] source)
        {
            return RandomHelper.RandomElement<T>(source, source.Length);
        }


        /// <summary>
        /// IList alan metodun array'ler ile kolay kullanımı için.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static T[] RandomElement<T>(this T[] source, int count)
        {
            return RandomHelper.RandomElement<T>(source.ToList(), count).ToArray();
        }

        public static T[] RandomElement<T>(this IEnumerable<T> source, int count)
        {
            return RandomHelper.RandomElement<T>(source.ToList(), count).ToArray();
        }

        public static T[] RandomElementWithRandomCount<T>(this T[] source, int minimum = 1)
        {
            int length = source.Length;
            if (length == 0)
                return new T[0];

            if (length == 1)
                return source[0].CreateArray();

            int count = RandomHelper.Between(minimum, length);
            return RandomHelper.RandomElement<T>(source.ToList(), count).ToArray();
        }

        public static T[] RandomElementWithRandomCount<T>(this T[] source, int minimum, int maximum)
        {
            int length = source.Length;
            if (length == 0)
                return new T[0];

            if (length == 1)
                return source[0].CreateArray();

            if (length < maximum)
                maximum = length;

            int count = RandomHelper.Between(minimum, maximum);
            return RandomHelper.RandomElement<T>(source.ToList(), count).ToArray();
        }



        /// <summary>
        /// IList alan metodun array'ler ile kolay kullanımı için.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            return RandomHelper.RandomElement<T>(source.ToList());
        }

        public static T[] RandomReorder<T>(this IEnumerable<T> source)
        {
            if (source.IsNullOrEmpty())
                return new T[0];


            Random rnd = SeededRandom();
            var result = source.OrderBy(_ => rnd.Next()).ToArray();
            return result;
        }

        public static string RandomSelect(int length, string source)
        {
            List<char> _chars = new List<char>();
            do
            {
                char[] chars = source.ToCharArray().RandomElement(length);
                _chars.AddRange(chars);
            } while (_chars.Count < length);
            string str = new string(_chars.ToArray());
            str = str.Substring(0, length);
            return str;
        }

        public static string RandomNonAlpha(int length)
        {
            return RandomHelper.RandomSelect(length, RandomHelper.NONALPHA);
        }

        /// <summary>
        /// Generates a random password
        /// </summary>
        /// <param name="length">length of the password</param>
        /// <returns></returns>
        public static string RandomText(int length)
        {
            return RandomHelper.RandomSelect(length, ALL_CHARS_AND_DIGITS);
        }

        /// <summary>
        /// Generates a random password
        /// </summary>
        /// <param name="length">length of the password</param>
        /// <returns></returns>
        public static string RandomTextWithRandomSpaces(int length)
        {
            //TODO: En fazla 20 ch en az 3ch aralık ile ..
            string text = RandomHelper.RandomSelect(length, ALL_CHARS_AND_DIGITS);
            char[] chars = text.ToArray();
            length = text.Length;
            if (length > 0)
            {
                int spaceCount = Random(length / 10) + 1;
                for (int i = 0; i < spaceCount; i++)
                {
                    int spaceLocation = Random(length - 1);
                    chars[spaceLocation] = ' ';
                }
            }

            text = new string(chars);
            text = text.Trim();
            return text;
        }

        /// <summary>
        /// Generates a random number
        /// </summary>
        /// <param name="length">length of the number</param>
        /// <returns></returns>
        public static string RandomNumeric(int length)
        {
            return RandomHelper.RandomSelect(length, DIGITS);
        }

        public static int Random(int max)
        {
            Random random = RandomHelper.SeededRandom();
            return random.Next(max) + 1;
        }

        public static long Random64(long max)
        {
            Random random = RandomHelper.SeededRandom();
            return random.NextInt64(max);
        }

        /// <summary>
        /// Verilen kesirde zar atar ve sonucunu true / false olarak döner
        /// Örn: 1/5 olasılık istiyor isek Dice(5):True, olasılığın gerçekleştiği sonucunu döndürür.
        /// </summary>
        /// <param name="diceValue"></param>
        /// <returns></returns>
        public static bool Dice(int diceValue)
        {
            return RandomHelper.Random(diceValue + 1) == 1;
        }

        public static T Dice<T>(int diceValue, Func<T> success, Func<T> fail)
        {
            return RandomHelper.Dice(diceValue) ? success() : fail();
        }

        public static int Between(int start, int end)
        {
            if (end <= start)
                throw new ArgumentException("must be: start < end");

            int max = end - start;
            int value = start + RandomHelper.Random(max);
            return value;
        }

        public static DateTimeOffset RandomDate(bool beforeToday)
        {
            try
            {
                DateTimeOffset today = DateTimeOffset.Now.Day();

                int maxMonth = 12;
                int maxYear = today.Year + 1;
                DateTimeOffset maxDate = today.AddYears(1);
                DateTimeOffset minDate = today;

                int year = today.Year;
                if (beforeToday)
                {
                    maxMonth = today.Month;
                    maxYear = today.Year;
                    maxDate = today.AddDays(-1);
                    minDate = today.AddMonths(-3);
                }
                else
                {
                    year = RandomHelper.Between(today.Year, maxYear);
                }


                int month = maxMonth == 1 ? 1 : RandomHelper.Between(1, maxMonth);
                int day = RandomHelper.Between(1, DateTime.DaysInMonth(year, month));

                DateTimeOffset result = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
                if (beforeToday && result >= today)
                    return RandomDate(beforeToday);

                if (!beforeToday && result <= today)
                    return RandomDate(beforeToday);

                if (result < minDate)
                    return RandomDate(beforeToday);

                if (result > maxDate)
                    return RandomDate(beforeToday);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static DateTimeOffset RandomDate(bool beforeToday, DateTimeOffset minDate)
        {
            DateTimeOffset result = RandomDate(beforeToday);
            if (result < minDate)
                return RandomDate(beforeToday, minDate);

            return result;
        }

        public static float RandomFloat(float min, float max)
        {
            if (min >= max)
                throw new ArgumentException("min must be less than max");
            Random random = RandomHelper.SeededRandom();
            return (float)(random.NextDouble() * (max - min) + min);
        }
    }
}
