using EmployeeSalaryProcessor.Core.Contracts;
using EmployeeSalaryProcessor.Core.Entities;
using Microsoft.Extensions.Options;

namespace EmployeeSalaryProcessor.Core.Services;
public class EmployeeDataService : IEmployeeDataService
{
    private readonly IXmlProcessor _xmlProcessor;
    private readonly string _sourceXmlFilePath;
    
    public List<string> AvailableMonths { get; private set; } = new();
    public event EventHandler? DataChanged;

    public EmployeeDataService(IXmlProcessor xmlProcessor, IOptions<AppConfig> config)
    {
        _xmlProcessor = xmlProcessor;
        _sourceXmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), 
            config.Value.AppSettings.DefaultInputFile);
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await Task.Run(() => _xmlProcessor.GetAllEmployees(_sourceXmlFilePath));
    }
      public async Task<List<MonthSalary>> GetEmployeeSalariesAsync(string name, string surname)
    {
        return await Task.Run(() => 
            _xmlProcessor.GetEmployeeSalaries(_sourceXmlFilePath, name, surname));
    }

    public async Task AddPaymentAsync(EmployeeData employeeData)
    {
        await Task.Run(() => _xmlProcessor.AddPaymentToData(_sourceXmlFilePath, employeeData));
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateAvailableMonths(List<string> months)
    {
        AvailableMonths = months;
    }

}