using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class StringExtension
    {
        /// <summary>
        /// Capitalizes first letter in a word.
        /// </summary>
        /// <param name="s">String to capitalize.</param>
        /// <returns>The capitalized string.</returns>
        public static string CapitalizeFirst(this string s)
        {
            //got this code from stack overflow
            bool IsNewSentense = true;
            var result = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (IsNewSentense && char.IsLetter(s[i]))
                {
                    result.Append(char.ToUpper(s[i]));
                    IsNewSentense = false;
                }
                else
                    result.Append(s[i]);

                if (s[i] == '!' || s[i] == '?' || s[i] == '.')
                {
                    IsNewSentense = true;
                }
            }

            return result.ToString();
        }
        /// <summary>
        /// Lower first letter of a word.
        /// </summary>
        /// <param name="s">String to make lowercase.</param>
        /// <returns>The lowered string.</returns>
        public static string LowerFirst(this string s)
        {
            bool IsNewSentense = true;
            var result = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (IsNewSentense && char.IsLetter(s[i]))
                {
                    result.Append(char.ToLower(s[i]));
                    IsNewSentense = false;
                }
                else
                    result.Append(s[i]);

                if (s[i] == '!' || s[i] == '?' || s[i] == '.')
                {
                    IsNewSentense = true;
                }
            }

            return result.ToString();
        }
        /// <summary>
        /// Capitalizes first letter of each string in a List of strings.
        /// </summary>
        /// <param name="s">List of strings to capitalize</param>
        /// <returns>The capitalized list of strings.</returns>
        public static List<String> CapitalizeStringList(this List<String> s)
        {
            List<String> capitalized = new List<string>();
            foreach (var _string in s)
            {
                capitalized.Add(_string.CapitalizeFirst());

            }
            return capitalized;
        }
        /// <summary>
        /// Lowers the first letter of each string in a list of strings.
        /// </summary>
        /// <param name="s">List of strings to make lowercase</param>
        /// <returns>List of lowercase strings.</returns>
        public static List<String> LowerStringList(this List<String> s)
        {
            List<String> lowered = new List<string>();
            foreach (var _string in s)
            {
                lowered.Add(_string.LowerFirst());

            }
            return lowered;
        }
    }
}