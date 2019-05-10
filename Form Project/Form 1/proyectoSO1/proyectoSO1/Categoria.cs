using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyectoSO1
{
    class Categoria
    {

        private string nombre;
        private int cantPalabras;
        private int cantCoincidencias;
        private double probabilidad;

        public Categoria() { }

        public Categoria(string nombre)
        {
            this.nombre = nombre;
            this.cantPalabras = 0;
            this.cantCoincidencias = 1;
            this.probabilidad = 0.0;
        }

        public Categoria(string nombre, int cantPalabras, int cantCoincidencias, double probabilidad)
        {
            this.nombre = nombre;
            this.cantPalabras = cantPalabras;
            this.cantCoincidencias = cantCoincidencias;
            this.probabilidad = probabilidad;
        }

        public void setNombre(string nombre)
        {
            this.nombre = nombre;
        }

        public string getNombre()
        {
            return this.nombre;
        }

        public void setCantPalabras(int cantPalabras)
        {
            this.cantPalabras = cantPalabras;
        }

        public int getCantPalabras()
        {
            return this.cantPalabras;
        }

        public void setCantCoincidencias(int cantCoincidencias)
        {
            this.cantCoincidencias = cantCoincidencias;
        }

        public int getCantCoincidencias()
        {
            return this.cantCoincidencias;
        }

        public void setProbabilidad(double probabilidad)
        {
            this.probabilidad = probabilidad;
        }

        public double getProbabilidad()
        {
            return this.probabilidad;
        }
    }
}
