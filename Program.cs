using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace SuspendProcess
{
    class Program
    {
        // Import Windows API functions
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern int CloseHandle(IntPtr hThread);

        [Flags]
        public enum ThreadAccess : uint
        {
            THREAD_SUSPEND_RESUME = 0x0002
        }

        static void Main()
        {
            try
            {
                // Rest of your code...

                // Find the RDR2 process
                Process rdr2Process = GetProcessByName("RDR2");
                if (rdr2Process != null)
                {
                    SuspendAndResumeProcess(rdr2Process);
                }
                else
                {
                    Console.WriteLine("Process 'RDR2' not found.");
                }

                // Find the GTA5 process
                Process gta5Process = GetProcessByName("GTA5");
                if (gta5Process != null)
                {
                    SuspendAndResumeProcess(gta5Process);
                }
                else
                {
                    Console.WriteLine("Process 'GTA5' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }


        private static void SuspendAndResumeProcess(Process process)
        {
            SuspendProcess(process);
            Console.WriteLine($"Process '{process.ProcessName}' suspended for 12 seconds.");

            // Display random messages while the user is waiting
            DisplayRandomMessages();

            // Add your testing logic here if needed

            Thread.Sleep(12000); // 12 seconds

            ResumeProcess(process);
            Console.WriteLine($"Process '{process.ProcessName}' resumed.");
        }


        private static void DisplayRandomMessages()
        {
            string[] messages = {
                "Hang tight, we're working on it!",
                "Just a moment, almost there!",
                "Time to grab a coffee! We'll be done shortly.",
                "Enjoy the silence... for now!",
                "The suspense is real, isn't it?",
                "In the world of processes, patience is a virtue.",
                "Counting down the seconds...",
                "This pause brought to you by the magic of debugging.",
                "Waiting patiently, like a well-behaved thread.",
                "Taking a moment to appreciate the beauty of code.",
                "Loading... just kidding!",
                "If only real life had a 'pause' button.",
                "Analyzing bits and bytes with a magnifying glass.",
                "Hold on, we're rearranging the electrons.",
                "The CPU is taking a breather, and so should you.",
                "Silence is golden, especially in the world of processes.",
                "When in doubt, pause and reflect.",
                "Making time for a quick power nap... just kidding!",
                "Your patience is truly commendable.",
                "Suspended in time, but not in progress."
            };

            Random random = new Random();

            // Display a single random message
            Console.WriteLine(messages[random.Next(messages.Length)]);
        }

        private static Process GetProcessByName(string processName)
        {
            return Process.GetProcessesByName(processName).FirstOrDefault();
        }

        private static void SuspendProcess(Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                IntPtr hThread = OpenThread(ThreadAccess.THREAD_SUSPEND_RESUME, false, (uint)thread.Id);
                if (hThread != IntPtr.Zero)
                {
                    SuspendThread(hThread);
                    CloseHandle(hThread);
                }
            }
        }

        private static void ResumeProcess(Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                IntPtr hThread = OpenThread(ThreadAccess.THREAD_SUSPEND_RESUME, false, (uint)thread.Id);
                if (hThread != IntPtr.Zero)
                {
                    ResumeThread(hThread);
                    CloseHandle(hThread);
                }
            }
        }
    }
}