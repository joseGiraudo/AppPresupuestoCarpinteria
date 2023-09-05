using AppPresupuestoCarpinteria.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AppPresupuestoCarpinteria.Datos;

namespace AppPresupuestoCarpinteria.Presentacion
{
    public partial class FrmNuevoPresupuesto : Form
    {
        Presupuesto nuevoPresupuesto = null;
        DBHelper dbHelper;
        public FrmNuevoPresupuesto()
        {
            InitializeComponent();
            nuevoPresupuesto = new Presupuesto();
            dbHelper = new DBHelper();
        }

        private void FrmNuevoProducto_Load(object sender, EventArgs e)
        {
            txtFecha.Text = DateTime.Today.ToShortDateString();
            txtCliente.Text = "Consumidor Final";
            txtDescuento.Text = "0";

            txtCantidad.Text = "1";

            lblNroPresupuesto.Text = lblNroPresupuesto.Text + " " + dbHelper.ProximoPresupuesto();
            CargarProductos();
        }

        private void CargarProductos()
        {
            DataTable table = dbHelper.CargarProductos();

            // cargo el combo box
            cboProducto.DataSource = table;
            cboProducto.ValueMember = table.Columns[0].ColumnName;
            cboProducto.DisplayMember = table.Columns[1].ColumnName;
        }

        private void btnAgregar_Click_1(object sender, EventArgs e)
        {
            // agrego productos al dataGridView

            // validar
            if (cboProducto.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un producto..", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(txtCantidad.Text) || !int.TryParse(txtCantidad.Text, out _))
            {
                MessageBox.Show("Debe ingresar una cantidad valida..", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            // me da error de objeto nulo en el if
            //foreach (DataGridViewRow row in dgvDetalle.Rows)
            //{
            //    if (row.Cells["colProducto"].Value.ToString().Equals(cboProducto.Text))
            //    {
            //        MessageBox.Show("Este producto ya está presupuestado...", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //        return;
            //    }
            //}

            DataRowView item = (DataRowView)cboProducto.SelectedItem;

            int nro = Convert.ToInt32(item.Row.ItemArray[0]);
            string nombre = item.Row.ItemArray[1].ToString();
            double precio = Convert.ToDouble(item.Row.ItemArray[2]);

            Producto prod = new Producto(nro, nombre, precio);

            int cant = Convert.ToInt32(txtCantidad.Text);
            DetallePresupuesto detalle = new DetallePresupuesto(prod, cant);

            nuevoPresupuesto.AgregarDetalle(detalle);

            dgvDetalle.Rows.Add(new object[] { detalle.Producto.NroProducto,
                                               detalle.Producto.Nombre,
                                               detalle.Producto.Precio,
                                               detalle.Cantidad,
                                               "Quitar" });

            CalcularTotales();


        }
        private void CalcularTotales()
        {
            txtSubTotal.Text = nuevoPresupuesto.CalcularTotal().ToString();
            if (!string.IsNullOrEmpty(txtDescuento.Text) && int.TryParse(txtDescuento.Text, out _))
            {
                double desc = nuevoPresupuesto.CalcularTotal() * Convert.ToDouble(txtDescuento.Text) / 100;
                txtTotal.Text = (nuevoPresupuesto.CalcularTotal() - desc).ToString();
            }
        }



        // fijarse que pasa cuando se aprieta el boton sin que haya un producto
        private void dgvDetalle_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvDetalle.CurrentCell.ColumnIndex == 4) // Boton quitar de la DGV, para eliminar un producto del presupuesto
            {
                nuevoPresupuesto.QuitarDetalle(dgvDetalle.CurrentRow.Index);
                dgvDetalle.Rows.RemoveAt(dgvDetalle.CurrentRow.Index);
                CalcularTotales();
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            //Validar
            if (string.IsNullOrEmpty(txtCliente.Text))
            {
                MessageBox.Show("Debe ingresar un cliente...", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (dgvDetalle.Rows.Count == 0)
            {
                MessageBox.Show("Debe ingresar al menos un detalle...", "Control", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            //Confirmar o Grabar
            GrabarPresupuesto();
        }

        private void GrabarPresupuesto()
        {
            nuevoPresupuesto.Fecha = Convert.ToDateTime(txtFecha.Text);
            nuevoPresupuesto.Cliente = txtCliente.Text;
            nuevoPresupuesto.Descuento = Convert.ToDouble(txtDescuento.Text);

            if (dbHelper.ConfirmarPresupuesto(nuevoPresupuesto))
            {
                MessageBox.Show("Se registro el presupuesto con exito", "Registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se pudo registrar el presupuesto", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
