//C#

// DOCS: https://github.com/broker0/path_server/tree/main
// http://127.0.0.1:3000/ui/  <= WEB UI for testing
// http://127.0.0.1/api/ <= APIs

using System;
using System.Runtime.InteropServices;

//#forcedebug

namespace RazorEnhanced
{
    static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    public class PathFindingServer_Dynamic_Load
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void start_path_server();

        public void Run()
        {

            IntPtr pDll = NativeMethods.LoadLibrary(@"path_server_lib.dll");
            IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "start_path_server");
            //oh dear, error handling here
            //if(pAddressOfFunctionToCall == IntPtr.Zero)

            start_path_server start_server = (start_path_server)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall,typeof(start_path_server));

            start_server();

            while (true) {
                Misc.SendMessage("Pathfinding Server is running");
                Misc.Pause(5000);
            }
        }

    }
}
