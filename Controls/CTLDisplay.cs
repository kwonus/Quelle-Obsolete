using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Definitions
{
    public interface IQuelleDisplayControls
    {
        string heading
        {
            get;
        }
        string record
        {
            get;
        }
        string format
        {
            get;
        }
    }
    public class CTLDisplay : QuelleControlConfig, IQuelleDisplayControls
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
        public CTLDisplay(string file): base(file)
        {
            if (!this.map.ContainsKey("heading"))
                this.map.Add("heading", this.heading);
            if (!this.map.ContainsKey("record"))
                this.map.Add("record", this.heading);
            if (!this.map.ContainsKey("format"))
                this.map.Add("format", this.heading);
        }
        private CTLDisplay(QuelleControlConfig source) : base(source)    // Copy constructor
        {
            ;
        }
        public IQuelleDisplayControls CreateCopy
        {
            get
            {
                return new CTLDisplay(this);
            }
        }
        public void Update(string heading, string record, string format)
        {
            this.heading = heading;
            this.record  = record;
            this.format  = format;
        }
    }
}
