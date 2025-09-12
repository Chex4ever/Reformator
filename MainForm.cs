using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EmployeeSalaryProcessor;

public partial class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly XmlProcessor _xmlProcessor;
    private readonly AppConfig _config;
    private DataGridView dataGridView = null!;
    private TextBox txtInputFile = null!;
    private TextBox txtOutputFile = null!;
    private TextBox txtXsltFile = null!;
    private Button btnBrowseInput = null!;
    private Button btnBrowseXslt = null!;
    private Button btnProcess = null!;
    private Button btnAddEmployee = null!;
    private List<string> _availableMonths = [];

    public MainForm(XmlProcessor xmlProcessor, IOptions<AppConfig> config, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _xmlProcessor = xmlProcessor;
        _config = config.Value;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        InitializeDataGridView();
        InitializeFileControls();
        InitializeButtons();

        SetupLayout();

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

    private void InitializeFileControls()
    {
        txtInputFile = new TextBox
        {
            Text = Path.Combine(Directory.GetCurrentDirectory(), _config.AppSettings.DefaultInputFile),
            Dock = DockStyle.Fill,
            Height = _config.AppSettings.TextBoxHeight,
            Width = 500

        };

        txtOutputFile = new TextBox
        {
            Text = Path.Combine(Directory.GetCurrentDirectory(), _config.AppSettings.DefaultOutputFile),
            Dock = DockStyle.Fill,
            Height = _config.AppSettings.TextBoxHeight,
            ReadOnly = true,
            Width = 500

        };

        txtXsltFile = new TextBox
        {
            Text = Path.Combine(Directory.GetCurrentDirectory(), _config.AppSettings.DefaultXsltFile),
            Dock = DockStyle.Fill,
            Height = _config.AppSettings.TextBoxHeight,
            Width = 500
        };

        btnBrowseInput = new Button
        {
            Text = _config.Labels.ButtonExplore,
            Width = 80,
            Dock = DockStyle.Right
        };
        btnBrowseInput.Click += BtnBrowseInput_Click;

        btnBrowseXslt = new Button
        {
            Text = _config.Labels.ButtonExplore,
            Width = 80,
            Dock = DockStyle.Right
        };
        btnBrowseXslt.Click += BtnBrowseXslt_Click;

        txtInputFile.TextChanged += TxtInputFile_TextChanged;
    }
    private void InitializeButtons()
    {
        btnProcess = new Button
        {
            Text = _config.Labels.ButtonProcess,
            Height = _config.AppSettings.ButtonHeight,
            Dock = DockStyle.Right,
            Width = 200
        };
        btnProcess.Click += BtnProcess_Click;

        btnAddEmployee = new Button
        {
            Text = _config.Labels.ButtonAddEmployee,
            Height = _config.AppSettings.ButtonHeight,
            Dock = DockStyle.Left,
            Width = 200
        };
        btnAddEmployee.Click += BtnAddEmployee_Click;
    }

    private void SetupLayout()
    {
        Text = _config.Titles.MainWindow;
        ClientSize = new Size(_config.AppSettings.FormWidth, _config.AppSettings.FormHeight);
        Padding = new Padding(_config.AppSettings.PaddingSize);

        var mainTableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1
        };
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Файлы
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // DataGridView
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Кнопки

        var filePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 3
        };

        filePanel.Controls.Add(new Label
        {
            Text = $"{_config.Labels.InputFile}:",
            TextAlign = ContentAlignment.MiddleRight
        }, 0, 0);
        filePanel.Controls.Add(txtInputFile!, 1, 0);
        filePanel.Controls.Add(btnBrowseInput!, 2, 0);

        filePanel.Controls.Add(new Label
        {
            Text = $"{_config.Labels.OutputFile}:",
            TextAlign = ContentAlignment.MiddleRight
        }, 0, 1);
        filePanel.Controls.Add(txtOutputFile!, 1, 1);

        filePanel.Controls.Add(new Label
        {
            Text = $"{_config.Labels.XsltFile}:",
            TextAlign = ContentAlignment.MiddleRight
        }, 0, 2);
        filePanel.Controls.Add(txtXsltFile!, 1, 2);
        filePanel.Controls.Add(btnBrowseXslt!, 2, 2);

        var buttonPanel = new Panel { Dock = DockStyle.Fill };
        buttonPanel.Controls.Add(btnAddEmployee!);
        buttonPanel.Controls.Add(btnProcess!);

        mainTableLayout.Controls.Add(filePanel, 0, 0);
        mainTableLayout.Controls.Add(dataGridView!, 0, 1);
        mainTableLayout.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(mainTableLayout);
    }

    private void TxtInputFile_TextChanged(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtInputFile?.Text) && File.Exists(txtInputFile.Text))
        {
            string outputFile = Path.Combine(
                Path.GetDirectoryName(txtInputFile.Text)!,
               _config.AppSettings.DefaultOutputFile
            );
            txtOutputFile!.Text = outputFile;
        }
    }

    private void BtnBrowseInput_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = _config.FileFilters.XmlWithAll,
            Title = _config.Messages.SelectXmlFile,
            InitialDirectory = Directory.Exists(Path.GetDirectoryName(txtInputFile?.Text))
                ? Path.GetDirectoryName(txtInputFile!.Text)
                : Application.StartupPath
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            txtInputFile!.Text = openFileDialog.FileName;
        }
    }
    private void BtnBrowseXslt_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = _config.FileFilters.XsltWithAll,
            Title = _config.Messages.SelectXsltFile,
            InitialDirectory = Directory.Exists(Path.GetDirectoryName(txtXsltFile?.Text))
                ? Path.GetDirectoryName(txtXsltFile!.Text)
                : Application.StartupPath
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            txtXsltFile!.Text = openFileDialog.FileName;
        }
    }

    private void BtnProcess_Click(object? sender, EventArgs? e)
    {
        if (string.IsNullOrEmpty(txtInputFile?.Text) || !File.Exists(txtInputFile.Text))
        {
            MessageBox.Show(
               _config.Messages.InputFileRequired,
               _config.Titles.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (string.IsNullOrEmpty(txtXsltFile?.Text) || !File.Exists(txtXsltFile.Text))
        {
            MessageBox.Show(
               _config.Messages.XsltFileRequired,
               _config.Titles.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        try
        {
            _xmlProcessor.TransformXml(txtInputFile.Text, txtXsltFile.Text, txtOutputFile!.Text);
            _xmlProcessor.AddTotalSalaryToEmployees(txtOutputFile.Text);
            _availableMonths = _xmlProcessor.GetAllMonths(txtOutputFile.Text);

            DisplayEmployeeData(txtOutputFile.Text);

            MessageBox.Show(
               _config.Messages.ProcessingComplete,
               _config.Titles.Information,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"{_config.Titles.Error}: {ex.Message}",
               _config.Titles.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void BtnAddEmployee_Click(object? sender, EventArgs e)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var form = scope.ServiceProvider.GetRequiredService<AddEmployeeForm>();

            form.SetAvailableMonths(_xmlProcessor.GetAllMonths(txtInputFile.Text));
            if (form.ShowDialog() == DialogResult.OK && form.EmployeeData != null)
            {
                _xmlProcessor.AddPaymentToData(txtInputFile?.Text ?? "", form.EmployeeData);
                BtnProcess_Click(null, null);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"{_config.Messages.AddPaymentError}: {ex.Message}",
               _config.Titles.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private void DisplayEmployeeData(string filePath)
    {
        if (dataGridView == null) return;

        dataGridView.Rows.Clear();
        dataGridView.Columns.Clear();

        dataGridView.Columns.Add("Employee", _config.Labels.ColumnEmployee);
        dataGridView.Columns["Employee"]!.FillWeight = 20;

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
        dataGridView.Columns.Add("Total", _config.Labels.ColumnTotal);
        dataGridView.Columns["Total"]!.FillWeight = 15;

        var employees = _xmlProcessor.GetEmployeeDisplayData(filePath);

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

}
// public static class ControlExtensions
// {
//     public static T WithClickHandler<T>(this T control, EventHandler handler) where T : Control
//     {
//         control.Click += handler;
//         return control;
//     }
// }