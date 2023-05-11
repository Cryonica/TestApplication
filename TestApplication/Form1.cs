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
    /// <summary>
    /// Global Variables
    /// <param name="db">Database Context</param>
    /// <param name="inventLocations">List of InventLocation. The list is updated only first launch</param>
    /// <param name="inventDims">List of InventDim.The list is updated on every InventLocation selection event in the table</param>
    /// <param name="firstLoad">Needed to disable string formatting on first load</param>
    /// <param name="infoForm">Its splash window for notify the user about manipulations in the database</param>
    /// <param name="selectedItem">Current InventLocation</param>
    /// <param name="appLogger">Its Logger for write message in the errors file</param>
    /// <param name="errorFileName">file name for save errors</param>
    /// </summary>
    public partial class Form1 : Form
    {
        
        TestDB db;
        List<InventLocation> inventLocations = new List<InventLocation>();
        List<InventDim> inventDims = new List<InventDim>();
        bool firstLoad = false;
        InfoForm infoForm = null;
        InventLocation selectedItem = null;
        
        public Form1()
        {
            Task.Run(() =>
            {
                infoForm = new InfoForm("Loading");
                infoForm.ShowDialog();
            });
            
            InitializeComponent();
            
            //read data from database
            AddDataToDateBase();

            //event after loading
            this.Load += Form1_Load;
            
        }

        #region UI events
        /// <summary>
        /// UI events
        /// <param name="Form1_Load">Event after mainForm load</param>
        /// <param name="InventDiDataGrid_CellFormatting">Event for DiDataGrid cell formatting</param>
        /// <param name="InventLocationDataGrid_DataError">Not used. This is an error handler during row creating</param>
        /// <paramref name="InventLocationDataGrid_SelectionChanged">Event for hooking selection changes in InventLocationDataGrid</paramref>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void Form1_Load(object sender, EventArgs e)
        {
           
            UniqueWMS.Text = "Please select row from left table";

            //loading records from dataBase
            bool loadResult = await LoadDataAsync();

            this.InventLocationDataGrid.CellFormatting += InventDiDataGrid_CellFormatting;
#if DEBUG
            this.InventLocationDataGrid.DataError += InventLocationDataGrid_DataError;
#endif
            this.InventLocationDataGrid.SelectionChanged += InventLocationDataGrid_SelectionChanged;
            if (loadResult)
            {
                CloseInfoForm(infoForm);
                this.Opacity = 100;
                this.TopLevel = true;
            }
            else
            {
                CloseInfoForm(infoForm);
                MessageBox.Show("Error during loading data");
                this.Close();
            }
        }

        private void InventDiDataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
#if DEBUG

            if (InventLocationDataGrid.Columns[e.ColumnIndex].ValueType == typeof(byte[]))
            {

                InventLocationDataGrid.Columns[e.ColumnIndex].DefaultCellStyle.Format = "MM/dd/yyyy HH:mm:ss";

                try
                {
                    #region Use this code if you want to convert timestamp to time

                    //byte[] data = (byte[])e.Value;
                    //if (BitConverter.IsLittleEndian)
                    //    Array.Reverse(data);
                    //long timestampValue = BitConverter.ToInt64(data, 0);
                    //var timestamp = DateTime.FromBinary(timestampValue).ToString("MM/dd/yyyy HH:mm:ss");
                    //InventLocationDataGrid.Columns[e.ColumnIndex].ValueType = typeof(string);
                    //e.Value = timestamp;
                    //e.FormattingApplied = true;
                    //e.CellStyle.NullValue = timestamp;
                    //e.CellStyle.Format = "MM/dd/yyyy HH:mm:ss";
                    //InventLocationDataGrid.Columns[e.ColumnIndex].ValueType = typeof(string);
                    #endregion
                    InventLocationDataGrid.Columns[e.ColumnIndex].Visible = false;

                }
                catch (DataException ex)
                {
                    //wrong value type handler
                    Loggeer.Instance.Log(ex.Message);
                }
            }
#endif
        }

        private void InventLocationDataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            Loggeer.Instance.Log(e.ToString());
        }

        private async void InventLocationDataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (!firstLoad)
            {
                var row = InventLocationDataGrid.CurrentRow;
                selectedItem = (InventLocation)row.Tag;

                var selectedIdims = inventDims
                    .Where(item => item.InventLocationId == selectedItem.InventLocationId).ToList();

                int wmsCount = selectedIdims.Select(el => el.WMSLocationId).Distinct().Count();

                await FillDataGridView(InventDiDataGrid, selectedIdims);
                UniqueWMS.Text = $"UniqueWMS: {wmsCount}";
            }

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
                    Loggeer.Instance.Log(ex.Message);
                }
            }
            InventLocationDataGrid.Columns[e.ColumnIndex].ReadOnly = true;

        }
