using System.ComponentModel;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace LMT_Symbolic_Mover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm=new WindowsViewModel(DialogCoordinator.Instance);
            DataContext = vm;
            Languages.ApplyLanguage("vi-VN");
        }
        /// <summary>
        /// Tat phan mem hoan toan
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

       
    }
}
