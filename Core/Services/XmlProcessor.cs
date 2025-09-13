using System.Globalization;
using System.Xml.Linq;
using System.Xml.Xsl;
using EmployeeSalaryProcessor.Core.Contracts;
using EmployeeSalaryProcessor.Core.Entities;
using EmployeeSalaryProcessor.Core.Utilities;
using Microsoft.Extensions.Options;

namespace EmployeeSalaryProcessor.Core.Services;

// TODO: Violation of SRP
// TODO: много строковых литералов
public class XmlProcessor(IOptions<AppConfig> config) : IXmlProcessor
{
    private readonly AppConfig _config = config.Value;

    public decimal ParseAmount(string? amountStr)
    {
        if (string.IsNullOrEmpty(amountStr))
            return _config.AppSettings.MinSalary;

        amountStr = amountStr.Replace(",", ".").Replace(" ", "");

        if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        {
            return result;
        }

        return _config.AppSettings.MinSalary;
    }
    public void TransformXml(string inputFilePath, string xsltFilePath, string outputFilePath)
    {
        ValidateAndEnsureFileDirectory(inputFilePath, nameof(inputFilePath));
        ValidateAndEnsureFileDirectory(xsltFilePath, nameof(xsltFilePath));
        if (string.IsNullOrEmpty(outputFilePath))
            throw new ArgumentException("Output file path cannot be null or empty", nameof(outputFilePath));


        try
        {
            FileSystemHelper.EnsureDirectoryExists(outputFilePath);
            var xslt = new XslCompiledTransform();
            xslt.Load(xsltFilePath);
            xslt.Transform(inputFilePath, outputFilePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"XSLT transformation failed: {ex.Message}", ex);
        }
    }

    public void AddTotalSalaryToEmployees(string xmlFilePath)
    {
        ValidateAndEnsureFileDirectory(xmlFilePath, nameof(xmlFilePath));
        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);

            foreach (var employee in doc.Descendants("Employee"))
            {
                decimal total = employee.Descendants("salary")
                .Sum(salary => ParseAmount(salary.Attribute("amount")?.Value));
                employee.SetAttributeValue("totalSalary", total.ToString("F2", CultureInfo.InvariantCulture));
            }

