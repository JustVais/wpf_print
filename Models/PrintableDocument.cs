using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace wpf_print.Models
{
    public class PrintableDocument : INotifyPropertyChanged, ICloneable
    {
        private string _title;
        private string _timeInSec;
        private string _documentType;
        private Tuple<int, int> _listSizes;
        private string _size;
        private string _status;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string TimeInSec
        {
            get { return _timeInSec; }
            set
            {
                _timeInSec = value+" сек";
                OnPropertyChanged("TimeInSec");
            }
        }

        public string DocumentType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
                OnPropertyChanged("DocumentType");
            }
        }

        public Tuple<int, int> ListSizes
        {
            get { return _listSizes; }
            set
            {
                _listSizes = value;
                OnPropertyChanged("ListSizes");
            }
        }

        public string Size
        {
            get { return _size; }
            set
            {
                _size = value + " кб";
                OnPropertyChanged("Size");
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }
        public object Clone()
        {
            return new PrintableDocument()
            {
                Title = _title,
                TimeInSec = _timeInSec.Split(' ')[0],
                DocumentType = _documentType,
                ListSizes = _listSizes,
                Size = _size.Split(' ')[0],
                Status = _status
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
