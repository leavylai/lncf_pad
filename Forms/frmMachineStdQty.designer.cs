namespace cf_pad.Forms
{
    partial class frmMachineStdQty
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMachineStdQty));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.BTNEXIT = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.BTNSAVE = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtMachine = new System.Windows.Forms.TextBox();
            this.lblMachine = new System.Windows.Forms.Label();
            this.txtStdQty = new System.Windows.Forms.TextBox();
            this.txtRunNo = new System.Windows.Forms.TextBox();
            this.lblStdQty = new System.Windows.Forms.Label();
            this.lblRunNo = new System.Windows.Forms.Label();
            this.txtLineNo = new System.Windows.Forms.TextBox();
            this.lblLineNo = new System.Windows.Forms.Label();
            this.txtDep = new System.Windows.Forms.TextBox();
            this.lblDep = new System.Windows.Forms.Label();
            this.dgvDetails = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.BTNFIND = new System.Windows.Forms.Button();
            this.txtFindMachine = new System.Windows.Forms.TextBox();
            this.lblFindDep = new System.Windows.Forms.Label();
            this.lblFindMachine = new System.Windows.Forms.Label();
            this.txtFindDep = new System.Windows.Forms.TextBox();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetails)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BTNEXIT,
            this.toolStripSeparator1,
            this.BTNSAVE,
            this.toolStripSeparator2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(944, 82);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // BTNEXIT
            // 
            this.BTNEXIT.AutoSize = false;
            this.BTNEXIT.Font = new System.Drawing.Font("新細明體", 16F);
            this.BTNEXIT.Image = ((System.Drawing.Image)(resources.GetObject("BTNEXIT.Image")));
            this.BTNEXIT.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BTNEXIT.Name = "BTNEXIT";
            this.BTNEXIT.Size = new System.Drawing.Size(120, 80);
            this.BTNEXIT.Text = "退出";
            this.BTNEXIT.Click += new System.EventHandler(this.BTNEXIT_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 82);
            // 
            // BTNSAVE
            // 
            this.BTNSAVE.AutoSize = false;
            this.BTNSAVE.Font = new System.Drawing.Font("新細明體", 16F);
            this.BTNSAVE.Image = ((System.Drawing.Image)(resources.GetObject("BTNSAVE.Image")));
            this.BTNSAVE.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BTNSAVE.Name = "BTNSAVE";
            this.BTNSAVE.Size = new System.Drawing.Size(120, 80);
            this.BTNSAVE.Text = "儲存";
            this.BTNSAVE.Click += new System.EventHandler(this.BTNSAVE_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 82);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 82);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtMachine);
            this.splitContainer1.Panel1.Controls.Add(this.lblMachine);
            this.splitContainer1.Panel1.Controls.Add(this.txtStdQty);
            this.splitContainer1.Panel1.Controls.Add(this.txtRunNo);
            this.splitContainer1.Panel1.Controls.Add(this.lblStdQty);
            this.splitContainer1.Panel1.Controls.Add(this.lblRunNo);
            this.splitContainer1.Panel1.Controls.Add(this.txtLineNo);
            this.splitContainer1.Panel1.Controls.Add(this.lblLineNo);
            this.splitContainer1.Panel1.Controls.Add(this.txtDep);
            this.splitContainer1.Panel1.Controls.Add(this.lblDep);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgvDetails);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(944, 665);
            this.splitContainer1.SplitterDistance = 157;
            this.splitContainer1.TabIndex = 1;
            // 
            // txtMachine
            // 
            this.txtMachine.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMachine.Location = new System.Drawing.Point(331, 13);
            this.txtMachine.Name = "txtMachine";
            this.txtMachine.ReadOnly = true;
            this.txtMachine.Size = new System.Drawing.Size(154, 49);
            this.txtMachine.TabIndex = 1;
            // 
            // lblMachine
            // 
            this.lblMachine.AutoSize = true;
            this.lblMachine.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblMachine.Location = new System.Drawing.Point(243, 24);
            this.lblMachine.Name = "lblMachine";
            this.lblMachine.Size = new System.Drawing.Size(94, 35);
            this.lblMachine.TabIndex = 0;
            this.lblMachine.Text = "機器:";
            // 
            // txtStdQty
            // 
            this.txtStdQty.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtStdQty.Location = new System.Drawing.Point(643, 82);
            this.txtStdQty.Name = "txtStdQty";
            this.txtStdQty.Size = new System.Drawing.Size(136, 49);
            this.txtStdQty.TabIndex = 4;
            // 
            // txtRunNo
            // 
            this.txtRunNo.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtRunNo.Location = new System.Drawing.Point(331, 82);
            this.txtRunNo.Name = "txtRunNo";
            this.txtRunNo.Size = new System.Drawing.Size(136, 49);
            this.txtRunNo.TabIndex = 3;
            this.txtRunNo.Leave += new System.EventHandler(this.txtRunNo_Leave);
            // 
            // lblStdQty
            // 
            this.lblStdQty.AutoSize = true;
            this.lblStdQty.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblStdQty.Location = new System.Drawing.Point(473, 89);
            this.lblStdQty.Name = "lblStdQty";
            this.lblStdQty.Size = new System.Drawing.Size(164, 35);
            this.lblStdQty.TabIndex = 0;
            this.lblStdQty.Text = "標準數量:";
            // 
            // lblRunNo
            // 
            this.lblRunNo.AutoSize = true;
            this.lblRunNo.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRunNo.Location = new System.Drawing.Point(243, 89);
            this.lblRunNo.Name = "lblRunNo";
            this.lblRunNo.Size = new System.Drawing.Size(94, 35);
            this.lblRunNo.TabIndex = 0;
            this.lblRunNo.Text = "轉數:";
            // 
            // txtLineNo
            // 
            this.txtLineNo.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtLineNo.Location = new System.Drawing.Point(132, 82);
            this.txtLineNo.Name = "txtLineNo";
            this.txtLineNo.Size = new System.Drawing.Size(100, 49);
            this.txtLineNo.TabIndex = 2;
            this.txtLineNo.Leave += new System.EventHandler(this.txtLineNo_Leave);
            // 
            // lblLineNo
            // 
            this.lblLineNo.AutoSize = true;
            this.lblLineNo.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblLineNo.Location = new System.Drawing.Point(30, 89);
            this.lblLineNo.Name = "lblLineNo";
            this.lblLineNo.Size = new System.Drawing.Size(94, 35);
            this.lblLineNo.TabIndex = 0;
            this.lblLineNo.Text = "行數:";
            // 
            // txtDep
            // 
            this.txtDep.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtDep.Location = new System.Drawing.Point(132, 13);
            this.txtDep.Name = "txtDep";
            this.txtDep.ReadOnly = true;
            this.txtDep.Size = new System.Drawing.Size(100, 49);
            this.txtDep.TabIndex = 0;
            // 
            // lblDep
            // 
            this.lblDep.AutoSize = true;
            this.lblDep.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDep.Location = new System.Drawing.Point(30, 24);
            this.lblDep.Name = "lblDep";
            this.lblDep.Size = new System.Drawing.Size(94, 35);
            this.lblDep.TabIndex = 0;
            this.lblDep.Text = "部門:";
            // 
            // dgvDetails
            // 
            this.dgvDetails.AllowUserToAddRows = false;
            this.dgvDetails.AllowUserToDeleteRows = false;
            this.dgvDetails.AllowUserToResizeColumns = false;
            this.dgvDetails.AllowUserToResizeRows = false;
            this.dgvDetails.ColumnHeadersHeight = 40;
            this.dgvDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvDetails.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5});
            this.dgvDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetails.Location = new System.Drawing.Point(0, 92);
            this.dgvDetails.Name = "dgvDetails";
            this.dgvDetails.RowHeadersWidth = 20;
            this.dgvDetails.RowTemplate.Height = 80;
            this.dgvDetails.Size = new System.Drawing.Size(942, 410);
            this.dgvDetails.TabIndex = 1;
            this.dgvDetails.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDetails_CellClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BTNFIND);
            this.panel1.Controls.Add(this.txtFindMachine);
            this.panel1.Controls.Add(this.lblFindDep);
            this.panel1.Controls.Add(this.lblFindMachine);
            this.panel1.Controls.Add(this.txtFindDep);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(942, 92);
            this.panel1.TabIndex = 0;
            // 
            // BTNFIND
            // 
            this.BTNFIND.Font = new System.Drawing.Font("新細明體", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.BTNFIND.Location = new System.Drawing.Point(557, 6);
            this.BTNFIND.Name = "BTNFIND";
            this.BTNFIND.Size = new System.Drawing.Size(120, 80);
            this.BTNFIND.TabIndex = 2;
            this.BTNFIND.Text = "查詢";
            this.BTNFIND.UseVisualStyleBackColor = true;
            this.BTNFIND.Click += new System.EventHandler(this.BTNFIND_Click);
            // 
            // txtFindMachine
            // 
            this.txtFindMachine.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtFindMachine.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtFindMachine.Location = new System.Drawing.Point(331, 18);
            this.txtFindMachine.Name = "txtFindMachine";
            this.txtFindMachine.Size = new System.Drawing.Size(189, 49);
            this.txtFindMachine.TabIndex = 1;
            // 
            // lblFindDep
            // 
            this.lblFindDep.AutoSize = true;
            this.lblFindDep.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblFindDep.Location = new System.Drawing.Point(30, 25);
            this.lblFindDep.Name = "lblFindDep";
            this.lblFindDep.Size = new System.Drawing.Size(94, 35);
            this.lblFindDep.TabIndex = 0;
            this.lblFindDep.Text = "部門:";
            // 
            // lblFindMachine
            // 
            this.lblFindMachine.AutoSize = true;
            this.lblFindMachine.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblFindMachine.Location = new System.Drawing.Point(243, 25);
            this.lblFindMachine.Name = "lblFindMachine";
            this.lblFindMachine.Size = new System.Drawing.Size(94, 35);
            this.lblFindMachine.TabIndex = 0;
            this.lblFindMachine.Text = "機器:";
            // 
            // txtFindDep
            // 
            this.txtFindDep.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtFindDep.Font = new System.Drawing.Font("新細明體", 26F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtFindDep.Location = new System.Drawing.Point(132, 18);
            this.txtFindDep.Name = "txtFindDep";
            this.txtFindDep.Size = new System.Drawing.Size(100, 49);
            this.txtFindDep.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "dep";
            this.dataGridViewTextBoxColumn1.HeaderText = "部門";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "machine_id";
            this.dataGridViewTextBoxColumn2.HeaderText = "機器代號";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "machine_mul";
            this.dataGridViewTextBoxColumn3.HeaderText = "行(碑)數";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "machine_rate";
            this.dataGridViewTextBoxColumn4.HeaderText = "轉數";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "machine_std_qty";
            this.dataGridViewTextBoxColumn5.HeaderText = "小時標準數量";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.Width = 120;
            // 
            // Column1
            // 
            this.Column1.DataPropertyName = "dep";
            this.Column1.HeaderText = "部門";
            this.Column1.Name = "Column1";
            // 
            // Column2
            // 
            this.Column2.DataPropertyName = "machine_id";
            this.Column2.HeaderText = "機器代號";
            this.Column2.Name = "Column2";
            this.Column2.Width = 140;
            // 
            // Column3
            // 
            this.Column3.DataPropertyName = "machine_mul";
            this.Column3.HeaderText = "行(碑)數";
            this.Column3.Name = "Column3";
            // 
            // Column4
            // 
            this.Column4.DataPropertyName = "machine_rate";
            this.Column4.HeaderText = "轉數";
            this.Column4.Name = "Column4";
            // 
            // Column5
            // 
            this.Column5.DataPropertyName = "machine_std_qty";
            this.Column5.HeaderText = "小時標準數量";
            this.Column5.Name = "Column5";
            this.Column5.Width = 160;
            // 
            // frmMachineStdQty
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 747);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "frmMachineStdQty";
            this.Text = "frmMachineStdQty";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmMachineStdQty_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetails)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton BTNEXIT;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton BTNSAVE;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtMachine;
        private System.Windows.Forms.Label lblMachine;
        private System.Windows.Forms.TextBox txtDep;
        private System.Windows.Forms.Label lblDep;
        private System.Windows.Forms.TextBox txtStdQty;
        private System.Windows.Forms.TextBox txtRunNo;
        private System.Windows.Forms.Label lblStdQty;
        private System.Windows.Forms.Label lblRunNo;
        private System.Windows.Forms.TextBox txtLineNo;
        private System.Windows.Forms.Label lblLineNo;
        private System.Windows.Forms.TextBox txtFindMachine;
        private System.Windows.Forms.Label lblFindDep;
        private System.Windows.Forms.Label lblFindMachine;
        private System.Windows.Forms.TextBox txtFindDep;
        private System.Windows.Forms.Button BTNFIND;
        private System.Windows.Forms.DataGridView dgvDetails;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
    }
}