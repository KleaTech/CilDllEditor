using System;
using System.IO;
using static CilDllEditor.Common;

namespace CilDllEditor
{
    static class ManualMode
    {
        public static void Run(string[] args)
        {
            //Check preconditions
            AssertTrue(() => { return args.Length == 1 || args.Length == 2; }, "Expected one or two arguments.");
            AssertTrue(() => args[0].EndsWith(".dll"), "First parameter must be a dll.");
            AssertTrue(() => File.Exists(args[0]), "Dll does not exists");
            AssertTrue(() => { if(args.Length == 2) return args[1].EndsWith(".snk"); else return true; }, "Second parameter must be an snk if given.");

            Disassemble(args[0]);

            //Edit
            Console.WriteLine("Edit the file, save and close. Waiting...");
            using var p = StartAndWaitProcess("cmd", $"/c start /WAIT notepad {disassembledDllName}");

            Backup(args[0]);
            Assemble(args[0], args.Length > 1 ? args[1] : null);
        }
    }
}
