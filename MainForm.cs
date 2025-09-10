using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace EmployeeSalaryProcessor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. XSLT преобразование
                TransformXmlWithXslt();

                // 2. Добавить сумму salary в Employee
                AddTotalSalaryToEmployees();

                // 3. Добавить общую сумму в Data1.xml
                AddTotalAmountToData1();

                // 4. Отобразить данные
                DisplayEmployeeData();

                MessageBox.Show("Обработка завершена успешно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void TransformXmlWithXslt()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load("Data1_to_Employees.xslt");
            xslt.Transform("Data1.xml", "Employees_transformed.xml");
        }

        private void AddTotalSalaryToEmployees()
        {
            XDocument doc = XDocument.Load("Employees_transformed.xml");
            
            foreach (var employee in doc.Descendants("Employee"))
            {
                double total = 0;
                foreach (var salary in employee.Descendants("salary"))
                {
                    string amountStr = salary.Attribute("amount")?.Value;
                    if (!string.IsNullOrEmpty(amountStr))
                    {
                        amountStr = amountStr.Replace(",", ".");
                        if (double.TryParse(amountStr, out double amount))
                        {
                            total += amount;
                        }
                    }
                }
                employee.SetAttributeValue("totalSalary", total.ToString("F2"));
            }
            
            doc.Save("Employees_final.xml");
        }

        private void AddTotalAmountToData1()
        {
            XDocument doc = XDocument.Load("Data1.xml");
            double total = 0;
            
            foreach (var item in doc.Descendants("item"))
            {
                string amountStr = item.Attribute("amount")?.Value;
                if (!string.IsNullOrEmpty(amountStr))
                {
                    amountStr = amountStr.Replace(",", ".");
                    if (double.TryParse(amountStr, out double amount))
                    {
                        total += amount;
                    }
                }
            }
            
            var payElement = doc.Descendants("Pay").First();
            payElement.SetAttributeValue("totalAmount", total.ToString("F2"));
            
            doc.Save("Data1_updated.xml");
        }

        private void DisplayEmployeeData()
        {
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();

            // Создаем колонки
            dataGridView.Columns.Add("Employee", "Сотрудник");
            dataGridView.Columns.Add("January", "Январь");
            dataGridView.Columns.Add("February", "Февраль");
            dataGridView.Columns.Add("March", "Март");
            dataGridView.Columns.Add("Total", "Всего");

            XDocument doc = XDocument.Load("Employees_final.xml");
            
            foreach (var employee in doc.Descendants("Employee"))
            {
                string name = employee.Attribute("name")?.Value;
                string surname = employee.Attribute("surname")?.Value;
                string totalSalary = employee.Attribute("totalSalary")?.Value;

                Dictionary<string, string> monthlySalaries = new Dictionary<string, string>
                {
                    {"january", ""},
                    {"february", ""},
                    {"march", ""}
                };

                foreach (var salary in employee.Descendants("salary"))
                {
                    string month = salary.Attribute("mount")?.Value;
                    string amount = salary.Attribute("amount")?.Value;
                    
                    if (monthlySalaries.ContainsKey(month))
                    {
                        monthlySalaries[month] = amount;
                    }
                }

                dataGridView.Rows.Add(
                    $"{name} {surname}",
                    monthlySalaries["january"],
                    monthlySalaries["february"],
                    monthlySalaries["march"],
                    totalSalary
                );
            }
        }

        // Код для дизайнера формы
        private DataGridView dataGridView;
        private Button btnProcess;

        private void InitializeComponent()
        {
            this.dataGridView = new DataGridView();
            this.btnProcess = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            
            // dataGridView
            this.dataGridView.Dock = DockStyle.Top;
            this.dataGridView.Height = 300;
            this.dataGridView.ReadOnly = true;
            
            // btnProcess
            this.btnProcess.Text = "Запустить обработку";
            this.btnProcess.Dock = DockStyle.Bottom;
            this.btnProcess.Height = 40;
            this.btnProcess.Click += new EventHandler(this.btnProcess_Click);
            
            // MainForm
            this.Text = "Обработка зарплат сотрудников";
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.btnProcess);
            
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
        }
    }
}