namespace TestApplication
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.InventLocationDataGrid = new System.Windows.Forms.DataGridView();
            this.InventDiDataGrid = new System.Windows.Forms.DataGridView();
            this.UniqueWMS = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.InventLocationDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InventDiDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // InventLocationDataGrid
            // 
            this.InventLocationDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.InventLocationDataGrid.Location = new System.Drawing.Point(26, 75);
            this.InventLocationDataGrid.Name = "InventLocationDataGrid";
            this.InventLocationDataGrid.Size = new System.Drawing.Size(550, 354);
            this.InventLocationDataGrid.TabIndex = 0;
            // 
            // InventDiDataGrid
            // 
            this.InventDiDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.InventDiDataGrid.Location = new System.Drawing.Point(582, 75);
            this.InventDiDataGrid.Name = "InventDiDataGrid";
            this.InventDiDataGrid.Size = new System.Drawing.Size(971, 354);
            this.InventDiDataGrid.TabIndex = 1;
            // 
            // UniqueWMS
            // 
            this.UniqueWMS.AutoSize = true;
            this.UniqueWMS.Location = new System.Drawing.Point(582, 457);
            this.UniqueWMS.Name = "UniqueWMS";
            this.UniqueWMS.Size = new System.Drawing.Size(13, 13);
            this.UniqueWMS.TabIndex = 2;
            this.UniqueWMS.Text = "0";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(26, 447);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 33);
            this.button1.TabIndex = 3;
            this.button1.Text = "Edit";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1565, 513);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.UniqueWMS);
            this.Controls.Add(this.InventDiDataGrid);
            this.Controls.Add(this.InventLocationDataGrid);
            this.Name = "Form1";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.InventLocationDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InventDiDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView InventLocationDataGrid;
        private System.Windows.Forms.DataGridView InventDiDataGrid;
        private System.Windows.Forms.Label UniqueWMS;
        private System.Windows.Forms.Button button1;
    }
}

