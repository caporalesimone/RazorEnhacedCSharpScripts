//C#

// DOCS: https://github.com/broker0/path_server/tree/main
// http://127.0.0.1:3000/ui/  <= WEB UI for testing
// http://127.0.0.1/api/ <= APIs

using System.Runtime.InteropServices;

//#forcedebug

namespace RazorEnhanced
{
    public class PathFindingServer
    {
        // DLL should be in the RE Path
        [DllImport("path_server_lib.dll")]
        private static extern void start_path_server();

        [DllImport("path_server_lib.dll")]
        private static extern void stop_path_server();


        public void Run()
        {
            start_path_server();
            while (true) {
                Misc.SendMessage("Pathfinding Server is running");
                Misc.Pause(5000);
            }
          }

        public void Dispose()
        {
            stop_path_server();
        }
    }
}
