using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Controls
{
    public class CTLQuelle: QuelleControlConfig
    {
        public string host
        {
            get
            {
                return this.map.ContainsKey("host") ? this.map["host"] : null;
            }
            set
            {
                if (value == null)
                    this.map.Remove("host");
                else
                    this.map["host"] = value;
            }
        }
        public int debug
        {
            get
            {
                string value = this.map.ContainsKey("debug") ? this.map["debug"] : null;
                if (value == null)
                    value = HMISession.StandardConfig_QUELLE["debug"].Default;
                
                switch (value.ToLower())
                {
                    case "1":    return 1;
                    case "true": return 1;
                    default:     return 0;
                }
            }
            set
            {
                this.map["debug"] = value == 1 ? "1" : "0";
            }
        }
        public int indentation
        {
            // 0 means tab ... zero is default
            get
            {
                var info = HMISession.StandardConfig_QUELLE["indentation"];
                string value = this.map.ContainsKey("indentation") ? this.map["indentation"] : null;
                if (value == null)
                    value = info.Default;

                int val = int.Parse(value);

                if (info.MinMax != null && (val < info.MinMax[0] || val > info.MinMax[1]))
                    val = int.Parse(info.Default);

                return val;
            }
            set
            {
                this.map["indentation"] = value.ToString();
            }
        }
        public string data
        {
            get
            {
                string value = this.map.ContainsKey("data") ? this.map["data"] : null;
                if (value == null)
                    value = HMISession.StandardConfig_QUELLE["data"].Default;
                return value;
            }
            set
            {
                if (value == null)
                    this.map.Remove("data");
                else
                    this.map["data"] = value;
            }
        }
    
        public CTLQuelle(string file) : base(file)
        {
            ;
        }
    }
}
