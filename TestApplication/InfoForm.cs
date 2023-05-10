using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApplication
{
    public partial class InfoForm : Form
    {
        
        public InfoForm()
        {
            
            InitializeComponent();
            this.Load += InfoForm_Load;
            
        }

        

        private async void InfoForm_Load(object sender, EventArgs e)
        {
            

            while (label1.Text.Length <=70) // пока задача не завершена
            {
                UpdateLoadingLabel(); // обновляем текст Label
                await Task.Delay(500); // ждем полсекунды
            }

            
        }

        private async void UpdateLoadingLabel()
        {
            const int MaxLength = 50; // максимальная длина текста в Label
            const string LoadingText = "Add records to database"; // текст, который будем добавлять в Label

            if (label1.Text.Length >= MaxLength) // если длина текста достигла максимальной длины
            {
                label1.Text = LoadingText; // обновляем текст Label
                await Task.Delay(500); // ждем полсекунды
            }
            else
            {
                label1.Text += ". "; // добавляем точку
            }
        }

        
    }
    
}
