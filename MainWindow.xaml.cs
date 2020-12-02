using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FileManager.FileEvents;
using FileManager.Windows;
using FileManager.SystemUtils;
using FileManager.Models;

namespace FileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CopyEvent buff;
        public DirectoryInfo lDirectory { get; set; }
        public DirectoryInfo rDirectory { get; set; }
        private Session session;
        private Logger logger;
        public MainWindow()
        {
            logger = new Logger();
            Configuration configuration = Configuration.GetConfiguration("appsettings.json");
            Config.sessionTarget = configuration.SessionConnectionString;
            session = new Session();
            InitializeComponent();
            lDirectory = rDirectory = DriveInfo.GetDrives().First().RootDirectory;
            ShowFiles(lDirectory, LeftList, lDirInfo);
            ShowFiles(rDirectory, RightList, rDirInfo);
            lPath.Text = lDirectory.FullName;
            rPath.Text = rDirectory.FullName;
            rPath.Text = System.Environment.CurrentDirectory;
        }
        public void ShowFiles(DirectoryInfo dir, ListView list, TextBlock info)
        {
            int fileCounter = 0;
            int dirCounter = 0;
            if (dir.FullName != DriveInfo.GetDrives().First().RootDirectory.FullName)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = dir.Parent;
                item.Content = "...";
                list.Items.Add(item);

                item.MouseDoubleClick += Item_MouseDoubleClick;
            }
            foreach (var subDir in dir.GetDirectories())
            {
                if (!subDir.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    ListViewItem item = new ListViewItem();
                    item.Tag = subDir;
                    item.Content = subDir.Name;
                    list.Items.Add(item);


                    item.MouseDoubleClick += Item_MouseDoubleClick;
                    ++dirCounter;
                    //To add context menu and clicks
                }
            }
            foreach (var file in dir.GetFiles())
            {
                if (!file.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    ListViewItem item = new ListViewItem();
                    item.Tag = file;
                    item.Content = file.Name;
                    list.Items.Add(item);

                    item.MouseDoubleClick += Item_MouseDoubleClick;
                    ++fileCounter;
                    //To add context menu and clicks
                }
            }
            lPath.Text = lDirectory.FullName;
            rPath.Text = rDirectory.FullName;

            info.Text = $"{fileCounter} files {dirCounter} directories";

            session.lDir = lDirectory.FullName;
            session.rDir = rDirectory.FullName;
            session.UpdateLog();
        }
        private void Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListViewItem item = (ListViewItem)sender;
                if (item != null)
                {
                    if (LeftList.Items.Contains(item))
                    {
                        if (item.Tag is DirectoryInfo)
                        {
                            lDirectory = (DirectoryInfo)item.Tag;
                            LeftList.Items.Clear();
                            ShowFiles(lDirectory, LeftList, lDirInfo);
                        }
                        else
                        {
                            //MessageBox.Show($"{((FileInfo)item.Tag).FullName} is open");
                            TextViewer textViewer = new TextViewer((FileInfo)item.Tag);
                            textViewer.ShowDialog();
                        }
                    }
                    else
                    {
                        if (item.Tag is DirectoryInfo)
                        {
                            rDirectory = (DirectoryInfo)item.Tag;
                            RightList.Items.Clear();
                            ShowFiles(rDirectory, RightList, rDirInfo);
                        }
                        else
                        {
                            //MessageBox.Show($"{((FileInfo)item.Tag).FullName} is open");
                            TextViewer textViewer = new TextViewer((FileInfo)item.Tag);
                            textViewer.ShowDialog();
                        }
                    }
                }
                else
                {
                    throw new NullReferenceException("Unable to find file");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            int resultCount = 0;
            string srchFile = SearchBar.Text;
            DirectoryInfo mountDir = rDirectory;
            RightList.Items.Clear();
            ListViewItem head_item = new ListViewItem();
            head_item.Tag = DriveInfo.GetDrives().First().RootDirectory;
            head_item.Content = "...";
            RightList.Items.Add(head_item);

            head_item.MouseDoubleClick += Item_MouseDoubleClick;
            foreach (var accesableDir in mountDir.GetDirectories().Where(subDir => !subDir.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                if (accesableDir.Name == srchFile)
                {
                    ListViewItem item = new ListViewItem();

                    item.Tag = accesableDir;
                    item.Content = accesableDir.FullName;
                    RightList.Items.Add(item);

                    item.MouseDoubleClick += SearchItem_MouseDoubleClick;
                }
                try
                {
                    var dirs = accesableDir.GetDirectories(srchFile, SearchOption.AllDirectories);
                    var files = accesableDir.GetFiles(srchFile, SearchOption.AllDirectories);
                    foreach (var dir in dirs)
                    {
                        ListViewItem item = new ListViewItem();

                        item.Tag = dir;
                        item.Content = dir.FullName;
                        RightList.Items.Add(item);

                        item.MouseDoubleClick += SearchItem_MouseDoubleClick;
                    }
                    foreach (var file in files)
                    {
                        ListViewItem item = new ListViewItem();

                        item.Tag = file;
                        item.Content = file.FullName;
                        RightList.Items.Add(item);

                        item.MouseDoubleClick += SearchItem_MouseDoubleClick;
                    }
                    resultCount += files.Length + dirs.Length;
                }
                catch
                {

                }
            }
            foreach (var accesableFile in mountDir.GetFiles().Where(subDir => !subDir.Attributes.HasFlag(FileAttributes.Hidden)))
            {
                if (accesableFile.Name == srchFile)
                {
                    ListViewItem item = new ListViewItem();

                    item.Tag = accesableFile;
                    item.Content = accesableFile.FullName;
                    RightList.Items.Add(item);

                    item.MouseDoubleClick += SearchItem_MouseDoubleClick;
                }
            }
            rPath.Text = $"Search result: {resultCount} files were found";
            rDirInfo.Text = $"search element - {srchFile}";
        }
        void SearchItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListViewItem item = (ListViewItem)sender;
                if (item != null)
                {
                    if (item.Tag is DirectoryInfo)
                    {
                        rDirectory = ((DirectoryInfo)item.Tag).Parent;
                        RightList.Items.Clear();
                        ShowFiles(rDirectory, RightList, rDirInfo);
                    }
                    else
                    {
                        rDirectory = ((FileInfo)item.Tag).Directory;
                        RightList.Items.Clear();
                        ShowFiles(rDirectory, RightList, rDirInfo);
                    }
                }
                else
                {
                    throw new NullReferenceException("Unable to find file");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteEvent deleteEvent = new DeleteEvent();

            if (((MenuItem)e.OriginalSource).Name == "lDeleteBtn")
            {
                deleteEvent.RestoreCollection = new List<FileSystemInfo>(LeftList.SelectedItems.Count);

                foreach (var fi in LeftList.SelectedItems)
                {
                    deleteEvent.RestoreCollection.Add((FileSystemInfo)((ListViewItem)fi).Tag);
                }
                LeftList.Items.Clear();
                deleteEvent.InvokeEvent();
                logger.fileChanges.Push(deleteEvent);
                ShowFiles(lDirectory, LeftList, lDirInfo);
            }
            else
            {
                deleteEvent.RestoreCollection = new List<FileSystemInfo>(RightList.SelectedItems.Count);

                foreach (var fi in RightList.SelectedItems)
                {
                    deleteEvent.RestoreCollection.Add((FileSystemInfo)((ListViewItem)fi).Tag);
                }
                RightList.Items.Clear();
                deleteEvent.InvokeEvent();
                logger.fileChanges.Push(deleteEvent);
                ShowFiles(rDirectory, RightList, rDirInfo);
            }

            
        }
        private void RestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            logger.CancelLastChange();
            LeftList.Items.Clear();
            RightList.Items.Clear();
            ShowFiles(lDirectory, LeftList, lDirInfo);
            ShowFiles(rDirectory, RightList, rDirInfo);
        }
        private void RenameBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((MenuItem)e.OriginalSource).Name == "lDeleteBtn")
                {
                    if (LeftList.SelectedItems.Count == 1)
                    {
                        RenameWindow win = new RenameWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        if (win.DialogResult == true)
                        {
                            FileSystemInfo target = (FileSystemInfo)((ListViewItem)LeftList.SelectedItem).Tag;

                            RenameEvent renameEvent = new RenameEvent(target, win.NewName.Text);

                            logger.fileChanges.Push(renameEvent);

                            renameEvent.InvokeEvent();
                        }
                        LeftList.Items.Clear();
                        ShowFiles(lDirectory, LeftList, lDirInfo);
                    }
                    else
                        throw new Exception("Only one file or directory need to be selected");
                }
                else
                {
                    if (RightList.SelectedItems.Count == 1)
                    {
                        RenameWindow win = new RenameWindow();
                        win.Owner = this;
                        win.ShowDialog();
                        if (win.DialogResult == true)
                        {
                            FileSystemInfo target = (FileSystemInfo)((ListViewItem)RightList.SelectedItem).Tag;

                            RenameEvent renameEvent = new RenameEvent(target, win.NewName.Text);

                            logger.fileChanges.Push(renameEvent);

                            renameEvent.InvokeEvent();
                        }
                        RightList.Items.Clear();
                        ShowFiles(rDirectory, RightList, rDirInfo);
                    }
                    else
                        throw new Exception("Only one file or directory need to be selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke RenameEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /*private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            CopyEvent copyEvent = new CopyEvent();

            if (((MenuItem)e.OriginalSource).Name == "lCopyBtn")
            {
                copyEvent.CopyCollection = new List<FileSystemInfo>(LeftList.SelectedItems.Count);

                foreach (var fi in LeftList.SelectedItems)
                {
                    copyEvent.CopyCollection.Add((FileSystemInfo)((ListViewItem)fi).Tag);
                }

                LeftList.SelectedItems.Clear();
            }
            else
            {
                copyEvent.CopyCollection = new List<FileSystemInfo>(RightList.SelectedItems.Count);

                foreach (var fi in RightList.SelectedItems)
                {
                    copyEvent.CopyCollection.Add((FileSystemInfo)((ListViewItem)fi).Tag);
                }

                RightList.SelectedItems.Clear();
            }
            buff = copyEvent;
        }
        private void PasteBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((MenuItem)e.OriginalSource).Name == "lPasteBtn")
                {
                    if (LeftList.SelectedItems.Count == 0)
                        buff.newPath = lDirectory;
                    else if (LeftList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo dest = (FileSystemInfo)(((ListViewItem)LeftList.SelectedItem).Tag);
                        if (dest is DirectoryInfo)
                            buff.newPath = (DirectoryInfo)dest;
                        else
                            buff.newPath = lDirectory;
                    }
                    else
                        throw new Exception("Too many destination folders!");
                }
                else
                {
                    if (RightList.SelectedItems.Count == 0)
                        buff.newPath = rDirectory;
                    else if (RightList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo dest = (FileSystemInfo)(((ListViewItem)RightList.SelectedItem).Tag);
                        if (dest is DirectoryInfo)
                            buff.newPath = (DirectoryInfo)dest;
                        else
                            buff.newPath = rDirectory;
                    }
                    else
                        throw new Exception("Too many destination folders!");
                }
                if (buff.CopyCollection.Count != 0)
                {
                    logger.fileChanges.Push(buff);
                    buff.InvokeEvent();
                }
                else
                    throw new Exception("No files in buff!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke PasteEvent", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }*/
        public void CollectionBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((MenuItem)e.OriginalSource).Name == "lCollBtn")
                {
                    CollectionWindow win = new CollectionWindow();

                    win.Path.Text = lDirectory.FullName;
                    win.ShowDialog();
                }
                else
                {
                    CollectionWindow win = new CollectionWindow();

                    win.Path.Text = rDirectory.FullName;
                    win.ShowDialog();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public void ReadCollBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((MenuItem)e.OriginalSource).Name == "lReadBtn")
                {
                    if (LeftList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo file = (FileSystemInfo)((ListViewItem)LeftList.SelectedItem).Tag;
                        if (file is DirectoryInfo)
                            throw new Exception("Unable to open folder");
                        if (file.Extension != "txt" && file.Extension != "bin")
                            throw new Exception("Wrong file format");
                        ReadCol win = new ReadCol((FileInfo)file);
                        win.ShowDialog();
                    }
                    else
                        throw new Exception("Select one item");
                }
                else
                {
                    if (RightList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo file = (FileSystemInfo)((ListViewItem)RightList.SelectedItem).Tag;
                        if (file is DirectoryInfo)
                            throw new Exception("Unable to open folder");
                        if (file.Extension != ".txt" && file.Extension != ".bin")
                            throw new Exception("Wrong file format");
                        ReadCol win = new ReadCol((FileInfo)file);
                        win.ShowDialog();
                    }
                    else
                        throw new Exception("Select one item");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public void CompressBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((MenuItem)e.OriginalSource).Name == "lCompressBtn")
                {
                    if (LeftList.SelectedItems.Count > 0)
                    {
                        ArchiveWindow win = new ArchiveWindow();
                        win.Owner = this;
                        win.ShowDialog();

                        foreach (var fi in LeftList.SelectedItems)
                        {
                            if (((ListViewItem)fi).Tag is DirectoryInfo)
                                throw new Exception("U can archive only files");
                            if (((FileInfo)((ListViewItem)fi).Tag).Name == win.NewName.Text)
                                throw new Exception("File already exist");
                        }

                        var zipPath = lDirectory.FullName + '\\' + win.NewName.Text + ".zip";

                        using (var fs = File.Create(zipPath))
                        {
                            fs.Close();
                        }

                        foreach (var fi in LeftList.SelectedItems)
                        {
                            FileUtils.AddToArchive(((FileInfo)((ListViewItem)fi).Tag).FullName, zipPath);
                        }

                        LeftList.Items.Clear();
                        ShowFiles(lDirectory, LeftList, lDirInfo);
                    }
                    else
                        throw new Exception("Select files!");
                }
                else
                {
                    if (RightList.SelectedItems.Count > 0)
                    {
                        ArchiveWindow win = new ArchiveWindow();
                        win.Owner = this;
                        win.ShowDialog();

                        foreach (var fi in RightList.SelectedItems)
                        {
                            if (((ListViewItem)fi).Tag is DirectoryInfo)
                                throw new Exception("U can archive only files");
                            if (((FileInfo)(((ListViewItem)fi).Tag)).Name == win.Name)
                                throw new Exception("File already exist");
                        }

                        var zipPath = rDirectory.FullName + '\\' + win.NewName.Text + ".zip";

                        using (var fs = File.Create(zipPath))
                        {
                            fs.Close();
                        }

                        foreach (var fi in RightList.SelectedItems)
                        {
                            FileUtils.AddToArchive(((FileInfo)((ListViewItem)fi).Tag).FullName, zipPath);
                        }

                        RightList.Items.Clear();
                        ShowFiles(rDirectory, RightList, rDirInfo);
                    }
                    else
                        throw new Exception("Select files!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke CompressEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void DecompressBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((MenuItem)e.OriginalSource).Name == "lDecompressBtn")
                {
                    if (LeftList.SelectedItems.Count == 1)
                    {

                        FileSystemInfo target = (FileSystemInfo)((ListViewItem)LeftList.SelectedItem).Tag;

                        if (target.Extension != ".zip")
                            throw new Exception(".zip file should be selected");

                        FileUtils.DecompressArchive(target.FullName, lDirectory.FullName);

                        LeftList.Items.Clear();
                        ShowFiles(lDirectory, LeftList, lDirInfo);
                    }
                    else
                        throw new Exception("Only one file need to be selected");
                }
                else
                {
                    if (RightList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo target = (FileSystemInfo)((ListViewItem)LeftList.SelectedItem).Tag;

                        if (target.Extension != ".zip")
                            throw new Exception(".zip file should be selected");

                        FileUtils.DecompressArchive(target.FullName, lDirectory.FullName);

                        RightList.Items.Clear();
                        ShowFiles(rDirectory, RightList, rDirInfo);
                    }
                    else
                        throw new Exception("Only one file need to be selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke DecompressEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void EncryptBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((MenuItem)e.OriginalSource).Name == "lEncryptBtn")
                {
                    if (LeftList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo target = (FileSystemInfo)((ListViewItem)LeftList.SelectedItem).Tag;

                        if (target is DirectoryInfo)
                            throw new Exception("Selected file should be a directory");

                        FileUtils.EncryptFile(target.FullName, target.FullName);
                    }
                    else
                        throw new Exception("Only ine file need to be selected");
                }
                else
                {
                    if (RightList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo target = (FileSystemInfo)((ListViewItem)RightList.SelectedItem).Tag;

                        if (target is DirectoryInfo)
                            throw new Exception("Selected file should be a directory");

                        FileUtils.EncryptFile(target.FullName, target.FullName);
                    }
                    else
                        throw new Exception("Only ine file need to be selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke encryptEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void DecryptBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((MenuItem)e.OriginalSource).Name == "lDecryptBtn")
                {
                    if (LeftList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo target = (FileSystemInfo)((ListViewItem)LeftList.SelectedItem).Tag;

                        if (target is DirectoryInfo)
                            throw new Exception("Selected file should be a directory");

                        FileUtils.DecryptFile(target.FullName, target.FullName);
                    }
                    else
                        throw new Exception("Only one file need to be selected");
                }
                else
                {
                    if (RightList.SelectedItems.Count == 1)
                    {
                        FileSystemInfo target = (FileSystemInfo)((ListViewItem)LeftList.SelectedItem).Tag;

                        if (target is DirectoryInfo)
                            throw new Exception("Selected file should be a directory");

                        FileUtils.DecryptFile(target.FullName, target.FullName);
                    }
                    else
                        throw new Exception("Only one file need to be selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to invoke decryptEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}