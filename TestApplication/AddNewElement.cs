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
        public AddNewElement(InventLocation inventLocation)
        {
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
                        
                    if (InvokeRequired)
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
            this.Visible = false;
            this.TopMost = false;
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
                    int tempInt = fromValueInt;
                    string tempString = comboBox1.SelectedItem.ToString();
                    fromValueInt = toValueInt;
                    toValueInt = tempInt;
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
            int targetInventLocationIdInt;
            if (!int.TryParse(targetInventLocationId, out targetInventLocationIdInt)) return;

            using (db = new TestDB())
            {
                bool presentTargetInventLocationId = db.InventLocation
                     .Where(el => el.InventLocationId == targetInventLocationId)
                     .Any();
                if (!presentTargetInventLocationId)
                {
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
                    infoForm = new InfoForm("Update records");
                    infoForm.TopMost = true;
                    Task.Run(() =>
                    {
                        infoForm.ShowDialog();
                    });
                    SaveChanges(fromValue, toValue, targetInventLocationId, db);
                    

                }
            }
            DialogResult = DialogResult.OK;
            
            this.Close();
            
        }
        private void SaveChanges(string fromValue, string toValue, string targetInventLocationId,  TestDB testDB)
        {
            testDB.InventDim
                     .Where(d => d.WMSLocationId.CompareTo(fromValue) >= 0 && d.WMSLocationId.CompareTo(toValue) <= 0)
                     .ToList()
                     .ForEach(d => d.InventLocationId = targetInventLocationId);
            db.SaveChanges();
        }
    }
}
