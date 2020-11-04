using System;
using System.IO;
using static CilDllEditor.Common;
using static System.StringComparison;

namespace CilDllEditor
{
    static class AutoPatchMode
    {
        public static void Run(string data)
        {
            pauseAfterError = true;
            Console.WriteLine("Starting auto-patch mode.");
            var args = Parse(data);
            Disassemble(args.dll);

            //Replace search term with replacement
            string text = File.ReadAllText(disassembledDllName);
            text = text.Replace(args.searchTerm, args.searchTermEnd_replaceWith);
            File.WriteAllText(disassembledDllName, text);

            Backup(args.dll);
            Assemble(args.dll, args.key);
        }

        static AutoPatchModeData Parse(string data)
        {
            //Ensure that linebreaks are consistently \n
            data = data.Replace("\r\n", "\r").Replace('\r', '\n').TrimStart();
            using var reader = new StringReader(data);
            var apmd = new AutoPatchModeData();

            //Parsing data header
            var line = reader.ReadLine();
            AssertTrue(() => line.Equals("CilDllEditorAutoPatchData_Begin"), "Data header must be exactly CilDllEditorAutoPatchData_Begin");

            //Parsing dll file path
            line = reader.ReadLine();
            AssertTrue(() => line.Equals("dll"), "First term must be 'dll'.");
            line = reader.ReadLine();
            AssertTrue(() => line.EndsWith(".dll"), "The line after 'dll' must be an existing dll file.");
            AssertTrue(() => File.Exists(line), "File does not exists: " + line);
            apmd.dll = line;

            //Parsing key file path (optional)
            line = reader.ReadLine();
            AssertTrue(() => line.Equals("key") || line.Equals("searchTerm"), "Second term must be 'key' or 'searchTerm'");
            if (line.Equals("key"))
            {
                line = reader.ReadLine();
                AssertTrue(() => line.EndsWith(".snk"), "The line after 'key' must be an existing snk file.");
                AssertTrue(() => File.Exists(line), "File does not exists: " + line);
                apmd.key = line;
                line = reader.ReadLine();
                AssertTrue(() => line.Equals("searchTerm"), "Third term must be 'searchTerm'");
            }

            //Parsing searchTerm
            line = reader.ReadLine();
            for(var i = 0; (i < 1000) && line != null && line != "searchTermEnd_replaceWith"; i++)
            {
                apmd.searchTerm += "\r\n" + line;
                line = reader.ReadLine();
            }
            AssertTrue(() => apmd.searchTerm.Trim().Length > 0, "Invalid search term: zero length term detected.");
            AssertTrue(() => line.Equals("searchTermEnd_replaceWith"), "Invalid search term: the end of the term did not found. Make sure that there is a line that is exactly 'searchTermEnd_replaceWith'.");

            //Parsing replacement
            apmd.searchTermEnd_replaceWith = reader.ReadToEnd();

            //Print out parsed data
            Console.WriteLine("Parsed data:");
            Console.WriteLine("dll: " + apmd.dll);
            Console.WriteLine("key: " + apmd.key);
            Console.WriteLine("search term:\n" + apmd.searchTerm);
            Console.WriteLine("replacement:\n" + apmd.searchTermEnd_replaceWith);

            return apmd;
        }
    }

    class AutoPatchModeData
    {
        public string dll;
        public string key;
        public string searchTerm = "";
        public string searchTermEnd_replaceWith;
    }
}
