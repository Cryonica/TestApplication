using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestApplication.Helpers;

namespace TestApplication
{
    public partial class AddNewElement : Form
    {
        TestDB db;
        InventLocation inventLocation;
        internal InfoForm infoForm;
        private readonly int inventLocationIdInt;
        public AddNewElement(InventLocation inventLocation)
        {

            int.TryParse(inventLocation.InventLocationId, out inventLocationIdInt);
            InitializeComponent();
            
            this.inventLocation = inventLocation;
            this.Load += AddNewElement_Load;
            
        }

        private void AddNewElement_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                using (db = new TestDB())
                {
                    var wmsLocIDs = db.InventDim
                        .Where(elem=> elem.InventLocationId == inventLocation.InventLocationId) 
                        .Select(i => i.WMSLocationId)
                        .Distinct()
                        .Cast<int>()
                        .OrderBy(s => s)
                        .AsEnumerable()
                        .Select(el=> el.ToString().PadLeft(2, '0'))
                        .ToArray();
                        
                    if (comboBox1.InvokeRequired)
                    {
                        comboBox1.Invoke((MethodInvoker)delegate ()
                        {
                            comboBox1.Items.Clear();
                            foreach (var wms in wmsLocIDs)
                            {
                                comboBox1.Items.Add(wms);
                                comboBox2.Items.Add(wms);
                            }
                        });
                    }

                }

            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            //chek for null
            if (comboBox1.SelectedItem == null) return;
            if (comboBox2.SelectedItem == null) return;
            var targetInventLocationId = textBox1.Text;
            if (string.IsNullOrEmpty(targetInventLocationId)) return;

            //add 0 before num
            targetInventLocationId = targetInventLocationId.Length == 1 ? "0" + targetInventLocationId : targetInventLocationId;

            //get WMS ID 
            if (int.TryParse(comboBox1.SelectedItem.ToString(), out int fromValueInt)
                && int.TryParse(comboBox2.SelectedItem.ToString(), out int toValueInt))
            {
                if (fromValueInt > toValueInt)
                {
                    string tempString = comboBox1.SelectedItem.ToString();
                    comboBox1.SelectedItem = comboBox2.SelectedItem;
                    comboBox2.SelectedItem = tempString;
                }
            }
            else
            {
                return;
            }
            string fromValue = comboBox1.SelectedItem.ToString();
            string toValue = comboBox2.SelectedItem.ToString();
            
            //check input ID format
            if (!int.TryParse(targetInventLocationId, out int targetInventLocationIdInt)) return;

            if (targetInventLocationIdInt == inventLocationIdInt)
            {
                MessageBox.Show("Target equals source");
                return;
            }
            


            using (db = new TestDB())
            {
                
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        this.TopLevel = false;
                        
                        Task.Run(() =>
                        {
                            infoForm = new InfoForm("Update records");
                            infoForm.ShowDialog();
                        });
                        bool presentTargetInventLocationId = db.InventLocation
                            .Where(el => el.InventLocationId == targetInventLocationId)
                            .Any();
                        if (!presentTargetInventLocationId)
                        {
                            //generation 1 mock record 
                            InventLocation newiIventLocation = MockDataFill.GetInventLocations(1).FirstOrDefault();
                            if (newiIventLocation != null)
                            {
                                newiIventLocation.InventLocationId = targetInventLocationId;
                                db.InventLocation.Add(newiIventLocation);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            UpdateWMS(fromValue, toValue, targetInventLocationId, db);
                        }
                        transaction.Commit();
                        DialogResult = DialogResult.OK;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        DialogResult = DialogResult.Cancel;
                        Loggeer.Instance.Log(ex.Message);
                        MessageBox.Show("Error during saving changes");
                    }
                }
                
            }
            this.Close();
            
        }
        private void UpdateWMS(string fromValue, string toValue, string targetInventLocationId,  TestDB testDB)
        {
           testDB.InventDim
                     .Where(d => d.WMSLocationId.CompareTo(fromValue) >= 0 && d.WMSLocationId.CompareTo(toValue) <= 0)
                     .ToList()
                     .ForEach(d => d.InventLocationId = targetInventLocationId);
            db.SaveChanges();
        }
    }
}
