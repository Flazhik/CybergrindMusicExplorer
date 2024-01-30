using UnityEngine;
using static UnityEngine.KeyCode;

namespace CybergrindMusicExplorer.Util
{
    public static class KeyUtils
    {
        public static string ToHumanReadable(KeyCode key)
        {
            if (key >= A && key <= Z)
                return ((char)('A'  + (key - A))).ToString();
            if (key >= Alpha0 && key <= Alpha9)
                return ((char)('0' + (key - Alpha0))).ToString();

            return key switch
            {
                KeyCode.Space => " ",
                Quote => "\"",
                LeftBracket => "[",
                RightBracket => "]",
                BackQuote => "`",
                Backslash => "\\",
                KeyCode.Equals => "=",
                Minus => "-",
                Semicolon => ";",
                Comma => ",",
                Period => ".",
                Slash => "/",
                _ => key.ToString()
            };
        }
    }
}