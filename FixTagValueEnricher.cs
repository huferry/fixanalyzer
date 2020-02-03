namespace FixAnalyzer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Binck.Systems.Sam.KeyManagement.ProRealTime;

    internal delegate string ValueEnricher(string value);

    public class FixTagValueEnricher
    {
        private const string HashedAccountPattern = "(3[a-h]){5,10}";
        private static List<FixTagValueEnricher> enrichers;

        private static readonly FixTagValueEnricher DefaultEnricher = new FixTagValueEnricher(0, AddUnhashedAccount);

        private readonly ValueEnricher enricher;
        private readonly int tag;

        private FixTagValueEnricher(int tag, ValueEnricher enricher)
        {
            this.tag = tag;
            this.enricher = enricher;
        }

        public string Enrich(string value)
        {
            return this.enricher(value);
        }

        private static void InitEnrichers()
        {
            if (enrichers == null)
            {
                enrichers = new List<FixTagValueEnricher>();
            }
        }

        private static string AddUnhashedAccount(string value)
        {
            Match match = Regex.Match(value, HashedAccountPattern);
            if (match.Success)
            {
                string unhased = ProRealTimeTool.Unhash(match.Value);
                return value.Replace(match.Value, string.Format("{0} [{1}]", match.Value, unhased));
            }
            return value;
        }

        public static FixTagValueEnricher GetEnricher(int tag)
        {
            InitEnrichers();
            FixTagValueEnricher enricher = enrichers.FirstOrDefault(e => e.tag.Equals(tag));
            return enricher ?? DefaultEnricher;
        }
    }
}