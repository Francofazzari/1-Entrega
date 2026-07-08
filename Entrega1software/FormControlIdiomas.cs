using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using BE;
using BLL;

namespace Entrega1software
{
    public partial class FormControlIdiomas : Form, IObservadorIdioma
    {
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.DataGridView dgvIdiomas;
        private System.Windows.Forms.Label lblNuevoIdioma;
        private System.Windows.Forms.TextBox txtNuevoIdioma;
        private System.Windows.Forms.Button btnAgregarIdioma;
        private System.Windows.Forms.Button btnRenombrar;
        private System.Windows.Forms.Button btnEliminar;
        private System.Windows.Forms.Label lblTraducciones;
        private System.Windows.Forms.DataGridView dgvTraducciones;
        private System.Windows.Forms.Button btnGuardarTraducciones;
        private System.Windows.Forms.Button btnAplicarIdioma;

        public FormControlIdiomas()
        {
            InitializeComponent();
            CargarIdiomas();
            IdiomaManager.Instancia.Suscribir(this);
        }

        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.dgvIdiomas = new System.Windows.Forms.DataGridView();
            this.lblNuevoIdioma = new System.Windows.Forms.Label();
            this.txtNuevoIdioma = new System.Windows.Forms.TextBox();
            this.btnAgregarIdioma = new System.Windows.Forms.Button();
            this.btnRenombrar = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.lblTraducciones = new System.Windows.Forms.Label();
            this.dgvTraducciones = new System.Windows.Forms.DataGridView();
            this.btnGuardarTraducciones = new System.Windows.Forms.Button();
            this.btnAplicarIdioma = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIdiomas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraducciones)).BeginInit();
            this.SuspendLayout();
            
            // lblTitulo
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblTitulo.Location = new System.Drawing.Point(20, 20);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(185, 22);
            this.lblTitulo.Text = "Gestion de Idiomas";
            
            // dgvIdiomas
            this.dgvIdiomas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIdiomas.Location = new System.Drawing.Point(20, 60);
            this.dgvIdiomas.Name = "dgvIdiomas";
            this.dgvIdiomas.Size = new System.Drawing.Size(340, 200);
            this.dgvIdiomas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvIdiomas.MultiSelect = false;
            this.dgvIdiomas.SelectionChanged += new System.EventHandler(this.dgvIdiomas_SelectionChanged);
            
            // lblNuevoIdioma
            this.lblNuevoIdioma.AutoSize = true;
            this.lblNuevoIdioma.Location = new System.Drawing.Point(20, 280);
            this.lblNuevoIdioma.Name = "lblNuevoIdioma";
            this.lblNuevoIdioma.Size = new System.Drawing.Size(76, 13);
            this.lblNuevoIdioma.Text = "Nuevo idioma:";
            
            // txtNuevoIdioma
            this.txtNuevoIdioma.Location = new System.Drawing.Point(100, 277);
            this.txtNuevoIdioma.Name = "txtNuevoIdioma";
            this.txtNuevoIdioma.Size = new System.Drawing.Size(130, 20);
            
            // btnAgregarIdioma
            this.btnAgregarIdioma.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.btnAgregarIdioma.ForeColor = System.Drawing.Color.White;
            this.btnAgregarIdioma.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarIdioma.Location = new System.Drawing.Point(240, 272);
            this.btnAgregarIdioma.Name = "btnAgregarIdioma";
            this.btnAgregarIdioma.Size = new System.Drawing.Size(120, 30);
            this.btnAgregarIdioma.Text = "Agregar idioma";
            this.btnAgregarIdioma.UseVisualStyleBackColor = false;
            this.btnAgregarIdioma.Click += new System.EventHandler(this.btnAgregarIdioma_Click);
            
            // btnRenombrar
            this.btnRenombrar.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnRenombrar.ForeColor = System.Drawing.Color.White;
            this.btnRenombrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRenombrar.Location = new System.Drawing.Point(20, 320);
            this.btnRenombrar.Name = "btnRenombrar";
            this.btnRenombrar.Size = new System.Drawing.Size(120, 30);
            this.btnRenombrar.Text = "Renombrar";
            this.btnRenombrar.UseVisualStyleBackColor = false;
            this.btnRenombrar.Click += new System.EventHandler(this.btnRenombrar_Click);
            
            // btnEliminar
            this.btnEliminar.BackColor = System.Drawing.Color.FromArgb(210, 50, 50);
            this.btnEliminar.ForeColor = System.Drawing.Color.White;
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.Location = new System.Drawing.Point(150, 320);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(120, 30);
            this.btnEliminar.Text = "Eliminar";
            this.btnEliminar.UseVisualStyleBackColor = false;
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            
            // btnAplicarIdioma
            this.btnAplicarIdioma.BackColor = System.Drawing.Color.FromArgb(29, 158, 117);
            this.btnAplicarIdioma.ForeColor = System.Drawing.Color.White;
            this.btnAplicarIdioma.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAplicarIdioma.Location = new System.Drawing.Point(20, 360);
            this.btnAplicarIdioma.Name = "btnAplicarIdioma";
            this.btnAplicarIdioma.Size = new System.Drawing.Size(340, 30);
            this.btnAplicarIdioma.Text = "Aplicar idioma a la app";
            this.btnAplicarIdioma.UseVisualStyleBackColor = false;
            this.btnAplicarIdioma.Click += new System.EventHandler(this.btnAplicarIdioma_Click);

            // lblTraducciones
            this.lblTraducciones.AutoSize = true;
            this.lblTraducciones.Location = new System.Drawing.Point(390, 44);
            this.lblTraducciones.Name = "lblTraducciones";
            this.lblTraducciones.Size = new System.Drawing.Size(75, 13);
            this.lblTraducciones.Text = "Traducciones";
            
            // dgvTraducciones
            this.dgvTraducciones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTraducciones.Location = new System.Drawing.Point(390, 60);
            this.dgvTraducciones.Name = "dgvTraducciones";
            this.dgvTraducciones.Size = new System.Drawing.Size(380, 280);
            
            // btnGuardarTraducciones
            this.btnGuardarTraducciones.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
            this.btnGuardarTraducciones.ForeColor = System.Drawing.Color.White;
            this.btnGuardarTraducciones.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarTraducciones.Location = new System.Drawing.Point(390, 350);
            this.btnGuardarTraducciones.Name = "btnGuardarTraducciones";
            this.btnGuardarTraducciones.Size = new System.Drawing.Size(150, 30);
            this.btnGuardarTraducciones.Text = "Guardar traducciones";
            this.btnGuardarTraducciones.UseVisualStyleBackColor = false;
            this.btnGuardarTraducciones.Click += new System.EventHandler(this.btnGuardarTraducciones_Click);
            
            // FormControlIdiomas
            this.ClientSize = new System.Drawing.Size(800, 420);
            this.Controls.Add(this.lblTraducciones);
            this.Controls.Add(this.btnGuardarTraducciones);
            this.Controls.Add(this.dgvTraducciones);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.btnRenombrar);
            this.Controls.Add(this.btnAgregarIdioma);
            this.Controls.Add(this.btnAplicarIdioma);
            this.Controls.Add(this.txtNuevoIdioma);
            this.Controls.Add(this.lblNuevoIdioma);
            this.Controls.Add(this.dgvIdiomas);
            this.Controls.Add(this.lblTitulo);
            this.Name = "FormControlIdiomas";
            this.Text = "Gestion de Idiomas";
            ((System.ComponentModel.ISupportInitialize)(this.dgvIdiomas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTraducciones)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public void ActualizarIdioma(Idioma idioma)
        {
            lblTitulo.Text = IdiomaManager.Instancia.ObtenerMensaje("lbl_titulo_idiomas") == "lbl_titulo_idiomas" ? "Gestion de Idiomas" : IdiomaManager.Instancia.ObtenerMensaje("lbl_titulo_idiomas");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            IdiomaManager.Instancia.Desuscribir(this);
            base.OnFormClosing(e);
        }

        private void CargarIdiomas()
        {
            dgvIdiomas.DataSource = null;
            dgvIdiomas.DataSource = IdiomaManager.Instancia.ObtenerIdiomas();
            if(dgvIdiomas.Columns.Count > 0)
            {
                dgvIdiomas.Columns["Codigo"].Visible = false;
                dgvIdiomas.Columns["DigitoVerificador"].Visible = false;
            }
        }

        private void dgvIdiomas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvIdiomas.SelectedRows.Count > 0)
            {
                Idioma i = (Idioma)dgvIdiomas.SelectedRows[0].DataBoundItem;
                lblTraducciones.Text = "Traducciones - " + i.Nombre;
                
                var traducciones = IdiomaManager.Instancia.ObtenerTraduccionesGrid(i.Id);
                var bindingList = new BindingList<Traduccion>(traducciones);
                dgvTraducciones.DataSource = bindingList;
                dgvTraducciones.Columns["Clave"].ReadOnly = true;
                dgvTraducciones.Columns["Valor"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void btnAgregarIdioma_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNuevoIdioma.Text)) return;
            Idioma i = new Idioma { Nombre = txtNuevoIdioma.Text.Trim(), Codigo = "" };
            if (IdiomaManager.Instancia.InsertarIdioma(i))
            {
                MessageBox.Show("Idioma agregado");
                txtNuevoIdioma.Text = "";
                CargarIdiomas();
            }
        }

        private void btnRenombrar_Click(object sender, EventArgs e)
        {
            if (dgvIdiomas.SelectedRows.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(txtNuevoIdioma.Text))
                {
                    MessageBox.Show("Escriba el nuevo nombre en el cuadro de texto.");
                    return;
                }
                Idioma i = (Idioma)dgvIdiomas.SelectedRows[0].DataBoundItem;
                if (IdiomaManager.Instancia.RenombrarIdioma(i.Id, txtNuevoIdioma.Text.Trim()))
                {
                    MessageBox.Show("Idioma renombrado");
                    txtNuevoIdioma.Text = "";
                    CargarIdiomas();
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvIdiomas.SelectedRows.Count > 0)
            {
                Idioma i = (Idioma)dgvIdiomas.SelectedRows[0].DataBoundItem;
                if(MessageBox.Show("Desea eliminar el idioma?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (IdiomaManager.Instancia.EliminarIdioma(i.Id))
                    {
                        MessageBox.Show("Idioma eliminado");
                        CargarIdiomas();
                    }
                }
            }
        }

        private void btnAplicarIdioma_Click(object sender, EventArgs e)
        {
            if (dgvIdiomas.SelectedRows.Count > 0)
            {
                Idioma i = (Idioma)dgvIdiomas.SelectedRows[0].DataBoundItem;
                IdiomaManager.Instancia.CambiarIdioma(i);
            }
        }

        private void btnGuardarTraducciones_Click(object sender, EventArgs e)
        {
            if (dgvIdiomas.SelectedRows.Count > 0)
            {
                Idioma i = (Idioma)dgvIdiomas.SelectedRows[0].DataBoundItem;
                var bindingList = (BindingList<Traduccion>)dgvTraducciones.DataSource;
                List<Traduccion> list = new List<Traduccion>(bindingList);
                
                if (IdiomaManager.Instancia.GuardarTraducciones(i.Id, list))
                {
                    MessageBox.Show("Traducciones guardadas exitosamente.");
                }
                else
                {
                    MessageBox.Show("Error al guardar traducciones.");
                }
            }
        }
    }
}
