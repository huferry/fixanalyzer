﻿namespace FixAnalyzer
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    internal class HtmlFragment
    {
        #region Read and decode from clipboard

        private string m_fragment;
        private string m_fullText;
        private Uri m_source;
        private string m_version;

        /// <summary>
        ///     Create an HTML fragment decoder around raw HTML text from the clipboard.
        ///     This text should have the header.
        /// </summary>
        /// <param name="rawClipboardText">raw html text, with header.</param>
        public HtmlFragment(string rawClipboardText)
        {
            // This decodes CF_HTML, which is an entirely text format using UTF-8.
            // Format of this header is described at:
            // http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp

            // Note the counters are byte counts in the original string, which may be Ansi. So byte counts
            // may be the same as character counts (since sizeof(char) == 1).
            // But System.String is unicode, and so byte couns are no longer the same as character counts,
            // (since sizeof(wchar) == 2). 
            int startHmtl = 0;

            int startFragment = 0;

            Regex r;
            Match m;

            r = new Regex("([a-zA-Z]+):(.+?)[\r\n]",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            for (m = r.Match(rawClipboardText); m.Success; m = m.NextMatch())
            {
                string key = m.Groups[1].Value.ToLower();
                string val = m.Groups[2].Value;

                switch (key)
                {
                        // Version number of the clipboard. Starting version is 0.9. 
                    case "version":
                        this.m_version = val;
                        break;

                        // Byte count from the beginning of the clipboard to the start of the context, or -1 if no context
                    case "starthtml":
                        if (startHmtl != 0) throw new FormatException("StartHtml is already declared");
                        startHmtl = int.Parse(val);
                        break;

                        // Byte count from the beginning of the clipboard to the end of the context, or -1 if no context.
                    case "endhtml":
                        if (startHmtl == 0) throw new FormatException("StartHTML must be declared before endHTML");
                        int endHtml = int.Parse(val);

                        this.m_fullText = rawClipboardText.Substring(startHmtl, endHtml - startHmtl);
                        break;

                        //  Byte count from the beginning of the clipboard to the start of the fragment.
                    case "startfragment":
                        if (startFragment != 0) throw new FormatException("StartFragment is already declared");
                        startFragment = int.Parse(val);
                        break;

                        // Byte count from the beginning of the clipboard to the end of the fragment.
                    case "endfragment":
                        if (startFragment == 0)
                            throw new FormatException("StartFragment must be declared before EndFragment");
                        int endFragment = int.Parse(val);
                        this.m_fragment = rawClipboardText.Substring(startFragment, endFragment - startFragment);
                        break;

                        // Optional Source URL, used for resolving relative links.
                    case "sourceurl":
                        this.m_source = new Uri(val);
                        break;
                }
            } // end for

            if (this.m_fullText == null && this.m_fragment == null)
            {
                throw new FormatException("No data specified");
            }
        }

        /// <summary>
        ///     Get the Version of the html. Usually something like "1.0".
        /// </summary>
        public string Version
        {
            get { return this.m_version; }
        }


        /// <summary>
        ///     Get the full text (context) of the HTML fragment. This includes tags that the HTML is enclosed in.
        ///     May be null if context is not specified.
        /// </summary>
        public string Context
        {
            get { return this.m_fullText; }
        }


        /// <summary>
        ///     Get just the fragment of HTML text.
        /// </summary>
        public string Fragment
        {
            get { return this.m_fragment; }
        }


        /// <summary>
        ///     Get the Source URL of the HTML. May be null if no SourceUrl is specified. This is useful for resolving relative
        ///     urls.
        /// </summary>
        public Uri SourceUrl
        {
            get { return this.m_source; }
        }

        /// <summary>
        ///     Get a HTML fragment from the clipboard.
        /// </summary>
        /// <example>
        ///     string html = "<b>Hello!</b>";
        ///     HtmlFragment.CopyToClipboard(html);
        ///     HtmlFragment html2 = HtmlFragment.FromClipboard();
        ///     Debug.Assert(html2.Fragment == html);
        /// </example>
        public static HtmlFragment FromClipboard()
        {
            string rawClipboardText = Clipboard.GetText(TextDataFormat.Html);
            HtmlFragment h = new HtmlFragment(rawClipboardText);
            return h;
        }

        #endregion // Read and decode from clipboard

        #region Write to Clipboard

        // Helper to convert an integer into an 8 digit string.
        // String must be 8 characters, because it will be used to replace an 8 character string within a larger string.    
        private static string To8DigitString(int x)
        {
            return String.Format("{0,8}", x);
        }

        /// <summary>
        ///     Clears clipboard and copy a HTML fragment to the clipboard. This generates the header.
        /// </summary>
        /// <param name="htmlFragment">A html fragment.</param>
        /// <example>
        ///     HtmlFragment.CopyToClipboard("<b>Hello!</b>");
        /// </example>
        public static void CopyToClipboard(string htmlFragment)
        {
            CopyToClipboard(htmlFragment, null, null);
        }


        /// <summary>
        ///     Clears clipboard and copy a HTML fragment to the clipboard, providing additional meta-information.
        /// </summary>
        /// <param name="htmlFragment">a html fragment</param>
        /// <param name="title">optional title of the HTML document (can be null)</param>
        /// <param name="sourceUrl">optional Source URL of the HTML document, for resolving relative links (can be null)</param>
        public static void CopyToClipboard(string htmlFragment, string title, Uri sourceUrl)
        {
            if (title == null) title = "From Clipboard";

            StringBuilder sb = new StringBuilder();

            // Builds the CF_HTML header. See format specification here:
            // http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp

            // The string contains index references to other spots in the string, so we need placeholders so we can compute the offsets. 
            // The <<<<<<<_ strings are just placeholders. We'll backpatch them actual values afterwards.
            // The string layout (<<<) also ensures that it can't appear in the body of the html because the <
            // character must be escaped.
            string header =
                @"Format:HTML Format
Version:1.0
StartHTML:<<<<<<<1
EndHTML:<<<<<<<2
StartFragment:<<<<<<<3
EndFragment:<<<<<<<4
StartSelection:<<<<<<<3
EndSelection:<<<<<<<3
";

            string pre =
                @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<HTML><HEAD><TITLE>" + title + @"</TITLE></HEAD><BODY><!--StartFragment-->";

            string post = @"<!--EndFragment--></BODY></HTML>";

            sb.Append(header);
            if (sourceUrl != null)
            {
                sb.AppendFormat("SourceURL:{0}", sourceUrl);
            }
            int startHtml = sb.Length;

            sb.Append(pre);
            int fragmentStart = sb.Length;

            sb.Append(htmlFragment);
            int fragmentEnd = sb.Length;

            sb.Append(post);
            int endHtml = sb.Length;

            // Backpatch offsets
            sb.Replace("<<<<<<<1", To8DigitString(startHtml));
            sb.Replace("<<<<<<<2", To8DigitString(endHtml));
            sb.Replace("<<<<<<<3", To8DigitString(fragmentStart));
            sb.Replace("<<<<<<<4", To8DigitString(fragmentEnd));


            // Finally copy to clipboard.
            string data = sb.ToString();
            Clipboard.Clear();
            Clipboard.SetText(data, TextDataFormat.Html);
        }

        #endregion // Write to Clipboard
    } // end of class
}