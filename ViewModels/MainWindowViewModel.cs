using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using wpf_print.Models;
using wpf_print.ViewModels;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Media;
using System.Diagnostics;

namespace wpf_print.View
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PrintableDocument> _documentsList;
        private ObservableCollection<PrintableDocument> _documentsInPrintList;
        private ObservableCollection<PrintableDocument> _sortedDocumentsList;
        private PrintableDocument _selectedDocument;
        private Thread _printingThread;
        private int _progressBar;
        private string _showControlPanel = "Hidden";
        private string[] _currentDocumentsStatuses = new string[]{ "Загружен" };

        private string LOADED = "Загружен";
        private string WAITING_FOR_PRINT = "Ждет печати";
        private string PRINTING = "Печатается";
        private string PRINTED = "Напечатан";
        private string CANCELED = "Отменен";

        private string _startPrint = "Collapsed";
        private string _cancelPrint = "Collapsed";
        private string _againPrint = "Collapsed";



        public string StartPrint
        {
            get { return _startPrint; }
            set
            {
                _startPrint = value;
                OnPropertyChanged("StartPrint");
            }
        }

        public string CancelPrint
        {
            get { return _cancelPrint; }
            set
            {
                _cancelPrint = value;
                OnPropertyChanged("CancelPrint");
            }
        }

        public string AgainPrint
        {
            get { return _againPrint; }
            set
            {
                _againPrint = value;
                OnPropertyChanged("AgainPrint");
            }
        }

        public string ShowControlPanel
        {
            get { return _showControlPanel; }
            set
            {
                _showControlPanel = value;
                OnPropertyChanged("ShowControlPanel");
            }
        }

        public ObservableCollection<PrintableDocument> DocumentsList
        {
            get { return _documentsList; }
            set
            {
                _documentsList = value;
                OnPropertyChanged("DocumentsList");
            }
        }

        public ObservableCollection<PrintableDocument> SortedDocumentsList
        {
            get { return _sortedDocumentsList; }
            set
            {
                _sortedDocumentsList = value;
                OnPropertyChanged("SortedDocumentsList");
            }
        }

        public int ProgressBar
        {
            get { return _progressBar; }
            set
            {
                _progressBar = value;
                OnPropertyChanged("ProgressBar");
            }
        }

        public PrintableDocument SelectedDocument
        {
            get { return _selectedDocument; }
            set
            {
                ShowControlPanel = value != null ? "Visibility" : "Hidden";

                _selectedDocument = value;

                ChangeControlButtonsVisibility();
                OnPropertyChanged("SelectedDocument");
            }
        }

        private void ChangeControlButtonsVisibility()
        {
            if (_selectedDocument != null)
            {
                if (_selectedDocument.Status == LOADED)
                {
                    StartPrint = "Visibility";
                    CancelPrint = "Collapsed";
                    AgainPrint = "Collapsed";
                }
                else if (_selectedDocument.Status == WAITING_FOR_PRINT)
                {
                    StartPrint = "Collapsed";
                    CancelPrint = "Visibility";
                    AgainPrint = "Visibility";
                }
                else if (_selectedDocument.Status == PRINTING)
                {
                    StartPrint = "Collapsed";
                    CancelPrint = "Collapsed";
                    AgainPrint = "Visibility";
                }
                else if (_selectedDocument.Status == PRINTED)
                {
                    StartPrint = "Collapsed";
                    CancelPrint = "Collapsed";
                    AgainPrint = "Visibility";
                }
                else if (_selectedDocument.Status == CANCELED)
                {
                    StartPrint = "Collapsed";
                    CancelPrint = "Collapsed";
                    AgainPrint = "Visibility";
                }
            }
        }

        public void ChangeDocumentStatus(PrintableDocument currentDocument, string status)
        {
            currentDocument.Status = status;
            ChangeControlButtonsVisibility();
            ShowDocuments(_currentDocumentsStatuses);
        }

        public MainWindowViewModel()
        {
            _documentsList = new ObservableCollection<PrintableDocument>()
            {
                new PrintableDocument()
                {
                    Title = "some title",
                    TimeInSec = 10,
                    DocumentType = "doc",
                    ListSizes = new Tuple<int, int>(1000, 1000),
                    Size = 100,
                    Status = LOADED
                }
            };

            _documentsInPrintList = new ObservableCollection<PrintableDocument>();

            SortedDocumentsList = _documentsList;
        }

        private void AddNewDocument()
        {
            OpenFileDialog fileWindow = new OpenFileDialog();

            fileWindow.Filter = "Pdf files(*.pdf)|*.pdf|Doc files(*.doc)|*.doc|Png files(*.png)|*.png|Jpg files(*.jpg)|*.jpg";

            DialogResult result = fileWindow.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Random rand = new Random();

                var newDocument = new PrintableDocument()
                {
                    Title = fileWindow.SafeFileName.Split('.')[0],
                    TimeInSec = rand.Next(5, 15),
                    DocumentType = fileWindow.SafeFileName.Split('.')[1],
                    ListSizes = new Tuple<int, int>(rand.Next(500, 2000), rand.Next(500, 2000)),
                    Size = rand.Next(10, 10000),
                    Status = "Ожидает"
                };

                DocumentsList.Add(newDocument);
            }
        }

        public ICommand AddNewDocumentCommand
        {
            get
            {
                return new RelayCommand(() => AddNewDocument());
            }
        }

        private bool CheckForDocumentsInPrinter()
        {
            for (var i = 0; i < _documentsInPrintList.Count; i++)
                if (_documentsInPrintList[i].Status == WAITING_FOR_PRINT) return true;

            return false;
        }

        private void Printing()
        {
            if (CheckForDocumentsInPrinter())
            {
                for (var i = 0; i < _documentsInPrintList.Count; i++)
                {
                    if (_documentsInPrintList[i].Status == WAITING_FOR_PRINT)
                    {
                        ChangeDocumentStatus(_documentsInPrintList[i], PRINTING);

                        ProgressBar = 0;

                        var seconds = _documentsInPrintList[i].TimeInSec;
                        for (var j = 0; j <= seconds; j++)
                        {
                            ProgressBar = j * (100 / seconds);
                            Thread.Sleep(1000);
                        }

                        ChangeDocumentStatus(_documentsInPrintList[i], PRINTED);
                    }
                }

                Printing();
            }
        }

        private void SendDocumentForPrint()
        {
            var selectedDocumentClone = (PrintableDocument)SelectedDocument.Clone();

            ChangeDocumentStatus(selectedDocumentClone, WAITING_FOR_PRINT);

            _documentsInPrintList.Add(selectedDocumentClone);


            if (_printingThread == null)
            {
                _printingThread = new Thread(new ThreadStart(() => Printing()));
                _printingThread.Start();
            }
            else if (!_printingThread.IsAlive)
            {
                _printingThread = new Thread(new ThreadStart(() => Printing()));
                _printingThread.Start();
            }
        }

        public ICommand SendDocumentForPrintCommand
        {
            get
            {
                return new RelayCommand(() => { SendDocumentForPrint(); });
            }
        }

        public void CancelAllPrints()
        {
            ProgressBar = 0;

            _printingThread.Abort();

            for (var i = 0; i < _documentsInPrintList.Count; i++)
            {
                if (_documentsInPrintList[i].Status == WAITING_FOR_PRINT)
                {
                    ChangeDocumentStatus(_documentsInPrintList[i], CANCELED);
                }
            }
        }

        public ICommand CancelAllPrintsCommand
        {
            get
            {
                return new RelayCommand(() => { CancelAllPrints(); });
            }
        }

        public void CancelSelectedPrint()
        {
            if (_selectedDocument.Status == WAITING_FOR_PRINT)
            {
                ChangeDocumentStatus(_selectedDocument, CANCELED);
            }
        }

        public ICommand CancelSelectedPrintCommand
        {
            get
            {
                return new RelayCommand(() => { CancelSelectedPrint(); });
            }
        }

        public ICommand ShowAllDocumentsCommand
        {
            get
            {
                return new RelayCommand(() => { ShowDocuments(LOADED); });
            }
        }

        public void ShowDocuments(params string[] statuses)
        {
            _currentDocumentsStatuses = statuses;
            var newDocumentsList = new ObservableCollection<PrintableDocument>();

            if (statuses[0] == LOADED)
            {
                for (var i = 0; i < _documentsList.Count; i++)
                    newDocumentsList.Add(_documentsList[i]);
            }
            else
            {
                for (var i = 0; i < _documentsInPrintList.Count; i++)
                {
                    var count = 0;

                    for (var j = 0; j < statuses.Length; j++)
                        if (_documentsInPrintList[i].Status == statuses[j]) count++;

                    if (count == 0) continue;

                    newDocumentsList.Add(_documentsInPrintList[i]);
                }
            }

            SortedDocumentsList = newDocumentsList;
        }

        public ICommand ShowDocumentsInPrinterCommand
        {
            get
            {
                return new RelayCommand(() => { ShowDocuments(WAITING_FOR_PRINT, PRINTING); });
            }
        }

        public ICommand ShowCanceledPrintsCommand
        {
            get
            {
                return new RelayCommand(() => { ShowDocuments(CANCELED); });
            }
        }

        public ICommand ShowComplitedPrintsCommand
        {
            get
            {
                return new RelayCommand(() => { ShowDocuments(PRINTED); });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
