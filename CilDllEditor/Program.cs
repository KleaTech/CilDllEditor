using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CilDllEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                ManualMode.Run(args);
            }
            else
            {
                var autoPatchData = GetAutoPathModeDataOrNull();
                if (autoPatchData != null) AutoPatchMode.Run(autoPatchData);
                else ManualMode.Run(args);
            }

            Console.WriteLine("The dll is patched. Done.");
        }

        static string GetAutoPathModeDataOrNull()
        {
            using var self = File.OpenRead(Process.GetCurrentProcess().MainModule.FileName);
            self.Seek(-10000, SeekOrigin.End);
            var buffer = new Span<byte>(new byte[10000]);
            self.Read(buffer);
            var searchTerm = Encoding.Default.GetBytes("CilDllEditorAutoPatchData_Begin");
            var index = buffer.IndexOf(searchTerm);
            if (index > 0)
            {
                var slice = buffer.Slice(index);
                var charray = new char[10000-index];
                for (int i = 0; i < slice.Length; i++) charray[i] = (char)slice[i];
                var data = new string(charray);
                return data;
            }
            return null;
        }
    }
}
