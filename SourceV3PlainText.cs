using System;
using System.Net;
using System.Text.RegularExpressions;

namespace DisplayItemSourceMod
{
    // Presentation only: never decode arbitrary text, identities or lookup keys.
    internal static class SourceV3PlainText
    {
        private const string Left = @"(?:<|&(?:amp;)?lt;|&(?:amp;)?#(?:0*60|x0*3c);)";
        private const string Right = @"(?:>|&(?:amp;)?gt;|&(?:amp;)?#(?:0*62|x0*3e);)";
        private static readonly Regex Tokens = new Regex(Left + @"[^<>\r\n]{1,160}?" + Right,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private const string Colour = @"(?:\#[0-9a-f]{6}(?:[0-9a-f]{2})?|black|blue|green|orange|purple|red|white|yellow)";
        private static readonly Regex Styles = new Regex(
            @"^<(?:/?(?:b|i|u|s|sup|sub)|/(?:color|size|alpha|mark|font-weight)|" +
            @"(?:color|mark)=(?:" + Colour + @"|""" + Colour + @""")|" +
            @"size=(?:[+-]?(?:\d+(?:\.\d+)?|\.\d+)(?:%|px|em)?|""[+-]?(?:\d+(?:\.\d+)?|\.\d+)(?:%|px|em)?"")|" +
            @"alpha=\#[0-9a-f]{2}|font-weight=(?:100|200|300|400|500|600|700|800|900)|\#[0-9a-f]{6}(?:[0-9a-f]{2})?)>$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static string Readable(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return Tokens.Replace(value, token =>
            {
                // Decode only a candidate tag, at most twice. Unknown tokens
                // retain their exact original spelling, including entities.
                string decoded = WebUtility.HtmlDecode(WebUtility.HtmlDecode(token.Value));
                return Styles.IsMatch(decoded) ? "" : token.Value;
            });
        }
    }
}
