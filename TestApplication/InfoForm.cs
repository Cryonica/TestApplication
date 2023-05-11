using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApplication
{
    public partial class InfoForm : Form
    {
        string loadingText;
        public InfoForm(string message)
        {
            loadingText = message;
            InitializeComponent();
            this.Load += InfoForm_Load;
            
        }

        

        private async void InfoForm_Load(object sender, EventArgs e)
        {
            label1.Text = loadingText;

            while (label1.Text.Length <=70)
            {
                await UpdateLoadingLabel();
                await Task.Delay(500);
            }

            
        }

        private async Task UpdateLoadingLabel()
        {
            const int MaxLength = 50;
            

            if (label1.Text.Length >= MaxLength)
            {
                label1.Text = loadingText; 
                await Task.Delay(500); 
            }
            else
            {
                label1.Text += ". "; // добавляем точку
            }
        }

        
    }
    
}
