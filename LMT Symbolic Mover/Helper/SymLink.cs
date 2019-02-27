using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LMT_Symbolic_Mover
{
    public static class SymLink
    {
        #region Symlink

        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(
            string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        public static bool MakeLink(this WindowsViewModel vm, string directory, string symlink)
        {
            return CreateSymbolicLink(symlink, directory, SymbolicLink.Directory);
        }


        #endregion

        #region private member

        private static bool success;

        #endregion


        
    }
}
