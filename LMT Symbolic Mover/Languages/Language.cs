using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LMT_Symbolic_Mover
{
    public static class Languages
    {
        /// <summary>
        /// Ap dung ngon ngu cho phan mem
        /// </summary>
        /// <param name="cultureName"></param>
        public static void ApplyLanguage(string cultureName = null)
        {
            if (cultureName != null)
                Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "vi-VN":
                    dict.Source = new Uri("..\\Languages\\Vietnamese.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Languages\\English.xaml", UriKind.Relative);
                    break;
            }
            Application.Current.Resources.MergedDictionaries.Remove(dict);
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
