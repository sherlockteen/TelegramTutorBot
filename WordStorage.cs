using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace telegram_Dictionary
{

    internal class WordStorage
    {
        private const string _path = "wordstorage.txt";

        public static Dictionary<string, string> GetAllWords()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(_path))
            {
                return dict;
            }

            foreach (var line in File.ReadAllLines(_path))
            {
                var words = line.Split("|");

                if (words.Length == 2)
                {
                    dict[words[0].Trim()] = words[1].Trim();
                }
            }
            return dict;
        }

        public static void AddWord(string engWord, string rusWord)
        {
            if (string.IsNullOrEmpty(engWord) || string.IsNullOrEmpty(rusWord))
            {
                throw new ArgumentException("Слова не могут быть пустыми.");
            }

            using var writer = new StreamWriter(_path, true);
            writer.WriteLine($"{engWord.Trim()}|{rusWord.Trim()}");
        }
    }
}
