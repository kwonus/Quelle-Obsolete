using System;
using System.IO;
using System.Net;

namespace QuelleHMI
{
    public class QWebClient
    {
        string site;

        public QWebClient(string site)
        {
            this.site = site != null ? site : "127.0.0.1";
            if (this.site.EndsWith("/"))
                this.site = this.site.Substring(0, site.Length-1);
        }

        public string Get(string endpoint, UInt16 maxResponseLength = 1024)
        {
            string url = site + (endpoint != null ? endpoint.StartsWith("/") ? endpoint : "/" + endpoint : "");
            Console.WriteLine("Getting " + url);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

            webRequest.Method = "GET";
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            byte[] result = new byte[maxResponseLength];
            int idx = 0;
            using (Stream responseStream = webResponse.GetResponseStream())
            {
                for (int cnt = responseStream.Read(result, 0, result.Length); cnt > 0; cnt = responseStream.Read(result, idx, result.Length-idx))
                {
                    idx += cnt;

                     Console.WriteLine("Read " + cnt.ToString() + " bytes");

                   if (idx >= maxResponseLength)
                        break;
                }
                //var response = MessagePackSerializer.Deserialize<FooResponse>(buffer);
            }
            Console.WriteLine("Status: " + webResponse.StatusDescription);
            Console.WriteLine("Done.");

            return System.Text.Encoding.Default.GetString(result);
        }
        public (byte[] data, int length) Post(string endpoint, byte[] payload, string mimetype, UInt16 maxResponseLength = 1024)
        {
            string url = site + (endpoint != null ? endpoint.StartsWith("/") ? endpoint : "/" + endpoint : "");
            Console.WriteLine("Post to " + url);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

            webRequest.Method = "POST";
            webRequest.ContentType = mimetype;
            webRequest.ContentLength = payload.Length;
            using (Stream postStream = webRequest.GetRequestStream())
            {
                // https://github.com/msgpack/msgpack-cli
                // Pack msg to stream.
                // serializer.Pack(postStream, msg);

                // Send the data.
                postStream.Write(payload, 0, payload.Length);
                postStream.Close();
            }
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            byte[] response = new byte[maxResponseLength];
            int idx = 0;
            using (Stream responseStream = webResponse.GetResponseStream())
            {
                for (int cnt = responseStream.Read(response, 0, response.Length); cnt > 0; cnt = responseStream.Read(response, idx, response.Length-idx))
                {
                    idx += cnt;

                    Console.WriteLine("Read " + cnt.ToString() + " bytes");

                    if (idx >= maxResponseLength)
                        break;
                }
                //var response = MessagePackSerializer.Deserialize<FooResponse>(buffer);
            }
            Console.WriteLine("Status: " + webResponse.StatusDescription);
            Console.WriteLine("Done.");

            return (response, idx);
        }
    }
}
