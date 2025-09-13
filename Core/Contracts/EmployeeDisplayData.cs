namespace EmployeeSalaryProcessor.Core.Contracts;

public class EmployeeDisplayData
{
    public string FullName { get; set; } = string.Empty;
    public Dictionary<string, string> MonthlySalaries { get; set; } = new Dictionary<string, string>();
    public string TotalSalary { get; set; } = string.Empty;
}