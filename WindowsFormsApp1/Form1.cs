using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ActUtlTypeLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Configuration;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public ActUtlType plc = new ActUtlType();
        int Longitud_bolsa_default;
        int calibrate = 0;
        int tiempo;
        int resultCode=1;
        int resultados2;
        int bagsizeread, bagsizeread1;
        string bagsizeSetting,tiemposelladoSetting,longitudretornoSetting;


        public Form1()
        {
            InitializeComponent();
            this.FormClosed += Form1_FormClosed;
            // Puedes agregar cualquier código de inicialización después de la llamada a InitializeComponent.
            this.Text = "Maquina empacadora Version Beta";
            this.BackColor = Color.LightGray;
            conexion_status.BackColor = Color.Red; // Enciende el indicador

        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        private void Form1_FormClosed(object sender, EventArgs e)
        {

            plc.SetDevice("M20", 0);
            label2.Text = ("Detenido");
            plc.Close();
            label2.Text = ("Desconectado");
            


        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (resultCode == 0) {
                int read_results;
                double value = Convert.ToDouble(textBox2.Text);
                double result = value * 31.23;
                plc.SetDevice("D10", Convert.ToInt16(result));
                plc.GetDevice("D10", out read_results);
                textBox2.Text = "";
                bagsizeread = (int)Math.Round(read_results / 31.23);
                label3.Text = bagsizeread.ToString();
                SavebagsizeSettings();
            }
            else {label2.Text = ("Conecta la maquina primero"); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M20", 0);
            plc.Close();
            resultCode = 1;
            label2.Text = ("Desconectado");
            conexion_status.BackColor = Color.Red; // Enciende el indicador
        }

        private void button4_Click(object sender, EventArgs e)
        {
            plc.ActLogicalStationNumber = 2;
            resultCode = plc.Open();
            bagsizeSetting = ConfigurationManager.AppSettings["bag_sizeSetting"];
            tiemposelladoSetting = ConfigurationManager.AppSettings["tiemposelladoSetting"];
            longitudretornoSetting = ConfigurationManager.AppSettings["longitudretornoSetting"];

            if (resultCode == 0) // 0 means the connection was successful
            {
                label2.Text = ("Conectado");
                ////////Cambia el valor de la bolsa del archivo config de los registros del plc
                int read_results1;
                double result1 = Convert.ToDouble(bagsizeSetting) * 31.23;
                plc.SetDevice("D10", Convert.ToInt16(result1));
                plc.GetDevice("D10", out read_results1);
                bagsizeread1 = (int)Math.Round(read_results1 / 31.23);
                label3.Text = bagsizeread1.ToString();

                int read_results2=Convert.ToInt16(tiemposelladoSetting);
                plc.SetDevice("D11", read_results2);
                plc.GetDevice("D11", out tiempo);
                label5.Text = tiempo.ToString();

                int read_results3;
                double value2 = Convert.ToDouble(longitudretornoSetting);
                double result2 = value2 * 31.23;
                double result2neg = result2 * (-1);
                plc.SetDevice("D15", Convert.ToInt16(result2neg));
                plc.SetDevice("D16", Convert.ToInt16(result2));
                plc.GetDevice("D16", out read_results3);

                resultados2 = (int)Math.Round(read_results3 / 31.23);
                label7.Text = resultados2.ToString();


                conexion_status.BackColor= Color.Green; // Enciende el indicador
                                                      

            }
            else
            {
                MessageBox.Show("No fue posible conectar al PLC: " + resultCode);
                conexion_status.BackColor = Color.Red; // Enciende el indicador
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                int machine_status;
                plc.GetDevice("M20", out machine_status);
                if (machine_status == 0)
                {
                    if (calibrate == 1)
                    {
                        int read_results;
                        plc.SetDevice("M20", 1);
                        plc.SetDevice("M16", 1);
                        Thread.Sleep(500);
                        plc.SetDevice("M15", 1);
                        Thread.Sleep(200);
                        plc.SetDevice("M15", 0);
                        plc.GetDevice("D10", out read_results);
                        read_results = read_results + 3500;
                        plc.SetDevice("D10", Convert.ToInt16(read_results));
                        plc.SetDevice("M21", 1);
                        Thread.Sleep(3000);
                        plc.SetDevice("M21", 0);
                        read_results = read_results - 3500;
                        plc.SetDevice("D10", Convert.ToInt16(read_results));
                        label2.Text = ("Iniciado");
                    }
                    else
                    {
                        label2.Text = ("Falta calibrar");
                    }
                }
                else { label2.Text = ("La maquina ya iniciada"); }


            }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                plc.SetDevice("M20", 0);
                calibrate = 0;
                label2.Text = ("Detenido");
            }
            else {label2.Text = ("Conecta la maquina primero"); }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                if (calibrate == 0)
                {
                    plc.SetDevice("M22", 1);
                    Thread.Sleep(500);
                    plc.SetDevice("M22", 0);
                    int estado_de_calibracion = 1;
                    while (estado_de_calibracion == 1)
                    {
                        plc.GetDevice("M23", out estado_de_calibracion);
                        if (estado_de_calibracion == 0)
                        {
                            calibrate = 1;
                            label2.Text = ("Listo para iniciar");
                        }
                    }
                }
                else
                {
                    label2.Text = ("Deten la maquina primero");
                }
            }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                if (tiempo == 2)
                {
                    plc.SetDevice("D11", 3);
                    plc.GetDevice("D11", out tiempo);
                    SavetiemposelladoSettings();
                }
                else if (tiempo == 3)
                {
                    plc.SetDevice("D11", 2);
                    plc.GetDevice("D11", out tiempo);
                    SavetiemposelladoSettings();
                }
                label5.Text = tiempo.ToString();
            }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                int read_results2;
                double value2 = Convert.ToDouble(textBox1.Text);
                double result2 = value2 * 31.23;
                double result2neg = result2 * (-1);
                plc.SetDevice("D15", Convert.ToInt16(result2neg));
                plc.SetDevice("D16", Convert.ToInt16(result2));
                plc.GetDevice("D16", out read_results2);

                textBox1.Text = "";
                resultados2 = (int)Math.Round(read_results2 / 31.23);
                label7.Text = resultados2.ToString();
                SavelongitudretornoSettings();
            }
            else { label2.Text = ("Conecta la maquina primero"); }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
        private void SavebagsizeSettings()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["bag_sizeSetting"].Value = bagsizeread.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        private void SavetiemposelladoSettings()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["tiemposelladoSetting"].Value = tiempo.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        private void SavelongitudretornoSettings()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["longitudretornoSetting"].Value = resultados2.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }


    }
}