            doc.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to add total salary: {ex.Message}", ex);
        }
    }

    public void AddTotalAmountToData(string xmlFilePath)
    {
        ValidateAndEnsureFileDirectory(xmlFilePath, nameof(xmlFilePath));
        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            decimal total = doc.Descendants("item")
                .Sum(item => ParseAmount(item.Attribute("amount")?.Value));
            doc.Descendants("Pay").First()
                .SetAttributeValue("totalAmount", total.ToString("F2", CultureInfo.InvariantCulture));
            doc.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to add total amount: {ex.Message}", ex);
        }
    }

    public void AddPaymentToData(string xmlFilePath, EmployeeData employeeData)
    {
        ValidateAndEnsureFileDirectory(xmlFilePath, nameof(xmlFilePath));
        ArgumentNullException.ThrowIfNull(employeeData);
        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            var payElement = doc.Descendants("Pay").First() ?? throw new InvalidOperationException("Pay element not found in XML file");
            foreach (var salary in employeeData.Salaries)
            {
                if (salary.Amount == 0) continue;
                if (PaymentExists(xmlFilePath, employeeData.Name, employeeData.Surname, salary.Month))
                {
                    AppLogger.LogDebug(this, $"Payment already exists: {employeeData.Name}, {employeeData.Surname}, {salary.Month}");
                }

                payElement.Add(new XElement("item",
                    new XAttribute("name", employeeData.Name),
                    new XAttribute("surname", employeeData.Surname),
                    new XAttribute("amount", salary.Amount.ToString("F2")),
                    new XAttribute("mount", salary.Month)
                ));
            }

            doc.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to add employee: {ex.Message}", ex);
        }
    }

    public List<EmployeeDisplayData> GetEmployeeDisplayData(string xmlFilePath)
    {
        AppLogger.LogDebug(this, $"GetEmployeeDisplayData started for: {xmlFilePath}");

        ValidateAndEnsureFileDirectory(xmlFilePath, nameof(xmlFilePath));
        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            var employees = new List<EmployeeDisplayData>();

            var allMonths = doc.Descendants("salary")
                .Select(s => s.Attribute("mount")?.Value)
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            AppLogger.LogDebug(this, $"Found {allMonths.Count} unique months");

            foreach (var employee in doc.Descendants("Employee"))
            {
                string name = employee.Attribute("name")?.Value ?? "Unknown";
                string surname = employee.Attribute("surname")?.Value ?? "Unknown";
                string totalSalary = employee.Attribute("totalSalary")?.Value ?? "0";

                AppLogger.LogDebug(this, $"Processing employee: {name} {surname}");

                var monthlySalaries = allMonths.ToDictionary(
                    month => month ?? throw new InvalidOperationException("allMonths.ToDictionary error"),
                    month => employee.Descendants("salary")
                        .FirstOrDefault(s => s.Attribute("mount")?.Value == month)?
                        .Attribute("amount")?.Value ?? "0"
                );

                foreach (var monthSalary in monthlySalaries)
                {
                    AppLogger.LogDebug(this, $"  {monthSalary.Key}: {monthSalary.Value}");
                }
                employees.Add(new EmployeeDisplayData
                {
                    FullName = $"{name} {surname}",
                    MonthlySalaries = monthlySalaries,
                    TotalSalary = totalSalary
                });
            }
            AppLogger.LogDebug(this, $"GetEmployeeDisplayData completed: {employees.Count} employees");
            return employees;
        }
        catch (Exception ex)
        {
            AppLogger.LogError(this, "GetEmployeeDisplayData failed", ex);

            throw new InvalidOperationException($"Failed to get employee data: {ex.Message}", ex);
        }
    }

    public List<string> GetAllMonths(string xmlFilePath)
    {
        ValidateAndEnsureFileDirectory(xmlFilePath, nameof(xmlFilePath));

        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            return doc.Descendants("salary")
                .Select(s => s.Attribute("mount")?.Value)
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct()
                .OrderBy(m => m)
                .ToList()!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse xml for payments months: {ex.Message}", ex);

        }
    }
    public List<Employee> GetAllEmployees(string xmlFilePath)
    {
        ValidateAndEnsureFileDirectory(xmlFilePath, nameof(xmlFilePath));
        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            return doc.Descendants("item")
            .Select(item => new Employee
            {
                Name = item.Attribute("name")?.Value ?? "Unknown",
                Surname = item.Attribute("surname")?.Value ?? "Unknown"
            })
            .GroupBy(e => new { e.Name, e.Surname })
            .Select(g => g.First())
            .OrderBy(e => e.Surname)
            .ThenBy(e => e.Name)
            .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get employees: {ex.Message}", ex);
        }
    }

    public List<MonthSalary> GetEmployeeSalaries(string xmlFilePath, string name, string surname)
    {
        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            return doc.Descendants("item")
                .Where(item => item.Attribute("name")?.Value == name &&
                              item.Attribute("surname")?.Value == surname)
                .Select(item => new MonthSalary
                {
                    Month = item.Attribute("mount")?.Value ?? "",
                    Amount = ParseAmount(item.Attribute("amount")?.Value)
                })
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get employee salaries: {ex.Message}", ex);
        }
    }
    private static void ValidateAndEnsureFileDirectory(string filePath, string paramName = "filePath")
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty", paramName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        FileSystemHelper.EnsureDirectoryExists(filePath);
    }
    public bool PaymentExists(string xmlFilePath, string name, string surname, string month)
    {
        ValidateAndEnsureFileDirectory(xmlFilePath, nameof(xmlFilePath));

        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);

            return doc.Descendants("item")
                .Any(item =>
                    item.Attribute("name")?.Value == name &&
                    item.Attribute("surname")?.Value == surname &&
                    item.Attribute("mount")?.Value == month);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to check payment existence: {ex.Message}", ex);
        }
    }
}