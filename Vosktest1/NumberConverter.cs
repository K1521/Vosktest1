using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vosktest1
{
    internal static class NumberConverter
    {
        public static ReadOnlyCollection<string> ones = Array.AsReadOnly(new string[]{ "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" , "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" });
        public static ReadOnlyCollection<string> tens = Array.AsReadOnly(new string[]{ null, "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" });
        static Dictionary<string, int> modifiers = new Dictionary<string, int>() {
            {"billion", 1000000000},
            {"million", 1000000},
            {"thousand", 1000},
            {"hundred", 100}
        };



        public static string NumberToWord(int number)
        {
            if (number < 0) 
                return "minus " + NumberToWord(number);

            if (number < 20)
                return ones[number];

            if (number < 100) {
                return tens[number/10]+ ((number%10==0)?"": " " + NumberToWord(number % 10)) ;
            }

            if (number < 1000)
            {
                return ones[number / 100]+ " hundred" + ((number % 100 == 0) ? "" :" "+ NumberToWord(number % 100));
            }

            if (number < 1000000)
                return NumberToWord(number / 1000) + " thousand" + ((number % 1000 == 0) ? "" : " " + NumberToWord(number % 1000));
            else if (number < 1000000000)
                return NumberToWord(number / 1000000) + " million" + ((number % 1000000 == 0) ? "" : " " + NumberToWord(number % 1000000));
            else if (number < 1000000000)
                return NumberToWord(number / 1000000000) + " billion" + ((number % 1000000000 == 0) ? "" : " " + NumberToWord(number % 1000000000));
            return "?";
        }

        public static int WordToNumber(string word)
            => WordToNumber(word.Split(" "));
        
        public static int WordToNumber(string[] words)
        {

            bool minusflag = false;
            int number = 0;
            //[one hundred ten thousand] [one hundred] [forty] [five]

            int current = 0;
            foreach (string curword in words) {
                int n;
                if ((n = ones.IndexOf(curword))! >= 0)
                    current += n;
                else if ((n = tens.IndexOf(curword))! >= 0)
                    current += n * 10;
                else if (curword == "hundred")
                    current *= 100;
                else if (curword == "minus")
                    minusflag = true;
                else if (modifiers.ContainsKey(curword))
                {
                    if (current == 0) current = 1;
                    number += current * modifiers[curword];
                    current = 0;
                }
            }
            number += current;
            if(minusflag)
                return -number ;
            return number;
        }

        public static bool TryWordToNumber(string word, out int number)
        {
            string[] words = word.Split(" ");
            number = 0;

            if(words.Length == 0) return false;

            bool minusflag = words[0]== "minus";
            if (words.Length == 1 && minusflag) return false;//handle minus
            int i = minusflag ? 1 : 0;
            if (words[i] == ones[0])//handle zero
            {
                if(i == words.Length-1) return true;
                return false;
            }
            //[one hundred ninty two  thousand] [one-twenty hundred] [forty] [five]


            bool valid = true;
            int current = 0;
            bool onesflag = false;
            bool tensflag = false;
            bool hunflag = false;
            int bigg = modifiers.Values.Max() + 1 ;
            for (;i<words.Length;i++)
            {
                string curword = words[i];


                int n;
                if ((n = ones.IndexOf(curword))! >= 0)
                {
                    current += n;
                    if (n == 0 || onesflag || (n >= 10 && tensflag)) valid = false;

                    if (n >= 10) tensflag = true;
                    onesflag = true;

                }
                else if ((n = tens.IndexOf(curword))! >= 0)
                {
                    current += n * 10;
                    if (n == 0 || onesflag || tensflag) valid = false;
                    tensflag = true;
                }

                else if (curword == "hundred")
                {
                    if (!(onesflag || tensflag)) current = 1;
                    current *= 100;
                    if (hunflag) valid = false;
                    hunflag = true;
                    onesflag = tensflag = false;
                }

                else if (modifiers.ContainsKey(curword))
                {
                    if (!(onesflag || tensflag || hunflag)) current = 1;
                    int mod= modifiers[curword];
                    number += current * mod;
                    if(mod>bigg) valid = false;
                    bigg= mod;
                    current = 0;
                    hunflag = tensflag = onesflag = false;
                }
                else {
                    valid = false;
                }
            }

            number += current;
            if (minusflag)
                number =- number;
            return valid;
        }

        /*public static bool TryWordToNumber(string word, out int number)
        {
            number = WordToNumber(word);
            if (word != NumberToWord(number))
                return false;//TODO doesnt work in all cases obviously 
            return true;
        }*/
        /*private static int WordToNumber1900(string[] words,ref int i)
        {
            //[one hundred twenty two thousand] [one hundred] [forty] [five]
            int current = 0;
            int n;
            if ((n = Array.IndexOf(ones, words[i])) >=0){
                current = n;
                i++;
            }
            if (i >= words.Length)return current;

            if ((n = Array.IndexOf(tens, words[i])) >= 0)
            {
                current += 10*n;
                i++;
            }
            if (i >= words.Length)
                return current;


            int current = 0;
            foreach (string curword in words)
            {
                int n;
                if ((n = Array.IndexOf(ones, curword))! >= 0)
                {
                    current += n;
                }
                else if ((n = Array.IndexOf(tens, curword))! >= 0)
                {
                    current += n * 10;
                }
                else
                {
                    current *= modifiers[curword];
                }
            }
            return number + current;
        }*/
    }
}
