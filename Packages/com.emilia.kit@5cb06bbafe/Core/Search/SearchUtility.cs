#if UNITY_EDITOR
using FuzzySharp;

namespace Emilia.Kit
{
    public static class SearchUtility
    {
        public const int MaxSearchScore = 100;
        public const int MinSearchScore = 0;

        /// <summary>
        /// 字符串搜索
        /// </summary>
        /// <param name="target">目标文本</param>
        /// <param name="input">输入文本</param>
        /// <param name="inputNullResult">输入为空时的返回结果，默认为true</param>
        /// <returns>搜索分数0-100</returns>
        public static int Search(string target, string input, bool inputNullResult = true, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(input)) return inputNullResult ? MaxSearchScore : MinSearchScore;
            if (string.IsNullOrEmpty(target)) return MinSearchScore;

            string searchTarget = ignoreCase ? target.ToLower() : target;
            string searchInput = ignoreCase ? input.ToLower() : input;

            int score = Fuzz.WeightedRatio(searchInput, searchTarget);
            if (score > MinSearchScore) return score;

            string pinYin = PinYinConverterUtility.ConvertToAllSpell(searchTarget);
            return Fuzz.WeightedRatio(searchInput, pinYin);
        }

        public static bool Matching(string target, string input) => Search(target, input) > MinSearchScore;
    }
}
#endif