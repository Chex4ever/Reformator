namespace EmployeeSalaryProcessor;

public class AppSettings
{
    public string DefaultInputFile { get; set; } = "";
    public string DefaultOutputFile { get; set; } = "";
    public string DefaultXsltFile { get; set; } = "";
    public string DefaultFontName { get; set; } = "Segoe UI";
    public float DefaultFontSize { get; set; } = 10F;
    public decimal MinSalary { get; set; } = 0;
    public decimal MaxSalary { get; set; } = 1000000;
    public int DecimalPlaces { get; set; } = 2;
    public int FormWidth { get; set; } = 1000;
    public int FormHeight { get; set; } = 600;
    public int PaddingSize { get; set; } = 10;
    public int ButtonHeight { get; set; } = 40;
    public int TextBoxHeight { get; set; } = 25;
}

public class FileFilters
{
    public string XmlFiles { get; set; } = "";
    public string XsltFiles { get; set; } = "";
    public string AllFiles { get; set; } = "";
    public string XmlWithAll => $"{XmlFiles}|{AllFiles}";
    public string XsltWithAll => $"{XsltFiles}|{AllFiles}";
}

public class Messages
{
    public string SelectXmlFile { get; set; } = "SelectXmlFile";
    public string SelectXsltFile { get; set; } = "SelectXsltFile";
    public string ProcessingComplete { get; set; } = "ProcessingComplete";
    public string InputFileRequired { get; set; } = "InputFileRequired";
    public string XsltFileRequired { get; set; } = "XsltFileRequired";
    public string EmployeeNameRequired { get; set; } = "EmployeeNameRequired";
    public string EmployeeSurnameRequired { get; set; } = "EmployeeSurnameRequired";
    public string SalaryRequired { get; set; } = "SalaryRequired";
    public string AddEmployeeError { get; set; } = "AddEmployeeError";
    public string AddPaymentError { get; set; } = "AddPaymentError";
}

public class Titles
{
    public string MainWindow { get; set; } = "MainWindow";
    public string AddEmployee { get; set; } = "AddEmployee";
    public string Error { get; set; } = "Error";
    public string Information { get; set; } = "Information";
}
public class Labels
{
    public string ColumnEmployee { get; set; } = "ColumnEmployee";
    public string ColumnTotal { get; set; } = "ColumnTotal";
    public string XsltFile { get; set; } = "XsltFile";
    public string OutputFile { get; set; } = "OutputFile";
    public string InputFile { get; set; } = "InputFile";
    public string ButtonAddEmployee { get; set; } = "ButtonAddEmployee";
    public string ButtonProcess { get; set; } = "ButtonProcess";
    public string ButtonExplore { get; set; } = "ButtonExplore";
    public string Name { get; set; } = "Name";
    public string Surname { get; set; } = "Surname";
    public string Payments { get; set; } = "Payments";
    public string Add { get; set; } = "Add";
    public string Cancel { get; set; } = "Cancel";
}

public class AppConfig
{
    public AppSettings AppSettings { get; set; } = new AppSettings();
    public FileFilters FileFilters { get; set; } = new FileFilters();
    public Messages Messages { get; set; } = new Messages();
    public Titles Titles { get; set; } = new Titles();
    public Labels Labels { get; set; } = new Labels();
}