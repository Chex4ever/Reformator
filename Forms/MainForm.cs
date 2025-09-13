using System.Globalization;
using EmployeeSalaryProcessor.Core.Services;
using EmployeeSalaryProcessor.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EmployeeSalaryProcessor.Forms;

public partial class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEmployeeDataService _employeeDataService;
    private readonly IXmlProcessor _xmlProcessor;
    private readonly AppConfig _config;
    private bool _isDataLoaded = false;

    private DataGridView dataGridView = null!;
    private TextBox txtInputFile = null!;
    private TextBox txtOutputFile = null!;
    private TextBox txtXsltFile = null!;
    private Button btnBrowseInput = null!;
    private Button btnBrowseXslt = null!;
    private Button btnProcess = null!;
    private Button btnAddPayment = null!;

    public MainForm(IEmployeeDataService employeeDataService, IXmlProcessor xmlProcessor, IOptions<AppConfig> config, IServiceProvider serviceProvider)
    {
        _employeeDataService = employeeDataService;
        _isDataLoaded = false;
        _serviceProvider = serviceProvider;
        _xmlProcessor = xmlProcessor;
        _config = config.Value;
        InitializeComponent();
        UpdateButtonStates();
        _employeeDataService.DataChanged += async (s, e) => await ReloadDataAsync();
    }
    private void UpdateButtonStates()
    {
        if (btnAddPayment == null) return;

        btnAddPayment.Enabled = _isDataLoaded;

        if (!_isDataLoaded)
        {
            btnAddPayment.Text = _config.Labels.ButtonAddPayment + " (запустите обработку)";
        }
        else
        {
            btnAddPayment.Text = _config.Labels.ButtonAddPayment;
        }
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

        btnAddPayment = new Button
        {
            Text = _config.Labels.ButtonAddPayment,
            Height = _config.AppSettings.ButtonHeight,
            Dock = DockStyle.Left,
            Width = 200
        };
        btnAddPayment.Click += BtnAddPayment_Click;
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
        filePanel.Controls.Add(txtXsltFile, 1, 2);
        filePanel.Controls.Add(btnBrowseXslt, 2, 2);

        var buttonPanel = new Panel { Dock = DockStyle.Fill };
        buttonPanel.Controls.Add(btnAddPayment);
        buttonPanel.Controls.Add(btnProcess);

        mainTableLayout.Controls.Add(filePanel, 0, 0);
        mainTableLayout.Controls.Add(dataGridView, 0, 1);
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
        _isDataLoaded = false;
        UpdateButtonStates();
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

    private async void BtnProcess_Click(object? sender, EventArgs? e)
    {
        await ProcessDataAsync();
    }

    private void BtnAddPayment_Click(object? sender, EventArgs e)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var form = scope.ServiceProvider.GetRequiredService<AddPaymentForm>();

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

        foreach (var month in _employeeDataService.AvailableMonths)
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
            foreach (var month in _employeeDataService.AvailableMonths)
            {
                rowData.Add(employee.MonthlySalaries.TryGetValue(month, out var salary)
                ? salary : "0");
            }
            rowData.Add(employee.TotalSalary);
            dataGridView.Rows.Add(rowData.ToArray());
        }
        dataGridView.AutoResizeColumns();
    }
    private async Task ReloadDataAsync()
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            btnProcess.Enabled = false;
            btnAddPayment.Enabled = false;

            await ProcessDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при перезагрузке данных: {ex.Message}",
                _config.Titles.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            btnProcess.Enabled = true;
            btnAddPayment.Enabled = _isDataLoaded;
        }
    }

    private async Task ProcessDataAsync()
    {
        AppLogger.LogDebug(this, "ProcessDataAsync started");
        if (string.IsNullOrEmpty(txtInputFile?.Text) || !File.Exists(txtInputFile.Text) ||
            string.IsNullOrEmpty(txtXsltFile?.Text) || !File.Exists(txtXsltFile.Text))
        {
            AppLogger.LogWarning(this, "ProcessDataAsync: Input files are missing or invalid");
            return;
        }

        try
        {
            AppLogger.LogDebug(this, "Starting XML transformation");
            _xmlProcessor.TransformXml(txtInputFile.Text, txtXsltFile.Text, txtOutputFile!.Text);
            AppLogger.LogDebug(this, "XML transformation completed");

            _xmlProcessor.AddTotalSalaryToEmployees(txtOutputFile.Text);
            AppLogger.LogDebug(this, "Total salaries added");

            var months = _xmlProcessor.GetAllMonths(txtOutputFile.Text);
            AppLogger.LogDebug(this, $"Found {months.Count} months: {string.Join(", ", months)}");

            _employeeDataService.UpdateAvailableMonths(months);
            AppLogger.LogDebug(this, "Employee data displayed");

            await DisplayEmployeeDataAsync(txtOutputFile.Text);

            _isDataLoaded = true;
            UpdateButtonStates();

            AppLogger.LogInfo(this, "Processing completed successfully");
        }
        catch (Exception ex)
        {
            AppLogger.LogError(this, "ProcessDataAsync failed", ex);
            _isDataLoaded = false;
            UpdateButtonStates();

            MessageBox.Show($"{_config.Titles.Error}: {ex.Message}",
                _config.Titles.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DisplayEmployeeDataAsync(string filePath)
    {
        AppLogger.LogDebug(this, $"DisplayEmployeeDataAsync started for file: {filePath}");

        if (dataGridView == null)
        {
            AppLogger.LogWarning(this, "DisplayEmployeeDataAsync: dataGridView is null");
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                AppLogger.LogDebug(this, "Loading employee display data");
                var employees = _xmlProcessor.GetEmployeeDisplayData(filePath);
                AppLogger.LogDebug(this, $"Loaded {employees.Count()} employees");

                foreach (var employee in employees)
                {
                    AppLogger.LogDebug(this, $"Employee: {employee.FullName}, Total: {employee.TotalSalary}");
                    foreach (var monthly in employee.MonthlySalaries)
                    {
                        AppLogger.LogDebug(this, $"  {monthly.Key}: {monthly.Value}");
                    }
                }

                this.Invoke((MethodInvoker)delegate
                {
                    try
                    {
                        AppLogger.LogDebug(this, "Updating UI controls");

                        dataGridView.Rows.Clear();
                        dataGridView.Columns.Clear();

                        dataGridView.Columns.Add("Employee", _config.Labels.ColumnEmployee);
                        dataGridView.Columns["Employee"]!.FillWeight = 20;

                        AppLogger.LogDebug(this, $"Adding {_employeeDataService.AvailableMonths.Count} month columns");
                        foreach (var month in _employeeDataService.AvailableMonths)
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

                        AppLogger.LogDebug(this, $"Adding {employees.Count()} rows to grid");
                        foreach (var employee in employees)
                        {
                            var rowData = new List<object> { employee.FullName };

                            foreach (var month in _employeeDataService.AvailableMonths)
                            {
                                if (employee.MonthlySalaries.TryGetValue(month, out var salary))
                                {
                                    rowData.Add(salary);
                                    AppLogger.LogDebug(this, $"  {employee.FullName} - {month}: {salary}");
                                }
                                else
                                {
                                    rowData.Add("0");
                                    AppLogger.LogDebug(this, $"  {employee.FullName} - {month}: 0 (not found)");
                                }
                            }

                            rowData.Add(employee.TotalSalary);
                            dataGridView.Rows.Add(rowData.ToArray());
                        }

                        dataGridView.AutoResizeColumns();
                        AppLogger.LogDebug(this, "UI update completed successfully");
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError(this, "UI update failed", ex);
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                AppLogger.LogError(this, "Background data loading failed", ex);
                throw;
            }
        });
    }

}