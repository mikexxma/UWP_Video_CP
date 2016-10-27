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
            lists.Add(new ListViewModel("Clip and Trim Video", cover1,typeof(ClipVideo)));
            lists.Add(new ListViewModel("Get Single Frame", cover2,typeof(GetSingleFrame)));
            lists.Add(new ListViewModel("Encode Images To Video", cover2, typeof(BitmapsToVideo)));
            lists.Add(new ListViewModel("Transcoding media", cover2, typeof(Transcoding_Media)));
            lists.Add(new ListViewModel("Face Detect", cover2, typeof(FaceDetect)));
            return lists;
        } 
    }
}
