using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace SuspendProcess
{
    class Program
    {
        // Windows API imports with correct return types
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hThread);

        [Flags]
        private enum ThreadAccess : uint
        {
            THREAD_SUSPEND_RESUME = 0x0002
        }

        private static readonly Random Random = new Random();
        private static readonly string[] Messages = {
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

        static void Main()
        {
            string[] targetProcesses = { "RDR2", "GTA5", "GTA5_Enhanced" };
            const int suspendDurationMs = 12000; // 12 seconds

            try
            {
                ProcessSuspender suspender = new ProcessSuspender();
                suspender.ManageProcesses(targetProcesses, suspendDurationMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }

        private class ProcessSuspender
        {
            public void ManageProcesses(IEnumerable<string> processNames, int suspendDurationMs)
            {
                foreach (string processName in processNames)
                {
                    Process process = GetProcessByName(processName);
                    if (process == null)
                    {
                        Console.WriteLine($"Process '{processName}' not found.");
                        continue;
                    }

                    try
                    {
                        SuspendAndResume(process, suspendDurationMs);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error handling '{processName}': {ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }

            private void SuspendAndResume(Process process, int durationMs)
            {
                Suspend(process);
                Console.WriteLine($"Process '{process.ProcessName}' suspended for {durationMs / 1000} seconds.");

                DisplayRandomMessage();
                Thread.Sleep(durationMs);

                Resume(process);
                Console.WriteLine($"Process '{process.ProcessName}' resumed.");
            }

            private Process GetProcessByName(string processName)
            {
                return Process.GetProcessesByName(processName).FirstOrDefault();
            }

            private void Suspend(Process process)
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    ModifyThread(thread.Id, SuspendThread);
                }
            }

            private void Resume(Process process)
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    ModifyThread(thread.Id, ResumeThread);
                }
            }

            private void ModifyThread(int threadId, Func<IntPtr, uint> threadAction)
            {
                IntPtr hThread = OpenThread(ThreadAccess.THREAD_SUSPEND_RESUME, false, (uint)threadId);
                if (hThread == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Failed to open thread {threadId}. Error code: {error}");
                    return;
                }

                try
                {
                    uint result = threadAction(hThread);
                    if (result == 0xFFFFFFFF) // -1 indicates an error
                    {
                        int error = Marshal.GetLastWin32Error();
                        Console.WriteLine($"Thread operation failed. Error code: {error}");
                    }
                }
                finally
                {
                    CloseHandle(hThread);
                }
            }

            private void DisplayRandomMessage()
            {
                Console.WriteLine(Messages[Random.Next(Messages.Length)]);
            }
        }
    }
}