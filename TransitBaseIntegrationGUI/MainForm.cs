using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransitBaseIntegration;

namespace BkvDbConvertGUI
{
    public partial class MainForm : Form
    {
        public static MainForm Current;

        public MainForm()
        {
            Current = this;
            InitializeComponent();
            try
            {
                this.selectedFile.Text = Path.GetFileName(LinksFile);
            }
            catch (IOException) { }
        }

        public string LinksFile
        {
            get
            {
                if (!File.Exists("lastFile.txt"))
                    throw new IOException();
                using (StreamReader lastFileReader = new StreamReader("lastFile.txt"))
                {
                    return lastFileReader.ReadLine();
                }
            }
            set
            {
                using (StreamWriter lastFileWriter = new StreamWriter(new FileStream("lastFile.txt", FileMode.Create, FileAccess.Write)))
                {
                    lastFileWriter.WriteLine(value);
                }
                this.selectedFile.Text = Path.GetFileName(value);
            }
        }

        private async void startBtn_Click(object sender, EventArgs e)
        {
            try
            {
                DataTask task = new DataTask(LinksFile, !skipCheckBox.Checked);
                task.Log += ProgressChanged;

                await Task.Run(async () =>
                    {
                        await task.Execute();
                    });
            }
            catch (IOException ex)
            {
                MessageBox.Show("Hiba: " + ex.Message);
            }
        }

        void ProgressChanged(int progress, string message)
        {
            this.Invoke((MethodInvoker)(() =>
                {
                    if (message != null)
                    {
                        log.Text = currentTaskLabel.Text + "\n" + log.Text;
                        currentTaskLabel.Text = message;
                    }
                    else
                        progressBar.Value = progress;
                }));
        }

        private void edit_Click(object sender, EventArgs e)
        {
            OpenFileDialog linksFile = new OpenFileDialog();
            linksFile.DefaultExt = "txt";
            linksFile.CheckFileExists = true;
            linksFile.InitialDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "links";
            if (linksFile.ShowDialog() != DialogResult.OK)
                return;
            LinksFile = linksFile.FileName;
        }
    }
}
