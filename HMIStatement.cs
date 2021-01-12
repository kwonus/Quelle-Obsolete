using System;
using System.Collections.Generic;
using System.Text;

namespace ClarityHMI
{
    public class HMIStatement
    {
        private HMICommand command;
        public void Notify(string mode, string message)
        {
            if (this.command != null)
                this.command.Notify(mode, message);
        }
		public UInt16 span { get; private set; }
		public string statement { get; private set; }
		public String[] rawSegments { get; private set; }  // right-side: each segment has a POLARITY
        private char[] polarities;
        private Dictionary<string, HMISegment> segmentation;
        public Dictionary<UInt64, HMISegment> segments { get; private set; }

        private static string[] plus = new string[] { "[+]", "(+)", " + ", "\t+ ", " +\t" };
        private static string[] minus = new string[] { "[-]", "(-)", " - ", "\t- ", " -\t" };
        private static string[] delimiter = null;
        ///xxx
        public HMIStatement(HMICommand command, UInt16 span, string statement)
        {
            this.command = command;
			this.span = 0;
			this.statement = statement.Trim();

            if (delimiter == null)
            {
                delimiter = new string[plus.Length + minus.Length];

                for (int i = 0; i < plus.Length; i++)
                    delimiter[i] = plus[i];
                for (int i = 0; i < minus.Length; i++)
                    delimiter[plus.Length + i] = plus[i];
            }
            this.span = span;
            this.statement = statement.Trim();
            string normalized = this.statement;
            if (normalized.StartsWith('-'))
                normalized = minus[0] + statement.Substring(1);
            else if (normalized.StartsWith('+'))
                normalized = plus[0] + statement.Substring(1);
            else
                normalized = plus[0] + statement;

            var rawSegments = normalized.Split(delimiter, StringSplitOptions.None);
            this.rawSegments = new string[rawSegments.Length-1];
            Array.Copy(rawSegments, 1, this.rawSegments, 0, this.rawSegments.Length);
            this.polarities = new char[this.rawSegments.Length];

            this.segmentation = new Dictionary<string, HMISegment>();
            this.segments = new Dictionary<ulong, HMISegment>();

            for (UInt32 i = 0; i < this.rawSegments.Length; i++)
            {
                int next = 3 + this.rawSegments[i].Length;
                this.polarities[i] = normalized[1];
                normalized = normalized.Substring(next);

                this.rawSegments[i] = this.rawSegments[i].Trim();
                string key = "[" + this.polarities[i] + "] " + this.rawSegments[i].ToLower();
                var current = new HMISegment(this, i + 1, span, polarities[i], this.rawSegments[i]);
                this.segmentation.Add(key, current);
                this.segments.Add(current.sequence, current);

                if (command.errors != null)
                    break;
            }

            // Delete redundant [+] and [-] polarities
            foreach (string key in this.segmentation.Keys)
            {
                if (key[1] == '-')
                {
                    string cancel = "[+]" + key.Substring(3);
                    if (this.segmentation.ContainsKey(cancel))
                        this.segments.Remove(this.segmentation[cancel].sequence);
                }
            }
        }
	}
}
