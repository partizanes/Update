using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Update : Form
    {
        public Update()
        {
            InitializeComponent();
        }

        private MySqlCommand	cmd;
        private MySqlConnection serverConn;
        private string connStr;
        private bool status = true;

        //import dll from use configuration file
        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileString(
        string lpAppName,
        string lpKeyName,
        string lpDefault,
        StringBuilder lpReturnedString,
        uint nSize,
        string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WritePrivateProfileString(string lpAppName,
           string lpKeyName, string lpString, string lpFileName);

        private void Update_Shown(object sender, EventArgs e)
        {
            //para for progress bar
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Step = 10;

            progressBar1.PerformStep();
            Thread.Sleep(600);

            StringBuilder buffer = new StringBuilder(50, 50);

            GetPrivateProfileString("SETTINGS", "srv_local", "192.168.1.11", buffer, 50, Environment.CurrentDirectory + "\\config.ini");
            connStr = string.Format("server={0};uid={1};pwd={2};database={3};", buffer, "pricechecker", "***REMOVED***", "action");
            serverConn = new MySqlConnection(connStr);

            GetPrivateProfileString("SETTINGS", "program", "null", buffer, 50, Environment.CurrentDirectory + "\\config.ini");

            query(buffer.ToString());
        }

        private void query(string name)
        {
            progressBar1.PerformStep();
            Thread.Sleep(600);

            MySqlDataReader reader;

            try
            {
                serverConn.Open();

                cmd = new MySqlCommand("SELECT source FROM VERSION WHERE NAME = '" + name + "'", serverConn);

                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    downloader(reader.GetString(0), "%TEMP%");

                    //log_write(reader.GetString(0), "Source", "pc");
                }
                else
                {
                    log_write("ВНИМАНИЕ! Путь к обновлению не задан на сервере", "Source", "pc");
                    MessageBox.Show("ВНИМАНИЕ! Путь к обновлению не задан на сервере");

                    WritePrivateProfileString("SETTINGS", "status", "0", Environment.CurrentDirectory + "\\config.ini");
                    Application.Exit();
                }
            }
            catch (Exception exc)
            {
                log_write(exc.Message, "Exception", "Exception");
                MessageBox.Show(exc.Message);

                WritePrivateProfileString("SETTINGS", "status", "0", Environment.CurrentDirectory + "\\config.ini");
            }
            finally
            {
                if (serverConn.State == ConnectionState.Open)
                    serverConn.Close();

                progressBar1.PerformStep();
                Thread.Sleep(600);
            }
        }

        private void downloader(string _URL, string _SaveAs)
        {
            progressBar1.PerformStep();
            Thread.Sleep(600);

            WebClient myWebClient = new WebClient();
            string downloadFileName = System.IO.Path.GetFileName(_URL);

            try
            {
                myWebClient.DownloadFile(_URL, "_" + downloadFileName);

                while (myWebClient.IsBusy)
                {
                    Application.DoEvents();
                }
            }
            catch (System.Exception exc)
            {
                log_write(exc.Message, "EXCEPTION", "pc");

                WritePrivateProfileString("SETTINGS", "status", "0", Environment.CurrentDirectory + "\\config.ini");
            }
            finally
            {
                progressBar1.PerformStep();
                copy(downloadFileName);
            }

        }

        private void copy(string name)
        {
            try
            {
                progressBar1.PerformStep();

                while (File.Exists(name))
                {
                    if (File.Exists("backup_" + name))
                    {
                        File.Delete("backup_" + name);
                        Thread.Sleep(100);
                    }

                    log_write("Делаем копию файла", "INFO", "pc");
                    File.Move(name, "backup_" + name);
                    Thread.Sleep(300);

                    log_write("Удаляем оригинальный файл", "INFO", "pc");
                    File.Delete(name);
                    Thread.Sleep(300);
                }

                progressBar1.PerformStep();

                if (File.Exists("_" + name))
                {
                    File.Copy("_" + name, name);
                    Thread.Sleep(600);
                }

                if (File.Exists(name))
                {
                    log_write("Запускаем обновленное приложение", "INFO", "pc");

                    Thread.Sleep(300);
                    System.Diagnostics.Process.Start(name);

                    WritePrivateProfileString("SETTINGS", "status", "1", Environment.CurrentDirectory + "\\config.ini");

                    if (File.Exists("_" + name))
                    {
                        File.Delete("_" + name);
                        log_write("Удаляем временный файл", "INFO", "pc");
                    }

                    log_write("Выходим из утилиты обновления", "INFO", "pc");

                    Application.Exit();
                }
                else
                {
                    log_write("Ошибка при обновлении приложения!", "EXCEPTION", "pc");
                    MessageBox.Show("Внимание!При обновлении произошла ошибка,обратитесь к системному администратору!");

                    //check this
                    revert_update(name);
                }
            }
            catch (System.Exception exс)
            {
                log_write(exс.Message, "EXCEPTION", "pc");
                status = false;

                WritePrivateProfileString("SETTINGS", "status", "0", Environment.CurrentDirectory + "\\config.ini");
            }
            finally
            {
                if(status)
                {
                    Thread.Sleep(600);
                    log_write("Успешно!", "INFO", "pc");
                    progressBar1.Value = 100;
                    Thread.Sleep(3000);
                    Application.Exit();
                }
            }

        }

        private void revert_update(string name)
        {
            try
            {
                WritePrivateProfileString("SETTINGS", "status", "1", Environment.CurrentDirectory + "\\config.ini");

                log_write("Возвращаем все обратно", "INFO", "pc");
                File.Move("backup_" + name, name);
                Thread.Sleep(300);

                log_write("Удаляем запасной файл", "INFO", "pc");
                File.Delete("backup" + name);
                Thread.Sleep(300);

                if (File.Exists(name))
                {
                    Thread.Sleep(300);
                    System.Diagnostics.Process.Start(name);
                    WritePrivateProfileString("SETTINGS", "status", "1", Environment.CurrentDirectory + "\\config.ini");
                }
            }
            catch (System.Exception exc)
            {
                log_write(exc.Message, "EXCEPTION", "pc");
            }
            finally
            {
                Application.Exit();
            }
        }

        private void log_write(string str, string reason, string logname)
        {
            string EntryTime = DateTime.Now.ToLongTimeString();
            string EntryDate = DateTime.Today.ToShortDateString();
            string fileName = "log/" + logname + ".log";  //log + data +logname ? 

            if (!Directory.Exists(Environment.CurrentDirectory + "/log/"))
            {
                Directory.CreateDirectory((Environment.CurrentDirectory + "/log/"));
            }

            try
            {
                StreamWriter sw = new StreamWriter(fileName, true, System.Text.Encoding.UTF8);
                sw.WriteLine("[" + EntryDate + "][" + EntryTime + "][" + reason + "]" + " " + str);
                sw.Close();
            }
            catch (Exception exc)
            {
                log_write(exc.Message, "Exception", "Exception");
                MessageBox.Show(exc.Message);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            log_write("Выход из прогаммы обновления ", "INFO", "pc");
            this.Hide();
            Application.Exit();
        }

        private void Update_Load(object sender, EventArgs e)
        {
            log_write("Программа обновления запущена ", "INFO", "pc");
        }
    }
}
