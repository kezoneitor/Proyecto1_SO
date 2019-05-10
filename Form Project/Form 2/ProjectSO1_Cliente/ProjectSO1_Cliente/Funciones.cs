using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace proyectoSO1
{
    class Funciones
    {
        public DatabaseFunc pgsql = new DatabaseFunc();
        public List<URL> URLs = new List<URL>();

        public void ComprobacionExistente(int posURL, string categoria)
        {

            bool existe = false;
            for (int i = 0; i < URLs[posURL].getCategorias().Count; i++)
            {
                if (URLs[posURL].getCategorias()[i].getNombre().Equals(categoria))
                {
                    int cantidad = URLs[posURL].getCategorias()[i].getCantCoincidencias() + 1;
                    URLs[posURL].getCategorias()[i].setCantCoincidencias(cantidad);
                    existe = true;
                    break;
                }
            }

            if (!existe)
            {
                URLs[posURL].agregarCategoria(new Categoria(categoria));
            }
        }

        public void ObtenerURLs(int inicio, int final)
        {
            URLs = pgsql.getURL(inicio, final);
        }

        //Secuencial-------------------------------------

        public void ClasificarURLsSecuencial(ref ProgressBar barForm, ref Label labelForm)
        {
            //labelForm.Text = "Ejecutando clasificador Sec:";
            Console.WriteLine("Ejecutando clasificador Sec:");
            List<string> datosRecibidos = pgsql.getData();
            //Console.WriteLine("Secuencial=================");
            //Variables para ayuda visual del usuario
            int count = 1;
            int total = URLs.Count;
            int cambio = -1;
            int bar = 0;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < URLs.Count; i++)//Recorrer lista de URLs
            {
                for (int x = 1; x < datosRecibidos.Count; x += 2)
                {
                    bool comprobacion = URLs[i].getTexto().Contains(datosRecibidos[x]);
                    if (comprobacion)
                    {
                        ComprobacionExistente(i, datosRecibidos[x - 1]);
                    }
                    bar = (count * 100 / total);
                    if (bar != cambio)
                    {
                        cambio = bar;
                        //barForm.Increment(1);
                    }
                    count++;
                }
                bar = ((i+1) * 100 / total);
                if (bar != cambio)
                {
                    cambio = bar;
                    //barForm.Increment(1);
                }
            }
            if (bar >= 100)
            {
                //barForm.Value = 0;
                Console.WriteLine("Clasificacion secuencial completada!!\nTiempo: " + Math.Round(sw.Elapsed.TotalSeconds / 60, 2) + " minutos");
            }
        }


        public void subirDatos() {

            for (int i = 0; i < URLs.Count; i++)
            {
                for (int x = 0; x < URLs[i].getCategorias().Count; x++)
                {
                    pgsql.insertarURL(URLs[i].getId(), URLs[i].getCategorias()[x].getNombre(), URLs[i].getCategorias()[x].getCantPalabras(), URLs[i].getCategorias()[x].getCantCoincidencias(), URLs[i].getCategorias()[x].getProbabilidad());
                }
            }
        }

        public void BayesSecuencial(ref ProgressBar barForm, ref Label labelForm)
        {
            //labelForm.Text = "Ejecutando bayes Sec:";
            Console.WriteLine("Ejecutando bayes Sec:");
            Int64 cantidadTotalBase = pgsql.ObtenerCantidadBase(); // obtiene la cantidad de palabras totales que hay en la base de datos
            //Variables para ayuda visual del usuario
            int total = URLs.Count;
            int cambio = -1;
            int bar = 0;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < URLs.Count; i++)
            {
                double probMayor = -1;
                string nombreProbMayor = " ";
                for (int x = 0; x < URLs[i].getCategorias().Count; x++)
                {
                    double totalCategoria = URLs[i].getCategorias()[x].getCantCoincidencias(); // total de coincidencias de una categoria
                    double cantCategoria = Convert.ToDouble(pgsql.ObtenerCantidadCategoria(URLs[i].getCategorias()[x].getNombre()));  //total de palabras en una categoria

                    double cantFinal = (cantCategoria / cantidadTotalBase) * (totalCategoria / cantCategoria);

                    if (probMayor < cantFinal)
                    {
                        probMayor = cantFinal;
                        nombreProbMayor = URLs[i].getCategorias()[x].getNombre();
                    }
                    URLs[i].getCategorias()[x].setProbabilidad(Math.Round(cantFinal, 4));
                    bar = ((i + 1) * 100 / total);
                    
                }
                if (bar != cambio)
                {
                    cambio = bar;
                    //barForm.Increment(1);
                }
            }
            if (bar >= 100)
            {
                //barForm.Value = 0;
                Console.WriteLine("Bayesiano secuencial completado!!\nTiempo: " + Math.Round(sw.Elapsed.TotalSeconds / 60, 2) + " minutos");
            }
        }
        //---Secuencial----------------------------------

        //Concurrente------------------------------------
        public void ClasificarURLs(List<string> datos, int index)
        {
            for (int x = 1; x < datos.Count; x += 2)
            {
                bool comprobacion = URLs[index].getTexto().Contains(datos[x]);
                if (comprobacion)
                {
                    ComprobacionExistente(index, datos[x - 1]);
                }
            }
            //  Console.WriteLine(".");
        }

        public void URLsConcurrente(ref ProgressBar barForm, ref Label labelForm)
        {
            //labelForm.Text = "Ejecutando clasificador Con:";
            Console.WriteLine("Ejecutando clasificador Con:");
            List<string> datosRecibidos = pgsql.getData();
            //Variables para ayuda visual del usuario
            int total = URLs.Count;
            int cambio = -1;
            int bar = 0;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < URLs.Count; i += 4)//Recorrer lista de URLs
            {
                Parallel.Invoke(() =>
                {
                    ClasificarURLs(datosRecibidos, i);
                },
                () =>
                {
                    if (i + 1 < URLs.Count)
                    {
                        ClasificarURLs(datosRecibidos, i + 1);
                    }
                },

                () =>
                {
                    if (i + 2 < URLs.Count)
                    {
                        ClasificarURLs(datosRecibidos, i + 2);
                    }
                },
                () =>
                {
                    if (i + 3 < URLs.Count)
                    {
                        ClasificarURLs(datosRecibidos, i + 3);
                    }
                });
                bar = ((i + 1) * 100 / total);
                if (bar != cambio)
                {
                    cambio = bar;
                    //barForm.Increment(1);
                }
            }
            if (bar >= 100)
            {
                //barForm.Value = 0;
                Console.WriteLine("Clasificacion concurrente completada!!\nTiempo: " + Math.Round(sw.Elapsed.TotalSeconds / 60, 2) + " minutos");
            }
        }

        private void funcBayesConc(int index, Int64 cantidadTotalBase, DatabaseFunc conn)
        {

            double probMayor = -1;
            string nombreProbMayor = " ";
            for (int x = 0; x < URLs[index].getCategorias().Count; x++)
            {
                double totalCategoria = URLs[index].getCategorias()[x].getCantCoincidencias(); // total de coincidencias de una categoria
                double cantCategoria = Convert.ToDouble(conn.ObtenerCantidadCategoria(URLs[index].getCategorias()[x].getNombre()));  //total de palabras en una categoria

                double cantFinal = (cantCategoria / cantidadTotalBase) * (totalCategoria / cantCategoria);

                if (probMayor < cantFinal)
                {
                    probMayor = cantFinal;
                    nombreProbMayor = URLs[index].getCategorias()[x].getNombre();
                }

                URLs[index].getCategorias()[x].setProbabilidad(Math.Round(cantFinal, 4));

            }

            //Console.WriteLine("URL: {0} \n|| prob: {1} \n|| nom: {2}", URLs[index].getTexto(), Math.Round(probMayor, 4), nombreProbMayor);
        }

        public void BayesConcurrente(ref ProgressBar barForm, ref Label labelForm)
        {
            //labelForm.Text = "Ejecutando bayes Con:";
            Console.WriteLine("Ejecutando bayes Con:");
            Int64 cantidadTotalBase = pgsql.ObtenerCantidadBase(); // obtiene la cantidad de palabras totales que hay en la base de datos
            //Variables para ayuda visual del usuario
            int total = URLs.Count;
            int cambio = -1;
            int bar = 0;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < URLs.Count; i+=4)
            {
                Parallel.Invoke(() =>
                {
                    DatabaseFunc conn = new DatabaseFunc();
                    conn.abrirConexion();
                    funcBayesConc(i, cantidadTotalBase, conn);
                    conn.cerrarConexion();
                },
               () =>
               {
                   if(i + 1 < URLs.Count)
                   {
                       DatabaseFunc conn = new DatabaseFunc();
                       conn.abrirConexion();
                       funcBayesConc(i + 1, cantidadTotalBase, conn);
                       conn.cerrarConexion();
                   }
               },

               () =>
               {
                   if (i  + 2< URLs.Count)
                   {
                       DatabaseFunc conn = new DatabaseFunc();
                       conn.abrirConexion();
                       funcBayesConc(i + 2, cantidadTotalBase, conn);
                       conn.cerrarConexion();
                   }
               },
               () =>
               {
                    if(i  + 3 < URLs.Count)
                   {
                       DatabaseFunc conn = new DatabaseFunc();
                       conn.abrirConexion();
                       funcBayesConc(i + 3, cantidadTotalBase, conn);
                       conn.cerrarConexion();
                   }
               });
                bar = ((i + 1) * 100 / total);
                if (bar != cambio)
                {
                    cambio = bar;
                    //barForm.Increment(1);
                }
            }
            if (bar >= 100)
            {
                //barForm.Value = 0;
                Console.WriteLine("Bayesiano concurrente completada!!\nTiempo: " + Math.Round(sw.Elapsed.TotalSeconds / 60, 2) + " minutos");
            }
        }
    }
}
