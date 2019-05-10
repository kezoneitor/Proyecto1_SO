using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyectoSO1
{
    class URL
    {
        public int id;
        public string texto;
        public List<Categoria> categorias = new List<Categoria>();
        public string clasificacion;


        public URL() { }

        public URL(string texto, int id)
        {
            this.id = id;
            this.texto = texto;
            this.categorias = new List<Categoria>();
            this.clasificacion = "";

        }

        public URL(string texto, string clasificacion, int id)
        {
            this.id = id;
            this.texto = texto;
            this.categorias = new List<Categoria>();
            this.clasificacion = clasificacion;
        }

        public void setTexto(string texto)
        {
            this.texto = texto;
        }

        public string getTexto()
        {
            return this.texto;
        }

        public void setClasificacion(string clasificacion)
        {
            this.clasificacion = clasificacion;
        }

        public string getclasificacion()
        {
            return this.clasificacion;
        }

        public void setId(int id)
        {
            this.id = id;
        }

        public int getId()
        {
            return this.id;
        }

        public List<Categoria> getCategorias()
        {
            return this.categorias;
        }

        public void agregarCategoria(Categoria categoria)
        {
            this.categorias.Add(categoria);
        }


        public string toString()
        {
            string cat = "List(";
            foreach (Categoria i in this.categorias)
            {
                cat += "cat: " + i.getNombre() + " coinc: " + i.getCantCoincidencias() + "\n";
            }
            return "URL: " + this.texto + cat + ")\n Clasi: " + this.clasificacion;
        }
    }
}
