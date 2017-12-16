using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace NeuroWeb
{
    class Classifier
    {
        protected const int MESSAGES_COUNT = 50;
        protected const double Ap = 0.5;
        protected const int w = 1;

        public static int WordsCount
        {
            get{ return Features.Count;}
        }
        static Classifier()
        {
            Categories.Add("good", MESSAGES_COUNT);
            Categories.Add("bad", MESSAGES_COUNT);
            CreateDictionary();
        }
        protected static Dictionary<string, int> Categories = new Dictionary<string, int>();
        protected static Dictionary<string, Dictionary<string, int>> Features = new Dictionary<string, Dictionary<string, int>>();
        protected static string[] GetWords(string text)
        {
            text = text.ToLowerInvariant();
            string pattern = @"\b([a-z]+)\b";
            Regex regex = new Regex(pattern);
            List<string> words = new List<string>();
            foreach (Match match in regex.Matches(text))
            {
                if (!words.Contains(match.Groups[0].Value))
                {
                    words.Add(match.Groups[0].Value);
                }
            }
            return words.ToArray();
        }
        protected static void AddFeatures(string word, string category)
        {
            if (!Features.ContainsKey(word))
            {
                Dictionary<string, int> categories = new Dictionary<string, int>();
                categories.Add(category, 1);
                Features.Add(word, categories);
            }
            else if (!Features[word].ContainsKey(category))
            {
                Features[word].Add(category, 1);
            }
            else
            {
                Features[word][category]++;
            }
        }
        protected static int GetCountOfOccurrencesInCategory(string word, string category)
        {
            if (!Features.ContainsKey(word))
            {
                return 0;
            }
            else if (!Features[word].ContainsKey(category))
            {
                return 0;
            }
            return Features[word][category];
        }
        protected static int GetCountOfExamplesInCategory(string category)
        {
            if (Categories.ContainsKey(category))
            {
                return Categories[category];
            }
            return 0;
        }           //N(category)
        protected static List<string> GetAllCategories()
        {
            return new List<string>(Categories.Keys);
        }
        protected static void CreateDictionary()
        {
            string hamFile = @"good.txt";
            string spamFile = @"bad.txt";
            string dictionaryFile = @"dictionary.txt";
            string message;
            StreamReader sr = new StreamReader(hamFile);
            for (int i = 0; i < Categories["good"]; i++)
            {
                message = sr.ReadLine();
                string[] words = GetWords(message);
                for (int j = 0; j < words.Length; j++)
                {
                    AddFeatures(words[j], "good");
                }
            }
            sr.Close();
            sr = new StreamReader(spamFile);
            for (int i = 0; i < Categories["bad"]; i++)
            {
                message = sr.ReadLine();
                string[] words = GetWords(message);
                for (int j = 0; j < words.Length; j++)
                {
                    AddFeatures(words[j], "bad");
                }
            }
            sr.Close();
            StringBuilder output = new StringBuilder();
            StreamWriter sw = new StreamWriter(dictionaryFile);
            foreach (var feature in Features)
            {
                output.Append("<");
                output.Append(feature.Key.ToString());
                output.Append(", <");
                foreach (var category in feature.Value)
                {
                    output.Append(category.Key.ToString());
                    output.Append(": ");
                    output.Append(category.Value.ToString());
                    output.Append(", ");
                }
                output.Remove(output.Length - 2, 2);
                output.Append(">>");
                sw.WriteLine(output);
                output.Clear();
            }
            sw.Close();
        }   //Dictionary
        protected static double ConditionalProbability(string word, string category)   //P(word|category)
        {
            return (double)GetCountOfOccurrencesInCategory(word, category) / GetCountOfExamplesInCategory(category);
        }
        protected static int GetCountOfOccurrencesInAllCategories(string word)      //N(word)
        {
            int N = 0;
            foreach (var category in Categories)
            {
                N += GetCountOfOccurrencesInCategory(word, category.Key);
            }
            return N;
        }
        protected static double WeightedProbability(string word, string category)     //Pw
        {
            int N = GetCountOfOccurrencesInAllCategories(word);
            double P = ConditionalProbability(word, category);
            return (w * Ap + N * P) / (N + w);
        }
        public static void DrawTable(string category)
        {
            string tableFile = @"C:\Users\Альберт\Desktop\SPAM\table.txt";
            StreamWriter sw = new StreamWriter(tableFile);
            string line = "|-----------------------------------------------------------------------------------------------------------|";
            sw.WriteLine(line);
            string str = string.Format("| {0, -15} | {1, -15} | {2, -15} | {3, -15} | {4, -15} | {5, -15} |",
                    "Слово",
                    "P(слово|" + category + ")",
                    "Pw(слово|" + category + ")",
                    "N(слово&" + category + ")",
                    "N(слово)",
                    "N(" + category + ")");
            sw.WriteLine(str);
            sw.WriteLine(line);
            foreach (var feature in Features)
            {
                str = string.Format("| {0, -15} | {1,15} | {2,15} | {3,15} | {4,15} | {5,15} |",
                    feature.Key,
                    Math.Round(ConditionalProbability(feature.Key, category), 5),
                    Math.Round(WeightedProbability(feature.Key, category), 5),
                    GetCountOfOccurrencesInCategory(feature.Key, category),
                    GetCountOfOccurrencesInAllCategories(feature.Key),
                    Categories[category]);
                sw.WriteLine(str);
            }
            sw.WriteLine(line);
            sw.Close();
        }

        public static bool[] GetVector(string text)
        {
            string[] words = GetWords(text);
            bool[] result = new bool[Features.Count];
            var keys= Features.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                bool flag = false;
                for (int j = 0; j < words.Length; j++)
                {
                    if (keys[i] == words[j])
                    {
                        flag = true;
                        break;
                    }
                }
                result[i] = flag;
            }
            return result;
        }

        public static string[] GetMesaages(string fileName)
        {
            StreamReader file = new StreamReader(fileName);
            List<string> result = new List<string>();
            while (!file.EndOfStream)
            {
                result.Add(file.ReadLine());
            }
            return result.ToArray();
        }
    }
}

