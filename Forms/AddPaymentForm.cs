using System.Globalization;
using EmployeeSalaryProcessor.Core.Contracts;
using EmployeeSalaryProcessor.Core.Entities;
using EmployeeSalaryProcessor.Core.Services;
using Microsoft.Extensions.Options;

namespace EmployeeSalaryProcessor.Forms;

public partial class AddPaymentForm : Form
{
    private readonly AppConfig _config;
    private readonly IEmployeeDataService _employeeDataService;

    public EmployeeData? EmployeeData { get; private set; }
    private List<string> _availableMonths = [];
    private TableLayoutPanel tableLayoutSalaries = null!;
    private TextBox txtName = null!;
    private TextBox txtSurname = null!;
    private Button btnAdd = null!;
    private Button btnCancel = null!;
    private Button btnAddMonth = null!;
    private ComboBox cmbEmployees = null!;
    private Dictionary<string, NumericUpDown> _salaryControls = [];

    public AddPaymentForm(IOptions<AppConfig> config, IEmployeeDataService employeeDataService)
    {
        _employeeDataService = employeeDataService;
        _config = config.Value;
        InitializeComponent();
        LoadEmployeesAsync();
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
        cmbEmployees = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(_config.AppSettings.PaddingSize / 2),
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
            Width = 250,
            DisplayMember = "FullName"

        };
        cmbEmployees.SelectedIndexChanged += CmbEmployees_SelectedIndexChanged;

        txtName = new TextBox
        {
            PlaceholderText = _config.Labels.Name,
            Margin = new Padding(_config.AppSettings.PaddingSize / 2),
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
            Width = 200
        };
        txtSurname = new TextBox
        {
            PlaceholderText = _config.Labels.Surname,
            Margin = new Padding(_config.AppSettings.PaddingSize / 2),
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
            Width = 200
        };

        tableLayoutSalaries = new TableLayoutPanel()
        {
            AutoScroll = true,
            ColumnCount = 2,
            Padding = new Padding(5)
        };

        InitializeSalaryControls();

        btnAdd = new Button
        {
            Text = _config.Labels.Add,
            DialogResult = DialogResult.OK,
            Margin = new Padding(_config.AppSettings.PaddingSize / 2),
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
            Width = 100
        };
        btnAdd.Click += BtnAdd_Click;

        btnCancel = new Button
        {
            Text = _config.Labels.Cancel,
            DialogResult = DialogResult.Cancel,
            Margin = new Padding(_config.AppSettings.PaddingSize / 2),
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
            Width = 100
        };

