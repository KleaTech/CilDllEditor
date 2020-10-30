using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace CilDllEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            //Check preconditions
            AssertTrue(() => { return args.Length == 1 || args.Length == 2; }, "Expected one or two arguments.");
            AssertTrue(() => args[0].EndsWith(".dll"), "First parameter must be a dll.");
            AssertTrue(() => File.Exists(args[0]), "Dll does not exists");
            AssertTrue(() => { if(args.Length == 2) return args[1].EndsWith(".snk"); else return true; }, "Second parameter must be an snk if given.");

            //Disassamble
            using var p1 = StartAndWaitProcess("ildasm", args[0] + " /out=disassambledDll.il");
            AssertTrue(() => { return p1.ExitCode == 0; }, "ildasm finished with errorcode " + p1.ExitCode);

            //Edit
            Console.WriteLine("Edit the file, save and close. Waiting...");
            using var p2 = StartAndWaitProcess("cmd", $"/c start /WAIT notepad disassambledDll.il");

            //Backup
            var backupName = args[0] + "_backup" + DateTime.Now.Ticks;
            File.Copy(args[0], backupName);
            Console.WriteLine("Created backup: " + backupName);

            //Assamble
            var key = args.Length > 1 ? $" /key:{args[1]}" : "";
            using var p3 = StartAndWaitProcess("ilasm", $"disassambledDll.il /dll{key} /output:{args[0]}");
            AssertTrue(() => { return p3.ExitCode == 0; }, "ilasm finished with errorcode " + p3.ExitCode);

            Console.WriteLine("The dll is patched. Done.");
        }

        static void AssertTrue(Func<bool> func, string message)
        {
            if (!func())
            {
                Console.WriteLine(message + "\nUsage:\n\tPass a dll as the first parameter to this exe, and an (optional) snk signature file as the second.\n\tRun this from devenv. Follow the instructions as you go on.");
                Environment.Exit(-1);
            }
        }

        static Process StartAndWaitProcess(string name, string args = null)
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
