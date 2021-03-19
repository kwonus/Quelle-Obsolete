using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Definitions
{
    public class CTLSystem: QuelleControlConfig
    {
        public const string delaultHost = "http://127.0.0.1/";
        public const uint maxIndentation = 10;
        public const uint minIndentation = 0;
        public const uint defaultIndentation = 0;
        public const string INDENTATION = "indentation";
        public readonly List<string> CONTROLS = new List<string>() { INDENTATION };

        public uint? indentation
        {
            // 0 means tab ... zero is default
            get
            {
                var info = QuelleControlConfig.Configuration[INDENTATION];
                string value = this.map.ContainsKey(INDENTATION) ? this.map[INDENTATION] : null;
                if (value == null)
                    return 0;  // default: zero means: use tabs

                uint val = uint.Parse(value);

                return val;
            }
            set
            {
                bool remove = (value == null) && this.map.ContainsKey(INDENTATION);
                if (remove)
                {
                    this.map.Remove(INDENTATION);
                    this.Update();
                }
                else if (value != null)
                {
                    bool update = (this.map.ContainsKey(INDENTATION) && (this.map[INDENTATION] != value.ToString())) || !this.map.ContainsKey(INDENTATION);
                    if (update)
                    {
                        this.map[INDENTATION] = value.ToString();
                        this.Update();
                    }
                }
            }
        }
   
        public CTLSystem(string file) : base(file)
        {
            ;
        }
        public override bool Update(string key, string value)
        {
            try
            {
                if (key != null)
                {
                    switch (key)
                    {
                        case INDENTATION:   this.indentation = value != null ? uint.Parse(value) : null;
                                            return true;
                    }
                }
            }
            catch
            {
                ;
            }
            return false;
        }
    }
}
