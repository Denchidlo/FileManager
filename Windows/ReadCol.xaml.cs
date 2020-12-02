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
    /// Interaction logic for ReadCol.xaml
    /// </summary>
    public partial class ReadCol : Window
    {
        public FileInfo file;
        public ReadCol(FileInfo file)
        {
            InitializeComponent();
            this.file = file;
            try
            {
                List<Quotation_> list = new List<Quotation_>();
                if (file.Extension == ".bin")
                {
                    BinaryReader binary = new BinaryReader(File.Open(file.FullName, FileMode.Open));
                    for (int i = 0; binary.PeekChar() > -1; i++)
                    {
                        try
                        {
                            string author = binary.ReadString();
                            string str = binary.ReadString();
                            list.Add(new Quotation_(author, str));
                        }
                        catch (EndOfStreamException)
                        {
                            throw new Exception("File format error");
                        }
                    }
                    foreach (var q in list)
                    {
                        ListViewItem it = new ListViewItem();
                        it.Content = q.ToString();
                        listview.Items.Add(it);
                    }
                }
                else if (file.Extension == ".txt")
                {
                    StreamReader stream = new StreamReader(file.FullName);
                    string str;
                    for (int i = 0; (str = stream.ReadLine()) != null; i++)
                    {
                        string[] data = str.Split(' ');
                        if (data[0] == null || data[1] == null)
                            throw new Exception("File format error");
                        list.Add(new Quotation_(data[0], data[1]));
                    }

                    foreach (var q in list)
                    {
                        ListViewItem it = new ListViewItem();
                        it.Content = q.ToString();
                        listview.Items.Add(it);
                    }
                }
                else
                    throw new Exception("Wrong extension");
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message, "Unable to invoke Event", MessageBoxButton.OK, MessageBoxImage.Warning);
                DialogResult = true;
            }
            
        }
    }
}
