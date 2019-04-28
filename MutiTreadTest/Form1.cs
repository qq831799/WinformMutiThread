using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MutiTreadTest
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int add(int x, int y) => x + y;
            Dictionary<string, string[]> test(Object a) => new Dictionary<string, string[]>();
            Func<Object, Dictionary<string, string[]>> fileProcess = ((folder) =>
            {
                var folderInfo = (KeyValuePair<string, string[]>)folder;

                List<Task<string>> childTasks = new List<Task<string>>();
                List<string> fileArray = new List<string>();
                foreach (var file in folderInfo.Value)
                {
                    childTasks.Add(Task<string>.Factory.StartNew((name) =>
                    {
                        Random random = new Random(Guid.NewGuid().GetHashCode());
                        int sleepTime = random.Next(1, 5);
                        Console.WriteLine($"{folderInfo.Key} - {name} Sleep {sleepTime} Sec");
                        Thread.Sleep(sleepTime * 1000);
                        Console.WriteLine($"{folderInfo.Key} - {name} wake up");
                        return $"{name} file";
                    }, file));
                }
                return new Dictionary<string, string[]>() { { folderInfo.Key, childTasks.Select(task => task.Result).ToArray() } };
            });

            statusLabel.Text = "處理中";
            Dictionary<string, string[]> folderAndFiles = new Dictionary<string, string[]>()
            {
                {"Folder A",new string[] { "AA thread", "AB thread", "AC thread" } },
                {"Folder B",new string[] { "BA thread", "BB thread"} },
                {"Folder C",new string[] { "CA thread", "CB thread", "CC thread" } },
            };
            List<Task<Dictionary<string, string[]>>> parentTasks = new List<Task<Dictionary<string, string[]>>>();
            foreach (var folder in folderAndFiles)
            {
                parentTasks.Add(Task<Dictionary<string, string[]>>.Factory.StartNew(fileProcess, folder));               

            }
            parentTasks.ForEach(task => task.ContinueWith(t =>
            {
                foreach (var result in t.Result)
                {
                    foreach (var file in result.Value)
                    {
                        resultGrid.Rows.Add(result.Key, file);
                    }
                }
                
            }, TaskScheduler.FromCurrentSynchronizationContext()));

            Task.WhenAll(parentTasks.ToArray()).ContinueWith((t)=> statusLabel.Text = "完成", TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void button2_Click(object sender, EventArgs e)
        {
            resultGrid.Rows.Clear();
        }
    }
}
