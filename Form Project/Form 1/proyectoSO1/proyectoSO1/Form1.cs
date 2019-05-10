using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace proyectoSO1
{
    public partial class Form1 : Form
    {

        static int conectionsNumber = 0;
        Funciones funcs = new Funciones();
        private Socket handler;
        static BackgroundWorker worker;
        static int eject;
        static List<Tuple<int, int, int, int>> rangos;
        DataTable dt;
        public Form1()
        {
            funcs.pgsql.abrirConexion();
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(Listening);
            worker.WorkerSupportsCancellation = true;
            eject = 0;
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            funcs.cargarBConocimiento(ref this.progressBar1);
        }


        //Mostrar los resultados en el form--------------------------
        private void ordenarLista(List<URL> lista)
        {
            dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[2] { new DataColumn("URL", typeof(string)), new DataColumn("Probabilidad", typeof(string)) });

            for (int var = 0; var < funcs.URLs.Count; var++)
            {
                List<Categoria> listaCategoria = funcs.URLs[var].getCategorias();
                listaCategoria.Sort((x, y) => (x.getProbabilidad().CompareTo(y.getProbabilidad())));
                listaCategoria.Reverse();

                string[] valores = new string[listaCategoria.Count];

                int index = 0;
                foreach (var ind in listaCategoria)
                {
                    if (ind.getProbabilidad().Equals(-1))
                    {
                        valores[index] = "N/A";
                    }
                    else
                    {
                        valores[index] = ind.getNombre() + " = " + ind.getProbabilidad() * 100 + "%  ";
                    }

                    index++;
                }
                dt.Rows.Add(funcs.URLs[var].getTexto(), string.Concat(valores));
            }
            dataGridView1.DataSource = dt;
        }


        //--Mostrar los resultados en el form--------------------------

        public void Listening(object sender, DoWorkEventArgs e)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = null;
            foreach (var ip in ipHostInfo.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }
            Console.WriteLine(ipAddress.ToString());
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                while (!worker.CancellationPending)
                {
                    handler = listener.Accept();
                    handler.Receive(bytes);

                    var mStream = new MemoryStream();
                    var binFormatter = new BinaryFormatter();

                    // Where 'objectBytes' is your byte array.
                    mStream.Write(bytes, 0, bytes.Length);
                    mStream.Position = 0;

                    Int16[] data = binFormatter.Deserialize(mStream) as Int16[];
                    if (data == null)
                        continue;
                    if (data[0] == 0)//Sumar al contador y dejarla conectada
                    {
                        //Datos para pasar una tupla
                        Tuple<int, int, int, int> aux = new Tuple<int, int, int, int>(-2, 0, 0, conectionsNumber);// Tuple<orden de los datos y tipos>(valores de los datos)
                        conectionsNumber++;
                        binFormatter = new BinaryFormatter();
                        mStream = new MemoryStream();
                        binFormatter.Serialize(mStream, aux);
                        handler.Send(mStream.ToArray());
                        Console.WriteLine("Conexion #{0} ", conectionsNumber);
                    }
                    if (data[0] == -1)//Restar al contador y desconectar
                    {
                        conectionsNumber--;
                        //Datos para pasar una tupla
                        Tuple<int, int, int, int> aux = new Tuple<int, int, int, int>(-1, 0, 0, 0);// Tuple<orden de los datos y tipos>(valores de los datos)
                        binFormatter = new BinaryFormatter();
                        mStream = new MemoryStream();
                        binFormatter.Serialize(mStream, aux);
                        handler.Send(mStream.ToArray());
                        Console.WriteLine("Conexion #{0} ", conectionsNumber);
                    }
                    if (data[0] == 1)
                    {

                        if(eject == 1)
                        {
                            // Datos para pasar una tupla
                            Monitor.Enter(rangos);
                            Tuple<int, int, int, int> aux = rangos[data[1]];// Tuple<orden de los datos y tipos>(valores de los datos)
                            Monitor.Exit(rangos);
                            binFormatter = new BinaryFormatter();
                            mStream = new MemoryStream();
                            binFormatter.Serialize(mStream, aux);
                            handler.Send(mStream.ToArray());
                            Console.WriteLine("Trabajando el método secuencial");
                        }
                        else if( eject == 2)
                        {
                            // Datos para pasar una tupla
                            Monitor.Enter(rangos);
                            Tuple<int, int, int, int> aux = rangos[data[1]];// Tuple<orden de los datos y tipos>(valores de los datos)
                            Monitor.Exit(rangos);
                            binFormatter = new BinaryFormatter();
                            mStream = new MemoryStream();
                            binFormatter.Serialize(mStream, aux);
                            handler.Send(mStream.ToArray());
                            Console.WriteLine("Trabajando el método concurrente");
                        }
                        else
                        {
                            //Datos para pasar una tupla
                            Tuple<int, int, int, int> aux = new Tuple<int, int, int, int>(-2, 0, 0, 0);// Tuple<orden de los datos y tipos>(valores de los datos)
                            binFormatter = new BinaryFormatter();
                            mStream = new MemoryStream();
                            binFormatter.Serialize(mStream, aux);
                            handler.Send(mStream.ToArray());
                        }
                    }
                    if (data[0] == 2)
                    {
                        eject = 0;
                        Console.WriteLine("Proceso eject cerrado!!");
                        Tuple<int, int, int, int> aux = new Tuple<int, int, int, int>(-2, 0, 0, 0);// Tuple<orden de los datos y tipos>(valores de los datos)
                        binFormatter = new BinaryFormatter();
                        mStream = new MemoryStream();
                        binFormatter.Serialize(mStream, aux);
                        handler.Send(mStream.ToArray());
                    }
                }
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
            worker.RunWorkerAsync();
            this.Listen.BackColor = Color.FromArgb(163, 0, 0);
            this.Listen.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            eject = divirURL(1);
            button5.BackColor = Color.FromArgb(218, 221, 216);
            button3.BackColor = Color.FromArgb(96, 152, 62);
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            eject = divirURL(2);
            button3.BackColor = Color.FromArgb(218, 221, 216);
            button5.BackColor = Color.FromArgb(96, 152, 62);
        }

        private void btnURL_Click(object sender, EventArgs e)
        {
            funcs.ObtenerURLs(ref progressBar1);
        }

        public int divirURL(int num)
        {
            rangos = new List<Tuple<int, int, int, int>>();
            //long totalURLs = funcs.pgsql.ObtenerCantidadURLs();
            //int cantidadURLs = (int)(totalURLs) / conectionsNumber;
            int cantidadURLs = 1000;
            int inicio = 0;
            int final = cantidadURLs;
            for (int i = 0; i < conectionsNumber; i++)
            {
                Console.WriteLine("inicio: {0} final {1} iteracion{2}", inicio, final, i);
                rangos.Add(new Tuple<int, int, int, int>(num, inicio, final, i));
                inicio = final + 1;
                final += cantidadURLs;
            }
            Console.WriteLine("Total de lineas "+ funcs.URLs.Count);
            return num;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            funcs.URLs = funcs.pgsql.getURL();
            funcs.pgsql.getPro(ref funcs.URLs);
            ordenarLista(funcs.URLs);
        }
    }
}
