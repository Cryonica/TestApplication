using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        InfoForm infoForm = null;
        InventLocation selectedItem = null;
        internal Loggeer appLogger = null;
        const string errorFileName = "errors.log";
        public Form1()
        {
            
            InitializeComponent();

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string directoryPath = Path.GetDirectoryName(assemblyPath);
            appLogger = new Loggeer(Path.Combine(directoryPath, errorFileName));
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
                CloseInfoForm(infoForm);

            }

        }
        private void AddDataToDateBase()
        {

            //first start check records and add mock data
            
            using (db = new TestDB())
            {
                if (db.Database.Exists())
                {
                    if (!db.InventLocation.Any() && !db.InventDim.Any())
                    {
                        infoForm = new InfoForm("Creating mock records ");
                        Task.Run(() =>
                        {
                            infoForm.ShowDialog();
                        });


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
                            StringBuilder stringBuilder = new StringBuilder();
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                stringBuilder.AppendFormat("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:\n",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    stringBuilder.AppendFormat("- Property: \"{0}\", Error: \"{1}\"\n", ve.PropertyName, ve.ErrorMessage);
                                }
                            }
                            appLogger.Log(stringBuilder.ToString());
                            throw;
                        }
#else
                        db.SaveChanges();
#endif

                    }

                    else
                    {

#if DEBUG
                        //for test mode - delete table

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
        }


        private async void Form1_Load(object sender, EventArgs e)
        {
            
            UniqueWMS.Text = "Please select row from left table";

            //loading records from dataBase
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

                var wmsCount =  await FillDataGridView(InventDiDataGrid, selectedIdims);
                UniqueWMS.Text = $"UniqueWMS: {wmsCount}";
            }
            
        }

        private Task<int> FillDataGridView<T>(DataGridView dataGridView, List<T> values)
        {

            var result = Task.Run(() =>
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
                return values.Count;
            });
            return result;
        }

        private void InventLocationDataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            appLogger.Log(e.ToString());
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
                    appLogger.Log(ex.Message);
                }
            }
#endif
        }
#if DEBUG

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
                    appLogger.Log(ex.Message);
                }
            }
            InventLocationDataGrid.Columns[e.ColumnIndex].ReadOnly = true;

        }
#endif

        private  void button1_Click(object sender, EventArgs e)
        {
            if (selectedItem == null) return;
            AddNewElement addNewElement = new AddNewElement(selectedItem)
            {
                TopMost = true,
                StartPosition = FormStartPosition.CenterScreen
            };
            this.TopMost = false;
            DialogResult dialogResult = addNewElement.ShowDialog();
            infoForm = addNewElement.infoForm;
            if (dialogResult == DialogResult.OK)
            {
                //update DataGridViews with new records

                Task.Run(async() =>
                {
                    using (db = new TestDB())
                    {
                        inventDims = db.InventDim.ToList();
                        inventLocations = db.InventLocation.ToList();
                        await FillDataGridView<InventDim>(InventDiDataGrid, inventDims);
                        await FillDataGridView<InventLocation>(InventLocationDataGrid, inventLocations);
                        if (InvokeRequired)
                        {
                            UniqueWMS.Invoke((MethodInvoker)delegate ()
                            {
                                UniqueWMS.Text = "Please select row from left table";
                            });
                        }
                        CloseInfoForm(infoForm);
                    }
                });
            }
            else
            {
                CloseInfoForm(infoForm);
            }

        }
        private void CloseInfoForm(InfoForm form)
        {
            if (form != null && InvokeRequired)
            {
                if (InvokeRequired)
                {
                    form.Invoke((MethodInvoker)delegate ()
                    {
                        form.Close();
                    });
                }
                else
                {
                    form.Close();
                }
                
                
            }
        }
    }
}
