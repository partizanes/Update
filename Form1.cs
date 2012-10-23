using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Update : Form
    {
        public Update()
        {
            InitializeComponent();
        }

        private void Update_Load(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void log_write(string str, string reason, string logname)
        {
            string EntryTime = DateTime.Now.ToLongTimeString();
            string EntryDate = DateTime.Today.ToShortDateString();
            string fileName = "log/" + EntryDate + "/" + logname + ".log";  //log + data +logname ? 

            if (!Directory.Exists(Environment.CurrentDirectory + "/log/" + EntryDate + "/"))
            {
                Directory.CreateDirectory((Environment.CurrentDirectory + "/log/" + EntryDate + "/"));
            }

            try
            {
                StreamWriter sw = new StreamWriter(fileName, true, System.Text.Encoding.UTF8);
                sw.WriteLine("[" + EntryDate + "][" + EntryTime + "][" + reason + "]" + " " + str);
                sw.Close();
            }
            catch (Exception ex)
            {
                log_write(ex.Message, "Exception", "Exception");
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            log_write("Выход из прогаммы обновления ", "INFO", "Update");
            Application.Exit();
        }

    }
}
