using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpf_print.View;

namespace wpf_print
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();

            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized; 
            MaximizeButton.Click += (s, e) => WindowState = WindowState.Maximized; 
            CloseButton.Click += (s, e) => Close();
            _mainWindowViewModel = new MainWindowViewModel();
            DataContext = _mainWindowViewModel;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition((UIElement)sender);

            HitTestResult result = VisualTreeHelper.HitTest(currentCanvas, pt);

            if (result != null && result.GetType() != typeof(ListViewItem))
            {
                _mainWindowViewModel.ShowControlPanel = "Hidden";
                _mainWindowViewModel.SelectedDocument = null;    
            }
        }
    }
}
