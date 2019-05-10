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

        public DatabaseFunc()
        {
            this.connection = new NpgsqlConnection();
        }

        public void abrirConexion()
        {
            try
            {
                this.connection.ConnectionString = "Username = postgres; Password = 12345; Host = localhost; Port = 5432; Database = proyectoSO1";
                this.connection.Open();
                Console.WriteLine("Correcta conexión !!");
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

        public void insertarPalabra(string categoria, string item)
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("INSERT INTO palabra (categoria, palabra) VALUES ('" + categoria + "', '" + item + "');", connection);
            queryPalabra.ExecuteNonQuery();
        }

        public void insertarURL(string url, string clasificacion)
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("INSERT INTO url (texto, clasificacion) VALUES ('"+ url +"', '"+ clasificacion +"');", connection);
            queryPalabra.ExecuteNonQuery();
        }

        public void eliminarURLs()
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("DELETE FROM categoria; DELETE FROM url", connection);
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

        public Int64 ObtenerCantidadURLs()
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("Select count(*) from url", connection);
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

        public List<URL> getURL()
        {
            List<URL> datos = new List<URL>();
            NpgsqlCommand queryPalabra = new NpgsqlCommand("SELECT * FROM url order by id_url", connection);
            NpgsqlDataReader dr = queryPalabra.ExecuteReader();
            while (dr.Read())
            {

                datos.Add(new URL(dr[1].ToString(), dr[2].ToString(), (int)dr[0]));
            }
            dr.Close();
            return datos;
        }

        public void getPro(ref List<URL> URLs)
        {
            NpgsqlCommand queryPalabra = new NpgsqlCommand("SELECT * FROM categoria order by id_url_cat", connection);
            NpgsqlDataReader dr = queryPalabra.ExecuteReader();
            while (dr.Read())
            {
                URL find = URLs.Find(url => url.getId().Equals((int)dr[0]));
                find.agregarCategoria(new Categoria(dr[1].ToString(), Convert.ToInt16(dr[2].ToString()), Convert.ToInt16( dr[3].ToString()), Convert.ToDouble(dr[4].ToString())));
            }
            dr.Close();
        }
    }
}
