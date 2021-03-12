using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Controls
{
    public interface IQuelleSearchControls
    {
        public string domain
        {
            get;
        }
        public int span
        {
            get;
        }
        public int exact
        {
            get;
        }
    }
    public class CTLSearch: QuelleControlConfig, IQuelleSearchControls
    {
        public string domain
        {
            get
            {
                return this.map.ContainsKey("domain") ? this.map["domain"] : null;
            }
            set
            {
                if (value == null)
                    this.map.Remove("domain");
                else
                    this.map["domain"] = value;
            }
        }
        public int span
        {
            get
            {
                var info = HMISession.StandardConfig_SEARCH["span"];
                string value = this.map.ContainsKey("span") ? this.map["span"] : null;
                if (value == null)
                    value = info.Default;

                int val = int.Parse(value);

                if (info.MinMax != null && (val < info.MinMax[0] || val > info.MinMax[1]))
                    val = int.Parse(info.Default);

                return val;
            }
            set
            {
                this.map["span"] = value.ToString();
            }
        }
        public int exact
        {
            get
            {
                string value = this.map.ContainsKey("exact") ? this.map["exact"] : null;
                if (value == null)
                    value = HMISession.StandardConfig_SEARCH["exact"].Default;

                switch (value.ToLower())
                {
                    case "1":    return 1;
                    case "true": return 1;
                    default:     return 0;
                }
            }
            set
            {
                this.map["exact"] = value == 1 ? "1" : "0";
            }
        }
        private static Dictionary<string, ControlInfo> StandardConfig_QUELLE = new Dictionary<string, ControlInfo>()
        {

        };
        public CTLSearch(string file) : base(file)
        {
            ;
        }
    }
}
