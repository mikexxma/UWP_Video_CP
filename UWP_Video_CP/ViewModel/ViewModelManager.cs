using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP_Video_CP.ViewModel
{
    class ViewModelManager
    {
        public static ObservableCollection<ListViewModel> getListViewModels()
        {
            ObservableCollection<ListViewModel> lists = new ObservableCollection<ListViewModel>();
            string cover1 = @"&#xE70F;";
            string cover2 = @"&#xE786;";
            

            lists.Add(new ListViewModel("clip and trim video", cover1,typeof(ClipVideo)));
            lists.Add(new ListViewModel("get single frame", cover2,typeof(GetSingleFrame)));
            return lists;
        } 
    }
}
