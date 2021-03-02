using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;

namespace QuelleHMI.Session
{
    public class SessionKey
    {
        public UInt64 TimeStamp;
        public UInt64 Bucket;   // Assuming no collisions, bucket-zero is normally adeequate

        public SessionKey()
        {
            this.TimeStamp = 0;
            this.Bucket = 0;
        }
        public SessionKey(UInt64 time, UInt64 bucket)
        {
            this.TimeStamp = time;
            this.Bucket = bucket;
        }
        public Guid AsGuid
        {
            get
            {
                UInt64[] source = { this.Bucket, this.TimeStamp };

                //Assignments

                byte[] decoded = new byte[source.Length * sizeof(UInt64)];
                Buffer.BlockCopy(source, 0, decoded, 0, decoded.Length);
                return new Guid(decoded);
            }
            set
            {
                UInt64[] target = new ulong[2];

                var bytes = value.ToByteArray();
                Buffer.BlockCopy(bytes, 0, target, 0, target.Length * sizeof(UInt64));
            }
        }
    }
    public class CallManager
    {
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static Int64 Write(UInt64 session, UInt64 bucket, UInt32 size, IntPtr unmanaged)
        {
            var sessionkey = new SessionKey(session, bucket).AsGuid;
            if (IPC.ContainsKey(sessionkey))
            {
                byte[] managed = new byte[(int) size];
                Marshal.Copy(unmanaged, managed, 0, (int) size);
                IPC[sessionkey] = managed;
                return size;
            }
            return -1;
        }
        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern UInt64 search(UInt64 session, UInt64 bucket, IntPtr request, delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer);

        private Dictionary<UInt64, UInt64> sessions;
        private static Dictionary<Guid, byte[]> IPC;

        public CallManager()
        {
            this.sessions = new Dictionary<UInt64, UInt64>();
            CallManager.IPC = new Dictionary<Guid, byte[]>();
        }

        public SessionKey CreateSession(UInt64 timestamp)   // DotNet still doesn't have UInt128
        {
            if (this.sessions.ContainsKey(timestamp))
            {
                var bucket = ++ this.sessions[timestamp];
                return new SessionKey(timestamp, bucket);
            }
            else
            {
                this.sessions.Add(timestamp, 0);
                return new SessionKey(timestamp, 0);
            }
        }
        public unsafe IQuelleSearchResult Search(SessionKey session, IQuelleSearchRequest request)
        {
            var csharp_mem = new byte[0];
            byte* cpp_mem = (byte*)0;

            delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer = &Write;
            UInt64 result = search(session.TimeStamp, session.Bucket, (IntPtr)cpp_mem, writer);

            return null;
        }
        public IQuelleFetchResult Fetch(IQuelleFetchRequest request)
        {
            return null;
        }
        public IQuelleStatusResult Status(IQuelleStatusRequest request)
        {
            return null;
        }
        public IQuellePageResult Page(IQuellePageRequest request)
        {
            return null;
        }
    }
}
