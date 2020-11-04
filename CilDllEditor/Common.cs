using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace CilDllEditor
{
    static class Common
    {
        public static bool pauseAfterError = false;
        public const string disassembledDllName = "disassembledDll.il";

        public static void Disassemble(string dll)
        {
            using var p = StartAndWaitProcess("ildasm", dll + " /out=" + disassembledDllName);
            AssertTrue(() => { return p.ExitCode == 0; }, "ildasm finished with errorcode " + p.ExitCode);
        }

        public static void Backup(string dll)
        {
            var backupName = dll + "_backup" + DateTime.Now.Ticks;
            File.Copy(dll, backupName);
            Console.WriteLine("Created backup: " + backupName);
        }

        public static void Assemble(string dll, string key = null)
        {
            key = key == null ? "" : $" /key:{key}";
            using var p = StartAndWaitProcess("ilasm", $"{disassembledDllName} /dll{key} /output:{dll}");
            AssertTrue(() => { return p.ExitCode == 0; }, "ilasm finished with errorcode " + p.ExitCode);
        }

        public static void AssertTrue(Func<bool> func, string message)
        {
            if (!func())
            {
                Console.WriteLine(message + "\nUsage:\n\tPass a dll as the first parameter to this exe, and an (optional) snk signature file as the second.\n\tRun this from devenv. Follow the instructions as you go on.");
                if(pauseAfterError) Console.ReadKey();
                Environment.Exit(-1);
            }
        }

        public static Process StartAndWaitProcess(string name, string args = null)
        {
            try
            {
                var p = new Process();
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.FileName = name;
                if (args != null) p.StartInfo.Arguments = args;
                p.Start();
                p.WaitForExit();
                return p;
            }
            catch (Win32Exception ex)
            {
                AssertTrue(() => false, $"Error while running {name} {args}, maybe it does not exists: {ex.Message}");
                return null;
            }
        }
    }
}
