using System.Text.RegularExpressions;

namespace Maidchan.Workflow.Extension
{
  public static class StringExtension
  {
    const string pattern = "^\\${(.*?)\\}";

    ///<summery>
    /// Check if string containe specific expression value
    ///</summery>
    public static string AsExpression(this string text)
    {
      var match = Regex.Match(text, pattern);
      if (match.Success)
      {
        return match.Groups[1]?.Value;
      }
      return $"\"{text}\"";
    }

    ///<summery>
    /// Padding string within ${.!--.!--.} for case 
    ///</summery>
    public static string DesExpression(this string text)
    {
      if (text.StartsWith("\"") && text.EndsWith("\""))
      {
        return text.Trim('\"');
      }
      else
      {
        return $"${{{text}}}";
      }
    }
  }
}