﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using POS.Context;
using POS.Entities;
using POS.Repository;
using POS.Repository.DAL;
using POS.Repository.Interfaces;

namespace POS
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        internal EmployeewsOfAsowell _unitempofwork;

        public Login()
        {
            _unitempofwork = new EmployeewsOfAsowell();
            InitializeComponent();

            this.WindowState = WindowState.Normal;

            this.Closing += Closing_LoginWindos;
        }
        

        private static int ID_SIZE_DBASOWELL = 10;
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string pass = txtPass.Password;

            bool isFound = false;
            List<Employee> empList = _unitempofwork.EmployeeRepository.Get().ToList();
            foreach (Employee emp in empList)
            {
                if (emp.Username.Equals(username) && emp.Pass.Equals(pass))
                {
                    App.Current.Properties["EmpLogin"] = emp;

                    try
                    {
                        SalaryNote empSalaryNote = _unitempofwork.SalaryNoteRepository.Get(sle => sle.EmpId.Equals(emp.EmpId) && sle.ForMonth.Equals(DateTime.Now.Month) && sle.ForYear.Equals(DateTime.Now.Year)).First();

                        App.Current.Properties["EmpSN"] = empSalaryNote;
                        WorkingHistory empWorkHistory = new WorkingHistory { ResultSalary = empSalaryNote.SnId, EmpId = empSalaryNote.EmpId };
                        App.Current.Properties["EmpWH"] = empWorkHistory;
                    }
                    catch(Exception ex)
                    {
                        SalaryNote empSalary = new SalaryNote { EmpId = emp.EmpId, SalaryValue = 0, WorkHour = 0, ForMonth = DateTime.Now.Month, ForYear = DateTime.Now.Year, IsPaid = 0 };
                        _unitempofwork.SalaryNoteRepository.Insert(empSalary);
                        _unitempofwork.Save();
                        WorkingHistory empWorkHistory = new WorkingHistory { ResultSalary = empSalary.SnId, EmpId = empSalary.EmpId };
                        App.Current.Properties["EmpWH"] = empWorkHistory;
                        App.Current.Properties["EmpSN"] = empSalary;
                    }
                    //create new salary note if null, create new workinghistory if not null
                    
                    //if (empSalaryNote == null)
                    //{
                    //    SalaryNote empSalary = new SalaryNote { EmpId = emp.EmpId, SalaryValue = 0, WorkHour = 0, ForMonth = DateTime.Now.Month, ForYear = DateTime.Now.Year, IsPaid = 0 };
                    //    _unitempofwork.SalaryNoteRepository.Insert(empSalary);
                    //    _unitempofwork.Save();
                    //    WorkingHistory empWorkHistory = new WorkingHistory { ResultSalary = empSalary.SnId, EmpId = empSalary.EmpId };
                    //    App.Current.Properties["EmpWH"] = empWorkHistory;
                    //    App.Current.Properties["EmpSN"] = empSalary;
                    //}
                    //else
                    //{
                    //    App.Current.Properties["EmpSN"] = empSalaryNote;
                    //    WorkingHistory empWorkHistory = new WorkingHistory { ResultSalary = empSalaryNote.SnId, EmpId = empSalaryNote.EmpId };
                    //    App.Current.Properties["EmpWH"] = empWorkHistory;

                    //}

                    EmployeeWorkSpace.MainWindow main = new EmployeeWorkSpace.MainWindow();
                    main.Show();

                    isFound = true;

                    //end create

                    break;
                }

            }

            if (!isFound)
            {
                MessageBox.Show("incorrect username or password");
                return;
            }
            this.Close();
        }

        private void btnDatabase_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Closing_LoginWindos(object sender, EventArgs args)
        {
            _unitempofwork.Dispose();
        }
    }
}