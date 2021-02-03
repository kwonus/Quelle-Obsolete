using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Controls
{
    class CTLDisplay : QuelleControlConfig
    {
        public string heading
        {
            get
            {
                return this.map.ContainsKey("heading") ? this.map["heading"] : null;
            }
            set
            {
                if (value == null)
                    this.map.Remove("heading");
                else
                    this.map["heading"] = value;
            }
        }
        public string record
        {
            get
            {
                return this.map.ContainsKey("record") ? this.map["record"] : null;
            }
            set
            {
                if (value == null)
                    this.map.Remove("record");
                else
                    this.map["record"] = value;
            }
        }
        public string format
        {
            get
            {
                string value = this.map.ContainsKey("format") ? this.map["format"] : null;
                if (value == null)
                    value = HMISession.StandardConfig_DISPLAY["format"].Default;
                return value;
            }
            set
            {
                if (value == null)
                    this.map.Remove("format");
                else
                    this.map["format"] = value;
            }
        }
        public CTLDisplay(string file): base(file)
        {
            ;
        }
    }
}