        btnAddMonth = new Button
        {
            Text = "Добавить месяц",
            Margin = new Padding(_config.AppSettings.PaddingSize / 2),
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
            Width = 120
        };
        btnAddMonth.Click += BtnAddMonth_Click;

    }
    private async void LoadEmployeesAsync()
    {
        try
        {
            var employees = await _employeeDataService.GetAllEmployeesAsync();
            SetEmployeeList(employees);
            InitializeSalaryControls();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
        }
    }
    private async void CmbEmployees_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbEmployees.SelectedItem?.ToString() == _config.Labels.ButtonAddEmployee)
        {
            ResetForm();
            return;
        }

        if (cmbEmployees.SelectedItem is Employee selectedEmployee)
        {
            txtName.Text = selectedEmployee.Name;
            txtSurname.Text = selectedEmployee.Surname;

            await LoadEmployeeSalariesAsync(selectedEmployee.Name, selectedEmployee.Surname);
        }
    }

    private async Task LoadEmployeeSalariesAsync(string name, string surname)
    {
        try
        {
            var salaries = await _employeeDataService.GetEmployeeSalariesAsync(name, surname);
            PrefillSalaries(salaries);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки зарплат: {ex.Message}");
        }
    }
    private void ResetForm()
    {
        txtName.Text = "";
        txtSurname.Text = "";
        ResetSalaryControls();

        if (cmbEmployees.Items.Count > 0 && cmbEmployees.Items[0]?.ToString() == _config.Labels.ButtonAddEmployee)
        {
            cmbEmployees.SelectedIndex = 0;
        }
    }
    private void PrefillSalaries(List<MonthSalary> salaries)
    {
        ResetSalaryControls();
        foreach (var salary in salaries)
        {
            if (_salaryControls.ContainsKey(salary.Month))
            {
                _salaryControls[salary.Month].Value = salary.Amount;
            }
        }
    }
    private void ResetSalaryControls()
    {
        foreach (var control in _salaryControls.Values)
        {
            control.Value = 0;
        }
    }

    public void SetEmployeeList(List<Employee> employees)
    {
        cmbEmployees.Items.Clear();
        cmbEmployees.Items.Add(_config.Labels.ButtonAddEmployee);

        foreach (var employee in employees)
        {
            cmbEmployees.Items.Add(employee);
        }
        cmbEmployees.SelectedIndex = 0;
    }

    private void InitializeSalaryControls()
    {
        if (tableLayoutSalaries == null) return;

        tableLayoutSalaries.Controls.Clear();
        _salaryControls.Clear();

        tableLayoutSalaries.RowCount = _availableMonths.Count;
        for (int i = 0; i < _employeeDataService.AvailableMonths.Count; i++)
        {
            var month = _employeeDataService.AvailableMonths[i];
            var label = new Label
            {
                Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(month) + ":",
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(_config.AppSettings.PaddingSize / 2),
                Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),

            };
            var numericUpDown = new NumericUpDown
            {
                Minimum = _config.AppSettings.MinSalary,
                Maximum = _config.AppSettings.MaxSalary,
                DecimalPlaces = _config.AppSettings.DecimalPlaces,
                Margin = new Padding(_config.AppSettings.PaddingSize / 2),
                Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
                Width = 120,
                Tag = month
            };

            tableLayoutSalaries.Controls.Add(label, 0, i);
            tableLayoutSalaries.Controls.Add(numericUpDown, 1, i);

            _salaryControls[month] = numericUpDown;
        }
    }

    private void SetupForm()
    {
        Text = _config.Titles.AddPayment;
        ClientSize = new Size(400, 500);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Padding = new Padding(_config.AppSettings.PaddingSize);

        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(_config.AppSettings.PaddingSize / 2)
        };

        AddControlsToTable(tableLayout);
        Controls.Add(tableLayout);

        AcceptButton = btnAdd;
        CancelButton = btnCancel;
    }

    private void AddControlsToTable(TableLayoutPanel tableLayout)
    {
        tableLayout.Controls.Add(new Label
        {
            Text = $"{_config.Labels.Name}:",
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
        }, 0, 0);
        tableLayout.Controls.Add(txtName, 1, 0);

        tableLayout.Controls.Add(new Label
        {
            Text = $"{_config.Labels.Surname}:",
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
        }, 0, 1);
        tableLayout.Controls.Add(txtSurname, 1, 1);

        tableLayout.Controls.Add(new Label
        {
            Text = $"{_config.Labels.Payments}:",
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font(_config.AppSettings.DefaultFontName, _config.AppSettings.DefaultFontSize),
        }, 0, 2);

        var scrollPanel = new Panel
        {
            AutoScroll = true,
            Height = 200,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill
        };

        scrollPanel.Controls.Add(tableLayoutSalaries);
        tableLayoutSalaries.Dock = DockStyle.Fill;

        tableLayout.Controls.Add(scrollPanel, 1, 2);

        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 0)
        };
        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnAdd);

        tableLayout.Controls.Add(buttonPanel, 0, 3);
        tableLayout.SetColumnSpan(buttonPanel, 2);
    }
    private void BtnAddMonth_Click(object? sender, EventArgs e)
    {
        using var inputDialog = new Form
        {
            Text = "Добавить новый месяц",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            Width = 300,
            Height = 150
        };

        var textBox = new TextBox { Width = 200, Top = 20, Left = 50 };
        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Top = 60, Left = 80 };
        var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Top = 60, Left = 160 };

        inputDialog.Controls.AddRange([textBox, btnOk, btnCancel]);
        inputDialog.AcceptButton = btnOk;
        inputDialog.CancelButton = btnCancel;

        if (inputDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            string newMonth = textBox.Text.Trim().ToLower();

            if (!_employeeDataService.AvailableMonths.Contains(newMonth))
            {
                _employeeDataService.AvailableMonths.Add(newMonth);
                _employeeDataService.AvailableMonths.Sort();
                InitializeSalaryControls();
            }
        }
    }

    private void BtnAdd_Click(object? sender, EventArgs? e)
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
            MessageBox.Show(
                _config.Messages.EmployeeNameRequired,
                _config.Titles.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
                );
            txtName?.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtSurname?.Text))
        {
            MessageBox.Show(
                _config.Messages.EmployeeSurnameRequired,
                _config.Titles.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            txtSurname?.Focus();
            return false;
        }

        bool hasSalary = _salaryControls.Values.Any(nud => nud.Value > 0);
        if (!hasSalary)
        {
            MessageBox.Show(
                _config.Messages.SalaryRequired,
                _config.Titles.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return false;
        }

        return true;
    }
    public void SetAvailableMonths(List<string> months)
    {
        if (months == null) return;

        _availableMonths = months;
        InitializeSalaryControls();
        if (this.Visible)
        {
            this.Invalidate();
            this.Refresh();
        }
    }
    public void UpdateAvailableMonths(List<string> newMonths)
    {
        if (newMonths == null) return;

        _availableMonths.Clear();
        _availableMonths.AddRange(newMonths);
        InitializeSalaryControls();
    }
    public void PrefillData(string name, string surname, List<MonthSalary>? salaries = null)
    {
        txtName.Text = name;
        txtSurname.Text = surname;

        if (salaries != null)
        {
            foreach (var salary in salaries)
            {
                if (_salaryControls.ContainsKey(salary.Month))
                {
                    _salaryControls[salary.Month].Value = salary.Amount;
                }
            }
        }
    }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        btnAdd.Click -= BtnAdd_Click;
    }
}