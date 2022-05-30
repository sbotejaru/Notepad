using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Collections;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MVP_TEMA1
{
    public class MainVM : INotifyPropertyChanged
    {
        private String contentText;
        private String replaceText;
        private String replaceToText;
        private int selectedTab;
        private int newIndex = 0;
        private bool saved;
        private bool cancel;
        private string myTitle = "Notepad++";
        private FindWindow findWindow;
        private ReplaceWindow replaceWindow;
        private List<int> indexes;
        bool findAllOpened = false;
        int findAllIndex;

        public ObservableCollection<TabItem> TabItems { get; set; }
        public List<string> ItemPaths { get; set; }
        public List<string> ItemData { get; set; }
        public List<bool> IsSaved { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public String ContentText
        {
            get { return contentText; }
            set
            {
                contentText = value;
                NotifyPropertyChanged("ContentText");
            }
        }
        public String ReplaceText
        {
            get { return replaceText; }
            set
            {
                replaceText = value;
                NotifyPropertyChanged("ReplaceText");
            }
        }
        public String ReplaceToText
        {
            get { return replaceToText; }
            set
            {
                replaceToText = value;
                NotifyPropertyChanged("ReplaceToText");
            }
        }

        public string MyTitle
        {
            get
            {
                if (TabItems.Count != 0 && SelectedItem >= 0)
                {
                    if (ItemPaths[SelectedItem] != null)
                        return ItemPaths[SelectedItem] + "  -  " + myTitle;
                    else
                        return TabItems[SelectedItem].Header.ToString() + "  -  " + myTitle;
                }
                else
                    return myTitle;
            }
        }
        public int SelectedItem
        {
            get { return selectedTab; }
            set { selectedTab = value; NotifyPropertyChanged(nameof(SelectedItem)); NotifyPropertyChanged("MyTitle"); }
        }
        public bool Saved
        {
            get { return IsSaved[SelectedItem]; }
            set
            {
                saved = value;
                NotifyPropertyChanged("Saved");
                if (saved)
                {
                    IsSaved[SelectedItem] = true;
                    string header = TabItems[SelectedItem].Header.ToString();
                    if (saved && header[header.Length - 1] == '*')
                        TabItems[SelectedItem].Header = header.Remove(header.Length - 1);
                }
                else
                {
                    IsSaved[SelectedItem] = false;
                    string header = TabItems[SelectedItem].Header.ToString();
                    if (header[header.Length - 1] != '*')
                        TabItems[SelectedItem].Header += "*";
                }
            }
        }
        public void AddNewTabItem(string content, string filename, string path, bool isNew)
        {
            var bc = new BrushConverter();
            var myBGColor = (Brush)bc.ConvertFrom("#333339");
            var myFGColor = (Brush)bc.ConvertFrom("#FFFFFF");

            TabItem newTab = new TabItem();
            TextBox newTb = new TextBox();

            #region textbox formatting

            newTb.Text = content;
            newTb.AcceptsReturn = true;
            newTb.Background = myBGColor;
            newTb.Foreground = myFGColor;
            newTb.FontFamily = new FontFamily("Courier New");
            newTb.FontSize = 15;
            newTb.BorderThickness = new Thickness(0, 0, 0, 0);
            newTb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            newTb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            newTb.TextChanged += TextChanged;

            #endregion

            newTab.Header = filename;
            newTab.Foreground = Brushes.LightGray;
            newTab.Content = newTb;

            TabItems.Add(newTab);
            ItemPaths.Add(path);
            ItemData.Add(content);
            IsSaved.Add(!isNew);
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            Saved = false;
        }

        public void OpenFromTreeView(string path, string filename)
        {
            string content = File.ReadAllText(path);
            AddNewTabItem(content, filename, path, false);
            SelectedItem = TabItems.Count - 1;
        }

        #region commands declaration
        public RelayCommand OpenCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand NewCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand CloseAllCommand { get; set; }
        public RelayCommand SaveAllCommand { get; set; }
        public RelayCommand SaveCloseCommand { get; set; }
        public RelayCommand AboutCommand { get; set; }
        public RelayCommand FindCommand { get; set; }
        public RelayCommand FindAllCommand { get; set; }
        public RelayCommand ReplaceCommand { get; set; }
        public RelayCommand ReplaceAllCommand { get; set; }
        public RelayCommand FindInputCommand { get; set; }
        public RelayCommand FindAllInputCommand { get; set; }
        public RelayCommand ReplaceInput { get; set; }
        public RelayCommand ReplaceAllInput { get; set; }
        #endregion

        public MainVM()
        {
            TabItems = new ObservableCollection<TabItem>();
            ItemPaths = new List<string>();
            ItemData = new List<string>();
            IsSaved = new List<bool>();
            indexes = new List<int>();

            #region commands function bodies
            SaveCommand = new RelayCommand(o =>
            {
                #region save function body
                if (TabItems.Any())
                {
                    if (!Saved)
                    {
                        var tabItem = TabItems[SelectedItem];
                        var content = (tabItem.Content as TextBox).Text;
                        if (ItemPaths[SelectedItem] != null)
                        {
                            File.WriteAllText(ItemPaths[SelectedItem], content);
                            // IsSaved[SelectedItem] = true;
                            Saved = true;
                        }
                        else
                            SaveAsCommand.Execute(null);
                    }
                }
                #endregion
            });

            SaveAsCommand = new RelayCommand(o =>
            {
                #region save as function body
                if (TabItems.Any())
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files(*.*)|*.*";

                    var tabItem = TabItems[SelectedItem];
                    var content = (tabItem.Content as TextBox).Text;

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllText(saveFileDialog.FileName, content);
                        tabItem.Header = saveFileDialog.SafeFileName;
                        ItemData[SelectedItem] = content;
                        ItemPaths[SelectedItem] = saveFileDialog.FileName;
                    }
                    Saved = true;
                }
                #endregion
            });

            SaveAllCommand = new RelayCommand(o =>
            {
                #region save all function body
                int copy = SelectedItem;
                SelectedItem = TabItems.Count - 1;
                while (SelectedItem >= 0)
                {
                    SaveCommand.Execute(null);
                    --SelectedItem;
                }
                SelectedItem = copy;
                #endregion
            });

            OpenCommand = new RelayCommand(o =>
            {
                #region open function body
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files(*.*)|*.*";
                bool opened = false;

                if (openFileDialog.ShowDialog() == true)
                {
                    string path = openFileDialog.FileName;
                    string content = File.ReadAllText(path);
                    string filename = openFileDialog.SafeFileName;
                    if (path != null)
                        opened = true;
                    if (opened)
                        AddNewTabItem(content, filename, path, false);
                }
                if (opened)
                {
                    SelectedItem = TabItems.Count - 1;
                    Saved = true;
                }
                #endregion
            });

            NewCommand = new RelayCommand(o =>
            {
                #region new file function body
                if (newIndex != 0)
                    AddNewTabItem(null, "unnamed" + newIndex.ToString() + ".txt", null, true);
                else
                    AddNewTabItem(null, "unnamed.txt", null, true);

                SelectedItem = TabItems.Count - 1;
                ++newIndex;
                #endregion
            });

            CloseCommand = new RelayCommand(o =>
            {
                #region close function body
                if (TabItems.Any())
                {
                    var tabItem = TabItems[SelectedItem];
                    var content = (tabItem.Content as TextBox).Text;

                    if (content == "" && ItemData[SelectedItem] == null)
                        Saved = true;

                    cancel = false;
                    if (!Saved)
                    {
                        string messageBoxText = "Do you want to save changes?";
                        string caption = TabItems[SelectedItem].Header.ToString();
                        MessageBoxButton button = MessageBoxButton.YesNoCancel;
                        MessageBoxImage icon = MessageBoxImage.Question;
                        MessageBoxResult result;
                        result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);


                        if (result == MessageBoxResult.Yes)
                            SaveCommand.Execute(null);

                        if (result == MessageBoxResult.Cancel)
                            cancel = true;
                    }

                    if (!cancel)
                    {
                        if (TabItems[SelectedItem].Header.ToString() == "Find All")
                            findAllOpened = false;

                        int index;
                        if (TabItems.Count == 1)
                        {
                            index = 0;
                            newIndex = 0;
                        }
                        else
                            index = SelectedItem;

                        if (index == TabItems.Count)
                            --index;

                        TabItems.RemoveAt(index);
                        ItemPaths.RemoveAt(index);
                        ItemData.RemoveAt(index);
                        IsSaved.RemoveAt(index);
                    }
                }
                #endregion
            });

            SaveCloseCommand = new RelayCommand(o =>
            {
                #region save and close function body
                SaveCommand.Execute(null);
                CloseCommand.Execute(null);
                #endregion
            });

            CloseAllCommand = new RelayCommand(o =>
            {
                #region close all function body
                if (TabItems.Any())
                {
                    SelectedItem = TabItems.Count - 1;
                    while (SelectedItem >= 0)
                    {
                        CloseCommand.Execute(null);
                        if (cancel)
                            --SelectedItem;
                    }
                }

                if (!cancel)
                {
                    TabItems.Clear();
                    ItemPaths.Clear();
                    ItemData.Clear();
                    IsSaved.Clear();
                }

                newIndex = 0;
                #endregion
            });

            AboutCommand = new RelayCommand(o =>
            {
                #region about function body
                AboutWindow about = new AboutWindow();
                about.Show();
                #endregion
            });

            FindCommand = new RelayCommand(o =>
            {
                #region find function body
                findWindow = new FindWindow(this);

                findWindow.Show();
                #endregion
            });

            FindAllCommand = new RelayCommand(o =>
            {
                #region find all function body
                findWindow = new FindWindow(this);
                findWindow.find_block.Text = "Find All:";

                findWindow.Show();
                #endregion
            });

            ReplaceCommand = new RelayCommand(o =>
            {
                #region replace function body
                replaceWindow = new ReplaceWindow(this);               
                //findWindow = new FindWindow(this);

                replaceWindow.Show();
                #endregion
            });

            ReplaceAllCommand = new RelayCommand(o =>
            {
                #region replace all function body
                replaceWindow = new ReplaceWindow(this);
                replaceWindow.replace_all_btn.Content = "Replace all:";
                //findWindow = new FindWindow(this);

                replaceWindow.Show();
                #endregion
            });

            FindInputCommand = new RelayCommand(o =>
            {
                #region find button worker
                if (findWindow.find_block.Text == "Find All:")
                    FindAllInputCommand.Execute(null);
                else
                {
                    var tb = (TabItems[SelectedItem].Content as TextBox);
                    var tbText = tb.Text;

                    var index = tbText.IndexOf(ContentText);
                    if (index != -1)
                    {
                        tb.SelectionStart = index;
                        tb.SelectionLength = ContentText.Length;
                    }
                    findWindow.Close();
                }
                ContentText = "";
                #endregion
            });

            FindAllInputCommand = new RelayCommand(o =>
            {
                #region find all button worker
                string content = "";
                string currFileName = "";
                bool found = false;

                for (int tab = 0; tab < TabItems.Count; ++tab)
                {
                    if (findAllOpened && TabItems[tab].Header.ToString() == "Find All")
                    {
                        findAllIndex = tab;                        
                        SelectedItem = findAllIndex;
                        content = (TabItems[tab].Content as TextBox).Text;
                        CloseCommand.Execute(null);
                        --tab;
                        continue;
                    }
                    var tb = TabItems[tab].Content as TextBox;
                    var tbText = tb.Text;
                    indexes.Add(-1);
                    indexes.Add(tab);

                    findWindow.Close();

                    for (int index = 0; ; index += ContentText.Length)
                    {
                        index = tbText.IndexOf(ContentText, index);
                        if (index != -1)
                            indexes.Add(index);
                        else
                            break;
                    }
                }

                for (int index = 0; index < indexes.Count; ++index)
                {
                    if (indexes[index] == -1)
                    {
                        currFileName = TabItems[indexes[++index]].Header.ToString();
                    }
                    else
                    {
                        string curr = "Instance of '" + ContentText + "' found at index " + indexes[index].ToString() + " in file " + currFileName + "\n";
                        found = true;
                        content += curr;
                    }
                }
                if (!found)
                    content += "Instance of '" + ContentText + "' was not found in any files.\n";

                AddNewTabItem(content, "Find All", null, false);
                findAllOpened = true;
                findAllIndex = TabItems.Count - 1;
                SelectedItem = TabItems.Count - 1;
                indexes.Clear();
                //ContentText = "";
                //++SelectedItem;
                #endregion
            });

            ReplaceInput = new RelayCommand(o =>
            {
                #region replace command worker
                if (replaceWindow.replace_all_btn.Content.ToString() == "Replace all:")
                    ReplaceAllInput.Execute(null);
                else
                {
                    var tb = TabItems[SelectedItem].Content as TextBox;
                    var tbText = tb.Text;

                    var index = tbText.IndexOf(ReplaceText);
                    if (index != -1)
                    {
                        tb.SelectionStart = index;
                        tb.SelectionLength = ReplaceText.Length;
                        tb.SelectedText = tb.SelectedText.Replace(ReplaceText, ReplaceToText);
                    }
                }
                replaceWindow.Close();
                ReplaceText = "";
                ReplaceToText = "";
                #endregion
            });

            ReplaceAllInput = new RelayCommand(o =>
            {
                #region replace all commmand worker
                var tb = TabItems[SelectedItem].Content as TextBox;
                var tbText = tb.Text;

                tb.Text = tbText.Replace(ReplaceText, ReplaceToText);

                replaceWindow.Close();
                #endregion
            });
                        
            #endregion            
        }
    }
}
