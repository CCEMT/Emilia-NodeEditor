using System;

namespace Emilia.Kit
{
    public class TextAttribute : Attribute
    {
        public string text;

        public TextAttribute(string text)
        {
            this.text = text;
        }
    }
}