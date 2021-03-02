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
        // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.dllimportattribute?view=net-5.0
        // https://github.com/dotnet/csharplang/blob/master/proposals/csharp-9.0/function-pointers.md
        // https://devblogs.microsoft.com/dotnet/improvements-in-native-code-interop-in-net-5-0/
        //
        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern UInt64 search(UInt64 session, UInt64 bucket, IntPtr request, delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer);
        
        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern UInt64 fetch(UInt64 session, UInt64 bucket, IntPtr request, delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer);

        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern UInt64 status(UInt64 session, UInt64 bucket, IntPtr request, delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer);
        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern UInt64 page(UInt64 session, UInt64 bucket, IntPtr request, delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer);

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
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = MessageAsBlob(request);
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer = &Write;
            UInt64 result = search(session.TimeStamp, session.Bucket, (IntPtr)cblob, writer);

            return SearchResponseFromBlob(session);
        }
        public unsafe IQuelleFetchResult Fetch(SessionKey session, IQuelleFetchRequest request)
        {
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = MessageAsBlob(request);
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer = &Write;
            UInt64 result = fetch(session.TimeStamp, session.Bucket, (IntPtr)cblob, writer);

            return FetchResponseFromBlob(session);
        }
        public unsafe IQuelleStatusResult Status(SessionKey session, IQuelleStatusRequest request)
        {
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = MessageAsBlob(request);
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer = &Write;
            UInt64 result = status(session.TimeStamp, session.Bucket, (IntPtr)cblob, writer);

            return StatusResponseFromBlob(session);
        }
        public unsafe IQuellePageResult Page(SessionKey session, IQuellePageRequest request)
        {
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = MessageAsBlob(request);
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            delegate* unmanaged[Cdecl]<UInt64, UInt64, UInt32, IntPtr, Int64> writer = &Write;
            UInt64 result = page(session.TimeStamp, session.Bucket, (IntPtr)cblob, writer);

            return PageResponseFromBlob(session);
        }
        private byte[] MessageAsBlob(IQuellePageRequest request)
        {
            byte[] blob = new byte[] { (byte)'h', (byte)'i' }; // msgpack encoding goes here
            return blob;
        }
        private IQuellePageResult PageResponseFromBlob(SessionKey session)
        {
            var sessionkey = session.AsGuid;

            if (IPC.ContainsKey(sessionkey))
            {
                byte[] message = IPC[sessionkey];
                IPC.Remove(sessionkey);

                // TODO: Read bytes from IPC object and convert to an object via msgpack
                return null;
            }
            return null;
        }
        private byte[] MessageAsBlob(IQuelleSearchRequest request)
        {
            byte[] blob = new byte[] { (byte)'h', (byte)'i' }; // msgpack encoding goes here
            return blob;
        }
        private IQuelleSearchResult SearchResponseFromBlob(SessionKey session)
        {
            var sessionkey = session.AsGuid;

            if (IPC.ContainsKey(sessionkey))
            {
                byte[] message = IPC[sessionkey];
                IPC.Remove(sessionkey);

                // TODO: Read bytes from IPC object and convert to an object via msgpack
                return null;
            }
            return null;
        }
        private byte[] MessageAsBlob(IQuelleFetchRequest request)
        {
            byte[] blob = new byte[] { (byte)'h', (byte)'i' }; // msgpack encoding goes here
            return blob;
        }
        private IQuelleFetchResult FetchResponseFromBlob(SessionKey session)
        {
            var sessionkey = session.AsGuid;

            if (IPC.ContainsKey(sessionkey))
            {
                byte[] message = IPC[sessionkey];
                IPC.Remove(sessionkey);

                // TODO: Read bytes from IPC object and convert to an object via msgpack
                return null;
            }
            return null;
        }
        private byte[] MessageAsBlob(IQuelleStatusRequest request)
        {
            byte[] blob = new byte[] { (byte)'h', (byte)'i' }; // msgpack encoding goes here
            return blob;
        }
        private IQuelleStatusResult StatusResponseFromBlob(SessionKey session)
        {
            var sessionkey = session.AsGuid;

            if (IPC.ContainsKey(sessionkey))
            {
                byte[] message = IPC[sessionkey];
                IPC.Remove(sessionkey);

                // TODO: Read bytes from IPC object and convert to an object via msgpack
                return null;
            }
            return null;
        }
    }
}