#endif

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedItem == null) return;
            AddNewElement addNewElement = new AddNewElement(selectedItem)
            {
                TopMost = true,
                StartPosition = FormStartPosition.CenterScreen
            };
            //this.TopMost = false;
            DialogResult dialogResult = addNewElement.ShowDialog();
            infoForm = addNewElement.infoForm;
            if (dialogResult == DialogResult.OK)
            {
                //update DataGridViews with new records

                Task.Run(async () =>
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

        #endregion

        #region Functions

        //Reading data from dataBase on first launch
        private async Task<bool> LoadDataAsync()
        {
            bool result = false;
            
            try
            {
                using (db = new TestDB())
                {
                    //get data from dataBase
                    inventLocations = await db.InventLocation.ToListAsync();
                    inventDims = await db.InventDim.ToListAsync();

                    //create rows and columns in the DataGridViews
                    await Task.Run(() => FillDataGridView<InventLocation>(InventLocationDataGrid, inventLocations));
                    await Task.Run(() => FillDataGridView<InventDim>(InventDiDataGrid, inventDims));
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Loggeer.Instance.Log(ex.Message);
            }
            return result;
        }
        
        //create mock records in dataBase
        private void AddDataToDateBase()
        {
            //first start check records and adding mock data
            const string mockInfo = "Creating mock records ";
            using (db = new TestDB())
            {
                if (db.Database.Exists())
                {
                    if (!db.InventLocation.Any() && !db.InventDim.Any())
                    {
                        //create splash window
                        Task.Run(() =>
                        {
                            if (infoForm != null && infoForm.InvokeRequired)
                            {
                                infoForm.Invoke(new MethodInvoker(() =>
                                {
                                    infoForm.SetLabelText(mockInfo);
                                }));
                            }
                            else
                            {
                                infoForm = new InfoForm(mockInfo)
                                {
                                    TopMost = true
                                };
                                infoForm.ShowDialog();
                            }
                        });

                        //set data into global variables
                        inventLocations = MockDataFill.GetInventLocations(10);
                        inventDims = MockDataFill.GetInventDims(inventLocations);

                        //create mock records in dataBase
                        db.InventLocation.AddRange(inventLocations);
                        db.InventDim.AddRange(inventDims);
                        using (var transaction = db.Database.BeginTransaction())
                        {
#if DEBUG
                            Loggeer.Instance.Log("creating mock data");
                            try
                            {
                                db.SaveChanges();
                                transaction.Commit();
                            }
                            catch (DbEntityValidationException e)
                            {
                                //exception handler (if datafields in mock lists with wrond type)

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
                                Loggeer.Instance.Log(stringBuilder.ToString());
                                transaction.Rollback();
                            }
                            catch (System.Data.SqlClient.SqlException e)
                            {
                                Loggeer.Instance.Log(e.Message);
                            }
#else
                            try
                            {
                                db.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                appLogger.Log(ex.Message);
                                transaction.Rollback();
                            }
                            
#endif
                        }
                    }

                    else
                    {

#if DEBUG

                        #region for test mode - Deleting Existing Tables

                        //Loggeer.Instance.Log("delete data");
                        //var tableName1 = "InventLocation";
                        //var tableName2 = "InventDim";
                        //var sql1 = $"TRUNCATE TABLE {tableName1}";
                        //var sql2 = $"TRUNCATE TABLE {tableName2}";
                        //db.Database.ExecuteSqlCommand(sql1);
                        //db.Database.ExecuteSqlCommand(sql2);
                        //db.SaveChanges();
                        #endregion
#endif


                    }
                }
            }
        }

        //main application function
        /// <summary>
        /// <param name="dataGridView">Instance of DataGridView</param>
        /// <param name="values">Values list for creating rows</param>
        /// </summary>
        private Task<int> FillDataGridView<T>(DataGridView dataGridView, List<T> values)
        {
            
            var result = Task.Run(() =>
            {
                //manipulation with dataGridView in another thread
                if (dataGridView.InvokeRequired)
                {
                    dataGridView.Invoke((MethodInvoker)delegate ()
                    {
                        firstLoad = !firstLoad;
                        dataGridView.Columns.Clear();
                        
                        //creating columns
                        var props = typeof(T).GetProperties();
                        foreach (var prop in props)
                        {
                            if (prop.PropertyType == typeof(byte[])) continue;
                            dataGridView.Columns.Add(prop.Name, prop.Name);
                        }

                        //creating rows
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

                        //add InventLocation instances to the row tag
                        if (typeof(T) == typeof(InventLocation)) selectedItem = (InventLocation)dataGridView.Rows[0].Tag;

                        //Reset selection
                        dataGridView.CurrentCell = null;
                        firstLoad = !firstLoad;


                    });
                }
                return values.Count;
            });
            return result;
        }

        //function for close splash window
        ///<summary>
        ///<param name="form">Splash window instance</param>
        /// </summary>
        private void CloseInfoForm(InfoForm form)
        {
           
            if (form != null)
            {
                if (form.InvokeRequired)
                {
                    form.Invoke(new MethodInvoker(() => {
                        form.Close();
                    }));
                }
                else
                {
                    form.Close();
                }
                
                
            }
        }
        #endregion

        #region Use this code if the number of rows in the InventDim table does not change, just for update cells
        private async Task UpdateChangedRows(DataGridView dgv, List<InventDim> selectedIdims)
        {
            // take last timestamp from InventDim

            var rowVersions = new Dictionary<string, byte[]>();
            foreach (var idim in selectedIdims)
            {
                var rowVersion = await GetRowVersionFromDatabase(idim.InventDimId);
                rowVersions.Add(idim.InventDimId, rowVersion);
            }

            // update rows with different timestamp in InventDim and database

            foreach (DataGridViewRow row in dgv.Rows)
            {
                var idim = row.Tag as InventDim;
                if (idim != null && rowVersions.ContainsKey(idim.InventDimId) &&
                    !rowVersions[idim.InventDimId].SequenceEqual(idim.RowVersion))
                {
                    //udate cells in DataGrid from db data
                    var updatedIdim = await GetInventDimFromDatabase(idim.InventDimId);
                    UpdateDataGridViewRow(row, updatedIdim);
                }
            }
        }
        private async Task<byte[]> GetRowVersionFromDatabase(string id)
        {
            using (db = new TestDB())
            {
                var idim = await db.InventDim.FindAsync(id);
                if (idim != null)
                {
                    return idim.RowVersion;
                }
                else
                {
                    return null;
                }
            }
        }
        private async Task<InventDim> GetInventDimFromDatabase(string id)
        {
            using (db = new TestDB())
            {
                return await db.InventDim.FindAsync(id);
            }
        }
        private void UpdateDataGridViewRow(DataGridViewRow row, InventDim idim)
        {
            row.Cells["WMSLocationId"].Value = idim.WMSLocationId;
            row.Cells["ModifiedBy"].Value = idim.ModifiedBy;

            //update tag in DataGrid row
            row.Tag = idim;
        }
        #endregion


    }
}
