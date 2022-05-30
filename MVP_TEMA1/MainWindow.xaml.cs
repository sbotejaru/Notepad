using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes; 

namespace MVP_TEMA1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void LoadDirectories()
        {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                this.treeView.Items.Add(this.GetItem(drive));
            }
        }
        private TreeViewItem GetItem(DriveInfo drive)
        {
            var item = new TreeViewItem
            {
                Header = drive.Name,
                DataContext = drive,
                Tag = drive
            };
            this.AddDummy(item);
            item.Expanded += new RoutedEventHandler(item_Expanded);
            return item;
        }

        public class DummyTreeViewItem : TreeViewItem
        {
            public DummyTreeViewItem()
                : base()
            {
                base.Header = "Dummy";
                base.Tag = "Dummy";
            }
        }
        private TreeViewItem GetItem(DirectoryInfo directory)
        {
            var item = new TreeViewItem
            {
                Header = directory.Name,
                DataContext = directory,
                Tag = directory
            };
            this.AddDummy(item);
            item.Expanded += new RoutedEventHandler(item_Expanded);
            return item;
        }

        private TreeViewItem GetItem(FileInfo file)
        {
            var item = new TreeViewItem
            {
                Header = file.Name,
                DataContext = file,
                Tag = file
            };
            return item;
        }

        private void AddDummy(TreeViewItem item)
        {
            item.Items.Add(new DummyTreeViewItem());
        }

        private bool HasDummy(TreeViewItem item)
        {
            return item.HasItems && (item.Items.OfType<TreeViewItem>().ToList().FindAll(tvi => tvi is DummyTreeViewItem).Count > 0);
        }

        private void RemoveDummy(TreeViewItem item)
        {
            var dummies = item.Items.OfType<TreeViewItem>().ToList().FindAll(tvi => tvi is DummyTreeViewItem);
            foreach (var dummy in dummies)
            {
                item.Items.Remove(dummy);
            }
        }
        private void ExploreDirectories(TreeViewItem item)
        {
            var directoryInfo = (DirectoryInfo)null;
            if (item.Tag is DriveInfo)
            {
                directoryInfo = ((DriveInfo)item.Tag).RootDirectory;
            }
            else if (item.Tag is DirectoryInfo)
            {
                directoryInfo = (DirectoryInfo)item.Tag;
            }
            else if (item.Tag is FileInfo)
            {
                directoryInfo = ((FileInfo)item.Tag).Directory;
            }
            if (object.ReferenceEquals(directoryInfo, null)) return;
            foreach (var directory in directoryInfo.GetDirectories())
            {
                var isHidden = (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (directory.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem)
                {
                    item.Items.Add(this.GetItem(directory));
                }
            }
        }

        private void ExploreFiles(TreeViewItem item)
        {
            var directoryInfo = (DirectoryInfo)null;
            if (item.Tag is DriveInfo)
            {
                directoryInfo = ((DriveInfo)item.Tag).RootDirectory;
            }
            else if (item.Tag is DirectoryInfo)
            {
                directoryInfo = (DirectoryInfo)item.Tag;
            }
            else if (item.Tag is FileInfo)
            {
                directoryInfo = ((FileInfo)item.Tag).Directory;
            }
            if (object.ReferenceEquals(directoryInfo, null)) return;
            foreach (var file in directoryInfo.GetFiles())
            {
                var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (file.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem)
                {
                    item.Items.Add(this.GetItem(file));
                }
            }
        }

        void item_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (this.HasDummy(item))
            {
                this.Cursor = Cursors.Wait;
                this.RemoveDummy(item);
                this.ExploreDirectories(item);
                this.ExploreFiles(item);
                this.Cursor = Cursors.Arrow;
            }
        }

        public MainWindow()
        {  
            InitializeComponent();            
            this.LoadDirectories();
        }

        private void treeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MainVM mvvm = new MainVM();
            
            if (sender is TreeViewItem)
            {
                if (!((TreeViewItem)sender).IsSelected)
                {
                    return;
                }
            }
            
            var hit = e.OriginalSource as DependencyObject;
            while (hit != null && !(hit is TreeViewItem))
                hit = VisualTreeHelper.GetParent(hit);
           
            TreeViewItem tvi = new TreeViewItem();
            tvi = hit as TreeViewItem;
            var result = Convert.ToString(tvi.Header);
            var filename = result;
           
            TreeView tv = new TreeView();

            while (tvi.Parent.GetType() != tv.GetType())
            {
                var parent = (TreeViewItem)tvi.Parent;
                var parentString = parent.Header.ToString();

                if (parentString[parentString.Length - 1] != '\\')
                    result = parent.Header + "\\" + result;
                else
                    result = parent.Header + result;

                tvi = (TreeViewItem)tvi.Parent;
            }
                             
            var dataContext = (MainVM)this.DataContext;
            dataContext.OpenFromTreeView(result, filename);
            this.directory_view.IsExpanded = false;            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dataContext = (MainVM)this.DataContext;
            var isSaved = dataContext.IsSaved;
            bool allSaved = false;
            if (isSaved.All(c => c == true))
                allSaved = true;

            if (!allSaved)
            {
                string msg = "Do you want to close without saving?";
                MessageBoxResult result =
                  MessageBox.Show(
                    msg,
                    "Exit",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                {                    
                    e.Cancel = true;                    
                }
                else if (result == MessageBoxResult.No)
                {
                    dataContext.SaveAllCommand.Execute(null);
                }
            }
        }
    }
}
