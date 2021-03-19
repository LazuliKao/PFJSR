using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PFJSRBDSAPI
{
    public static class Ex
    {
        public static bool HasConsole
        {
            get
            {
                try
                {
                    _ = Console.WindowHeight;
                    return true;
                }
                catch { return false; }
            }
        }

        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        //private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        //private const uint ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002;
        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
        public static void FixConsole()
        {
            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (iStdOut == IntPtr.Zero) { return; }
            if (GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                if (ENABLE_VIRTUAL_TERMINAL_PROCESSING != (outConsoleMode & ENABLE_VIRTUAL_TERMINAL_PROCESSING))
                {
                    outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                    if (SetConsoleMode(iStdOut, outConsoleMode))
                    {
                        Console.WriteLine("[PF+] Console ENABLE_VIRTUAL_TERMINAL_PROCESSING enabled.");
                    }
                }
            }
        }
    }
}
