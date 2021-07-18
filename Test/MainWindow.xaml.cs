using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Wildgoat.WPFUtility.Collections;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ListProperty = DependencyProperty.Register(
            nameof(List),
            typeof(ConcatenatedCollectionsView),
            typeof(MainWindow));

        private SourceCollectionList<string> array1 = new SourceCollectionList<string>();
        private SourceCollectionList<string> array2 = new SourceCollectionList<string>();
        private SourceCollectionList<string> array3 = new SourceCollectionList<string>();

        private int c = 0;

        public MainWindow()
        {
            InitializeComponent();

            array1.Add("test");
            array3.Add("test3");
            array2.Add("test2");
            List = new ConcatenatedCollectionsView(array1, array2, array3);
        }

        public ConcatenatedCollectionsView List { get => (ConcatenatedCollectionsView)GetValue(ListProperty); set => SetValue(ListProperty, value); }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            array1.Add($"new{c++}");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            array2.Add($"new{c++}");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            array3.Add($"new{c++}");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (array1.Count > 1)
                array1.Remove(array1[1]);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            array2.Clear();
        }
    }
}