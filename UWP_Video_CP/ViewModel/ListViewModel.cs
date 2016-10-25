using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UWP_Video_CP.ViewModel
{
    class ListViewModel: INotifyPropertyChanged
    {
        private string functionName;
        private string imageCover;
        private Type classType;

        public ListViewModel(string function, string cover,Type type)
        {
            functionName = function;
            imageCover = cover;
            classType = type;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string FunctionName
        {
            get { return functionName; }
            set { functionName = value; OnPropertyChanged(); }
        }

        public string ImageCover
        {
            get { return imageCover; }
            set { imageCover = value; OnPropertyChanged(); }
        }

        public Type ClassType
        {
            get { return classType; }
            set { this.classType = value; OnPropertyChanged(); }
        }

        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            }
        }

    }
}
