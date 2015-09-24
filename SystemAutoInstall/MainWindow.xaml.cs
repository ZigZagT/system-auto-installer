using System;
using System.Threading;
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
using System.Diagnostics;

namespace SystemAutoInstall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer timer;
        public SoftwareList InstallList;
        bool cancelClose = false;
        public MainWindow()
        {
            InitializeComponent();
            ThreadControl.Task(init, () => { Dispatcher.Invoke(activeUI); });
        }

        public void init()
        {
            InstallList = new SoftwareList();
            Thread.Sleep(1000);
        }

        public void activeUI()
        {
            foreach (var software in InstallList.Content) {
                var line = new ListViewItem();
                line.Tag = software;
                line.Content = software.Name;
                ListView_list.Items.Add(line);
                line.IsSelected = software.IsSelected;
                line.ToolTip = software.Description;
            }

            String baseText = "请选中需要安装的软件，点击\"安装\"。\n";
            int timeout = 10;
            TimerCallback timerCallback = (obj) =>
            {
                Dispatcher.Invoke(() =>
                {
                    /*for (int i = 0; i < timeout; ++i)
                    {
                        if (Button_Install.Visibility == Visibility.Hidden)
                        {
                            return;
                        }
                        TextBox_Msg.Text = baseText + (timeout - i) + "秒后执行默认安装" + Environment.NewLine;
                    }*/
                    TextBox_Msg.Text = baseText + (timeout) + "秒后执行默认安装" + Environment.NewLine;
                    if (timeout != 0)
                    {
                        timeout--;
                    }
                    else
                    {
                        timer.Dispose();
                        Button_Install_Click(new object(), new RoutedEventArgs());
                        //MessageBox.Show("123");
                    }
                    
                });
            };
            /*TimerCallback timerCallback = (obj) =>
            {
                MessageBox.Show("show");
            };*/
            timer = new Timer(timerCallback, null, 0, 1000);

            TextBox_Msg.Text = baseText;

            ListView_list.IsEnabled = true;
            Button_Install.IsEnabled = true;
            ListView_list.Focus();
        }

        private void Button_Install_Click(object sender, RoutedEventArgs e)
        {
            timer.Dispose();
            cancelClose = true;
            ListView_list.IsEnabled = false;
            Button_Install.Visibility = Visibility.Hidden;
            PrograssBar_Install.Visibility = Visibility.Visible;
            //PrograssBar_Install.IsIndeterminate = true;
            TextBox_Msg.Text = "正在安装...";
            int counter = 0;
            foreach (ListViewItem item in ListView_list.Items)
            {
                if (item.IsSelected)
                {
                    counter++;
                }
            }
            if (counter == 0)
            {
                PrograssBar_Install.Value = 100;
                exit();
                return;
            }
            int PrograssBar_Step = 100 / counter;
            int installCounter = 0;
            PrograssBar_Install.Value = 0;
            Action installEnd = () =>
            {
                installCounter++;
                PrograssBar_Install.Value += PrograssBar_Step;
                TextBox_Msg.Text += "\n已完成 " + installCounter + "/" + counter;
                if (installCounter == counter)
                {
                    cancelClose = false;
                    PrograssBar_Install.Value = 100;
                    exit();
                }
            };

            foreach (ListViewItem item in ListView_list.Items)
            {
                if (item.IsSelected)
                {
                    TextBox_Msg.Text += "\n正在执行： " + ((SoftwareList.SoftwareItem)item.Tag).Name;
                    SoftwareList.install((SoftwareList.SoftwareItem)item.Tag, (() => { Dispatcher.Invoke(installEnd); }));
                }
            }
        }

        private void Button_Test_Click(object sender, RoutedEventArgs e)
        {
            TextBox_Msg.Text = InstallList.TypeData;
        }

        public void exit()
        {
            MessageBox.Show("安装完成");
            //this.Close();
            Application.Current.Shutdown();
        }

        private void TextBox_Msg_TextChanged(object sender, TextChangedEventArgs e)
        {
            var self = (TextBox)sender;
            self.ScrollToEnd();
        }

        private void TextBox_Msg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timer.Dispose();
        }

        private void ListView_list_LostFocus(object sender, RoutedEventArgs e)
        {
            timer.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = cancelClose;
            if (cancelClose)
            {
                MessageBox.Show("正在执行关键进程，无法退出");
            }
        }
    }
}
