﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cryptology;

namespace Cryptology.Vigener
{
    public class VigenerCypher : ICypher
    {
        public string KeyWord { get; set; }

        private Alphabet _alphabet;

        private CaesarCypher _caesarCypher;
        public VigenerCypher(string keyWord)
        {
            KeyWord = keyWord;
            _alphabet = Data.GetAlphabets()[AlphabetType.Russian];
            _caesarCypher = new CaesarCypher(0);
        }
        public string Encrypt(string text)
        {
            var textFromKeyWord = CreateTextFromKeyWord(text.Length);
            var encryptedText = "";
            for (int i = 0; i < text.Length; i++)
            {
                encryptedText += GetEncryptedChar(textFromKeyWord[i], text[i]);

                if (encryptedText.Length > 4 && encryptedText.Length % 6 == 0)
                    encryptedText += " ";
            }
            return encryptedText;
        }

        private char GetEncryptedChar(char key, char srcSymb)
        {
            var row = _alphabet.Letters.IndexOf(key);
            var column = _alphabet.Letters.IndexOf(srcSymb);
            return Data.vigenerTable[row][column];
        }

        private string CreateTextFromKeyWord(int count)
        {
            var result = "";
            for (int i = 0; i < count; i++)
            {
                var j = i;
                while (j >= KeyWord.Length)
                    j -= KeyWord.Length;
                result += KeyWord[j];
            }
            return result;
        }

        public string Decrypt(string text)
        {
            var textFromKeyWord = CreateTextFromKeyWord(text.Length);
            var decryptedText = "";
            for (int i = 0; i < text.Length; i++)
            {
                decryptedText += GetSourceSymbol(textFromKeyWord[i], text[i]);
                if (decryptedText.Length > 4 && decryptedText.Length % 6 == 0)
                    decryptedText += " ";
            }
            return decryptedText;
        }

        private char GetSourceSymbol(char key, char encrSymb)
        {
            var row = _alphabet.Letters.IndexOf(key);
            var index = Data.vigenerTable[row].IndexOf(encrSymb);
            return _alphabet.Letters[index];
        }

        public string BreakOpen(string text)
        {
            int keyWordLength = FindKeyWordLength(text);
            var breakText = new char[text.Length];
            
            for (int j = 0; j < keyWordLength; j++)
            {
                var sameShiftString = "";
                for (int i = j; i < text.Length; i += keyWordLength)
                {
                    sameShiftString += text[i];
                }
                var breakOpenStr = _caesarCypher.BreakOpen(sameShiftString);
                int k = 0;
                for (int i = j; i < text.Length; i += keyWordLength)
                {
                    breakText[i] = breakOpenStr[k];
                    k++;
                }
            }

            string keyWord = "";
            var resultString = "";
            foreach(var symb in breakText)
            {
                resultString += symb;
                if (resultString.Length > 4 && resultString.Length % 6 == 0)
                {
                    resultString += " ";
                }
            }
            
            for (int i = 0; i < keyWordLength; i++)
            {
                keyWord += GetKeyChar(breakText[i], text.ElementAt(i));
            }
           
            resultString += " \nКлючевое слово: " + keyWord;
            return resultString;
        }

        private char GetKeyChar(char columnSymb, char value)
        {
            int columnNumber = 0;
            for (int i = 0; i < Data.vigenerTable[0].Count; i++)
            {
                if (Data.vigenerTable[0][i] == columnSymb)
                {
                    columnNumber = i;
                    break;
                }
            }

            foreach (var row in Data.vigenerTable)
            {
                if (row[columnNumber] == value)
                    return Data.GetAlphabets()[AlphabetType.Russian].Letters[Data.vigenerTable.IndexOf(row)];
            }
            return ' ';
        }

        private int FindKeyWordLength(string text)
        {
            var text2 = text;
            
            List<int> distances = new List<int>();
            for (int i = 0; i < text.Length-2; i++)
            {                
                for (int j = i+1; j < text2.Length - 2; j++)
                {
                    if (text[i] == text2[j] && text[i + 1] == text2[j + 1] && text[i + 2] == text2[j + 2])
                    {
                        distances.Add(j - i);
                    }
                }
            }
            return FindNOD(distances);
        }
        private int FindNOD(IEnumerable<int> numbers)
        {
            List<int> dividers = new List<int>();
            for (int i = 0; i < numbers.Count(); i++)
            {
                for (int j = i + 1; j < numbers.Count(); j++)
                {
                    int dividend = numbers.ElementAt(i);
                    int divider = numbers.ElementAt(j);
                    while (true)
                    {
                        int residue = dividend % divider;
                        if (residue == 0)
                        {
                            if (divider > 2)
                                dividers.Add(divider);
                            break;
                        }
                        else
                        {
                            dividend = divider;
                            divider = residue;
                        }
                    }
                }
            }

            dividers.Sort();

            var dividerCountDict = CountDividersAmount(dividers);

            return GetMostFrequentDivider(dividerCountDict);
        }

        private Dictionary<int, int> CountDividersAmount(List<int> dividers)
        {
            var dividerCountDict = new Dictionary<int, int>();
            foreach (var divider in dividers)
            {
                if (!dividerCountDict.ContainsKey(divider))
                    dividerCountDict.Add(divider, 1);
                else
                    dividerCountDict[divider] += 1;
            }
            return dividerCountDict;
        }

        private int GetMostFrequentDivider(Dictionary<int, int> dividerCountDict)
        {
            var maxCount = 0;
            int maxDivider = 0;
            foreach (var divider in dividerCountDict.Keys)
            {
                if (dividerCountDict[divider] > maxCount)
                {
                    maxCount = dividerCountDict[divider];
                    maxDivider = divider;
                }
            }
            return maxDivider;
        }
    }
}
