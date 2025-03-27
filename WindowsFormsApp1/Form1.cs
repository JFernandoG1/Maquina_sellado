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
        int tiempo, tiempo_inflado;
        int resultCode=1;
        int resultados2;
        int bagsizeread, bagsizeread1;
        int error = 0;
        // Variable global para almacenar el tamaño en mm
        private int tamañoBolsaMM = 0;
        int bolsa;
        string bagsizeSetting,tiemposelladoSetting,longitudretornoSetting,tiempoinfladoSetting;


        public Form1()
        {
            InitializeComponent();
            this.FormClosed += Form1_FormClosed;
            // Puedes agregar cualquier código de inicialización después de la llamada a InitializeComponent.
            this.Text = "Maquina empacadora Version Beta";
            this.BackColor = Color.LightGray;
            conexion_status.BackColor = Color.Red; // Enciende el indicador
            label9.Text = "";

            // Define los puntos para la flecha hacia arriba
            Point[] pointsUp =
            {
        new Point(button12.Width / 2, 0),
        new Point(button12.Width, button12.Height),
        new Point(0, button12.Height)
    };

            // Crea un nuevo gráfico de trazado para el botón 12
            System.Drawing.Drawing2D.GraphicsPath buttonPathUp = new System.Drawing.Drawing2D.GraphicsPath();
            buttonPathUp.AddPolygon(pointsUp);

            // Usa el trazado para definir la Región del botón
            button12.Region = new System.Drawing.Region(buttonPathUp);

            // Define los puntos para la flecha hacia abajo
            Point[] pointsDown =
            {
        new Point(0, 0),
        new Point(button13.Width, 0),
        new Point(button13.Width / 2, button13.Height)
    };

            // Crea un nuevo gráfico de trazado para el botón 13
            System.Drawing.Drawing2D.GraphicsPath buttonPathDown = new System.Drawing.Drawing2D.GraphicsPath();
            buttonPathDown.AddPolygon(pointsDown);

            // Usa el trazado para definir la Región del botón
            button13.Region = new System.Drawing.Region(buttonPathDown);

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
        private void button1_Click(object sender, EventArgs e) // Botón Cambiar tamaño de bolsa
        {
            resultCode = 0;
            if (resultCode == 0)
            {
                // Validar entrada
                if (!double.TryParse(textBox2.Text, out double value))
                {
                    label2.Text = "Ingrese un número válido.";
                    return;
                }

                // Calcular resultado
                double result = (value * 31.23) + 2000;

                // Validar límites de Int16
                if (result > short.MaxValue || result < short.MinValue)
                {
                    label2.Text = "El valor calculado está fuera del rango permitido.";
                    return;
                }

                try
                {
                    // Escribir y leer del PLC
                    plc.SetDevice("D10", Convert.ToInt16(result));
                    plc.GetDevice("D10", out int read_results);

                    // Calcular tamaño de la bolsa
                    bagsizeread = (int)Math.Round((read_results - 2000) / 31.23);
                    label3.Text = bagsizeread.ToString();

                    // Guardar configuración
                    SavebagsizeSettings();

                    // Limpiar entrada
                    textBox2.Text = "";
                }
                catch (Exception ex)
                {
                    label2.Text = "Error de comunicación con el PLC: " + ex.Message;
                }
            }
            else
            {
                label2.Text = "Conecta la máquina primero.";
            }
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
            tiempoinfladoSetting = ConfigurationManager.AppSettings["tiempoinfladoSetting"];
            if (resultCode == 0) // 0 means the connection was successful
            {
                label2.Text = ("Conectado");
                plc.SetDevice("M32", 0);

                int safety_light;
                int emergency_button;
                int sensor_sello;
                int boton_derecho;
                int sensor_falta_bolsa;
                error = 0;
                plc.GetDevice("X1", out safety_light);
                if(safety_light == 0)
                {
                    label2.Text = ("Error en Barras de seguridad");
                    error = 1;
                }
                plc.GetDevice("X3", out sensor_falta_bolsa);
                if (sensor_falta_bolsa == 0)
                {
                    label2.Text = ("Error en sensor bolsa trasero");
                    error = 1;
                }
                plc.GetDevice("X4", out emergency_button);
                if (emergency_button == 0)
                {
                    label2.Text = ("Error en Boton de emergencia");
                    error = 1;
                }
                plc.GetDevice("X5", out sensor_sello);
                if (sensor_sello == 1)
                {
                    label2.Text = ("Error en sensor final de carrera sello");
                    error = 1;
                }
                plc.GetDevice("X6", out boton_derecho);
                if (boton_derecho == 1)
                {
                    label2.Text = ("Error en boton derecho");
                    error = 1;
                }

                ////////Cambia el valor de la bolsa del archivo config de los registros del plc
                int read_results1;
                double result1 = (Convert.ToDouble(bagsizeSetting) * 31.23)+2000;
                plc.SetDevice("D10", Convert.ToInt16(result1));
                plc.GetDevice("D10", out read_results1);
                bagsizeread1 = (int)Math.Round((read_results1-2000) / 31.23);
                label3.Text = bagsizeread1.ToString();

                int read_results2=Convert.ToInt16(tiemposelladoSetting);
                plc.SetDevice("D11", read_results2);
                plc.SetDevice("D12", read_results2 + 2);
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


                int read_results4 = Convert.ToInt16(tiempoinfladoSetting);
                plc.SetDevice("D17", read_results4);
                plc.GetDevice("D17", out tiempo_inflado);
                label10.Text = tiempo_inflado.ToString();
                conexion_status.BackColor= Color.Green; // Enciende el indicador
                plc.SetDevice("M29", 0);
                plc.SetDevice("D16", 0);
                plc.SetDevice("D15", 0);
                label12.Text = "Impresión Apagada";
                label7.Text = "";

                    plc.SetDevice("M41", 1);
                    label15.Text = "Corte Encendido";

                // Leer el valor del PLC
                plc.GetDevice("D10", out bolsa);

                // Convertir a mm
                tamañoBolsaMM = (int)Math.Round((bolsa - 2000) / 31.23);
                if (error == 1)
                {
                    resultCode = 1;
                    plc.Close();
                    conexion_status.BackColor = Color.Red; // Enciende el indicador
                }

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
            if ((resultCode == 0) && (error == 0))
            {
                int machine_status;
                plc.GetDevice("M20", out machine_status);
                if (machine_status == 0)
                {
                    if (calibrate == 1)
                    {
                    
                        plc.SetDevice("M20", 1);//Inicia la maquina
                        plc.SetDevice("M24", 1);//Inicia la impresion
                        textBox2.Text = "";
                        // Leer el valor del PLC
                        plc.GetDevice("D10", out bolsa);

                        // Convertir a mm
                        tamañoBolsaMM = (int)Math.Round((bolsa - 2000) / 31.23);
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
                plc.SetDevice("M24", 0);//detiene la impresion
                label2.Text = ("Detenido");
            }
            else {label2.Text = ("Conecta la maquina primero"); }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if ((resultCode == 0)&&(error==0))
            {
                if (calibrate == 0)
                {
                    int sensor_bolsa;
                    textBox2.Text = "";
                    // Leer el valor del PLC
                    plc.GetDevice("D10", out bolsa);

                    // Convertir a mm
                    tamañoBolsaMM = (int)Math.Round((bolsa - 2000) / 31.23);
                    plc.GetDevice("M23", out sensor_bolsa);
                    if (sensor_bolsa == 0)
                    {
                        //label2.Text = ("No hay bolsa");
                        plc.SetDevice("M3", 1);
                        Thread.Sleep(1000);
                        plc.GetDevice("M23", out sensor_bolsa);
                        if (sensor_bolsa == 1)
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
                            plc.SetDevice("M3", 1);
                            Thread.Sleep(1000);
                            plc.GetDevice("M23", out sensor_bolsa);
                            if (sensor_bolsa == 1)
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
                                label2.Text = ("No hay bolsa");

                            }
                        }


                    }
                    else { 
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
                // Asegurar que está dentro del rango esperado
                if (tiempo < 5 || tiempo > 14)
                {
                    tiempo = 5;  // Si está fuera de rango, reiniciarlo a 8
                }

                // Incrementar y ciclar dentro del rango 8-14
                int nuevoD11 = (tiempo == 14) ? 5 : tiempo + 1;

                // Escribir valores en el PLC
                plc.SetDevice("D11", nuevoD11);
                plc.SetDevice("D12", nuevoD11 + 2);
                plc.GetDevice("D11", out tiempo);
                // Actualizar la interfaz gráfica
                label5.Text = nuevoD11.ToString();

                // Guardar la configuración
                SavetiemposelladoSettings();

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
                if ((value2 >= 0) && (value2 <= 40))
                {
                    label9.Text = "";
                   
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
                else
                {
                    label9.Text = ("Ingresa un numero entre 0 y 40");
                }
            }
            else { label2.Text = ("Conecta la maquina primero"); }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                plc.SetDevice("M29", 0);
                plc.SetDevice("D16", 0);
                plc.SetDevice("D15", 0);
                label12.Text = "Impresión Apagada";
                label7.Text = "";
            }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }

        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                plc.SetDevice("M29", 1);
                int read_results3;
                longitudretornoSetting = ConfigurationManager.AppSettings["longitudretornoSetting"];
                double value2 = Convert.ToDouble(longitudretornoSetting);
                double result2 = value2 * 31.23;
                double result2neg = result2 * (-1);
                plc.SetDevice("D15", Convert.ToInt16(result2neg));
                plc.SetDevice("D16", Convert.ToInt16(result2));
                plc.GetDevice("D16", out read_results3);

                resultados2 = (int)Math.Round(read_results3 / 31.23);
                label7.Text = resultados2.ToString();
                label12.Text = "Impresión Encendida";
            }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }

        }

        private void label13_Click(object sender, EventArgs e)
        {


        }
        // Método para actualizar el tamaño de la bolsa en la variable y el TextBox
        private void MoverBolsa(int incrementoMM)
        {
            // Actualizar la variable local en mm
            tamañoBolsaMM += incrementoMM;

            // Mostrar en el TextBox antes de enviarlo al PLC
            textBox2.Text = tamañoBolsaMM.ToString();
        }
        //Boton subir bolsa
        private void button12_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M12", 1);
            MoverBolsa(-5); // Aumenta en 10 mm
            Console.WriteLine("Subiendo bolsa");
        }
        
        //Boton bajar bolsa
        private void button13_Click(object sender, EventArgs e)
        {
            plc.SetDevice("M13", 1);
            MoverBolsa(5); // Disminuye en 10 mm
            Console.WriteLine("Bajando bolsa");
        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                plc.SetDevice("M41", 0);
                label15.Text = "Corte Apagado";
                }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                plc.SetDevice("M41", 1);
                label15.Text = "Corte Encendido";
            }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                // Asegurar que está dentro del rango esperado
                if (tiempo < 5 || tiempo > 14)
                {
                    tiempo = 5;  // Si está fuera de rango, reiniciarlo a 8
                }

                // Incrementar y ciclar dentro del rango 8-14
                int nuevoD11 = (tiempo == 14) ? 5 : tiempo - 1;

                // Escribir valores en el PLC
                plc.SetDevice("D11", nuevoD11);
                plc.SetDevice("D12", nuevoD11 + 2);
                plc.GetDevice("D11", out tiempo);
                // Actualizar la interfaz gráfica
                label5.Text = nuevoD11.ToString();

                // Guardar la configuración
                SavetiemposelladoSettings();

            }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            if (resultCode == 0)
            {
                if (tiempo_inflado == 5)
                {
                    plc.SetDevice("D17", 10);
                    plc.GetDevice("D17", out tiempo_inflado);
                    SavetiempoinfladoSettings();
                }
                else if (tiempo_inflado == 10)
                {
                    plc.SetDevice("D17", 5);
                    plc.GetDevice("D17", out tiempo_inflado);
                    SavetiemposelladoSettings();
                }
                else
                {
                    // Si no es 5 ni 10, lo establecemos en 5
                    plc.SetDevice("D17", 5);
                    plc.GetDevice("D17", out tiempo_inflado);
                }

                label10.Text = tiempo_inflado.ToString();

            }
            else
            {
                label2.Text = ("Conecta la maquina primero");
            }
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
        private void SavetiempoinfladoSettings()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["tiempoinfladoSetting"].Value = tiempo_inflado.ToString();
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
