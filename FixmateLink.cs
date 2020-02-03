namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class FixmateLink
    {
        public static IEnumerable<FixmateLink> GetUsedInMessages(string html)
        {
            return GetUsedIn(html, "messages");
        }

        public static IEnumerable<FixmateLink> GetUsedInComponents(string html)
        {
            return GetUsedIn(html, "components");
        }

        private static IEnumerable<FixmateLink> GetUsedIn(string html, string subject)
        {
            string[] needles =
            {
                string.Format("Used in {0}:", subject),
                "<table"
            };

            string[] splits = html.Split(needles, StringSplitOptions.None);
            string split =
                splits.FirstOrDefault(s => s.Trim().StartsWith("<div class=\"TagUsedIn\"") && s.Trim().EndsWith("</p>"));

            return string.IsNullOrEmpty(split) ? null : GetLinks(split);
        }

        private static IEnumerable<FixmateLink> GetLinks(string data)
        {
            Regex regex = new Regex(">(\\w+)<");
            Regex link = new Regex("href=\"(.+)\\?");
            string[] splits = regex.Split(data);
            int i = 0;
            while (i < splits.Length - 1)
            {
                string[] linkSplits = link.Split(splits[i]);
                if (linkSplits.Length > 1)
                {
                    yield return new FixmateLink();
                }
                i += 2;
            }
        }
    }
}