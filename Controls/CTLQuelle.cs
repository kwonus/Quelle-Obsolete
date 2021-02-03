using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Controls
{
    class CTLQuelle: QuelleControlConfig
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
