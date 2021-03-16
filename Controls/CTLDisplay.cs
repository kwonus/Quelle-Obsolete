using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Controls
{
    public class CTLDisplay : QuelleControlConfig
    {
        private static List<string> formats = new List<string>() { "text", "html", "md" };
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
                string value = this.map.ContainsKey("format") ? this.map["format"] : "html";
                if (value == null || !formats.Contains(value))
                    value = "html"; // default
                return value;
            }
            set
            {
                if (value == null || !formats.Contains(value))
                {
                    this.map.Remove("format");
                    this.map["format"] = "html";
                }
                else if (this.map["format"] == null || this.map["format"] != value)
                {
                    this.map["format"] = value;
                    Update();
                }
            }
        }
        public string output
        {
            get
            {
                return this.map.ContainsKey("output") ? this.map["output"] : null;
            }
            set
            {
                if (value == null)
                    this.map.Remove("output");
                else
                    this.map["output"] = value;
            }
        }
        public CTLDisplay(string file): base(file)
        {
            ;
        }
    }
}
