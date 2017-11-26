﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using POS.Entities;
using POS.Repository.DAL;
using POS.EmployeeWorkSpace;
using System.Windows.Input;
using System.Windows.Threading;

namespace POS
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        internal EmployeewsOfAsowell _unitempofwork;
        private DispatcherTimer LoadCodeLogin;

        public Login()
        {
            _unitempofwork = new EmployeewsOfAsowell();
            InitializeComponent();

            txtUsername.Focus();

            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.NoResize;

            LoadCodeLogin = new DispatcherTimer();
            LoadCodeLogin.Tick += LoadCodeLogin_Tick; ;
            LoadCodeLogin.Interval = new TimeSpan(0, 0, 0, 0, 1);

            this.Closing += Closing_LoginWindos;

            App.Current.Properties["IsConfigDB"] = "";
        }

        private void LoadCodeLogin_Tick(object sender, EventArgs e)
        {
            gNormalLoginForm.Width -= 10;
            if (gNormalLoginForm.Width == 0)
            {
                gNormalLoginForm.Visibility = Visibility.Collapsed;
                LoadCodeLogin.Stop();
            }
        }

        private void txtUsername_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPass.Focus();
            }
        }

        private async void txtPass_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string username = txtUsername.Text;
                string pass = txtPass.Password;
                try
                {
                    btnLogin.IsEnabled = false;
                    PgbLoginProcess.Visibility = Visibility.Visible;
                    await LoginAsync(username, pass);

                    btnLogin.IsEnabled = true;
                    PgbLoginProcess.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }



        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string pass = txtPass.Password;
            try
            {
                btnLogin.IsEnabled = false;
                PgbLoginProcess.Visibility = Visibility.Visible;
                await LoginAsync(username, pass);

                btnLogin.IsEnabled = true;
                PgbLoginProcess.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task LoginAsync(string username, string pass)
        {
            try
            {
                await Task.Run(() =>
                {
                    bool isFound = false;
                    List<Employee> empList = _unitempofwork.EmployeeRepository.Get().ToList();
                    List<AdminRe> AdList = _unitempofwork.AdminreRepository.Get().ToList();
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
                            catch (Exception ex)
                            {
                                SalaryNote empSalary = new SalaryNote { EmpId = emp.EmpId, SalaryValue = 0, WorkHour = 0, ForMonth = DateTime.Now.Month, ForYear = DateTime.Now.Year, IsPaid = 0 };
                                _unitempofwork.SalaryNoteRepository.Insert(empSalary);
                                _unitempofwork.Save();
                                WorkingHistory empWorkHistory = new WorkingHistory { ResultSalary = empSalary.SnId, EmpId = empSalary.EmpId };
                                App.Current.Properties["EmpWH"] = empWorkHistory;
                                App.Current.Properties["EmpSN"] = empSalary;
                            }

                            Dispatcher.Invoke(() =>
                            {
                                EmpLoginListData.emploglist.Clear();
                                EmpLoginListData.emploglist.Add(new EmpLoginList { Emp = emp, EmpSal = App.Current.Properties["EmpSN"] as SalaryNote, EmpWH = App.Current.Properties["EmpWH"] as WorkingHistory, TimePercent = 0 });

                                EmployeeWorkSpace.MainWindow main = new EmployeeWorkSpace.MainWindow();
                                main.Show();
                            });
                            isFound = true;

                            //end create

                            break;
                        }

                    }
                    //Get Admin
                    bool isFoundAd = false;
                    if (!isFound)
                    {
                        foreach (var item in AdList)
                        {
                            if (item.Username.Equals(username) && item.Pass.Equals(pass))
                            {
                                App.Current.Properties["AdLogin"] = item;

                                Dispatcher.Invoke(() =>
                                {
                                    AdminNavWindow navwindow = new AdminNavWindow();
                                    navwindow.Show();
                                });

                                isFoundAd = true;
                                break;

                            }

                        }
                        if (!isFoundAd)
                        {
                            MessageBox.Show("incorrect username or password");
                            return;
                        }
                    }
                    Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });

                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async void btnLoginCode_Click(object sender, RoutedEventArgs e)
        {
            int code;
            try
            {
                code = int.Parse(KbEmpCodeLoginForm.InputValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Incorrect input!");
                return;
            }

            try
            {
                KbEmpCodeLoginForm.ButtonGoAbleState(false);
                await LoginByCodeAsync(code);
                KbEmpCodeLoginForm.ButtonGoAbleState(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task LoginByCodeAsync(int code)
        {
            try
            {
                await Task.Run(() =>
                {
                    bool isFound = false;
                    Employee loginEmp = _unitempofwork.EmployeeRepository.Get(x => x.empCode == code).FirstOrDefault();
                    if (loginEmp != null)
                    {
                        App.Current.Properties["EmpLogin"] = loginEmp;

                        try
                        {
                            SalaryNote empSalaryNote = _unitempofwork.SalaryNoteRepository.Get(sle => sle.EmpId.Equals(loginEmp.EmpId) && sle.ForMonth.Equals(DateTime.Now.Month) && sle.ForYear.Equals(DateTime.Now.Year)).First();

                            App.Current.Properties["EmpSN"] = empSalaryNote;
                            WorkingHistory empWorkHistory = new WorkingHistory { ResultSalary = empSalaryNote.SnId, EmpId = empSalaryNote.EmpId };
                            App.Current.Properties["EmpWH"] = empWorkHistory;
                        }
                        catch (Exception ex)
                        {
                            SalaryNote empSalary = new SalaryNote { EmpId = loginEmp.EmpId, SalaryValue = 0, WorkHour = 0, ForMonth = DateTime.Now.Month, ForYear = DateTime.Now.Year, IsPaid = 0 };
                            _unitempofwork.SalaryNoteRepository.Insert(empSalary);
                            _unitempofwork.Save();
                            WorkingHistory empWorkHistory = new WorkingHistory { ResultSalary = empSalary.SnId, EmpId = empSalary.EmpId };
                            App.Current.Properties["EmpWH"] = empWorkHistory;
                            App.Current.Properties["EmpSN"] = empSalary;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            EmpLoginListData.emploglist.Clear();
                            EmpLoginListData.emploglist.Add(new EmpLoginList { Emp = loginEmp, EmpSal = App.Current.Properties["EmpSN"] as SalaryNote, EmpWH = App.Current.Properties["EmpWH"] as WorkingHistory, TimePercent = 0 });

                            EmployeeWorkSpace.MainWindow main = new EmployeeWorkSpace.MainWindow();
                            main.Show();
                        });
                        isFound = true;

                    }

                    if (!isFound)
                    {
                        MessageBox.Show("incorrect username or password");
                        return;
                    }
                    Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });

                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        private void btnDatabase_Click(object sender, RoutedEventArgs e)
        {
            DatabaseConfigWindow dbConfig = new DatabaseConfigWindow();
            dbConfig.ShowDialog();

            if ((App.Current.Properties["IsConfigDB"] as string).Equals("true"))
            {
                _unitempofwork = new EmployeewsOfAsowell(App.Current.Properties["InitialCatalog"] as string,
                    App.Current.Properties["Source"] as string,
                    App.Current.Properties["UserId"] as string,
                    App.Current.Properties["Password"] as string);
            }
        }

        private void Closing_LoginWindos(object sender, EventArgs args)
        {
            _unitempofwork.Dispose();
        }

        private void ButtonChangeLoginType_Click(object sender, RoutedEventArgs e)
        {
            LoadCodeLogin.Start();
            //gNormalLoginForm.Visibility = Visibility.Collapsed;
        }
    }
}
