using AppPresupuestoCarpinteria.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppPresupuestoCarpinteria.Datos
{
    internal class DBHelper
    {

        // creo y configuro la conexion
        private SqlConnection connection;
        // string connectionString = @"Data Source=172.16.10.196;Initial Catalog=Carpinteria_2023;User ID=alumno1w1;Password=alumno1w1";
        string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=CARPINTERIA_2023;Integrated Security=True";


        // constructor
        public DBHelper()
        {
            connection = new SqlConnection(connectionString);
        }

        public DataTable Consultar(string nombreSP)
        {
            connection.Open();

            // configuro el comando con el SP pasado por parametros
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = nombreSP;

            DataTable table = new DataTable();
            table.Load(command.ExecuteReader());

            connection.Close();

            return table;
        }


        public DataTable Consultar(string nombreSP, List<Parametro> listParam)
        {
            connection.Open();

            // configuro el comando con el SP pasado por parametros
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = nombreSP;

            command.Parameters.Clear();

            foreach(Parametro param in listParam)
            {
                command.Parameters.AddWithValue(param.Nombre, param.Valor);
            }

            DataTable table = new DataTable();
            table.Load(command.ExecuteReader());

            connection.Close();

            return table;
        }


        public string ProximoPresupuesto()
        {
            try
            {

                // abro la conexion
                connection.Open();

                // creo y configuro el comando con el SP
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_PROXIMO_ID";

                // creo y configuro un parametro porque el SP devuelve un parametro
                SqlParameter parameter = new SqlParameter();
                parameter.ParameterName = "@next";
                parameter.SqlDbType = SqlDbType.Int;
                parameter.Direction = ParameterDirection.Output;

                // paso el parametro al comando (para que traiga el resultado del SP)
                command.Parameters.Add(parameter);

                // ejecuto el comando
                command.ExecuteNonQuery();

                connection.Close();
                
                return parameter.Value.ToString();

            }
            catch (Exception ex)
            {
                return "Hubo un error de datos";
            }

        }


        public DataTable CargarProductos() // ver de agregar el nombre del SP como un parametro
        {
            // abro la conexion
            connection.Open();

            // creo y configuro el comando
            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "SP_CONSULTAR_PRODUCTOS";

            DataTable table = new DataTable();
            table.Load(command.ExecuteReader());

            connection.Close();

            return table;
        }

        public bool ConfirmarPresupuesto(Presupuesto nuevoPre)
        {
            bool result = true;

            // objeto transaction propio de ADO
            SqlTransaction transaction = null;

            try
            {
                // abro la conexion y abro una transaction con esa conexion
                connection.Open();
                transaction = connection.BeginTransaction(); // Comienzo de la transaction

                // creo y configuro el comando
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.Transaction = transaction; // Asigno el objeto transaction al command
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_INSERTAR_MAESTRO"; // procedimientos almacenados creados en la bd

                // configuracion de los parametros de entrada del SP
                command.Parameters.AddWithValue("@cliente", nuevoPre.Cliente);
                command.Parameters.AddWithValue("@dto", nuevoPre.Descuento);
                command.Parameters.AddWithValue("@total", nuevoPre.CalcularTotal());

                // creo y configuro el parametro porque el SP devuelve un parametro (parametro de salida): nro de presupuesto
                SqlParameter parameter = new SqlParameter();
                parameter.ParameterName = "@presupuesto_nro";
                parameter.SqlDbType = SqlDbType.Int;
                parameter.Direction = ParameterDirection.Output;

                // paso el parametro al comando (para que traiga el resultado del SP)
                command.Parameters.Add(parameter);

                // ejecuto el comando
                command.ExecuteNonQuery();

                // ahora debo insertar los detalles del presupueto al presupuesto (del cual ya tengo el numero)
                // el nro de presupuesto es el parametro de salida
                int presupuestoNro = (int)parameter.Value;

                // genero el nro de detalle
                int detalleNro = 1;

                // creo un command para llamar al SP que inserta los detalles al presupuesto
                SqlCommand commandDetalles;

                foreach (DetallePresupuesto detP in nuevoPre.Detalles)
                {
                    commandDetalles = new SqlCommand("SP_INSERTAR_DETALLE", connection, transaction); // paso los valores como parametros
                    commandDetalles.CommandType = CommandType.StoredProcedure;
                    // paso los parametros de entrada del sp de cada detalle que agrego
                    commandDetalles.Parameters.AddWithValue("@presupuesto_nro", presupuestoNro);
                    commandDetalles.Parameters.AddWithValue("@detalle", detalleNro);
                    commandDetalles.Parameters.AddWithValue("@id_producto", detP.Producto.NroProducto);
                    commandDetalles.Parameters.AddWithValue("@cantidad", detP.Cantidad);
                    commandDetalles.ExecuteNonQuery();
                    detalleNro++;

                }


                // confirmo la transaction
                transaction.Commit();
            }

            catch (Exception ex)
            {
                // si la transaction no es nula quiere decir que comenzo la transaction
                if(transaction != null)
                {
                    transaction.Rollback();
                    result = false;
                }
            }
            finally
            {
                if(connection != null && connection.State == ConnectionState.Open)
                {
                    // por las dudas la coneccion quede abierta, la cerramos
                    connection.Close();
                }
            }

            return result;
        }

    }
}
