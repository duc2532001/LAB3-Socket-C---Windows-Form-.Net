﻿using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace Menu
{
    public partial class Form_Bai4_TCPClient : Form
    {
        TcpClient client;
        IPEndPoint remote_ipe;

        Stream stream;
        StreamWriter writer;

        Thread thread_receiving_handle;

        IPAddress remote_ip;
        int remote_port;

        public class Infor
        {
            public string Name { get; set; }
            public string Message { get; set; }
        }
        /// <summary>
        /// Child functions
        /// </summary>

        private void Show_ERROR(string err)
        {
            MessageBox.Show(err, "Error");
        }

        public delegate void Add_Items(ListBox lb, string info);
        public void Infor_Message(ListBox lb, string info)
        {
            if (lb.InvokeRequired)
            {
                Add_Items d = new Add_Items(Infor_Message);
                this.Invoke(d, new object[] { lb, info });
            }
            else
            {
                lb_message.Items.Add(info);
            }
        }

        public void Send_Message(string name, string message)
        {
            Infor infor = new Infor
            {
                Name = name,
                Message = message
            };
            string infor_str = JsonConvert.SerializeObject(infor);

            try
            {
                if (name == string.Empty)
                {
                    Show_ERROR("Name is empty");
                    return;
                }

                if (message != string.Empty)
                {
                    writer.WriteLine(infor_str);

                    Infor_Message(lb_message, message);

                    txt_message.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Show_ERROR(ex.Message);
            }
        }

        public void Receiving_Handle(object o)
        {
            TcpClient client = (TcpClient)o;
            StreamReader reader = new StreamReader(client.GetStream());

            while (true)
            {
                try
                {
                    string data_str = reader.ReadLine();
                    Infor infor = JsonConvert.DeserializeObject<Infor>(data_str);

                    if (data_str != string.Empty)
                        Infor_Message(lb_message, $"{infor.Name}: {infor.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    client.Close();
                    break;
                }
            }
        }

        public void Close_Connection()
        {
            try
            {
                thread_receiving_handle.Abort();

                stream.Close();
                client.Close();
                
                Infor_Message(lb_message, "Disconnected");

                btn_connect.Enabled = true;
                btn_disconnect.Enabled = false;
                btn_send.Enabled = false;

                txt_remoteIP.Enabled = true;
                txt_remotePort.Enabled = true;
                txt_message.Enabled = false;
                txt_name.Enabled = true;
            }
            catch (Exception ex)
            {
                Show_ERROR(ex.Message);
            }
        }

        /// <summary>
        /// Main functions
        /// </summary>

        public Form_Bai4_TCPClient()
        {
            InitializeComponent();
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (txt_name.Text == string.Empty)
            {
                Show_ERROR("Name is empty");
                return;
            }

            try
            {
                remote_ip = IPAddress.Parse(txt_remoteIP.Text);
                remote_port = int.Parse(txt_remotePort.Text);
                remote_ipe = new IPEndPoint(remote_ip, remote_port);

                client = new TcpClient();
                client.Connect(remote_ipe);

                stream = client.GetStream();
                writer = new StreamWriter(stream);
                writer.AutoFlush = true;

                // Send indentify name
                writer.WriteLine(txt_name.Text);

                // Create receiving handle thread
                thread_receiving_handle = new Thread(Receiving_Handle);
                thread_receiving_handle.Start(client);

                btn_connect.Enabled = false;
                btn_disconnect.Enabled = true;
                btn_send.Enabled = true;

                txt_remoteIP.Enabled = false;
                txt_name.Enabled = false;
                txt_remotePort.Enabled = false;
                txt_message.Enabled = true;

                Infor_Message(lb_message, $"Connected to {remote_ip}:{remote_port}");
            }
            catch (Exception ex)
            {
                Show_ERROR(ex.Message);
                Infor_Message(lb_message, "Connection was aborted");
            }
            finally
            {
                btn_clearMessage.Enabled = true;
            }
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            Send_Message(txt_name.Text, txt_message.Text);
        }

        private void btn_clearMessage_Click(object sender, EventArgs e)
        {
            lb_message.Items.Clear();
        }

        private void btn_disconnect_Click(object sender, EventArgs e)
        {
            Close_Connection();
        }
    }
}
