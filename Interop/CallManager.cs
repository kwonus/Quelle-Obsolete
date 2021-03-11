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
        /*
        private static Dictionary<IntPtr, UInt16> MemorySize = new Dictionary<IntPtr, UInt16>();

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        private IntPtr QuelleSearchProvider;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static IntPtr Alloc(UInt16 size)
        {
            IntPtr mem = Marshal.AllocHGlobal((Int32)size);
            CallManager.MemorySize[mem] = size;
            return mem;
        }
        */
        /* THIS WOULD WORK IF WE WANTED TO CONSTRAIN OURSELFS TO STATIC LINKINMG.  USING LoadLibrary() ALLOWS USER TO CONFIGURE THE QuelleSearchProvider
        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern IntPtr search(IntPtr request, UInt16 size, delegate* unmanaged[Cdecl]<UInt16, IntPtr> alloc);
        
        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern IntPtr fetch(IntPtr request, UInt16 size, delegate* unmanaged[Cdecl]<UInt16, IntPtr> alloc);

        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern IntPtr status(IntPtr request, UInt16 size, delegate* unmanaged[Cdecl]<UInt16, IntPtr> alloc);
 
        [DllImport("QuelleSearchProvider.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern IntPtr page(IntPtr request, UInt16 size, delegate* unmanaged[Cdecl]<UInt16, IntPtr> alloc);
        */

        private Dictionary<UInt64, UInt64> sessions;

        public CallManager()
        {
            /*
            this.sessions = new Dictionary<UInt64, UInt64>();
            this.QuelleSearchProvider = CallManager.LoadLibrary(@"C:\Users\kevin\source\repos\Quelle-AVX\target\debug\QuelleSearchLib.dll");
            */
        }
        ~CallManager()
        {
            /*
            CallManager.FreeLibrary(this.QuelleSearchProvider);
            */
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
        /*
        public unsafe byte[] RequestReply(string function, byte* request, UInt16 size)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.dllimportattribute?view=net-5.0
            // https://github.com/dotnet/csharplang/blob/master/proposals/csharp-9.0/function-pointers.md
            // https://devblogs.microsoft.com/dotnet/improvements-in-native-code-interop-in-net-5-0/
            //
            IntPtr Function = CallManager.GetProcAddress(this.QuelleSearchProvider, function);
            delegate* unmanaged[Cdecl]<IntPtr, UInt16, delegate* unmanaged[Cdecl]<UInt16, IntPtr>, IntPtr> FUNCTION
                = (delegate* unmanaged[Cdecl]<IntPtr, UInt16, delegate * unmanaged[Cdecl]<UInt16, IntPtr>, IntPtr>) Function;

            delegate* unmanaged[Cdecl]<UInt16, IntPtr> alloc = &Alloc;
            IntPtr result = FUNCTION((IntPtr)request, size, alloc);

            if ( (result != IntPtr.Zero) && CallManager.MemorySize.ContainsKey(result))
            {
                UInt16 msize = CallManager.MemorySize[result];
                CallManager.MemorySize.Remove(result);
                byte[] managed = new byte[msize];
                Marshal.Copy(result, managed, 0, msize);
                Marshal.FreeHGlobal(result);
                return managed;
            }
            return Array.Empty<byte>();
        }
        public unsafe IQuelleSearchResult Search(IQuelleSearchRequest request)
        {
            string function = "search";
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = MessageAsBlob(request);
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            byte[] result = RequestReply(function, cblob, (UInt16)blob.Length);

            return SearchResponseFromBlob(result);
        }
        public unsafe IQuelleFetchResult Fetch(IQuelleFetchRequest request)
        {
            string function = "fetch";
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = MessageAsBlob(request);
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            byte[] result = RequestReply(function, cblob, (UInt16)blob.Length);

            return FetchResponseFromBlob(result);
        }
        public unsafe IQuelleStatusResult Status(SessionKey session, IQuelleStatusRequest request)
        {
            string function = "status";
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = MessageAsBlob(request);
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            byte[] result = RequestReply(function, cblob, (UInt16)blob.Length);

            return StatusResponseFromBlob(result);
        }
        public unsafe IQuellePageResult Page(SessionKey session, IQuellePageRequest request)
        {
            string function = "page";
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = MessageAsBlob(request);
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            byte[] result = RequestReply(function, cblob, (UInt16)blob.Length);

            return PageResponseFromBlob(result);
        }
        public unsafe void Test(SessionKey session)
        {
            string function = "status";
            // http://newapputil.blogspot.com/2015/09/pass-image-byte-from-c-to-c-and-vice.html
            // https://stackoverflow.com/questions/785226/practical-use-of-stackalloc-keyword/785264
            // 
            byte[] blob = new byte[] { (byte)'h', (byte)'i', (byte)' ', (byte)'C', (byte)'!', (byte)'\0' };
            byte* cblob = stackalloc byte[blob.Length];          // This must be done on the stack; it goes away on method return
            Marshal.Copy(blob, 0, (IntPtr)cblob, blob.Length);

            byte[] result = RequestReply(function, cblob, (UInt16)blob.Length);
            char[] str = new char[result.Length];
            for (int i = 0; i < str.Length; i++)
                str[i] = (char) result[i];

            Console.WriteLine(new string(str));
        }
        private byte[] MessageAsBlob(IQuellePageRequest request)
        {
            byte[] blob = new byte[] { (byte)'h', (byte)'i' }; // msgpack encoding goes here
            return blob;
        }
        private IQuellePageResult PageResponseFromBlob(byte[] blob)
        {
            return null;
        }
        private byte[] MessageAsBlob(IQuelleSearchRequest request)
        {
            byte[] blob = new byte[] { (byte)'h', (byte)'i' }; // msgpack encoding goes here
            return blob;
        }
        private IQuelleSearchResult SearchResponseFromBlob(byte[] blob)
        {
            return null;
        }
        private byte[] MessageAsBlob(IQuelleFetchRequest request)
        {
            byte[] blob = new byte[] { (byte)'h', (byte)'i' }; // msgpack encoding goes here
            return blob;
        }
        private IQuelleFetchResult FetchResponseFromBlob(byte[] blob)
        {
            return null;
        }
        private byte[] MessageAsBlob(IQuelleStatusRequest request)
        {
            byte[] blob = new byte[] { (byte)'h', (byte)'i' }; // msgpack encoding goes here
            return blob;
        }
        private IQuelleStatusResult StatusResponseFromBlob(byte[] blob)
        {
            return null;
        }
        */
    }
}
