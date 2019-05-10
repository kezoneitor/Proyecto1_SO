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
        private List<String> filesDirs = new List<String>() { /*"./Files/minidmoz.csv"*/"./Files/dmoz.csv" };
        public List<Palabra> dataKnowledge = new List<Palabra>(); //Por el momento es una lista con las palabras
        public DatabaseFunc pgsql = new DatabaseFunc();
        public List<URL> URLs = new List<URL>();

        /*
         * Parametros:  lista > lista de strings para clasificar, 
         *              categoria > categoria de las palabras, 
         *              innecesarios > lista de caracteres a omitir
         * 
         * Descripcion: Por cada elemento en la lista revisa si existe 
         *              la palabra en la base de conocimiento para sumar a cantidad
         *              sino para agregar a la base de conocimiento.
         *  
         */
        private void agregar(String[] lista, String categoria)
        {
            //Excepciones para quitar de una palabra
            string[] excepciones = new string[]
            {
                "\"", " ", "\"", "[", "]", "\'s", "\'ve", "\'re", "i\'m", "\'u",
                "o\'", "don\'t", "didn't", "l\'il"
            };
            string[] omitir = new string[]
            {
                "a", "b","c","d","e","f","g","h","i","j","k",
                "l","m","n","o","p","q","r","s","t","u","v",
                "w","x","y","z", "and", "of", "to", "by", "so",
                "about", "then", "that", "this", "from", "its",
                "you", "he", "she", "they", "ve", "on", "in","under",
                "these","those", "me", "i","am","the", "for", "all", "at",
                "are", "is", "her", "him", "his", "ur", "has", "if", "else",
                "an", "any", "be", "de", "my", "as", "or", "and", "ni", "mi",
                "las", "los", "we", "&amp", "a&amp"
            };
            foreach (var element in lista)
            {
                string item = element;
                //Limpiar la palabra con las excepciones
                Parallel.ForEach(excepciones, exp =>
                {

                    item = item.Replace(exp, "");
                });
                //Eliminar las palabras omitidas 
                Parallel.ForEach(omitir, omi =>
                {
                    if (omi.Equals(item))
                    {
                        item = "";
                        return;
                    }
                });
                if (!item.Equals(""))
                {
                    bool existe = false;
                    Parallel.ForEach(dataKnowledge, data =>
                    {
                        if (item.Equals(data.getPalabra()) && categoria.Equals(data.getCategoria()))
                        {
                            existe = true;
                        }
                    });

                    if (!existe)
                    {
                        Palabra pal = new Palabra(categoria, item);
                        dataKnowledge.Add(pal);

                        pgsql.insertarPalabra(categoria, item);

                    }
                }
            }
        }
        /*
         * Parametros:  palabra > string a limpiar, 
         *              
         * 
         * Descripcion: Limpiar la linea que se trae del archivo
         *              para eliminar los caracteres innecesarios
         *  
         */
        private string CleanInput(string palabra)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(palabra, @"[()@:;0123456789/\%+|=*'!?#$&�.-]", " ",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters, 
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        public void cargarBConocimiento(ref ProgressBar barForm)
        {
            int bar = 0;
            foreach (String file in filesDirs)
            {
                try
                {
                    string[] lineas = File.ReadAllLines(file); //Extraer la infomacion del csv

                    //Variables para ayuda visual del usuario
                    int count = 1;
                    int total = lineas.Length;
                    int cambio = -1;
                    var sw = Stopwatch.StartNew();
                    //--Variables para ayuda visual del usuario
                    foreach (var linea in lineas)
                    {
                        string cleanLinea = CleanInput(linea.ToLower());//Limpiar la linea de caracteres innecesarios
                        var dato = cleanLinea.Split(',');
                        String[] titulo;
                        String[] descripcion;
                        try
                        {
                            //Quitar los puntos y separar en categoria, titulo y descripcion
                            titulo = dato[2].Replace(",", "").Split(' '); //Separar el titulo
                            descripcion = dato[3].Replace(",", "").Split(' '); //Separar la descripcion
                            agregar(titulo, dato[1].ToLower());
                            agregar(descripcion, dato[1].ToLower());
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            Console.WriteLine("Split Error: {0} \n Linea <{1}>", e, dato[0]);
                        }

                        //Agregar a la base de conocimiento las palabras
                        bar = (count * 100 / total);
                        if (bar != cambio)
                        {
                            cambio = bar;
                            barForm.Increment(1);
                        }
                        count++;
                    }
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("File Error <{0}>: {1}", file, e);
                }
                if (bar >= 100)
                {
                    barForm.Value = 0;
                    MessageBox.Show(file + " cargada completamente!!");
                }
            }
        }

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

        public void ObtenerURLs(ref ProgressBar barForm)
        {
            pgsql.eliminarURLs();
            string[] listaInformacion = File.ReadAllLines("./Files/URL-Classification.csv");
            //Variables para ayuda visual del usuario
            int total = listaInformacion.Length;
            int cambio = -1;
            int bar = 0;
            //--Variables para ayuda visual del usuario
            for (int i = 0; i < listaInformacion.Length; i++)
            {
                string[] linea = listaInformacion[i].Split(',');
                URL url = new URL(linea[1].Replace("\'", ""), 0);
                URLs.Add(url);
                pgsql.insertarURL(url.getTexto(), url.getclasificacion());
                bar = ((i + 1) * 100 / total);
                if (bar != cambio)
                {
                    cambio = bar;
                    barForm.Increment(1);
                }
            }
            if (bar >= 100)
            {
                barForm.Value = 0;
                MessageBox.Show("URLs cargadas completamente!!");
            }
        }
    }
}
