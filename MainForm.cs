using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
namespace EmployeeSalaryProcessor
{
    public partial class MainForm : Form
    {
        private readonly XmlProcessor _xmlProcessor;
        private TextBox? txtInputFile;
        private TextBox? txtOutputFile;
        private Button? btnBrowse;
        private Button? btnProcess;
        private Button? btnAddEmployee;
        private DataGridView? dataGridView;
        private List<string> _availableMonths = new List<string>();

        public MainForm()
        {
            _xmlProcessor = new XmlProcessor("Data2.xml", "DataX_to_Employees_with_correction.xslt");
            _xmlProcessor.MountErrorHandling = XmlProcessor.ErrorHandlingStrategy.KeepAsIs;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            InitializeDataGridView();
            InitializeButtons();
            SetupForm();
            ResumeLayout(false);
        }

        private void InitializeDataGridView()
        {
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };
        }

        private void InitializeButtons()
        {
            btnProcess = CreateButton("Запустить обработку", DockStyle.Bottom, btnProcess_Click);
            btnAddEmployee = CreateButton("Добавить сотрудника", DockStyle.Bottom, btnAddEmployee_Click);
        }

        private Button CreateButton(string text, DockStyle dockStyle, EventHandler clickHandler)
        {
            return new Button
            {
                Text = text,
                Dock = dockStyle,
                Height = 40,
                Margin = new Padding(3),
                Font = new System.Drawing.Font("Segoe UI", 10F)
            }.WithClickHandler(clickHandler);
        }

        private void SetupForm()
        {
            Text = "Обработка зарплат сотрудников - навороченый";
            ClientSize = new System.Drawing.Size(1000, 600);
            Padding = new Padding(10);

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            buttonPanel.Controls.Add(btnAddEmployee!);
            buttonPanel.Controls.Add(btnProcess!);

            btnAddEmployee!.Dock = DockStyle.Left;
            btnAddEmployee.Width = 200;
            btnProcess!.Dock = DockStyle.Right;
            btnProcess.Width = 200;

            tableLayout.Controls.Add(dataGridView!, 0, 0);
            tableLayout.Controls.Add(buttonPanel, 0, 1);

            Controls.Add(tableLayout);
        }

        private void btnProcess_Click(object? sender, EventArgs? e)
        {
            try
            {
                ProcessData();
                DisplayEmployeeData();
                MessageBox.Show("Обработка завершена успешно!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка обработки: {ex.Message}");
            }
        }

        private void ProcessData()
        {
            _xmlProcessor.TransformXmlUniversal("Employees_transformed.xml");
            _xmlProcessor.AddTotalSalaryToEmployees("Employees_transformed.xml", "Employees_final.xml");
            _xmlProcessor.AddTotalAmountToData("Data1_updated.xml");
            _availableMonths = _xmlProcessor.GetAllMonths("Employees_final.xml");
        }

        private void DisplayEmployeeData()
        {
            if (dataGridView == null) return;

            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();

            dataGridView.Columns.Add("Employee", "Сотрудник");
            dataGridView.Columns["Employee"].FillWeight = 20;

            foreach (var month in _availableMonths)
            {
                var column = new DataGridViewTextBoxColumn
                {
                    Name = month,
                    HeaderText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(month),
                    FillWeight = 15
                };
                dataGridView.Columns.Add(column);
            }
            dataGridView.Columns.Add("Total", "Всего");
            dataGridView.Columns["Total"].FillWeight = 15;

            var employees = _xmlProcessor.GetEmployeeDisplayData("Employees_final.xml");

            foreach (var employee in employees)
            {
                var rowData = new List<object> { employee.FullName };
                foreach (var month in _availableMonths)
                {
                    rowData.Add(employee.MonthlySalaries.TryGetValue(month, out var salary)
                    ? salary : "0");
                }
                rowData.Add(employee.TotalSalary);
                dataGridView.Rows.Add(rowData.ToArray());
            }
            dataGridView.AutoResizeColumns();
        }

        private void btnAddEmployee_Click(object? sender, EventArgs? e)
        {
            try
            {
                if (!_availableMonths.Any())
                {
                    _availableMonths = _xmlProcessor.GetAllMonths("Data1.xml");
                }

                using (var form = new AddEmployeeForm(_availableMonths))
                {
                    if (form.ShowDialog() == DialogResult.OK && form.EmployeeData != null)
                    {
                        _xmlProcessor.AddEmployeeToData(form.EmployeeData);
                        btnProcess_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка добавления сотрудника: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    public static class ControlExtensions
    {
        public static T WithClickHandler<T>(this T control, EventHandler handler) where T : Control
        {
            control.Click += handler;
            return control;
        }
    }
}