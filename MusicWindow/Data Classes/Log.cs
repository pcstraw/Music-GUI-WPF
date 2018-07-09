using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glaxion.Music
{
    public static class Log
    {
        public static List<string> Messages = new List<string>();
        public static string Message(string text)
        {
            Messages.Add(text);
            return text;
        }

        public static void Clear()
        {
            Messages.Clear();
        }
    }
}
