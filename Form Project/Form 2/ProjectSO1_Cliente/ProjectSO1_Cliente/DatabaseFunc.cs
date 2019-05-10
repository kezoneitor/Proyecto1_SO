using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace proyectoSO1
{
    class DatabaseFunc
    {

        NpgsqlConnection connection;
        string ip;

        public DatabaseFunc()
        {
            this.connection = new NpgsqlConnection();
            ip = "localhost";
        }

        public void setIP(string ip)
        {
            this.ip = ip;
        }

        public void abrirConexion()
        {
            try
            {
                this.connection.ConnectionString = "Username = postgres; Password = 12345; Host = " + ip + "; Port = 5432; Database = proyectoSO1";
                this.connection.Open();
            }
            catch (global::System.Exception)
            {
                Console.WriteLine("ERROR... de conexión");
                throw;
            }
        }

        public void cerrarConexion()
        {
            this.connection.Close();
        }


        //INSERT INTO categoria (id_url_cat, nombre,cantPalabras,cantCoincidencias, probabilidad) 

        public void insertarURL(int id_url_cat, string nombre, int cantPalabras, int cantCoincidencias, double probabilidad)
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("INSERT INTO categoria (id_url_cat, nombre,cantPalabras,cantCoincidencias, probabilidad) VALUES (" + id_url_cat +", '" + nombre + "', " + cantPalabras + "," + cantCoincidencias + "," + probabilidad.ToString().Replace(",", ".") +");", connection);
            queryPalabra.ExecuteNonQuery();
        }

        public void insertarPalabra(string categoria, string item)
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("INSERT INTO palabra (categoria, palabra) VALUES ('" + categoria + "', '" + item + "');", connection);
            queryPalabra.ExecuteNonQuery();
        }

        public Int64 ObtenerCantidadCategoria(string categoria)
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("Select count(*) from palabra where palabra.categoria = '" + categoria + "';", connection);
            return (Int64)queryPalabra.ExecuteScalar();
        }

        public Int64 ObtenerCantidadBase()
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("Select count(*) from palabra", connection);
            return (Int64)queryPalabra.ExecuteScalar();
        }

        public List<string> getData()
        {
            List<string> datos = new List<string>();
            NpgsqlCommand queryPalabra = new NpgsqlCommand("SELECT * FROM palabra", connection);
            NpgsqlDataReader dr = queryPalabra.ExecuteReader();
            while (dr.Read())
            {
                datos.Add(dr[0].ToString());
                datos.Add(dr[1].ToString());
            }
            dr.Close();
            return datos;
        }

        public List<URL> getURL(int inicio, int final)
        {
            List<URL> datos = new List<URL>();
            NpgsqlCommand queryPalabra = new NpgsqlCommand("SELECT * FROM url order by id_url limit " + final + " offset " + inicio, connection);
            NpgsqlDataReader dr = queryPalabra.ExecuteReader();
            while (dr.Read())
            {

                datos.Add(new URL(dr[1].ToString(), dr[2].ToString(), (int)dr[0]));
            }
            dr.Close();
            return datos;
        }
    }
}
