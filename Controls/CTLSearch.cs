using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Definitions
{
    public interface IQuelleSearchControls
    {
        string domain
        {
            get;
        }
        uint? span
        {
            get;
        }
        bool? exact
        {
            get;
        }
        string host
        {
            get;
        }
    }
    public class CTLSearch: QuelleControlConfig, IQuelleSearchControls
    {
        public const uint maxSpan = 1000;
        public const uint minSpan = 0;
        public const uint defaultSpan = 0;
        public const string defaultDomain = "Bible.AV";
        public const string defaultHost = "http://127.0.0.1:7878";

        public const bool defaultExact = false;

        public const string HOST = "host";
        public const string DOMAIN = "domain";
        public const string SPAN = "span";
        public const string EXACT = "exact";
        public readonly List<string> CONTROLS = new List<string>() { HOST, DOMAIN, SPAN, EXACT };

        public string host
        {
            get
            {
                return this.map.ContainsKey(HOST) ? this.map[HOST] : null;
            }
            set
            {
                bool remove = (value == null) && this.map.ContainsKey(HOST);
                if (remove)
                {
                    this.map.Remove(HOST);
                    this.Update();
                }
                else if (value != null)
                {
                    bool update = (this.map.ContainsKey(HOST) && (this.map[HOST] != value)) || !this.map.ContainsKey(HOST);
                    if (update)
                    {
                        this.map[HOST] = value;
                        this.Update();
                    }
                }
            }
        }
        public string domain
        {
            get
            {
                return this.map.ContainsKey(DOMAIN) ? this.map[DOMAIN] : null;
            }
            set
            {
                bool remove = (value == null) && this.map.ContainsKey(DOMAIN);
                if (remove)
                {
                    this.map.Remove(DOMAIN);
                    this.Update();
                }
                else if (value != null)
                {
                    bool update = (this.map.ContainsKey(DOMAIN) && (this.map[DOMAIN] != value)) || !this.map.ContainsKey(DOMAIN);
                    if (update)
                    {
                        this.map[DOMAIN] = value;
                        this.Update();
                    }
                }
            }
        }
        public uint? span
        {
            get
            {
                string value = this.map.ContainsKey(SPAN) ? this.map[SPAN] : null;
                if (value == null)
                    return defaultSpan;   // default

                uint val = uint.Parse(value);
                return val >= minSpan && val <= maxSpan ? val : defaultSpan;
            }
            set
            {
                bool remove = (value == null) && this.map.ContainsKey(SPAN);
                if (remove)
                {
                    this.map.Remove(SPAN);
                    this.Update();
                }
                else if (value != null)
                {
                    bool update = (this.map.ContainsKey(SPAN) && (this.map[SPAN] != value.ToString())) || !this.map.ContainsKey(SPAN);
                    if (update)
                    {
                        this.map[SPAN] = value.ToString();
                        this.Update();
                    }
                }
            }
        }
        public bool? exact
        {
            get
            {
                string value = this.map.ContainsKey("exact") ? this.map["exact"] : null;
                if (value == null)
                    return defaultExact;

                switch (value.ToLower())
                {
                    case "1":
                    case "true":  return true;
                    case "0":
                    case "false": return false;
                    default:      return defaultExact;
                }
            }
            set
            {
                bool remove = (value == null) && this.map.ContainsKey(EXACT);
                if (remove)
                {
                    this.map.Remove(EXACT);
                    this.Update();
                }
                else if (value != null)
                {
                    string boolVal = value.Value ? "true" : "false";
                    bool update = (this.map.ContainsKey(EXACT) && (this.map[EXACT] != boolVal)) || !this.map.ContainsKey(EXACT);
                    if (update)
                    {
                        this.map[EXACT] = boolVal;
                        this.Update();
                    }
                }
            }
        }
        public CTLSearch(string file) : base(file)
        {
            ;
        }
        private CTLSearch(QuelleControlConfig source) : base(source)    // Copy constructor
        {
            ;
        }
        public IQuelleSearchControls CreateCopy
        {
            get
            {
                return new CTLSearch(this);
            }
        }
        public void Update(string host, string domain, uint span, bool exact)
        {
            this.host   = host;
            this.domain = domain;
            this.span   = span;
            this.exact  = exact;
        }
        public override bool Update(string key, string value)
        {
            if (key != null)
            {
                try
                {
                    switch (key)
                    {
                        case HOST:   this.host   = value;   return true;
                        case DOMAIN: this.domain = value;   return true;
                        case SPAN:   this.span   = value != null ? (uint?) uint.Parse(value) : (uint?) null; return true;
                        case EXACT:  this.exact  = value != null ? (bool?) (value == "true") : (bool?) null; return true;
                    }
                }
                catch (Exception ex)
                {
                    ;
                }
            }
            return false;
        }
    }
}
