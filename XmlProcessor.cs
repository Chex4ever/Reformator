using System.Globalization;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace EmployeeSalaryProcessor
{
    public class XmlProcessor
    {
        private readonly string _dataFilePath;
        private readonly string _xsltFilePath;

        public XmlProcessor(string dataFilePath, string xsltFilePath)
        {
            _dataFilePath = dataFilePath;
            _xsltFilePath = xsltFilePath;
        }

        public double ParseAmount(string? amountStr)
        {
            if (string.IsNullOrEmpty(amountStr))
                return 0;

            amountStr = amountStr.Replace(",", ".").Replace(" ", "");

            if (double.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            return 0;
        }
        public enum ErrorHandlingStrategy
        {
            KeepAsIs,
            UseParentElement
        }
        public ErrorHandlingStrategy MountErrorHandling { get; set; } = ErrorHandlingStrategy.KeepAsIs;
        public void TransformXmlUniversal(string outputFilePath)
        {
            var xslt = new XslCompiledTransform();

            if (MountErrorHandling == ErrorHandlingStrategy.UseParentElement)
            {
                // Используем XSLT с автокоррекцией
                xslt.Load("DataX_to_Employees_with_correction.xslt");
            }
            else
            {
                // Используем простой универсальный XSLT
                xslt.Load("DataX_to_Employees.xslt");
            }

            xslt.Transform(_dataFilePath, outputFilePath);
        }

        public void AddTotalSalaryToEmployees(string inputFilePath, string outputFilePath)
        {
            XDocument doc = XDocument.Load(inputFilePath);

            foreach (var employee in doc.Descendants("Employee"))
            {
                double total = employee.Descendants("salary")
                .Sum(salary => ParseAmount(salary.Attribute("amount")?.Value));
                employee.SetAttributeValue("totalSalary", total.ToString("F2", CultureInfo.InvariantCulture));
            }

            doc.Save(outputFilePath);
        }

        public void AddTotalAmountToData(string outputFilePath)
        {
            XDocument doc = XDocument.Load(_dataFilePath);
            double total = doc.Descendants("item")
                .Sum(item => ParseAmount(item.Attribute("amount")?.Value));
            doc.Descendants("Pay").First()
                .SetAttributeValue("totalAmount", total.ToString("F2", CultureInfo.InvariantCulture));
            doc.Save(outputFilePath);
        }

        public void AddEmployeeToData(EmployeeData employeeData)
        {
            XDocument doc = XDocument.Load(_dataFilePath);
            var payElement = doc.Descendants("Pay").First();

            foreach (var salary in employeeData.Salaries)
            {
                payElement.Add(new XElement("item",
                    new XAttribute("name", employeeData.Name),
                    new XAttribute("surname", employeeData.Surname),
                    new XAttribute("amount", salary.Amount.ToString("F2")),
                    new XAttribute("mount", salary.Month)
                ));
            }

            doc.Save(_dataFilePath);
        }

        public IEnumerable<EmployeeDisplayData> GetEmployeeDisplayData(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);
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
                    month => month,
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

        public List<string> GetAllMonths(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);
            return doc.Descendants("salary")
                .Select(s => s.Attribute("mount")?.Value)
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct()
                .OrderBy(m => m)
                .ToList()!;
        }
    }

    public class EmployeeData
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public List<MonthSalary> Salaries { get; set; } = new List<MonthSalary>();
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
}