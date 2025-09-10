using System;
using System.Windows.Forms;

namespace EmployeeSalaryProcessor
{
    public partial class AddEmployeeForm : Form
    {
        public string EmployeeName { get; private set; }
        public string EmployeeSurname { get; private set; }
        public decimal JanuarySalary { get; private set; }
        public decimal FebruarySalary { get; private set; }
        public decimal MarchSalary { get; private set; }

        public AddEmployeeForm()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                EmployeeName = txtName.Text;
                EmployeeSurname = txtSurname.Text;
                JanuarySalary = numJanuary.Value;
                FebruarySalary = numFebruary.Value;
                MarchSalary = numMarch.Value;
                
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите имя сотрудника");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(txtSurname.Text))
            {
                MessageBox.Show("Введите фамилию сотрудника");
                return false;
            }
            
            return true;
        }

        // Код для дизайнера формы
        private TextBox txtName;
        private TextBox txtSurname;
        private NumericUpDown numJanuary;
        private NumericUpDown numFebruary;
        private NumericUpDown numMarch;
        private Button btnAdd;

        private void InitializeComponent()
        {
            this.txtName = new TextBox();
            this.txtSurname = new TextBox();
            this.numJanuary = new NumericUpDown();
            this.numFebruary = new NumericUpDown();
            this.numMarch = new NumericUpDown();
            this.btnAdd = new Button();
            
            // Настройка контролов
            // ... (реализация layout формы)
            
            this.Text = "Добавить сотрудника";
            this.ClientSize = new System.Drawing.Size(300, 200);
        }
    }
}