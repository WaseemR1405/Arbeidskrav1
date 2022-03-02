using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Microsoft.VisualBasic;
using System.IO;
using System.Windows.Forms;

namespace Arbeidskrav1
{
    public partial class Arbeidskrav1 : Form
    {
        List<float>analogreading = new List<float>();
        List<float> timestamp = new List<float>();

        List<float>readscaled = new List<float>();
        List<float>yreading = new List<float>();

        float xval = 0;
        float x_verdier = 0;

        string tingtang;

        public Arbeidskrav1()
        {
            InitializeComponent();
            comport_combobox.Items.AddRange(SerialPort.GetPortNames());
            comport_combobox.Text = "--Select--";
            string[] bitrate = new string[] { "1200", "2400", "4800", "9600", "19200" };
            bitrate_combobox.Items.AddRange(bitrate);
            bitrate_combobox.SelectedIndex = bitrate_combobox.Items.IndexOf("9600");
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataRecievedHandler);

            timer1.Interval = 5000;
            timer1.Tick += new EventHandler(timer1_tick);

            timer2.Interval = 5000;
            timer2.Tick += new EventHandler(timer2_tick);

            timer3.Interval = 1000;
            timer3.Tick += new EventHandler(timer3_tick);

            
            xval = 0;
            x_verdier = 0;


        }

        private void timer1_tick(object sender, EventArgs e)
        {
            serialPort1.WriteLine("readraw");
            tingtang = "readraw";
        }

        private void timer2_tick(object sender, EventArgs e)
        {
            serialPort1.WriteLine("readscaled");
            tingtang = "readscaled";
        }

