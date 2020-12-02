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
    /// Interaction logic for CollectionWindow.xaml
    /// </summary>
    public partial class CollectionWindow : Window
    {
        List<Quotation_> list = new List<Quotation_>();
        public CollectionWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Choice.SelectedItem == null)
                    throw new Exception("Select file type");

                if (((ComboBoxItem)Choice.SelectedItem).Name == "BIN")
                {
                    using (var file = File.Open(Path.Text + "\\" + FileName.Text + ".bin", FileMode.OpenOrCreate))
                    {
                        BinaryWriter writer = new BinaryWriter(file);
                        foreach (var it in list)
                        {
                            writer.Write(it.Author);
                            writer.Write(it.Str);
                        }

                        writer.Close();
                    }
                }
                else
                {
                    using (var file = File.Open(Path.Text + "\\" + FileName.Text + ".txt", FileMode.OpenOrCreate))
                    {
                        StreamWriter writer = new StreamWriter(file);
                        foreach (var it in list)
                        {
                            writer.WriteLine(it.Author + " " + it.Str);
                        }

                        writer.Close();
                    }
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (Author.Text != "" || Author.Text != null || Quotation.Text != "" || Quotation.Text != null)
            {
                Quotation_ quotation = new Quotation_(Quotation.Text, Author.Text);
                list.Add(quotation);
            }
            Col.Items.Clear();
            foreach (var item in list)
            {
                ListViewItem it = new ListViewItem();
                it.Content = item.ToString();
                Col.Items.Add(it);
            }
        }
    }
}
