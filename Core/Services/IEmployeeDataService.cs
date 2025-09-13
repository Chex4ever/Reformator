using EmployeeSalaryProcessor.Core.Contracts;
using EmployeeSalaryProcessor.Core.Entities;

namespace EmployeeSalaryProcessor.Core.Services;

public interface IEmployeeDataService
{
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<List<MonthSalary>> GetEmployeeSalariesAsync(string name, string surname);
    Task AddPaymentAsync(EmployeeData employeeData);
    void UpdateAvailableMonths(List<string> months);

    List<string> AvailableMonths { get; }
    event EventHandler DataChanged;
}