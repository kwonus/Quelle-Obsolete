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
        public const string HEADING = "heading";
        public const string RECORD = "record";
        public const string FORMAT = "format";

        private static List<string> formats = new List<string>() { "text", "html", "md" };
        public string heading
        {
            get
            {
                return this.map.ContainsKey(HEADING) ? this.map[HEADING] : null;
            }
            set
            {
                bool remove = (value == null) && this.map.ContainsKey(HEADING);
                if (remove)
                {
                    this.map.Remove(HEADING);
                    this.Update();
                }
                else if (value != null)
                {
                    bool update = (this.map.ContainsKey(HEADING) && (this.map[HEADING] != value)) || !this.map.ContainsKey(HEADING);
                    if (update)
                    {
                        this.map[HEADING] = value;
                        this.Update();
                    }
                }
            }
        }
        public string record
        {
            get
            {
                return this.map.ContainsKey(RECORD) ? this.map[RECORD] : null;
            }
            set
            {
                bool remove = (value == null) && this.map.ContainsKey(RECORD);
                if (remove)
                {
                    this.map.Remove(RECORD);
                    this.Update();
                }
                else if (value != null)
                {
                    bool update = (this.map.ContainsKey(RECORD) && (this.map[RECORD] != value)) || !this.map.ContainsKey(RECORD);
                    if (update)
                    {
                        this.map[RECORD] = value;
                        this.Update();
                    }
                }
            }
        }
        public string format
        {
            get
            {
                string value = this.map.ContainsKey(FORMAT) ? this.map[RECORD] : "html";
                if (value == null || !formats.Contains(value))
                    value = "html"; // default
                return value;
            }
            set
            {
                bool remove = (value == null) && this.map.ContainsKey(RECORD);
                if (remove)
                {
                    this.map.Remove(RECORD);
                    this.Update();
                }
                else if (value != null)
                {
                    bool update = (this.map.ContainsKey(RECORD) && (this.map[RECORD] != value)) || !this.map.ContainsKey(RECORD);
                    if (update)
                    {
                        this.map[RECORD] = value;
                        this.Update();
                    }
                }
            }
        }
        public CTLDisplay(string file): base(file)
        {
            ;
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
        public override bool Update(string key, string value)
        {
            if (key != null)
            {
                switch (key)
                {
                    case HEADING:   this.heading = value; return true;
                    case RECORD:    this.record  = value; return true;
                    case FORMAT:    this.format  = value; return true;
                }
            }
            return false;
        }
    }
}
