using proyectoSO1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectSO1_Cliente
{
    public partial class Form1 : Form
    {
        static Socket socketCon;
        BackgroundWorker worker;
        Tuple<int, int, int, int> selectedFilter;
        Funciones funcs = new Funciones();
        static Object socketLink = new object();
        static bool connected;
        //static IPAddress ip = IPAddress.Parse("192.168.43.59");
        static IPAddress ip = IPAddress.Parse("127.0.0.1");
        static int port = 11000;
        static Int16 idComputadora;


        public Form1()
        {
            connected = false;
            worker = new BackgroundWorker();
            selectedFilter = new Tuple<int, int, int, int>(-1, 0, 0, 0);
            worker.DoWork += new DoWorkEventHandler(connectionWork);
            worker.WorkerSupportsCancellation = true;
            funcs.pgsql.setIP(ip.ToString());
            InitializeComponent();
        }
       
        //Sockets----------------------------------------
        public static byte[] sendMsg(Int16[] msg)
        {
            byte[] bytes = null;
            lock (socketLink)
                try
                {
                    socketCon = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socketCon.Connect(new IPEndPoint(ip, port));

                    BinaryFormatter binFormatter = new BinaryFormatter();
                    MemoryStream mStream = new MemoryStream();

                    binFormatter.Serialize(mStream, msg);

                    socketCon.Send(mStream.ToArray());
                    bytes = new byte[1024];
                    socketCon.Receive(bytes);
                    socketCon.Shutdown(SocketShutdown.Both);
                    socketCon.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            return bytes;

        }

        private void connectionWork(object sender, DoWorkEventArgs e)
        {
            while (!worker.CancellationPending)
            {
                if (selectedFilter.Item1 == -1)
                {
                    byte[] objectBytes = sendMsg(new Int16[1] { 0 });
                    if (objectBytes == null)
                        continue;
                    var mStream = new MemoryStream();
                    var binFormatter = new BinaryFormatter();

                    // Where 'objectBytes' is your byte array.
                    mStream.Write(objectBytes, 0, objectBytes.Length);
                    mStream.Position = 0;
                    selectedFilter = binFormatter.Deserialize(mStream) as Tuple<int, int, int, int>;
                    idComputadora = Convert.ToInt16(selectedFilter.Item4);
                    Console.WriteLine("Conectado...");
                }//Crear una conexion con el servidor
                else if (selectedFilter.Item1 == -3)
                {
                    byte[] objectBytes = sendMsg(new Int16[1] { -1 });
                    if (objectBytes == null)
                        continue;
                    var mStream = new MemoryStream();
                    var binFormatter = new BinaryFormatter();

                    // Where 'objectBytes' is your byte array.
                    mStream.Write(objectBytes, 0, objectBytes.Length);
                    mStream.Position = 0;
                    selectedFilter = binFormatter.Deserialize(mStream) as Tuple<int, int, int, int>;
                    Console.WriteLine("Desconectado...");
                    worker.CancelAsync();
                }//Desconectar el servidor
                else
                {
                    byte[] objectBytes = sendMsg(new Int16[2] { 1, idComputadora });
                    if (objectBytes == null)
                        continue;
                    var mStream = new MemoryStream();
                    var binFormatter = new BinaryFormatter();

                    // Where 'objectBytes' is your byte array.
                    mStream.Write(objectBytes, 0, objectBytes.Length);
                    mStream.Position = 0;
                    selectedFilter = binFormatter.Deserialize(mStream) as Tuple<int, int, int, int>;
                    if (selectedFilter.Item1 == 1)
                    {

                        funcs.ObtenerURLs(selectedFilter.Item2, selectedFilter.Item3);
                        Console.WriteLine("Indices inicio: {0} final: {1}",selectedFilter.Item2, selectedFilter.Item3);
                        funcs.ClasificarURLsSecuencial(ref progressBar1, ref label3);
                        funcs.BayesSecuencial(ref progressBar1, ref label3);
                        Console.WriteLine("subiendo datos...");
                        funcs.subirDatos();
                        Console.WriteLine("Proceso Terminado");

                        objectBytes = sendMsg(new Int16[1] { 2 });
                        if (objectBytes == null)
                            continue;
                        mStream = new MemoryStream();
                        binFormatter = new BinaryFormatter();

                        // Where 'objectBytes' is your byte array.
                        mStream.Write(objectBytes, 0, objectBytes.Length);
                        mStream.Position = 0;
                        selectedFilter = binFormatter.Deserialize(mStream) as Tuple<int, int, int, int>;

                    }
                    else if (selectedFilter.Item1 == 2)
                    {
                        funcs.ObtenerURLs(selectedFilter.Item2, selectedFilter.Item3);
                        Console.WriteLine("Indices inicio: {0} final: {1}", selectedFilter.Item2, selectedFilter.Item3);
                        funcs.URLsConcurrente(ref progressBar1, ref label3);
                        funcs.BayesConcurrente(ref progressBar1, ref label3);
                        Console.WriteLine("subiendo datos...");
                        funcs.subirDatos();
                        Console.WriteLine("Proceso Terminado");

                        objectBytes = sendMsg(new Int16[1] { 2 });
                        if (objectBytes == null)
                            continue;
                        mStream = new MemoryStream();
                        binFormatter = new BinaryFormatter();

                        // Where 'objectBytes' is your byte array.
                        mStream.Write(objectBytes, 0, objectBytes.Length);
                        mStream.Position = 0;
                        selectedFilter = binFormatter.Deserialize(mStream) as Tuple<int, int, int, int>;
                    }
                }//Esperando a realizar un proceso
            }
        }

        private void Conection_Click(object sender, EventArgs e)
        {
            ip = IPAddress.Parse(textBox1.Text);
            if (!connected)
                try
                {
                    funcs.pgsql.abrirConexion();
                    connected = true;
                    worker.RunWorkerAsync();
                    this.Conection.BackColor = Color.FromArgb(163, 0, 0);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Unexpected exception : {0}", exc.ToString());
                }
            else
            {
                funcs.pgsql.cerrarConexion();
                selectedFilter = new Tuple<int, int, int, int>(-3, 0, 0, 0);
                connected = false;
                this.Conection.BackColor = Color.FromArgb(0, 74, 119);
            }
        }
    }
}
