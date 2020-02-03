namespace FixAnalyzer
{
    using System.Collections.Generic;
    using System.Linq;

    internal enum FixTagCompareState
    {
        Equal,
        Unequal,
        Missing1,
        Missing2,
        MissingAll
    };

    internal class FixComparer
    {
        private FixMessage fix1;
        private FixMessage fix2;

        public FixComparer(FixMessage fix1, FixMessage fix2)
        {
            this.fix1 = fix1;
            this.fix2 = fix2;
        }

        public FixComparer(string fix1, string fix2)
        {
            this.fix1 = new FixMessage(fix1);
            this.fix2 = new FixMessage(fix2);
        }

        public FixMessage Fix1
        {
            get { return this.fix1; }
        }

        public FixMessage Fix2
        {
            get { return this.fix2; }
        }

        public IEnumerable<FixTag> GetEqualTags()
        {
            foreach (FixTag tag in this.fix1.Tags)
            {
                FixTag tag2 = this.fix2[tag.Tag];

                if ((tag2 != null) && (tag2.Value == tag.Value))
                    yield return tag;
            }
        }

        public IEnumerable<FixTag> GetUnequalTags()
        {
            foreach (FixTag tag in this.fix1.Tags)
            {
                FixTag tag2 = this.fix2[tag.Tag];

                if ((tag2 != null) && (tag2.Value != tag.Value))
                    yield return tag;
            }
        }

        public IEnumerable<FixTag> GetMissingTagsIn2()
        {
            foreach (FixTag tag in this.fix1.Tags)
            {
                FixTag tag2 = this.fix2[tag.Tag];
                if (tag2 == null)
                    yield return tag;
            }
        }

        public IEnumerable<FixTag> GetMissingTagsIn1()
        {
            foreach (FixTag tag in this.fix2.Tags)
            {
                FixTag tag1 = this.fix1[tag.Tag];
                if (tag1 == null)
                    yield return tag;
            }
        }

        public IEnumerable<int> GetAllFixTagNumbers()
        {
            List<int> tags1 = this.fix1.Tags.Select(x => x.Tag).ToList();
            List<int> tags2 = this.fix2.Tags.Select(x => x.Tag).ToList();

            Dictionary<int, int> indexes = new Dictionary<int, int>();

            List<int> tags = new List<int>();

            for (int i = 0; i < tags1.Count; i++)
            {
                if (tags2.Contains(tags1[i]))
                {
                    int idx2 = tags2.IndexOf(tags1[i]);
                    indexes.Add(i, idx2);
                    tags2[idx2] = -1;
                }
            }

            for (int i = 0; i < tags1.Count; i++)
            {
                tags.Add(tags1[i]);
            }

            for (int i = 0; i < tags2.Count; i++)
            {
                if (tags2[i] > 0)
                    tags.Add(tags2[i]);
            }

            return tags;
        }

        public FixTagCompareState CompareTag(int tagNumber)
        {
            FixTag tag1 = this.fix1[tagNumber];
            FixTag tag2 = this.fix2[tagNumber];

            if ((tag1 == null) && (tag2 == null))
                return FixTagCompareState.MissingAll;

            if (tag1 == null)
                return FixTagCompareState.Missing1;

            if (tag2 == null)
                return FixTagCompareState.Missing2;

            return tag1.Value == tag2.Value ? FixTagCompareState.Equal : FixTagCompareState.Unequal;
        }
    }
}