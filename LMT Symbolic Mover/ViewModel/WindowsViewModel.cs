using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;


namespace LMT_Symbolic_Mover
{
    public class WindowsViewModel : BaseViewModel
    {
        #region Public member

        public ObservableCollection<FolderInfo> ListViewFolderInfos { get; set; }

        public string MotherFolder { get; set; }

        public string DestinationFolder { get; set; }

        public bool IsRunning { get; set; }

        public int ProgressValue { get; set; }
        #endregion

        #region private member

        private string motherFoler = "";
        private string destinFolder = "";
        private bool success;
        /// <summary>
        /// So luong cac thu muc da tao symlink thanh cong
        /// </summary>
        private double number = 0;

        private int totalFile;

        private IDialogCoordinator dialogCoordinator;

        private string errors;

        #endregion

        #region Command
        /// <summary>
        /// Command de lay danh sach cac thu muc
        /// </summary>
        public ICommand OpenFolderCommand { get; set; }

        /// <summary>
        /// Command de lay duong dan thu muc se chuyen den
        /// </summary>
        public ICommand OpenDistinFolderCommand { get; set; }

        /// <summary>
        /// Start move folder and create symlink
        /// </summary>
        public ICommand StartMoveCommand { get; set; }

        /// <summary>
        /// Show thong tin ve tac gia
        /// </summary>
        public ICommand AboutCommand { get; set; }

        /// <summary>
        /// Thay doi ngon ngu hien thi phan mem
        /// </summary>
        public ICommand ChangeLanguageCommand { get; set; }
        #endregion

        #region Contructor

        public WindowsViewModel(IDialogCoordinator instance)
        {
            dialogCoordinator = instance;

            ListViewFolderInfos = new ObservableCollection<FolderInfo>();
            //Create command to select folder
            OpenFolderCommand = new RelayCommand(() =>
            {
                ListViewFolderInfos.Clear();
                foreach (var item in this.OpenFolder(ref motherFoler))
                {
                    ListViewFolderInfos.Add(item);
                }

                MotherFolder = motherFoler;
            });
            //Create command to select distination folder
            OpenDistinFolderCommand = new RelayCommand(() =>
              {
                  destinFolder = this.OpenDistiFolder();
                  DestinationFolder = destinFolder;
              });
            //Command start to move
            StartMoveCommand = new RelayParameterizedCommand(async (parameter) => await Start(parameter));
            //Hien thi thong tin phan mem
            AboutCommand=new RelayCommand(async()=> await MyMessageAsync((string)Application.Current.FindResource("About"), "Copyright © 2018 Le Minh Thanh\nWebsite: http://lêminhthành.vn\nLMT Symbolic Mover ver 1.0"));

            //Thay doi ngon ngu
            ChangeLanguageCommand=new RelayParameterizedCommand((parameter) =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(parameter.ToString());
                Languages.ApplyLanguage();
            });
        }


        #endregion

        #region Func