        private void timer3_tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                config_status.Clear();
                sc_status_box.Clear();
                monit_status.Clear();
                config_status.AppendText("Connected");
                sc_status_box.AppendText("Connected");
                monit_status.AppendText("Connected");
                confi_light.BackColor = Color.Green;
                sc_statuslight.BackColor = Color.Green;
                monit_light.BackColor = Color.Green;
            }

            if (serialPort1.IsOpen == false)
            {
                timer3.Stop();
                config_status.Clear();
                sc_status_box.Clear();
                monit_status.Clear();
                MessageBox.Show("Connection lost, check connection");
                config_status.AppendText("Disconnected");
                sc_status_box.AppendText("Disconnected");
                monit_status.AppendText("Disconnected");
                confi_light.BackColor = Color.Red;
                sc_statuslight.BackColor = Color.Red;
                monit_light.BackColor = Color.Red;

            }
        }

        private void comport_Click(object sender, EventArgs e)
        {
            comport_combobox.Items.Clear();
            comport_combobox.Items.AddRange(SerialPort.GetPortNames());
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();
                serialPort1.PortName = comport_combobox.Text;
                while (serialPort1.IsOpen) ;
                serialPort1.Open();
                textBox1.Text = (" Tilkoblet" + " " + comport_combobox.Text + "\r\n");
                MessageBox.Show("Connection Successful Established");
                timer3.Start();
            }


            catch (Exception ex)
            {
                serialPort1.Close();
                if (ex is ArgumentException)
                {
                    MessageBox.Show("Ikke gyldig Port valgt, vennligst velg en annen port");
                }

                else if (ex is UnauthorizedAccessException)
                {
                    MessageBox.Show("Ikke tilkoblet. Porten kan være i bruk");
                }

                else
                {
                    MessageBox.Show("Ikke gyldig Port valgt, vennligst velg en annen port");
                }
               
            }

        }

        private void discon_button_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            textBox1.Text = "Frakoblet";
        }


        void DataRecievedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string DataRecieved = ((SerialPort)sender).ReadLine();
            textBox1.Invoke((MethodInvoker)delegate { textBox1.AppendText("Recieved: " + DataRecieved + "\r\n"); });
            
            string[] seperateParts = DataRecieved.Split(';');
            string[] RS = DataRecieved.Split(';');
            string[] configparts = DataRecieved.Split(';');
            string DataType = seperateParts[0];
            string[] ReadStat = DataRecieved.Split(';');
            float iVab;
            float iVab2;
            xval += 5;
            x_verdier += 5;
            
            
            if (tingtang == "readraw")
            {
                if (float.TryParse(seperateParts[1], out iVab))
                {
                    analogreading.Add(iVab);
                    timestamp.Add(xval);
                    chart1.Series["readraw"].Points.DataBindXY(timestamp, analogreading);
                    chart1.Invalidate();
                    
                } 

            }
            

            if (seperateParts[0] == "readscaled")
            {
                
                iVab2 = float.Parse(seperateParts[1], CultureInfo.InvariantCulture.NumberFormat);
                {
                    readscaled.Add(iVab2);
                    yreading.Add(x_verdier);
                    chart2.Series["readscaled"].Points.DataBindXY(yreading, readscaled);
                    chart2.Invalidate();
                }

            }


            
            if (tingtang == "readconf")
            {
                name_textBox2.Text = configparts[1];
                lrv_textBox5.Text = configparts[2];
                urv_textBox6.Text = configparts[3];
                alarml_textBox7.Text = configparts[4];
                alarmh_textBox8.Text = configparts[5];
                
            }




            if (tingtang == "writeconf")
            {
                if (seperateParts[1] == "1\r")
                {
                    MessageBox.Show("Success");
                }

                if (seperateParts[1] == "0\r")
                {
                    MessageBox.Show("Error, something went wrong");
                }
            }

            if (tingtang == "readstatus")
            {
                if (ReadStat[1] == "0\r")
                {
                    readstat_textBox2.Text = "Read status: Ok";
                }
                if (ReadStat[1] == "1\r")
                {
                    readstat_textBox2.Text = "Read status: Fail";
                }
                if (ReadStat[1] == "2\r")
                {
                    serialPort1.WriteLine("readconf");
                    readstat_textBox2.Text = ("Read status: Alarm low");
                    name_textBox2.Clear();
                    alarml_textBox7.Clear();
                    alarmh_textBox8.Clear();
                    lrv_textBox5.Clear();
                    urv_textBox6.Clear();
                }
                if (ReadStat[1] == "3\r")
                {
                    serialPort1.WriteLine("readconf");
                    readstat_textBox2.Text = ("Read status: Alarm high");
                    name_textBox2.Clear();
                    alarml_textBox7.Clear();
                    alarmh_textBox8.Clear();
                    lrv_textBox5.Clear();
                    urv_textBox6.Clear();
                }
            }
            
            

            
            

        }

        private void savepar_button1_Click(object sender, EventArgs e)
        {

            string str_name, lrv, urv, alarml, alarmh;
            str_name = name_textBox2.Text;
            lrv = lrv_textBox5.Text;
            urv = urv_textBox6.Text;
            alarml = alarml_textBox7.Text;
            alarmh = alarmh_textBox8.Text;

            


            if ((str_name == string.Empty) || (lrv == string.Empty) || (urv == string.Empty) || (alarmh == string.Empty)
                || (alarml == string.Empty))
            {
                MessageBox.Show("Empty textboxs, cannot save");
            }

            float d_lrv, d_urv, d_alarml, d_alarmh;
            d_lrv = float.Parse(lrv);
            d_urv = float.Parse(urv);
            d_alarml = float.Parse(alarml);
            d_alarmh = float.Parse(alarmh);

            if ((str_name != string.Empty) || (lrv != string.Empty) || (urv != string.Empty) || (alarmh != string.Empty)
                || (alarml != string.Empty))
            {
               
                d_lrv = float.Parse(lrv);
                d_urv = float.Parse(urv);
                d_alarml = float.Parse(alarml);
                d_alarmh = float.Parse(alarmh);
            }

            

            if ((d_lrv > d_urv) || (d_lrv > 500) || (d_urv < 40) || (d_alarml > d_alarmh)
                || (d_alarmh < 300) || (d_alarml > 100) || (name_textBox2.TextLength != 9)) 
            {
                if ((d_urv < 40) || (d_lrv > 500)) 
                {
                    MessageBox.Show("Upper range value can not be lower than 40 and lower range value must be lower than 500");
                }
                if (d_lrv > d_urv)
                {
                    MessageBox.Show("Upper range can not be lower than Lower range");
                }
                if (d_alarml > d_alarmh)
                {
                    MessageBox.Show("Alarm low can not be higher than Alarm high");
                }
                if (d_alarmh < 300)
                {
                    MessageBox.Show("Alarm high must be higher than 300");
                }
                if (d_alarml > 100)
                {
                    MessageBox.Show("Alarm low must be lower than 100");
                }
                if (name_textBox2.TextLength != 9)
                {
                    MessageBox.Show("Name must contain 9 letters.");
                }
            }

            if ((name_textBox2.TextLength == 9) && (d_alarml < d_alarmh) &&
                (d_lrv < d_urv) && (d_lrv < 500) && (d_urv > 40) &&
                (d_alarmh > 300) && (d_alarml < 100) && (str_name != string.Empty) && (lrv!=string.Empty) && (urv != string.Empty) 
                && (alarml != string.Empty) && (alarmh != string.Empty) )
            {
                string namebox, alarmh1, lrv1, urv1, alarml1;
                lrv1 = d_lrv.ToString();
                urv1 = d_urv.ToString();
                alarml1 = d_alarml.ToString();
                alarmh1 = d_alarmh.ToString();
                namebox = name_textBox2.Text;

                string parameter;
                parameter = namebox + ";" + lrv1 + ";" + urv1 + ";" + alarml1 + ";" + alarmh;
                string[] parameter_liste = { parameter };
                StreamWriter write_param = new StreamWriter(@"C:\arbeidskrav\parameters.ssc");


                foreach (string param1 in parameter_liste)
                {
                    write_param.WriteLine(param1);
                }
                write_param.Close();

                MessageBox.Show("Parameters saved to local file succesfull");

                
            }

        }

        private void readpar_button2_Click(object sender, EventArgs e)
        {
            StreamReader readpar = new StreamReader(@"C:\arbeidskrav\parameters.ssc");
            readpar_textBox3.Text = readpar.ReadToEnd();
            readpar.Close();
        }

        private void send_button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine(send_textBox5.Text);
                send_textBox5.Text = null;


            }
        }

        private void viewconf_button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                name_textBox2.Clear();
                lrv_textBox5.Clear();
                urv_textBox6.Clear();
                alarmh_textBox8.Clear();
                alarml_textBox7.Clear();
                serialPort1.WriteLine("readconf");
                tingtang = "readconf";
            }
            else
            {
                MessageBox.Show("Failed to retrieve configuration from Arduino, check connection");
            }
            

        }

        private void start_button_Click(object sender, EventArgs e)
        {
            

            if (serialPort1.IsOpen)
            {
                chart1.ResetAutoValues();
                serialPort1.WriteLine("readraw");
                tingtang = "readraw";
                timer1.Start();
                timer2.Stop();
            }

            else
            {
                MessageBox.Show("Something wrong, check connection again");
            }
        }

        private void savereading_button_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            StreamWriter reading = new StreamWriter(@"C:\arbeidskrav\readraw_reading.csv");

            for (int i = 0; i <timestamp.Count; i++)
            {
                reading.Write("Timestamp{0}," + "Value[{1}, {2}];" + "\r\n", i, timestamp[i], analogreading[i]);
                
            }
            reading.Close();

            MessageBox.Show("Readraw values saved to local file");
        }

        private void readreading_button_Click(object sender, EventArgs e)
        {
            StreamReader read_readin = new StreamReader(@"C:\arbeidskrav\readraw_reading.csv");
            readreading_textBox2.Text = ("Readraw values" + "\r\n"+ read_readin.ReadToEnd());
            read_readin.Close();
        }

        private void stop_moni_button_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                timer1.Stop();
                timer2.Stop();
               
            }
        }

        private void writeconf_button_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                string value = Interaction.InputBox("Enter password", "Please Enter Password");

                if (value == "password")
                {
                    MessageBox.Show("Correct Password");
                   
                    string str_name2, lrv2, urv2, alarml2, alarmh2;
                    str_name2 = name_textBox2.Text;
                    lrv2 = lrv_textBox5.Text;
                    urv2 = urv_textBox6.Text;
                    alarml2 = alarml_textBox7.Text;
                    alarmh2 = alarmh_textBox8.Text;

                    float d_lrv2, d_urv2, d_alarml2, d_alarmh2;
                    d_lrv2 = float.Parse(lrv2);
                    d_urv2 = float.Parse(urv2);
                    d_alarml2 = float.Parse(alarml2);
                    d_alarmh2 = float.Parse(alarmh2);


                    if ((d_lrv2 > d_urv2) || (d_lrv2 > 500) || (d_urv2 < 40) || (d_alarml2 > d_alarmh2)
                        || (d_alarmh2 < 300) || (d_alarml2 > 100) || (name_textBox2.TextLength != 9))
                    {
                        if ((d_urv2 < 40) || (d_lrv2 > 500))
                        {
                            MessageBox.Show("Upper range value can not be lower than 40 and lower range value must be lower than 500");
                        }
                        if (d_lrv2 > d_urv2)
                        {
                            MessageBox.Show("Upper range can not be lower than Lower range");
                        }
                        if (d_alarml2 > d_alarmh2)
                        {
                            MessageBox.Show("Alarm low can not be higher than Alarm high");
                        }
                        if (d_alarmh2 < 300)
                        {
                            MessageBox.Show("Alarm high must be higher than 300");
                        }
                        if (d_alarml2 > 100)
                        {
                            MessageBox.Show("Alarm low must be lower than 100");
                        }
                        if (name_textBox2.TextLength != 9)
                        {
                            MessageBox.Show("Name must contain 9 letters.");
                        }
                    }

                    if ((name_textBox2.TextLength == 9) && (d_alarml2 < d_alarmh2) && (d_lrv2 < 500) && (d_urv2 > 40) &&
                        (d_alarmh2 > 300) && (d_alarml2 < 100) && (d_lrv2 < d_urv2))
                    {
                        string namebox3, alarmh13, lrv13, urv13, alarml13;
                        lrv13 = d_lrv2.ToString();
                        urv13 = d_urv2.ToString();
                        alarml13 = d_alarml2.ToString();
                        alarmh13 = d_alarmh2.ToString();
                        namebox3 = name_textBox2.Text;

                        string parameter1;
                        parameter1 = namebox3 + ";" + lrv13 + ";" + urv13 + ";" + alarml13 + ";" + alarmh13;

                        serialPort1.WriteLine("writeconf>password>" + parameter1);
                        tingtang = "writeconf";
                    }
                }

                if (value != "password")
                {
                    MessageBox.Show("Wrong password, try again");

                }
            }
            
        }

        private void readconf_button_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("readconf");
                tingtang = "readconf";
            }

            if (serialPort1.IsOpen == false)
            {
                MessageBox.Show("Check connection again");
            }
        }

        private void cleardata_button_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            readpar_textBox3.Clear();
            name_textBox2.Clear();
            alarml_textBox7.Clear();
            alarmh_textBox8.Clear();
            lrv_textBox5.Clear();
            urv_textBox6.Clear();
        }

        private void writeconfig1_button_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                string value = Interaction.InputBox("Enter password", "Please Enter Password");


                if (value == "password")
                {
                    MessageBox.Show("Correct password!");

                    string str_name2, lrv2, urv2, alarml2, alarmh2;
                    str_name2 = name_textBox2.Text;
                    lrv2 = lrv_textBox5.Text;
                    urv2 = urv_textBox6.Text;
                    alarml2 = alarml_textBox7.Text;
                    alarmh2 = alarmh_textBox8.Text;

                    float d_lrv2, d_urv2, d_alarml2, d_alarmh2;
                    d_lrv2 = float.Parse(lrv2);
                    d_urv2 = float.Parse(urv2);
                    d_alarml2 = float.Parse(alarml2);
                    d_alarmh2 = float.Parse(alarmh2);

                    if ((d_lrv2 > d_urv2) || (d_lrv2 > 500) || (d_urv2 < 40) || (d_alarml2 > d_alarmh2)
                        || (d_alarmh2 < 300) || (d_alarml2 > 100) || (name_textBox2.TextLength != 9))
                    {
                        if ((d_urv2 < 40) || (d_lrv2 > 500))
                        {
                            MessageBox.Show("Upper range value can not be lower than 40 and lower range value must be lower than 500");
                        }
                        if (d_lrv2 > d_urv2)
                        {
                            MessageBox.Show("Upper range can not be lower than Lower range");
                        }
                        if (d_alarml2 > d_alarmh2)
                        {
                            MessageBox.Show("Alarm low can not be higher than Alarm high");
                        }
                        if (d_alarmh2 < 300)
                        {
                            MessageBox.Show("Alarm high must be higher than 300");
                        }
                        if (d_alarml2 > 100)
                        {
                            MessageBox.Show("Alarm low must be lower than 100");
                        }
                        if (name_textBox2.TextLength != 9)
                        {
                            MessageBox.Show("Name must contain 9 letters.");
                        }
                    }

                    if ((name_textBox2.TextLength == 0) && (lrv_textBox5.TextLength == 0) && (urv_textBox6.TextLength == 0) && (alarmh_textBox8.TextLength == 0)
                          && (alarml_textBox7.TextLength == 0))
                    {
                        MessageBox.Show("Empty textboxs, cannot save");
                    }

                    if ((name_textBox2.TextLength == 9) && (d_alarml2 < d_alarmh2) && (d_lrv2 < 500) && (d_urv2 > 40) &&
                       (d_alarmh2 > 300) && (d_alarml2 < 100) && (d_lrv2 < d_urv2))
                    {
                        string namebox3, alarmh13, lrv13, urv13, alarml13;
                        lrv13 = d_lrv2.ToString();
                        urv13 = d_urv2.ToString();
                        alarml13 = d_alarml2.ToString();
                        alarmh13 = d_alarmh2.ToString();
                        namebox3 = name_textBox2.Text;

                        string parameter1;
                        parameter1 = namebox3 + ";" + lrv13 + ";" + urv13 + ";" + alarml13 + ";" + alarmh13;

                        serialPort1.WriteLine("writeconf>password>" + parameter1);
                        tingtang = "writeconf";
                    }

                }

                if (value != "password")
                {
                    MessageBox.Show("Wrong password, try again");

                }
            }

            else
            {
                MessageBox.Show("Failed to write configuration, check connection");
            }
        }

        private void clearconfig_button_Click(object sender, EventArgs e)
        {
            name_textBox2.Clear();
            alarml_textBox7.Clear();
            alarmh_textBox8.Clear();
            lrv_textBox5.Clear();
            urv_textBox6.Clear();
           
        }

        private void readstat_alarmlow_button_Click(object sender, EventArgs e)
        {
            readstat_textBox2.Clear();
            if (serialPort1.IsOpen)
            {
                serialPort1.WriteLine("readstatus");
                tingtang = "readstatus";
                name_textBox2.Clear();
                alarml_textBox7.Clear();
                alarmh_textBox8.Clear();
                lrv_textBox5.Clear();
                urv_textBox6.Clear();
            }
            else
            {
                MessageBox.Show("Something went wrong, check connection");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            

            if (serialPort1.IsOpen)
            {
                tingtang = "readscaled";
                chart1.ResetAutoValues();
                serialPort1.WriteLine("readscaled");
                timer2.Start();
                timer1.Stop();

            }
            else
            {
                MessageBox.Show("Something went wrong, check connection again");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            timer2.Stop();
            StreamWriter scaled_reading = new StreamWriter(@"C:\arbeidskrav\readscaled_reading.csv");

            for (int i = 0; i < yreading.Count; i++)
            {
                scaled_reading.Write("Timestamp{0}," + "Value[{1}, {2}];" + "\r\n", i, yreading[i], readscaled[i]);

            }
            scaled_reading.Close();


            MessageBox.Show("Readscaled values saved to local file");
        }

        private void readscaled_file_button_Click(object sender, EventArgs e)
        {
            StreamReader scaled_readin = new StreamReader(@"C:\arbeidskrav\readscaled_reading.csv");
            readreading_textBox2.Text = ("Readscaled values" + "\r\n" + scaled_readin.ReadToEnd());
            scaled_readin.Close();
        }

        private void clear_readingvalues_button_Click(object sender, EventArgs e)
        {
            readreading_textBox2.Clear();
        }

        private void import_config_button_Click(object sender, EventArgs e)
        {
            name_textBox2.Clear();
            lrv_textBox5.Clear();
            urv_textBox6.Clear();
            alarmh_textBox8.Clear();
            alarml_textBox7.Clear();

            StreamReader import_config = new StreamReader(@"C:\arbeidskrav\parameters.ssc");
            string importsc = import_config.ReadToEnd();
            string[] imported_config = importsc.Split(';');
            
            name_textBox2.Text = imported_config[0];
            lrv_textBox5.Text = imported_config[1];
            urv_textBox6.Text = imported_config[2];
            alarml_textBox7.Text = imported_config[3];
            alarmh_textBox8.Text = imported_config[4];

            import_config.Close();
        }
    }

}