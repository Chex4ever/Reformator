using EmployeeSalaryProcessor.Core.Contracts;
using EmployeeSalaryProcessor.Core.Entities;

namespace EmployeeSalaryProcessor.Core.Services;

public interface IXmlProcessor
{
    void TransformXml(string inputFilePath, string xsltFilePath, string outputFilePath);
    void AddTotalSalaryToEmployees(string xmlFilePath);
    void AddTotalAmountToData(string xmlFilePath);
    void AddPaymentToData(string xmlFilePath, EmployeeData employeeData);
    List<EmployeeDisplayData> GetEmployeeDisplayData(string xmlFilePath);
    List<string> GetAllMonths(string xmlFilePath);
    List<Employee> GetAllEmployees(string xmlFilePath);
    List<MonthSalary> GetEmployeeSalaries(string xmlFilePath, string name, string surname);
    decimal ParseAmount(string? amountStr);
    bool PaymentExists(string xmlFilePath, string name, string surname, string month);
}