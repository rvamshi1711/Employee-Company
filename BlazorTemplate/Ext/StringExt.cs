using System.Text.RegularExpressions;

namespace BlazorTemplate.Ext;

public static class StringExt {

    /// <summary>
    ///     Formats the input string as a phone number.
    /// </summary>
    /// <param name="str">The input string to format.</param>
    /// <returns>
    ///     A formatted phone number string if the input contains 10 or more numeric characters. If
    ///     the input contains more than 10 numeric characters, the extra characters are treated as
    ///     an extension. If the input contains fewer than 10 numeric characters, the numeric string
    ///     is returned as is. If the input is null, an empty string is returned.
    /// </returns>
    public static string FormatAsPhoneNumber(this string str) {
        if (str is null) {
            return string.Empty;
        }

        var numericString = string.Join(null, Regex.Matches(str, "[0-9]").Select(m => m.Value));

        if (numericString.Length == 10) {
            return string.Format("{0:(###) ###-####}", long.Parse(numericString));
        }
        else if (numericString.Length > 10) {
            var number = numericString[..10];
            var extension = numericString[10..];
            return string.Format("{0:(###) ###-####} ext{1}", long.Parse(number), extension);
        }
        else {
            return numericString;
        }
    }
}