﻿using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using AutoMapper;
using CommunityToolkit.Mvvm.Input;
using Workshop.Core.DataBase;
using Workshop.Core.Domains;
using Workshop.Core.Entites;
using Workshop.Core.Validators;
using Workshop.Model;
using Workshop.Core.Helper;
using Workshop.Core;
using Workshop.Model.Dto;
using CommunityToolkit.Mvvm.DependencyInjection;
using Workshop.Core.Validators.Implements;
using Workshop.Helper;
using Workshop.Core.Excel.Models;

namespace Workshop.ViewModel
{
    public class ImportPageViewModel : ObservableObject
    {
        private readonly WorkshopDbContext _dbContext;
        private Validator validator;
        public ImportPageViewModel(WorkshopDbContext dbContext)
        {
            validator = Ioc.Default.GetRequiredService<Validator>();
            validator.SetValidatorProvider(new DefaultValidatorProvider<EmployeeEntity>());
            this.ImportCommand = new RelayCommand(ImportAction, ()=>true);
            this.ValidDataCommand = new RelayCommand(GetDataAction, CanValidate);
            this.SubmitCommand = new RelayCommand(SubmitAction, CanSubmit);
            this.Employees = new ObservableCollection<EmployeeEntity>();
            this.ProcessResultList = new ObservableCollection<ProcessResultDto>();
            this.ProcessResultList.CollectionChanged += ProcessResultList_CollectionChanged;
            this.PropertyChanged += ImportPageViewModel_PropertyChanged;
            this._dbContext = dbContext;
        }

