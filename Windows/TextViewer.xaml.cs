using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace FileManager.Windows
{
    /// <summary>
    /// Interaction logic for TextViewer.xaml
    /// </summary>
    public partial class TextViewer : Window
    {
        public TextViewer(FileInfo file)
        {
            InitializeComponent();
            Path.Text = file.FullName;
            StreamReader reader = new StreamReader(file.FullName);
            Content_.Text = reader.ReadToEnd();
            reader.Close();
        }
    }
}
