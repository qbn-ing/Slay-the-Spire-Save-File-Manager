using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Security.Cryptography;

using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Policy;
using System.Runtime.CompilerServices;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string savePth = @"\\SlayTheSpire\\saves";
        const string savebackupPth = "backups";
        string gamePth = "";
        readonly string currentPth =  Directory.GetCurrentDirectory();
        //readonly string basefilename = DateTime.Now.ToString("yyyyMMddhhmm");

        CancellationTokenSource source;
        CancellationToken token;
        //Task thread;
        
        public MainWindow()
        {
            source = new CancellationTokenSource();
            token = source.Token;
            //thread = new Thread(() => listenSaves(token,gamePth, System.IO.Path.Combine(currentPth, savebackupPth)));
            InitializeComponent();
            LoadOrCreateConfigFile();
            if (!Directory.Exists(System.IO.Path.Combine(currentPth, savebackupPth)))
            {
                // 文件夹不存在，创建新的文件夹
                Directory.CreateDirectory(System.IO.Path.Combine(currentPth, savebackupPth));
            }
            string[] Jobs = ["IRONCLAD", "THE_SILENT", "DEFECT", "WATCHER"];
            for (int i = 0; i < 4; i++)
            {
                string filePth = System.IO.Path.Combine(System.IO.Path.Combine(currentPth, savebackupPth), Jobs[i]);
                if (!Directory.Exists(filePth))
                {
                    Directory.CreateDirectory(filePth);
                }
            }
        }
        private void LoadOrCreateConfigFile()
        {
            string directoryPath = AppDomain.CurrentDomain.BaseDirectory; // 获取应用程序的基目录
            string configFilePath = System.IO.Path.Combine(directoryPath, "config.txt"); // 配置文件路径

            if (File.Exists(configFilePath))
            {
                string configContent = File.ReadAllText(configFilePath);
                Console.WriteLine("配置文件" + configContent);
                if (Regex.IsMatch(configContent, savePth))
                {
                    gamePth = configContent;
                    Console.WriteLine("配置的目录有效：" + configContent);
                }
                else
                {
                    SelectAndSaveDirectory(configFilePath);
                }
            }
            else
            {
                SelectAndSaveDirectory(configFilePath);
                string configContent = File.ReadAllText(configFilePath);
                gamePth = configContent;
            }
        }
        private static void SelectAndSaveDirectory(string configFilePath)
        {
            Microsoft.Win32.OpenFolderDialog dialog = new();
            dialog.Multiselect = false;
            dialog.Title = "选择SlayTheSpire/saves目录";

            bool? result = false;
            while (result != true)
            {
                result = dialog.ShowDialog();
                if (result == false)
                {
                    Environment.Exit(0);
                }
                else if (!Regex.IsMatch(dialog.FolderName, savePth))
                {
                    MessageBox.Show("请选择SlayTheSpire/saves目录。");
                    result = false;
                }
            }
            string fullPathToFolder = dialog.FolderName;
            try
            {
                using (StreamWriter sw = File.CreateText(configFilePath))
                {
                    sw.Write(fullPathToFolder);
                }
                Console.WriteLine("配置文件已创建。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("写入配置文件时出错: " + ex.Message);
            }
        }
        private void btn1Click(object sender, RoutedEventArgs e)
        {
            Button? btn1 = sender as Button;
            if (btn1 != null)
            {
                if (btn1.Content.ToString() == "开始备份存档")
                {
                    
                    btn1.Content = "停止备份存档";
                    source = new CancellationTokenSource();
                    token = source.Token;

                    Task mytask = Task.Run(() => listenSaves(token, gamePth, System.IO.Path.Combine(currentPth, savebackupPth)),token);

                    // 启动线程
                    //thread.Start();
                    
                    MessageBox.Show("开始备份！");
                }
                else
                {
                    btn1.Content = "开始备份存档";
                    MessageBox.Show("已停止！");
                    source.Cancel();
                }   
            }
            else
                MessageBox.Show("按钮为空！");
        }
        private void btn2Click(object sender, RoutedEventArgs e)
        {
            if (!token.IsCancellationRequested)
                source.Cancel();
            Button btn1 = (Button)FindName("btn1");
            if(btn1.Content.ToString() == "停止备份存档")
            {
                MessageBox.Show("已停止！");
                btn1.Content = "开始备份存档";
            }
            
            this.Visibility = Visibility.Hidden;
            Window1 window1 = new Window1();
            window1.Closed += (s, args) =>
            {
                this.Visibility = Visibility.Visible;
            };
            window1.Show();
        }
        private static void listenSaves(CancellationToken token,string gamesavePth,string bkPth)
        {
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine("线程正在执行。");
                //gamesavePth = gamesavePth.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                string[] result = Directory.GetFiles(gamesavePth, "*.autosave");
                //接下来这里每5s读取一次存档，与上次进行比较，不同就保存新的
                if (result.Length<=0)
                {
                    Console.WriteLine("游戏还没有存档");
                    Thread.Sleep(100);
                    continue;
                }
                string sourcePth = result[0];
                //MessageBox.Show("发现存档！" + sourcePth);
                string desthead = System.IO.Path.GetFileNameWithoutExtension(sourcePth);
                string destionPth = System.IO.Path.Combine(System.IO.Path.Combine(bkPth,desthead), DateTime.Now.ToString("yyyyMMddHHmmss") + ".save");
                string hash0 = hashFile(sourcePth);
                
                if (!DirectoryHasFiles(System.IO.Path.Combine(bkPth, desthead)))
                {
                    try
                    {
                        // 复制文件到目标目录，并使用当前时间戳作为文件名
                        File.Copy(sourcePth, destionPth);
                        Console.WriteLine($"文件已成功复制到 '{destionPth}'");
                        //MessageBox.Show($"文件已成功复制到 '{destionPth}'");
                        //MessageBox.Show("复制文件完成！ " );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("复制文件时发生错误: " + ex.Message);
                        //MessageBox.Show("复制文件时发生错误: " + ex.Message);
                    }
                }
                else
                {
                    FileInfo latestFile = FindLatestFileByNameTimestamp(System.IO.Path.Combine(bkPth, desthead));
                    if (latestFile != null)
                    {
                        string hash2 = hashFile(latestFile.FullName);
                        //MessageBox.Show("hash0: " + hash0);
                        //MessageBox.Show("hash2: " + hash2);
                        if (hash0 != hash2)
                        {
                            //MessageBox.Show("newest file: " + latestFile.FullName);
                            //MessageBox.Show("hash 不相等");
                            destionPth = System.IO.Path.Combine(System.IO.Path.Combine(bkPth, desthead), DateTime.Now.ToString("yyyyMMddHHmmss") + ".save");
                            try
                            {
                                // 复制文件到目标目录，并使用当前时间戳作为文件名
                                File.Copy(sourcePth, destionPth);
                                Console.WriteLine($"文件已成功复制到 '{destionPth}'");
                                //MessageBox.Show($"文件已成功复制到 '{destionPth}'");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("复制文件时发生错误: " + ex.Message);
                                //MessageBox.Show("复制文件时发生错误: " + ex.Message);
                            }
                            
                        }
                        //else
                        //{
                        //    MessageBox.Show("存档没有变化！");
                        //}
                    }
                }

                Thread.Sleep(50);
            }
            Console.WriteLine("线程已取消。");
        }
        static bool DirectoryHasFiles(string directoryPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            return directoryInfo.GetFiles().Length > 0;
        }

        static FileInfo FindLatestFileByNameTimestamp(string directoryPath)
        {
            var di = new DirectoryInfo(directoryPath);//文件夹所在目录
            FileInfo[] fileList = di.GetFiles();
            if (fileList.Length > 0)
            {
                Array.Sort(fileList, new Comparison<FileInfo>((x, y) => x.CreationTime.CompareTo(y.CreationTime)));
                return fileList[fileList.Length - 1];
            }
            return null;
        }
        // 计算文件的哈希值
        static string hashFile(string filePath)
        {
            //using (HashAlgorithm hash = HashAlgorithm.Create())
            //{
            //    using (FileStream file1 = new FileStream(filePath, FileMode.Open))
            //    {
            //        byte[] hashByte1 = hash.ComputeHash(file1);
                    
            //    }
            //}
            using (var sha256 = SHA256.Create())
            {
                using (FileStream file1 = new FileStream(filePath, FileMode.Open))
                {
                    byte[] hash = sha256.ComputeHash(file1);
                    return BitConverter.ToString(hash).Replace("-", String.Empty);

                }
                
            }
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://blog.csdn.net/chouzhou9701/article/details/121235873";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // 处理可能发生的异常
                MessageBox.Show($"无法打开链接：{ex.Message}");
            }
        }
        private void Hyperlink_Click2(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/qbn-ing/Slay-the-Spire-Save-File-Manager/";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // 处理可能发生的异常
                MessageBox.Show($"无法打开链接：{ex.Message}");
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            source.Cancel();
        }
    }
}