        /// <summary>
        /// Tien hanh tao symlink cho cac thu muc
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private async Task Start(object parameter)
        {
            await RunCommand(() => IsRunning, async () =>
            {
                List<string> FolderSources = new List<string>();
                System.Collections.IList items = (System.Collections.IList)parameter;
                foreach (var item in items)
                {
                    var path = (item as FolderInfo).FolderPath;
                    FolderSources.Add(path);
                }
                await Task.Factory.StartNew(async () =>
                {
                    if (this.CheckFolder(FolderSources, DestinationFolder, ref errors))
                    {
                        totalFile = this.GetNumberOfFiles(FolderSources);
                        number = 0;
                        foreach (var item in FolderSources)
                        {
                            if (Directory.GetDirectoryRoot(item) == Directory.GetDirectoryRoot(destinFolder))
                            {
                                try
                                {
                                    Directory.Move(item, destinFolder + "\\" + new DirectoryInfo(item).Name);
                                    if (this.MakeLink(destinFolder + "\\" + new DirectoryInfo(item).Name, item))
                                    {
                                        ProgressValue = (int)Math.Truncate(++number / FolderSources.Count * 100);
                                        success = true;
                                    }
                                    else success = false;
                                }
                                catch (Exception e)
                                {
                                    success = false;
                                    errors = e.Message;
                                    break;
                                }
                            }
                            else
                            {
                                try
                                {
                                    CopyFolder(item, destinFolder + "\\" + new DirectoryInfo(item).Name, false);
                                    Directory.Delete(item, true);
                                    if (this.MakeLink(destinFolder + "\\" + new DirectoryInfo(item).Name, item))
                                        success = true;
                                    else success = false;
                                }
                                catch (Exception e)
                                {
                                    success = false;
                                    errors = e.Message;
                                    break;
                                }
                            }
                        }

                        if (success)
                        {
                            await MyMessageAsync((string)Application.Current.FindResource("Success"), (string)Application.Current.FindResource("SuccessMove"));
                            ProgressValue = 0;
                        }
                        else
                            await MyMessageAsync((string)Application.Current.FindResource("Error"),
                                (string)Application.Current.FindResource("HaveError")+ $" {errors}"+"\n"+(string)Application
                                .Current.FindResource("ErrorInfo")+"\n"+(string)Application.Current.FindResource("MoveBack"),
                                MotherFolder, DestinationFolder, FolderSources);
                    }
                    else await MyMessageAsync((string)Application.Current.FindResource("Error"), errors);
                });

            });
        }

        /// <summary>
        /// Copy cac file va thu muc sang thu muc dich
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        public void CopyFolder(string sourceFolder, string destFolder, bool isError)
        {
            try
            {
                if (Directory.Exists(destFolder))
                    Directory.Delete(destFolder, true);
                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);
                string[] files = Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    if (!File.Exists(dest))
                        File.Copy(file, dest);
                    File.SetAttributes(file, FileAttributes.Normal);
                    if (!isError)
                    {
                        ProgressValue = (int)Math.Truncate(++number / totalFile * 100);
                    }
                }
                string[] folders = Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Path.Combine(destFolder, name);
                    CopyFolder(folder, dest, isError);
                }
            }
            catch
            {
                string[] files = Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                    if (!File.Exists(dest))
                        File.Copy(file, dest);
                    if (!isError)
                    {
                        ProgressValue = (int)Math.Truncate(++number / totalFile * 100);
                        OnPropertyChanged(nameof(ProgressValue));
                    }
                }
                string[] folders = Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Path.Combine(destFolder, name);
                    CopyFolder(folder, dest, isError);
                }
            }
        }

        /// <summary>
        /// SHow thong bao loi
        /// </summary>
        /// <param name="header"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task MyMessageAsync(string header, string message)
        {
            await dialogCoordinator.ShowMessageAsync(this, header, message);
        }

        /// <summary>
        /// Show thong bao loi va tien hanh di chuyenc cac file lai vi tri cu
        /// </summary>
        /// <param name="header"></param>
        /// <param name="message"></param>
        /// <param name="source"></param>
        /// <param name="des"></param>
        /// <param name="listSource"></param>
        /// <returns></returns>
        private async Task MyMessageAsync(string header, string message, string source, string des, List<string> listSource)
        {

            // Show...
            ProgressDialogController controller = await dialogCoordinator.ShowProgressAsync(this, header, message);
            controller.SetIndeterminate();
            //List ten cac thu muc con o thu muc source ban dau
            var listsubFolderInSourceFolder = new List<string>();
            listSource.ForEach(x => listsubFolderInSourceFolder.Add(new DirectoryInfo(x).Name));

            foreach (var item in Directory.GetDirectories(des))
            {

                //Luc nay se di chuyen cac file trong thu muc con tu thu muc des tro lai thu muc source
                if (listsubFolderInSourceFolder.Contains(new DirectoryInfo(item).Name))
                {
                    CopyFolder(item, source + "\\" + new DirectoryInfo(item).Name, true);
                    try
                    {
                        Directory.Delete(item, true);//Tien hanh xoa di thu muc des
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            await Task.Delay(3000);
            ProgressValue = 0;
            OnPropertyChanged(nameof(ProgressValue));
            // Close...
            await controller.CloseAsync();
        }

        #endregion

    }
}
