using System.Globalization;
using System.Xml.Linq;
using System.Xml.Xsl;
using Microsoft.Extensions.Options;

namespace EmployeeSalaryProcessor;

// TODO - Violation of SRP
// TODO - много строковых литералов
public class XmlProcessor(IOptions<AppConfig> config)
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
        if (string.IsNullOrEmpty(inputFilePath))
            throw new ArgumentException("Input file path cannot be null or empty", nameof(inputFilePath));
        if (string.IsNullOrEmpty(xsltFilePath))
            throw new ArgumentException("XSLT file path cannot be null or empty", nameof(xsltFilePath));
        if (string.IsNullOrEmpty(outputFilePath))
            throw new ArgumentException("Output file path cannot be null or empty", nameof(outputFilePath));
        if (!File.Exists(inputFilePath))
            throw new FileNotFoundException("Input file not found", inputFilePath);
        if (!File.Exists(xsltFilePath))
            throw new FileNotFoundException("XSLT file not found", xsltFilePath);


        try
        {
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
        if (string.IsNullOrEmpty(xmlFilePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(xmlFilePath));
        if (!File.Exists(xmlFilePath))
            throw new FileNotFoundException("File not found", xmlFilePath);

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
        if (string.IsNullOrEmpty(xmlFilePath))
            throw new ArgumentException("Source file path cannot be null or empty", nameof(xmlFilePath));
        if (!File.Exists(xmlFilePath))
            throw new FileNotFoundException("Source file not found", xmlFilePath);

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
        if (string.IsNullOrEmpty(xmlFilePath))
            throw new ArgumentException("Data file path cannot be null or empty", nameof(xmlFilePath));
        if (employeeData == null)
            throw new ArgumentNullException(nameof(employeeData));
        if (!File.Exists(xmlFilePath))
            throw new FileNotFoundException("Data file not found", xmlFilePath);

        try
        {
            XDocument doc = XDocument.Load(xmlFilePath);
            var payElement = doc.Descendants("Pay").First();
            if (payElement == null)
            {
                throw new InvalidOperationException("Pay element not found in XML file");
            }

            foreach (var salary in employeeData.Salaries)
            {
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

    public IEnumerable<EmployeeDisplayData> GetEmployeeDisplayData(string xmlFilePath)
    {
        if (string.IsNullOrEmpty(xmlFilePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(xmlFilePath));
        if (!File.Exists(xmlFilePath))
            throw new FileNotFoundException("File not found", xmlFilePath);

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

            foreach (var employee in doc.Descendants("Employee"))
            {
                string name = employee.Attribute("name")?.Value ?? "Unknown";
                string surname = employee.Attribute("surname")?.Value ?? "Unknown";
                string totalSalary = employee.Attribute("totalSalary")?.Value ?? "0";

                var monthlySalaries = allMonths.ToDictionary(
                    month => month ?? throw new InvalidOperationException("allMonths.ToDictionary error"),
                    month => employee.Descendants("salary")
                        .FirstOrDefault(s => s.Attribute("mount")?.Value == month)?
                        .Attribute("amount")?.Value ?? "0"
                );

                employees.Add(new EmployeeDisplayData
                {
                    FullName = $"{name} {surname}",
                    MonthlySalaries = monthlySalaries,
                    TotalSalary = totalSalary
                });
            }

            return employees;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get employee data: {ex.Message}", ex);
        }
    }

    public List<string> GetAllMonths(string xmlFilePath)
    {
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
}

public class EmployeeData
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public List<MonthSalary> Salaries { get; set; } = [];
}
public class MonthSalary
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
public class EmployeeDisplayData
{
    public string FullName { get; set; } = string.Empty;
    public Dictionary<string, string> MonthlySalaries { get; set; } = new Dictionary<string, string>();
    public string TotalSalary { get; set; } = string.Empty;
}