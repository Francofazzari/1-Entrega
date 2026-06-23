namespace Entrega1software
{
    partial class FormPerfiles
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.treeViewPermisos = new System.Windows.Forms.TreeView();
            this.lblNuevaFamilia = new System.Windows.Forms.Label();
            this.txtNombreFamilia = new System.Windows.Forms.TextBox();
            this.btnAgregarFamiliaLista = new System.Windows.Forms.Button();
            this.lstFamilias = new System.Windows.Forms.ListBox();
            this.lstPatentes = new System.Windows.Forms.ListBox();
            this.btnAgregarFamiliaPreview = new System.Windows.Forms.Button();
            this.btnAgregarPatentePreview = new System.Windows.Forms.Button();
            this.treeViewPreview = new System.Windows.Forms.TreeView();
            this.btnEliminarSeleccionado = new System.Windows.Forms.Button();
            this.btnGuardarFamilia = new System.Windows.Forms.Button();
            this.panelCentral = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            
            // treeViewPermisos
            this.treeViewPermisos.Location = new System.Drawing.Point(10, 10);
            this.treeViewPermisos.Size = new System.Drawing.Size(280, 480);
            this.treeViewPermisos.Name = "treeViewPermisos";
            
            // panelCentral
            this.panelCentral.Location = new System.Drawing.Point(300, 10);
            this.panelCentral.Size = new System.Drawing.Size(420, 480);
            this.panelCentral.Name = "panelCentral";
            
            // lblNuevaFamilia
            this.lblNuevaFamilia.Text = "Nueva familia";
            this.lblNuevaFamilia.Location = new System.Drawing.Point(300, 10);
            this.lblNuevaFamilia.Size = new System.Drawing.Size(100, 20);
            
            // txtNombreFamilia
            this.txtNombreFamilia.Location = new System.Drawing.Point(300, 30);
            this.txtNombreFamilia.Size = new System.Drawing.Size(410, 20);
            this.txtNombreFamilia.Name = "txtNombreFamilia";
            
            // btnAgregarFamiliaLista
            this.btnAgregarFamiliaLista.Text = "Agregar";
            this.btnAgregarFamiliaLista.Location = new System.Drawing.Point(300, 60);
            this.btnAgregarFamiliaLista.Size = new System.Drawing.Size(410, 30);
            this.btnAgregarFamiliaLista.Name = "btnAgregarFamiliaLista";
            this.btnAgregarFamiliaLista.Click += new System.EventHandler(this.btnAgregarFamiliaLista_Click);
            
            // lstFamilias
            this.lstFamilias.Location = new System.Drawing.Point(300, 100);
            this.lstFamilias.Size = new System.Drawing.Size(200, 340);
            this.lstFamilias.Name = "lstFamilias";
            
            // lstPatentes
            this.lstPatentes.Location = new System.Drawing.Point(510, 100);
            this.lstPatentes.Size = new System.Drawing.Size(200, 340);
            this.lstPatentes.Name = "lstPatentes";
            
            // btnAgregarFamiliaPreview
            this.btnAgregarFamiliaPreview.Text = "Agregar Familia";
            this.btnAgregarFamiliaPreview.Location = new System.Drawing.Point(300, 450);
            this.btnAgregarFamiliaPreview.Size = new System.Drawing.Size(200, 30);
            this.btnAgregarFamiliaPreview.Name = "btnAgregarFamiliaPreview";
            this.btnAgregarFamiliaPreview.Click += new System.EventHandler(this.btnAgregarFamiliaPreview_Click);
            
            // btnAgregarPatentePreview
            this.btnAgregarPatentePreview.Text = "Agregar Patente";
            this.btnAgregarPatentePreview.Location = new System.Drawing.Point(510, 450);
            this.btnAgregarPatentePreview.Size = new System.Drawing.Size(200, 30);
            this.btnAgregarPatentePreview.Name = "btnAgregarPatentePreview";
            this.btnAgregarPatentePreview.Click += new System.EventHandler(this.btnAgregarPatentePreview_Click);
            
            // treeViewPreview
            this.treeViewPreview.Location = new System.Drawing.Point(730, 10);
            this.treeViewPreview.Size = new System.Drawing.Size(240, 410);
            this.treeViewPreview.Name = "treeViewPreview";
            
            // btnEliminarSeleccionado
            this.btnEliminarSeleccionado.Text = "Eliminar seleccionado";
            this.btnEliminarSeleccionado.Location = new System.Drawing.Point(730, 430);
            this.btnEliminarSeleccionado.Size = new System.Drawing.Size(240, 30);
            this.btnEliminarSeleccionado.Name = "btnEliminarSeleccionado";
            this.btnEliminarSeleccionado.Click += new System.EventHandler(this.btnEliminarSeleccionado_Click);
            
            // btnGuardarFamilia
            this.btnGuardarFamilia.Text = "Guardar familia";
            this.btnGuardarFamilia.Location = new System.Drawing.Point(730, 470);
            this.btnGuardarFamilia.Size = new System.Drawing.Size(240, 30);
            this.btnGuardarFamilia.Name = "btnGuardarFamilia";
            this.btnGuardarFamilia.Click += new System.EventHandler(this.btnGuardarFamilia_Click);
            
            // FormPerfiles
            this.ClientSize = new System.Drawing.Size(990, 520);
            this.Controls.Add(this.treeViewPermisos);
            this.Controls.Add(this.lblNuevaFamilia);
            this.Controls.Add(this.txtNombreFamilia);
            this.Controls.Add(this.btnAgregarFamiliaLista);
            this.Controls.Add(this.lstFamilias);
            this.Controls.Add(this.lstPatentes);
            this.Controls.Add(this.btnAgregarFamiliaPreview);
            this.Controls.Add(this.btnAgregarPatentePreview);
            this.Controls.Add(this.treeViewPreview);
            this.Controls.Add(this.btnEliminarSeleccionado);
            this.Controls.Add(this.btnGuardarFamilia);
            this.Name = "FormPerfiles";
            this.Text = "Gestion de Familias y Patentes";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TreeView treeViewPermisos;
        private System.Windows.Forms.Label lblNuevaFamilia;
        private System.Windows.Forms.TextBox txtNombreFamilia;
        private System.Windows.Forms.Button btnAgregarFamiliaLista;
        private System.Windows.Forms.ListBox lstFamilias;
        private System.Windows.Forms.ListBox lstPatentes;
        private System.Windows.Forms.Button btnAgregarFamiliaPreview;
        private System.Windows.Forms.Button btnAgregarPatentePreview;
        private System.Windows.Forms.TreeView treeViewPreview;
        private System.Windows.Forms.Button btnEliminarSeleccionado;
        private System.Windows.Forms.Button btnGuardarFamilia;
        private System.Windows.Forms.Panel panelCentral;
    }
}