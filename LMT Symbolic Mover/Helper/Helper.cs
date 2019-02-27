using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPFFolderBrowser;

namespace LMT_Symbolic_Mover
{
    public static class Helper
    {
        /// <summary>
        /// Lay danh sach cac thu muc con
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="motherFolder"></param>
        /// <returns></returns>
        public static List<FolderInfo> OpenFolder(this WindowsViewModel vm, ref string motherFolder)
        {
            List<FolderInfo> listFolder = new List<FolderInfo>();
            WPFFolderBrowserDialog dlg = new WPFFolderBrowserDialog((string)Application.Current.FindResource("SelectFolder"));
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                motherFolder = dlg.FileName;
                var directories = Directory.GetDirectories(dlg.FileName);
                foreach (var item in directories)
                {
                    var name = new DirectoryInfo(item).Name;
                    var path = item;
                    listFolder.Add(new FolderInfo() { FolderName = name, FolderPath = path });
                }
            }
            return listFolder;
        }

        /// <summary>
        /// Chon thu muc se chuyen den
        /// </summary>
        /// <returns></returns>
        public static string OpenDistiFolder(this WindowsViewModel vm)
        {
            WPFFolderBrowserDialog dlg = new WPFFolderBrowserDialog((string)Application.Current.FindResource("SelectFolder"));
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            else return String.Empty;
        }

        /// <summary>
        /// Lay ra tong so luong cac file can chuyen
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static int GetNumberOfFiles(this WindowsViewModel vm, List<string> sources)
        {
            int number = 0;
            foreach (var item in sources)
            {
                number += Directory.GetFiles(item, "*", SearchOption.AllDirectories).Length;
            }

            return number;
        }


        /// <summary>
        /// Kiem tra xem co du dieu kien de chuyen qua thu muc des hay khong
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="sources"></param>
        /// <param name="des"></param>
        /// <returns></returns>
        public static bool CheckFolder(this WindowsViewModel vm, List<string> sources, string des, ref string errors)
        {
            errors = "";
            var result = true;
            string[] blacklist = { @"C:\Windows", @"C:\Windows\System32", @"C:\Windows\Config", @"C:\ProgramData" };
            
            if (sources.Count<1)
            {
                result = false;
                errors += (string)Application.Current.FindResource("NoSources");
                return result;
            }

            if (!Directory.Exists(des))
            {
                result = false;
                errors += (string)Application.Current.FindResource("NoDes");
                return result;
            }

            var listNameSubFolderInSourceFolder = new List<string>();
            sources.ForEach(x => listNameSubFolderInSourceFolder.Add(new DirectoryInfo(x).Name));

            //Kiem tra xem co trung ten hay khong
            foreach (var s in listNameSubFolderInSourceFolder)
            {
                if (Directory.Exists(des + "\\" + s))
                {
                    result = false;
                    errors += (string)Application.Current.FindResource("SameName");
                    return result;
                }
            }

            //Kiem tra xem co nam trong blacklist hay khong
            foreach (var item in sources)
            {
                if (blacklist.Contains(item))
                {
                    result = false;
                    errors += (string)Application.Current.FindResource("BlackList");
                    return result;
                }
            }

            long size = 0;
            foreach (var item in sources)
            {
                List<string> files = Directory.GetFiles(item, "*", SearchOption.AllDirectories).ToList();
                files.ForEach(x =>
                {
                    size += new FileInfo(x).Length;
                });
            }
            DriveInfo driveInfo=new DriveInfo(Path.GetPathRoot(des));
            if (driveInfo.AvailableFreeSpace < size)
            {
                result = false;
                errors +=
                    (string)Application.Current.FindResource("NotEnoughSpace") +$" {size / 1000000} MB trống";
            }
            return result;
        }

    }
}
