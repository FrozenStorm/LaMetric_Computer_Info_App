using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Drawing;

namespace LaMetricComputerInfoApp
{
    public partial class Form1 : Form
    {
        const int CHARTLENGTH = 37;
        int temp = 0;
        int tickCount = 0;
        PerformanceCounter cpuCounter;
        List<double> myCpuLoad;

        Configuration myConfiguration = new Configuration();

        NotifyIcon symbol = new NotifyIcon();
        ContextMenuStrip cms = new ContextMenuStrip();
        ToolStripMenuItem tsmi_close = new ToolStripMenuItem();
        ToolStripMenuItem tsmi_config = new ToolStripMenuItem();

        public Form1()
        {
            InitializeComponent();

            symbol.Icon = SystemIcons.Application;
            symbol.Visible = true;

            symbol.ContextMenuStrip = cms;

            tsmi_config.Text = "configuration";
            tsmi_config.Enabled = true;
            cms.Items.Add(tsmi_config);
            tsmi_config.Click += new EventHandler(tsmi_config_click);

            tsmi_close.Text = "close";
            tsmi_close.Enabled = true;
            cms.Items.Add(tsmi_close);
            tsmi_close.Click += new EventHandler(tsmi_close_click);

            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            myCpuLoad = new List<double>();
            myCpuLoad.Add(100);
            myCpuLoad.Add(0);
            for(int i = 2; i < CHARTLENGTH; i++)
            {
                myCpuLoad.Add(0);
            }
        }

        private void tsmi_close_click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsmi_config_click(object sender, EventArgs e)
        {
            myConfiguration.ShowDialog();
        }

        private string sendToLaMetric(string frame, string accessToken, string widgetId, string callback)
        {
            string postURL = "https://developer.lametric.com/api/V1/dev/widget/update/" + widgetId;
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(postURL);
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Headers.Add("X-Access-Token", accessToken);
                httpWebRequest.Headers.Add("Cache-Control", "no-cache");

                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(string.Format("{0}", frame));
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string report;
            double cpuLoad = cpuCounter.NextValue();
            double ramLoad = 100 - (double)PerformanceInfo.GetPhysicalAvailableMemoryInMiB() / PerformanceInfo.GetTotalMemoryInMiB() * 100;
            string s_cpuload = "";

            temp++;
            string s_temp = cpuLoad.ToString().Split('.')[0] + "°";
            string s_memory = "RAM " + ramLoad.ToString().Split('.')[0] + "%";

            myCpuLoad.Add(cpuLoad);
            if (myCpuLoad.Count > CHARTLENGTH) myCpuLoad.RemoveAt(2);
            for(int i=0; i < myCpuLoad.Count; i++)
            {
                s_cpuload += myCpuLoad[i].ToString().Split('.')[0];
                if (i < myCpuLoad.Count - 1) s_cpuload += ',';
            }

            string accessToken = "ZDI1NTY4NTZjNGI4NjA3NjA0Y2E0ZmY0OGQ3ZDQzNGQxZTNjOTU1NmIzYjBmNTI4NjgyMmU2ZTQ4ZjdiNDVkNA=="; // a base64 encoded id ends with ==
            string widgetId = "com.lametric.1562db3bbb0ff3bec28d53c28e82959d/3"; // basicly a URL of the Widget (app) without https://developer.lametric.com/api/V1/dev/widget/update/
            string data = string.Format("{{\"frames\": [{{ \"index\": 0, \"chartData\": [{0}] }}, {{ \"text\": \"{1}\", \"icon\": \"null\", \"index\": 1 }}, {{ \"text\": \"{2}\", \"icon\": \"i2056\", \"index\": 2 }}] }}", s_cpuload, s_memory, s_temp); //i294

            tickCount++;
            if (tickCount == 10)
            {
                tickCount = 0;
                report = sendToLaMetric(data, accessToken, widgetId, "www.google.ch");
                l_send.Text = data;
            }
        }
    }
}
