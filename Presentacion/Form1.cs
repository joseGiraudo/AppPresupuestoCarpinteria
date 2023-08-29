using AppPresupuestoCarpinteria.Presentacion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppPresupuestoCarpinteria
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmNuevoPresupuesto nuevoPresupuesto = new FrmNuevoPresupuesto();
            nuevoPresupuesto.ShowDialog(); // el ShowDialog no permite moverse entre ventanas
        }

        private void consultarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmConsultarPresupuesto consulta = new FrmConsultarPresupuesto();
            consulta.ShowDialog();
        }

    }
}
