using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace telegram_Dictionary
{
    public class Tutor
    {
        private readonly Dictionary<string, string> _dict;
        private readonly Random _rand = new();

        public Tutor()
        {
            _dict = WordStorage.GetAllWords();
        }

        public void AddWord(string engWord, string rusWord)
        {
            if (string.IsNullOrEmpty(engWord) || string.IsNullOrEmpty(rusWord))
            {
                throw new ArgumentException("Слова не могут быть пустыми.");
            }


            var key = engWord.Trim();

            if (_dict.ContainsKey(key))
            {
                throw new FoundDublicateException { MethodName = nameof(AddWord)};
            }

            // Добавляем в память
            _dict[key] = rusWord;
            // пишем на диск
            WordStorage.AddWord(engWord, rusWord);
        }

        public bool CheckWord(string engWord, string rusWord)
        {
            if (_dict.TryGetValue(engWord.Trim(), out var correctTranslation))
            {
                return correctTranslation.Equals(rusWord.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public string? Translate(string engWord)
        {
            return _dict.TryGetValue(engWord.Trim(), out var value) ? value : null;
        }

        public string GetRandomEngWord()
        {
            if (_dict.Count == 0)
            {
                return "Словарь пуст.";
            }
            
            var keys = new List<string>(_dict.Keys);
            var randomIndex = _rand.Next(0, keys.Count);
            return keys[randomIndex];
        }

    }
}
