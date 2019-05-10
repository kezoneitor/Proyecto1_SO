using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyectoSO1
{
    class Palabra
    {

        private String palabra;
        private String categoria;

        public Palabra(String categoria, String palabra)
        {
            this.categoria = categoria;
            this.palabra = palabra;
        }

        public void setCategoria(String categoria)
        {
            this.categoria = categoria;
        }

        public String getCategoria()
        {
            return this.categoria;
        }

        public void setPalabra(String palabra)
        {
            this.palabra = palabra;
        }

        public String getPalabra()
        {
            return this.palabra;
        }

        public String pString()
        {
            return "{Cat: " + this.categoria + ", Pal: " + this.palabra + "}";
        }
    }
}
