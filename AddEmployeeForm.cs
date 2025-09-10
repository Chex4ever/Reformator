using System;
using System.Globalization;
using System.Windows.Forms;

namespace EmployeeSalaryProcessor
{
    public partial class AddEmployeeForm : Form
    {
        public EmployeeData? EmployeeData { get; private set; }

        private readonly List<string> _availableMonths;

        private TextBox? txtName;
        private TextBox? txtSurname;
        private TableLayoutPanel? tableLayoutSalaries;
        private Button? btnAdd;
        private Button? btnCancel;
        private Dictionary<string, NumericUpDown> _salaryControls = new Dictionary<string, NumericUpDown>();

        public AddEmployeeForm(List<string> availableMonths)
        {
            _availableMonths = availableMonths;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            InitializeControls();
            SetupForm();
            ResumeLayout(false);
        }

        private void InitializeControls()
        {
            txtName = CreateTextBox("Имя");
            txtSurname = CreateTextBox("Фамилия");

            tableLayoutSalaries = new TableLayoutPanel()
            {
                AutoScroll = true,
                ColumnCount = 2,
                Padding = new Padding(5)
            };

            InitializeSalaryControls();

            btnAdd = CreateButton("Добавить", DialogResult.OK);
            btnCancel = CreateButton("Отмена", DialogResult.Cancel);

            btnAdd.Click += btnAdd_Click;
        }

        private void InitializeSalaryControls()
        {
            if (tableLayoutSalaries == null) return;
            tableLayoutSalaries.RowCount = _availableMonths.Count;
            for (int i = 1; i < _availableMonths.Count; i++)
            {
                var month = _availableMonths[i];
                var label = new Label
                {
                    Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(month) + ":",
                    TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                    Margin = new Padding(5),
                    Font = new System.Drawing.Font("Segoe UI", 10F)
                };
                var numericUpDown = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = 1000000,
                    DecimalPlaces = 2,
                    Margin = new Padding(5),
                    Font = new System.Drawing.Font("Segoe UI", 10F),
                    Tag = month
                };

                tableLayoutSalaries.Controls.Add(label, 0, i);
                tableLayoutSalaries.Controls.Add(numericUpDown, 1, i);

                _salaryControls[month] = numericUpDown;
            }
        }

        private TextBox CreateTextBox(string placeholder)
        {
            return new TextBox
            {
                PlaceholderText = placeholder,
                Margin = new Padding(5),
                Font = new System.Drawing.Font("Segoe UI", 10F)
            };
        }

        private Button CreateButton(string text, DialogResult dialogResult)
        {
            return new Button
            {
                Text = text,
                DialogResult = dialogResult,
                Margin = new Padding(5),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                Width = 100
            };
        }

        private void SetupForm()
        {
            Text = "Добавить сотрудника";
            ClientSize = new System.Drawing.Size(300, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10)
            };

            AddControlsToTable(tableLayout);
            Controls.Add(tableLayout);

            AcceptButton = btnAdd;
            CancelButton = btnCancel;
        }

        private void AddControlsToTable(TableLayoutPanel tableLayout)
        {
            tableLayout.Controls.Add(new Label { Text = "Имя:", TextAlign = System.Drawing.ContentAlignment.MiddleRight }, 0, 0);
            tableLayout.Controls.Add(txtName!, 1, 0);
            
            tableLayout.Controls.Add(new Label { Text = "Фамилия:", TextAlign = System.Drawing.ContentAlignment.MiddleRight }, 0, 1);
            tableLayout.Controls.Add(txtSurname!, 1, 1);
            
            tableLayout.Controls.Add(new Label { Text = "Зарплаты:", TextAlign = System.Drawing.ContentAlignment.MiddleRight }, 0, 2);
            var scrollPanel = new Panel
            {
                AutoScroll = true,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle
            };
            scrollPanel.Controls.Add(tableLayoutSalaries!);
            tableLayoutSalaries!.Dock = DockStyle.Fill;

            tableLayout.Controls.Add(scrollPanel, 1, 2);

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            buttonPanel.Controls.Add(btnCancel!);
            buttonPanel.Controls.Add(btnAdd!);

            tableLayout.Controls.Add(buttonPanel, 0, 3);
            tableLayout.SetColumnSpan(buttonPanel, 2);
        }

        private void btnAdd_Click(object? sender, EventArgs? e)
        {
            if (ValidateInput())
            {
                EmployeeData = new EmployeeData
                {
                    Name = txtName?.Text ?? string.Empty,
                    Surname = txtSurname?.Text ?? string.Empty,
                    Salaries = _salaryControls.Select(x => new MonthSalary
                    {
                        Month = x.Key,
                        Amount = x.Value.Value
                    }).ToList()
                };
            }
            else
            {
                DialogResult = DialogResult.None;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName?.Text))
            {
                MessageBox.Show("Введите имя сотрудника", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName?.Focus();
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(txtSurname?.Text))
            {
                MessageBox.Show("Введите фамилию сотрудника", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSurname?.Focus();
                return false;
            }
            
            return true;
        }
    }
}