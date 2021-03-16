using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Controls
{
    public class CTLSystem: QuelleControlConfig
    {
        public const string delaultHost = "http://127.0.0.1/";
        public const uint maxIndentation = 10;
        public const uint minIndentation = 0;
        public const uint defaultIndentation = 0;
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
        public uint indentation
        {
            // 0 means tab ... zero is default
            get
            {
                var info = QuelleControlConfig.Configuration["indentation"];
                string value = this.map.ContainsKey("indentation") ? this.map["indentation"] : null;
                if (value == null)
                    return 0;  // default: zero means: use tabs

                uint val = uint.Parse(value);

                return val;
            }
            set
            {
                this.map["indentation"] = value.ToString();
            }
        }
   
        public CTLSystem(string file) : base(file)
        {
            ;
        }
    }
}
