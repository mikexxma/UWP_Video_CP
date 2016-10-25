using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWP_Video_CP.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWP_Video_CP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<ListViewModel> ListViewModels;
        public MainPage()
        {
            ListViewModels = ViewModelManager.getListViewModels();
            this.InitializeComponent();
        }
        private void HambegerBt_Click(object sender, RoutedEventArgs e)
        {
            MySpliteView.IsPaneOpen = !MySpliteView.IsPaneOpen;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!MySpliteView.IsPaneOpen)
            {
                MySpliteView.IsPaneOpen = !MySpliteView.IsPaneOpen;
            }

            ListView mylistView = sender as ListView;
            var s = (ListViewModel)mylistView.SelectedItem ;
            if (s != null)
            {
                VideoFrame.Navigate(s.ClassType);
            }
        }
    }
}
