using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestApplication.Helpers;

namespace TestApplication
{
    public partial class Form1 : Form
    {
        TestDB db;
        List<InventLocation> inventLocations = new List<InventLocation>();
        List<InventDim> inventDims = new List<InventDim>();
        bool firstLoad = false;
        InventLocation selectedItem = null;
        public Form1()
        {
            
            InitializeComponent();
            AddDataToDateBase();
            this.Load += Form1_Load;

        }
        private async Task LoadDataAsync()
        {
            using (db = new TestDB())
            {
                inventLocations = await db.InventLocation.ToListAsync();
                inventDims = await db.InventDim.ToListAsync();
                await Task.Run(()=> FillDataGridView<InventLocation>(InventLocationDataGrid, inventLocations));
                await Task.Run(() => FillDataGridView<InventDim>(InventDiDataGrid, inventDims));
            }

        }
        private void AddDataToDateBase()
        {

            InfoForm infoForm = new InfoForm();
            Task.Run(() =>
            {
                infoForm.ShowDialog();
            });
            
            //first start check records and add mock date
            
            using (db = new TestDB())
            {
                if (db.Database.Exists())
                {
                    if (!db.InventLocation.Any() && !db.InventDim.Any())
                    {
                        inventLocations = MockDataFill.GetInventLocations(10);
                        inventDims = MockDataFill.GetInventDims(inventLocations);
                        db.InventLocation.AddRange(inventLocations);
                        db.InventDim.AddRange(inventDims);
#if DEBUG
                        try
                        {

                            db.SaveChanges();

                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                        ve.PropertyName, ve.ErrorMessage);
                                }
                            }
                            throw;
                        }
#else
                        db.SaveChanges();
#endif

                    }

                    else
                    {
                        //for test mode - delete table

#if DEBUG
                        //var tableName1 = "InventLocation";
                        //var tableName2 = "InventDim";
                        //var sql1 = $"TRUNCATE TABLE {tableName1}";
                        //var sql2 = $"TRUNCATE TABLE {tableName2}";
                        //db.Database.ExecuteSqlCommand(sql1);
                        //db.Database.ExecuteSqlCommand(sql2);
                        //db.SaveChanges();
#endif


                    }
                }
            }
            infoForm.Invoke((MethodInvoker)delegate ()
            {
                infoForm.Close();
            });
        }


        private async void Form1_Load(object sender, EventArgs e)
        {
            UniqueWMS.Text = "Please select row from left table";
            await LoadDataAsync();

            this.InventLocationDataGrid.CellFormatting += InventDiDataGrid_CellFormatting;
#if DEBUG
            this.InventLocationDataGrid.DataError += InventLocationDataGrid_DataError;
#endif
           
            this.InventLocationDataGrid.SelectionChanged += InventLocationDataGrid_SelectionChanged;
            this.TopMost = true;
            
        }

        private async void InventLocationDataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (!firstLoad)
            {
                var row = InventLocationDataGrid.CurrentRow;
                selectedItem = (InventLocation)row.Tag;
                
                var selectedIdims = inventDims
                    .Where(item => item.InventLocationId == selectedItem.InventLocationId).ToList();
                
                var uniqWMS = await Task.Run(()=> FillDataGridView(InventDiDataGrid, selectedIdims));
                UniqueWMS.Text = $"UniqueWMS: {uniqWMS}";

            }
            
        }

        private int FillDataGridView<T>(DataGridView dataGridView, List<T> values)
        {
            if (InvokeRequired)
            {
                dataGridView.Invoke((MethodInvoker)delegate ()
                {
                    firstLoad = !firstLoad;
                    dataGridView.Columns.Clear();
                    var props = typeof(T).GetProperties();
                    foreach (var prop in props)
                    {
                        if (prop.PropertyType == typeof(byte[])) continue;
                        dataGridView.Columns.Add(prop.Name, prop.Name);
                    }
                    foreach (var invent in values)
                    {
                        var columns = dataGridView.Columns;
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(dataGridView);
                        foreach (DataGridViewColumn column in columns)
                        {
                            PropertyInfo propertyInfo = typeof(T).GetProperty(column.Name);
                            if (propertyInfo.PropertyType == typeof(byte[])) continue;
                            var t = propertyInfo.GetValue(invent);
                            row.Cells[column.Index].Value = propertyInfo.GetValue(invent);
                        }
                        row.Tag = invent;
                        row.ReadOnly = true;
                        dataGridView.Rows.Add(row);

                    }

                    if (typeof(T) == typeof(InventLocation)) selectedItem = (InventLocation)dataGridView.Rows[0].Tag;
                    dataGridView.CurrentCell = null;
                    firstLoad = !firstLoad;


                });
                
                
            }


            return values.Count();
        }

        private void InventLocationDataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
           
        }

        private void InventDiDataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
#if DEBUG           
            if (InventLocationDataGrid.Columns[e.ColumnIndex].ValueType == typeof(byte[]))
            {

                InventLocationDataGrid.Columns[e.ColumnIndex].DefaultCellStyle.Format = "MM/dd/yyyy HH:mm:ss";

                try
                {

                    byte[] data = (byte[])e.Value;
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(data);
                    long timestampValue = BitConverter.ToInt64(data, 0);
                    var timestamp = DateTime.FromBinary(timestampValue).ToString("MM/dd/yyyy HH:mm:ss");
                  

                    //InventLocationDataGrid.Columns[e.ColumnIndex].ValueType = typeof(string);
                    //e.Value = timestamp;
                    //e.FormattingApplied = true;
                    //e.CellStyle.NullValue = timestamp;
                    //e.CellStyle.Format = "MM/dd/yyyy HH:mm:ss";
                    //InventLocationDataGrid.Columns[e.ColumnIndex].ValueType = typeof(string);
                    InventLocationDataGrid.Columns[e.ColumnIndex].Visible = false;

                }
                catch (DataException ex)
                {
                    var s = ex.Message;
                }
            }
#endif
        }

        private void InventLocationDataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (InventLocationDataGrid.Columns[e.ColumnIndex].ValueType == typeof(byte[]))
            {
                try
                {
                   
                    byte[] data = (byte[])e.Value;
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(data);
                    long timestampValue = BitConverter.ToInt64(data, 0);
                    var timestamp = DateTime.FromBinary(timestampValue);
                    InventLocationDataGrid.Columns[e.ColumnIndex].Visible = false;
                    
                 }
                catch (DataException ex)
                {
                    var s = ex.Message;
                }
            }
            InventLocationDataGrid.Columns[e.ColumnIndex].ReadOnly = true;

        }

        private  void button1_Click(object sender, EventArgs e)
        {
            if (selectedItem == null) return;
            AddNewElement addNewElement = new AddNewElement(selectedItem);
            addNewElement.TopMost = true;
            addNewElement.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = false;
            DialogResult dialogResult = addNewElement.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                this.TopMost = true;
                Task.Run(() =>
                {
                    using (db = new TestDB())
                    {
                        inventDims = db.InventDim.ToList();
                        inventLocations = db.InventLocation.ToList();
                        FillDataGridView<InventDim>(InventDiDataGrid, inventDims);
                        FillDataGridView<InventLocation>(InventLocationDataGrid, inventLocations);
                    }
                });
               
            }

        }
    }
}
