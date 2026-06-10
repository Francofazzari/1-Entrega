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
            this.txtNombreEmpleado = new System.Windows.Forms.TextBox();
            this.btnAgregarEmpleado = new System.Windows.Forms.Button();
            this.lstFunciones = new System.Windows.Forms.CheckedListBox();
            this.btnAsignar = new System.Windows.Forms.Button();
            this.treeViewPermisos = new System.Windows.Forms.TreeView();
            this.lblNombre = new System.Windows.Forms.Label();
            this.lblFunciones = new System.Windows.Forms.Label();
            this.lblArbol = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();

            // panelTop
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 50;
            this.panelTop.Controls.Add(this.lblNombre);
            this.panelTop.Controls.Add(this.txtNombreEmpleado);
            this.panelTop.Controls.Add(this.btnAgregarEmpleado);

            // lblNombre
            this.lblNombre.Text = "Nombre del empleado:";
            this.lblNombre.ForeColor = System.Drawing.Color.White;
            this.lblNombre.Font = new System.Drawing.Font("Arial", 9F);
            this.lblNombre.Location = new System.Drawing.Point(10, 16);
            this.lblNombre.Size = new System.Drawing.Size(150, 20);

            // txtNombreEmpleado
            this.txtNombreEmpleado.Location = new System.Drawing.Point(165, 13);
            this.txtNombreEmpleado.Size = new System.Drawing.Size(200, 26);
            this.txtNombreEmpleado.Font = new System.Drawing.Font("Arial", 10F);
            this.txtNombreEmpleado.Name = "txtNombreEmpleado";

            // btnAgregarEmpleado
            this.btnAgregarEmpleado.Text = "+ Agregar Empleado";
            this.btnAgregarEmpleado.Location = new System.Drawing.Point(375, 12);
            this.btnAgregarEmpleado.Size = new System.Drawing.Size(160, 28);
            this.btnAgregarEmpleado.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.btnAgregarEmpleado.BackColor = System.Drawing.Color.FromArgb(29, 158, 117);
            this.btnAgregarEmpleado.ForeColor = System.Drawing.Color.White;
            this.btnAgregarEmpleado.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarEmpleado.Name = "btnAgregarEmpleado";
            this.btnAgregarEmpleado.Click += new System.EventHandler(this.btnAgregarEmpleado_Click);

            // lblFunciones
            this.lblFunciones.Text = "Funciones disponibles:";
            this.lblFunciones.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblFunciones.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblFunciones.Location = new System.Drawing.Point(10, 60);
            this.lblFunciones.Size = new System.Drawing.Size(200, 20);

            // lstFunciones
            this.lstFunciones.Location = new System.Drawing.Point(10, 82);
            this.lstFunciones.Size = new System.Drawing.Size(280, 360);
            this.lstFunciones.Font = new System.Drawing.Font("Arial", 10F);
            this.lstFunciones.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstFunciones.Name = "lstFunciones";

            // btnAsignar
            this.btnAsignar.Text = "Asignar funciones al empleado seleccionado >>";
            this.btnAsignar.Location = new System.Drawing.Point(10, 455);
            this.btnAsignar.Size = new System.Drawing.Size(280, 34);
            this.btnAsignar.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.btnAsignar.BackColor = System.Drawing.Color.FromArgb(24, 95, 165);
            this.btnAsignar.ForeColor = System.Drawing.Color.White;
            this.btnAsignar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAsignar.Name = "btnAsignar";
            this.btnAsignar.Click += new System.EventHandler(this.btnAsignar_Click);

            // lblArbol
            this.lblArbol.Text = "Empleados y sus funciones:";
            this.lblArbol.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblArbol.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            this.lblArbol.Location = new System.Drawing.Point(310, 60);
            this.lblArbol.Size = new System.Drawing.Size(250, 20);

            // treeViewPermisos
            this.treeViewPermisos.Location = new System.Drawing.Point(310, 82);
            this.treeViewPermisos.Size = new System.Drawing.Size(540, 407);
            this.treeViewPermisos.Font = new System.Drawing.Font("Arial", 10F);
            this.treeViewPermisos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewPermisos.Name = "treeViewPermisos";

            // FormPerfiles
            this.ClientSize = new System.Drawing.Size(870, 510);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.lblFunciones);
            this.Controls.Add(this.lstFunciones);
            this.Controls.Add(this.btnAsignar);
            this.Controls.Add(this.lblArbol);
            this.Controls.Add(this.treeViewPermisos);
            this.Text = "Gestion de Perfiles y Funciones";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "FormPerfiles";
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TextBox txtNombreEmpleado;
        private System.Windows.Forms.Button btnAgregarEmpleado;
        private System.Windows.Forms.CheckedListBox lstFunciones;
        private System.Windows.Forms.Button btnAsignar;
        private System.Windows.Forms.TreeView treeViewPermisos;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblFunciones;
        private System.Windows.Forms.Label lblArbol;
        private System.Windows.Forms.Panel panelTop;

    }
}