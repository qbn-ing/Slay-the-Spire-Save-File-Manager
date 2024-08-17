using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using Newtonsoft.Json;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Runtime.InteropServices.JavaScript;
using System.Diagnostics;

namespace WpfApp2
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        readonly string currentPth = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "backups");
        readonly string configFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
        public Window1()
        {
            InitializeComponent();
            getallSaves();
            savesdt.AutoGenerateColumns = false;
            //SaveData sd = new SaveData("D:\\Software\\Steam\\steamapps\\common\\SlayTheSpire\\saves\\IRONCLAD.autosave");
        }
        //这里要解析保存的存档，简略信息，点击可以显示存档详细信息，来让用户选择
        public void getallSaves()
        {
            List<SaveData> saveDataList = [];
            try
            {
                string[] Job = ["IRONCLAD", "THE_SILENT", "DEFECT", "WATCHER"];
                for (int i = 0; i < 4; i++)
                {
                    string filePth = System.IO.Path.Combine(currentPth, Job[i]);
                    if (Directory.Exists(filePth))
                    {
                        string[] files = Directory.GetFiles(filePth);
                        foreach (string file in files)
                        {
                            SaveData saveData = new SaveData(file, Job[i]);
                            saveDataList.Add(saveData);
                        }
                    }
                }
                savesdt.ItemsSource = saveDataList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        private SaveData selectedData;
        public void dt_select(object sender, SelectionChangedEventArgs e)
        {
            if (savesdt.SelectedItem != null)
            {
                selectedData = savesdt.SelectedItem as SaveData;
            }
        }
        public void repleaceSaves(SaveData destSave)
        {
            string configContent = File.ReadAllText(configFilePath);
            string[] files = Directory.GetFiles(configContent,"*.autosave");
            string targetJob = destSave.Job;
            try
            {
                if (files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        string name = System.IO.Path.GetFileNameWithoutExtension(file);
                        if (name == targetJob)
                        {
                            File.Delete(file);
                            File.Copy(destSave.Pth, file);
                        }
                        else
                        {
                            File.Delete(file);
                            File.Delete(System.IO.Path.Combine(configContent, name + ".autosave.backUp"));
                            File.Copy(destSave.Pth, System.IO.Path.Combine(configContent, targetJob + ".autosave"));
                            File.Copy(destSave.Pth, System.IO.Path.Combine(configContent, targetJob + ".autosave.backUp"));
                        }
                    }
                }
                else
                {
                    File.Copy(destSave.Pth, System.IO.Path.Combine(configContent, targetJob + ".autosave"));
                    File.Copy(destSave.Pth, System.IO.Path.Combine(configContent, targetJob + ".autosave.backUp"));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("文件正在被使用，请稍后再试！");
                Console.WriteLine(e.Message);
            }

        }
        public void btn3_click(object sender, RoutedEventArgs e)
        {
            if (selectedData != null)
            {
                //MessageBox.Show($"Name: {selectedData.Name}\n" +
                //                $"Level: {selectedData.Level}\n" +
                //                $"Floor: {selectedData.Floor}\n" +
                //                $"Gold: {selectedData.Gold}\n" +
                //                $"Seed: {selectedData.Seed}\n" +
                //                $"Max Health: {selectedData.MaxHealth}\n" +
                //                $"Current Health: {selectedData.CurrentHealth}\n" +
                //                $"Job: {selectedData.Job}\n" +
                //                $"Date: {selectedData.Date}\n");
                MessageBoxResult result = MessageBox.Show(
                                "确定要替换为当前存档吗？此操作不可撤销！！",
                                "替换存档",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    repleaceSaves(selectedData);
                }


            }
        }
        public void btn5_click(object sender, RoutedEventArgs e)
        {
            if (selectedData != null)
            {
                MessageBoxResult result = MessageBox.Show(
                                "确定要删除当前存档吗？此操作不可撤销！！",
                                "删除存档",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {

                    File.Delete(selectedData.Pth);
                    getallSaves();
                }


            }
        }
        public void btn4_click(object sender, RoutedEventArgs e)
        {
            if (selectedData != null)
            {
                if (File.Exists(selectedData.Pth))
                {
                    // 获取文件所在的目录路径
                    string directoryPath = System.IO.Path.GetDirectoryName(selectedData.Pth);
                    // 打开该目录
                    Process.Start("explorer.exe", directoryPath);
                }
                else
                {
                    MessageBox.Show("文件路径无效或文件不存在。");
                }

            }
        }
    }

    public class SaveData
    {
        public string Name { get; private set; }
        public Int32 CurrentHealth { get; private set; }
        public Int32 MaxHealth { get; private set; }
        public Int32 Level { get; private set; }
        public Int32 Floor { get; private set; }
        public Int32 Gold { get; private set; }
        public Int64 Seed { get; private set; }
        public string Job { get; private set; }
        public string Date { get; private set; }
        public string Pth { get; private set; }
        public SaveData(string pth, string job)
        {
            //MessageBox.Show(pth);
            this.Job = job;
            this.Date = File.GetLastWriteTime(pth).ToString();
            this.Pth = pth;
            string base64String = File.ReadAllText(pth);
            byte[] bytes = Convert.FromBase64String(base64String);
            byte[] bytes1 = { (byte)'k', (byte)'e', (byte)'y' };
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= bytes1[i % 3];
            }
            string saveJson = Encoding.UTF8.GetString(bytes);
            JObject jsonObj;
            try
            {
                jsonObj = JObject.Parse(saveJson);

                //foreach (var property in jsonObj.Properties())
                //{
                //    // 获取键（Key）
                //    string key = property.Name;
                //    // 获取值（Value）
                //    JToken value = property.Value;

                //    // 输出键值对
                //    //MessageBox.Show($"{key}: {value}");
                //    Console.WriteLine($"{key}: {value}");
                //}

                if (jsonObj != null)
                {
                    this.Floor = jsonObj.Value<int?>("floor_num") ?? default(int);
                    this.Seed = jsonObj.Value<long?>("seed") ?? default(int);
                    this.Gold = jsonObj.Value<int?>("gold") ?? default(int);
                    this.Level = jsonObj.Value<int?>("ascension_level") ?? default(int);
                    this.MaxHealth = jsonObj.Value<int?>("max_health") ?? default(int);
                    this.CurrentHealth = jsonObj.Value<int?>("current_health") ?? default(int);
                    this.Name = jsonObj.Value<string>("name");
                    //MessageBox.Show($"Name: {this.Name}\n" +
                    //                $"Level: {this.Level}\n" +
                    //                $"Floor: {this.Floor}\n" +
                    //                $"Gold: {this.Gold}\n" +
                    //                $"Seed: {this.Seed}\n" +
                    //                $"Max Health: {this.MaxHealth}\n" +
                    //                $"Current Health: {this.CurrentHealth}\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
