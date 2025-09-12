#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.International.Converters.PinYinConverter;
using NPinyin;
using UnityEngine;

namespace Emilia.Kit
{
    public static class PinYinConverterUtility
    {
        private static readonly Dictionary<string, string> _pinyinCache = new Dictionary<string, string>();

        public static string ConvertToAllSpell(string strChinese)
        {
            if (string.IsNullOrEmpty(strChinese)) return string.Empty;
            if (_pinyinCache.TryGetValue(strChinese, out string cachedResult)) return cachedResult;

            try
            {
                StringBuilder fullSpell = new StringBuilder();
                for (int i = 0; i < strChinese.Length; i++)
                {
                    var chr = strChinese[i];
                    fullSpell.Append(GetSpell(chr));
                }

                string result = fullSpell.ToString();

                _pinyinCache[strChinese] = result;
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError("全拼转化出错！" + e.Message);
                return string.Empty;
            }
        }

        public static bool ContainsChinese(string str)
        {
            bool isContains = false;
            for (int i = 0; i < str.Length; i++)
            {
                var chr = str[i];
                bool isChineseChar = ChineseChar.IsValidChar(chr);
                if (isChineseChar == false) continue;
                isContains = true;
                break;
            }

            return isContains;
        }

        public static bool AllChinese(string str)
        {
            bool isAllChinese = true;
            for (int i = 0; i < str.Length; i++)
            {
                var chr = str[i];
                bool isChineseChar = ChineseChar.IsValidChar(chr);
                if (isChineseChar) continue;
                isAllChinese = false;
                break;
            }

            return isAllChinese;
        }

        private static string GetSpell(char chr)
        {
            string converter = Pinyin.GetPinyin(chr);

            bool isChinese = ChineseChar.IsValidChar(converter[0]);
            if (isChinese)
            {
                ChineseChar chineseChar = new ChineseChar(converter[0]);
                foreach (string value in chineseChar.Pinyins)
                {
                    if (! string.IsNullOrEmpty(value))
                    {
                        return value.Remove(value.Length - 1, 1);
                    }
                }
            }

            return converter;
        }
    }
}
#endif