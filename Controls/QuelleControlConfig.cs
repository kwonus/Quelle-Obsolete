using System;
using System.Collections.Generic;
using System.IO;

namespace QuelleHMI.Controls
{
    public abstract class QuelleControlConfig
    {
        public QuelleControlConfig (string file)
        {
            this.conf = file;
            this.map = new Dictionary<string, string>();
            if (File.Exists(file))
                this.Read(this.conf);            
        }
        private string conf;
        protected Dictionary<String, String> map;

        protected (bool ok, string message) Read(string file)
        {
            (bool ok, string message) result = (false, null);

            this.map.Clear();
            using (StreamReader reader = new StreamReader(file))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var parts = line.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        parts[0] = parts[0].Trim();
                        parts[1] = parts[1].Trim();

                        if (parts[0].Length > 0 && parts[1].Length > 0)
                        {
                            this.map.Add(parts[0], parts[1]);
                        }
                    }
                }
            }
            result.ok = true;
            return result;
        }
        protected (bool ok, string message) Write(string file)
        {
            (bool ok, string message) result = (false, null);

            using (StreamWriter writer = new StreamWriter(file))
            {
                foreach (var entry in this.map)
                {
                    writer.Write(entry.Key);
                    writer.Write("\t= ");
                    writer.WriteLine(entry.Value);
                }
            }
            result.ok = true;
            return result;

        }
        public (bool ok, string message) Update()
        {
            return this.Write(this.conf);
        }
        public (bool ok, string message) Retreive()
        {
            return this.Read(this.conf);
        }
        public (bool ok, string message) Backup(string file)
        {
            return this.Write(file);
        }
        public (bool ok, string message) Restore(string file)
        {
            var result = this.Read(file);
            if (result.ok)
                result = this.Write(this.conf);
            return result;
        }
    }
}