        private void ImportPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.IsValidSuccess))
            {
                SubmitCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(this.Employees))
            {
                SubmitCommand.NotifyCanExecuteChanged();
                ValidDataCommand.NotifyCanExecuteChanged();

            }
        }

        private void ProcessResultList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.IsValidSuccess = this.ProcessResultList.Count == 0;
        }

        private async void SubmitAction()
        {
            var task = InvokeHelper.InvokeOnUi<IEnumerable<Employee>>(null, () =>
            {
                var employeeAccount = AutoMapperHelper.MapToList<EmployeeEntity, EmployeeAccount>(this.Employees);
                var employeeSalay = AutoMapperHelper.MapToList<EmployeeEntity, EmployeeSalay>(this.Employees, new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<double, double>().ConvertUsing(s => Math.Round(s, 2));
                    cfg.CreateMap<EmployeeEntity, EmployeeSalay>()
                        .ForMember(dest => dest.Sum, opt => opt.MapFrom(src => src.Sum));
                }));
                var employeeSocialInsuranceAndFund = AutoMapperHelper.MapToList<EmployeeEntity, EmployeeSocialInsuranceAndFund>(this.Employees, new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<double, double>().ConvertUsing(s => Math.Round(s, 2));
                    cfg.CreateMap<EmployeeEntity, EmployeeSocialInsuranceAndFund>()
                        .ForMember(dest => dest.Sum, opt => opt.MapFrom(src => src.Sum1));
                }));
                var enterpriseSocialInsuranceAndFund = AutoMapperHelper.MapToList<EmployeeEntity, EnterpriseSocialInsuranceAndFund>(this.Employees, new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<double, double>().ConvertUsing(s => Math.Round(s, 2));
                    cfg.CreateMap<EmployeeEntity, EnterpriseSocialInsuranceAndFund>()
                        .ForMember(dest => dest.Sum, opt => opt.MapFrom(src => src.Sum2));
                }));
                var employeeSocialInsuranceDetail = AutoMapperHelper.MapToList<EmployeeEntity, EmployeeSocialInsuranceDetail>(this.Employees, new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<double, double>().ConvertUsing(s => Math.Round(s, 2));
                    cfg.CreateMap<EmployeeEntity, EmployeeSocialInsuranceDetail>();
                }));
                var resultEmployees = AutoMapperHelper.MapToList<EmployeeEntity, Employee>(this.Employees).Select(c => new Employee()
                {
                    Year = c.Year,
                    Mounth = c.Mounth,
                    Batch = c.Batch,
                    SerialNum = c.SerialNum,
                    Dept = c.Dept,
                    Proj = c.Proj,
                    State = c.State,
                    Name = c.Name,
                    IDCard = c.IDCard,
                    Level = c.Level,
                    JobCate = c.JobCate,
                    EmployeeAccount = employeeAccount.FirstOrDefault(d => d.Id == c.Id),
                    EmployeeSalay = employeeSalay.FirstOrDefault(d => d.Id == c.Id),
                    EmployeeSocialInsuranceAndFund = employeeSocialInsuranceAndFund.FirstOrDefault(d => d.Id == c.Id),
                    EnterpriseSocialInsuranceAndFund = enterpriseSocialInsuranceAndFund.FirstOrDefault(d => d.Id == c.Id),
                    EmployeeSocialInsuranceDetail = employeeSocialInsuranceDetail.FirstOrDefault(d => d.Id == c.Id)

                });
                this._dbContext.Employee.AddRangeAsync(resultEmployees);
                var result = this._dbContext.SaveChanges();

                return resultEmployees;



            }, async (t) =>
            {
                _dbContext.Log.Add(new Log(Log.IMPORT, "成功", t.Count()));
                var result = this._dbContext.SaveChanges();

                this.Employees.Clear();

                MessageBox.Show("已完成导入");

            });
        }

        private void GetDataAction()
        {
            this.ProcessResultList.Clear();
            foreach (var item in this.Employees)
            {

                var row = item.RowNumber + 1;
                var id = ProcessResultList.Count + 1;
                var level = 1;


                var validateResult = validator.Validate(item);
                var result = validateResult.Where(c => c.IsValidated == false)
                    .Select(c => new ProcessResultDto()
                    {
                        Id = id,
                        Row = row,
                        Column = c.Column,
                        Level = level,
                        Content = c.Content,
                        KeyName = c.KeyName,
                    });


                foreach (var processResultDto in result)
                {
                    this.ProcessResultList.Add(processResultDto);

                }


            }
            var currentCount = ProcessResultList.Count();
            if (currentCount > 0)
            {
                _dbContext.Log.Add(new Log(Log.CHECK, "失败", currentCount));

            }
            else
            {
                _dbContext.Log.Add(new Log(Log.CHECK, "成功", this.Employees.Count));

            }
            this._dbContext.SaveChanges();


        }



        private void ImportAction()
        {

            this.Employees.Clear();
            var task = InvokeHelper.InvokeOnUi<dynamic>(null, () =>
            {
                var result = DocHelper.ImportFromDelegator((importer) =>
                {

                    var op1 = new ImportOption<EmployeeEntity>(0, 2);
                    op1.SheetName = "全职";
                    var r1 = importer.Process<EmployeeEntity>(op1);


                    return new { Employees = r1 };

                });
                return result;


            }, (t) =>
            {
                var data = t;
                if (data != null)
                {


                    this.Employees = new ObservableCollection<EmployeeEntity>(data.Employees);
                    this.IsValidSuccess = null;
                }
            });

        }


        private ObservableCollection<ProcessResultDto> _processResultList;

        public ObservableCollection<ProcessResultDto> ProcessResultList
        {
            get { return _processResultList; }
            set
            {
                _processResultList = value;
                OnPropertyChanged(nameof(ProcessResultList));
            }
        }
        private ObservableCollection<EmployeeEntity> _employees;

        public ObservableCollection<EmployeeEntity> Employees
        {
            get { return _employees; }
            set
            {
                _employees = value;
                OnPropertyChanged(nameof(Employees));
            }
        }


        private bool? _isValidSuccess;

        public bool? IsValidSuccess
        {
            get { return _isValidSuccess; }
            set
            {
                _isValidSuccess = value;

                OnPropertyChanged();
            }
        }

        private bool CanSubmit()
        {
            return IsValidSuccess.HasValue && IsValidSuccess.Value;
        }

        private bool CanValidate()
        {
            if (this.Employees.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public RelayCommand ValidDataCommand { get; set; }
        public RelayCommand SubmitCommand { get; set; }

        public RelayCommand ImportCommand { get; set; }
    }
}
