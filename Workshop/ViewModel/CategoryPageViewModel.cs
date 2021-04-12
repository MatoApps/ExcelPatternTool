﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using SimpleExcelImport;
using Workshop.Common;
using Workshop.Control;
using Workshop.Helper;
using Workshop.Model;
using Workshop.Model.Excel;
using Workshop.Model.Tables;
using Workshop.Service;
using Workshop.View;
namespace Workshop.ViewModel
{
    public class CategoryPageViewModel : ViewModelBase
    {

        private string filePath;

        public CategoryPageViewModel()
        {
            this.SubmitCommand = new RelayCommand(SubmitAction, CanSubmit);
            this.CreateCommand = new RelayCommand(CreateAction);
            this.RemoveCommand = new RelayCommand<CategoryInfo>(RemoveAction);
            this.EditCommand = new RelayCommand<CategoryInfo>(EditAction);
            InitData();
            this.PropertyChanged += CategoryPageViewModel_PropertyChanged;


        }

        private void InitData()
        {
            IList<CategoryInfo> data = null;

            InvokeHelper.InvokeOnUi(null, () =>
            {
                data = WebDataService.GetCategory();
                Thread.Sleep(2000);

            }).ContinueWith((t) =>
            {

                this.CategoryInfos = new ObservableCollection<CategoryInfo>(data);
                this.CategoryInfos.CollectionChanged += CategoryInfos_CollectionChanged;
            });


        }

        private void CategoryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.CategoryInfos))
            {
                if (this.CategoryInfos != null && this.CategoryInfos.Count > 0)
                {
                    LocalDataService.SaveCollectionLocal(this.CategoryInfos);

                }
            }
        }

        private void CategoryInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            SubmitCommand.RaiseCanExecuteChanged();
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (!WebDataService.EditCategory(e.NewItems[0] as CategoryInfo))
                {
                    MessageBox.Show("更新失败");

                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (!WebDataService.DelCategory(e.OldItems[0] as CategoryInfo))
                {
                    MessageBox.Show("删除失败");

                }

            }
            App.GolobelCategorys = new List<CategoryInfo>(WebDataService.GetCategory());

        }

        private void RemoveAction(CategoryInfo obj)
        {
            if (obj == null)
            {
                return;

            }
            RemoveCategory(obj);
        }


        internal void RemoveCategory(CategoryInfo CategoryInfo)
        {
            if (CategoryInfos.Any(c => c.Id == CategoryInfo.Id))
            {
                var current = CategoryInfos.FirstOrDefault(c => c.Id == CategoryInfo.Id);
                CategoryInfos.RemoveAt(CategoryInfos.IndexOf(current));
            }
            else
            {
                MessageBox.Show("条目不存在");

            }
        }

        private void EditAction(CategoryInfo obj)
        {
            if (obj == null)
            {
                return;

            }
            var childvm = SimpleIoc.Default.GetInstance<CreateCategoryViewModel>();
            childvm.CurrentCategoryInfo = obj;

            var cpwindow = new CreateCategoryWindow();
            cpwindow.ShowDialog();

        }



        private void CreateAction()
        {
            var cpwindow = new CreateCategoryWindow();
            cpwindow.ShowDialog();
        }

        private void SubmitAction()
        {
            var odInfos = CategoryInfos.ToList();



            if (odInfos.Count > 0)
            {


                DocHelper.SaveTo(odInfos);
            }

        }


        private ObservableCollection<CategoryInfo> _categoryTypeInfos;

        public ObservableCollection<CategoryInfo> CategoryInfos
        {
            get
            {
                if (_categoryTypeInfos == null)
                {
                    _categoryTypeInfos = new ObservableCollection<CategoryInfo>();
                }
                return _categoryTypeInfos;
            }
            set
            {
                _categoryTypeInfos = value;
                RaisePropertyChanged(nameof(CategoryInfos));
            }
        }

        public void CreateCategory(CategoryInfo CategoryInfo)
        {
            var id = Guid.NewGuid().ToString("N");
            var createtime = DateTime.Now;
            CategoryInfo.Id = id;
            CategoryInfo.CreateTime = createtime;
            if (CategoryInfos.Any(c => c.Id == CategoryInfo.Id))
            {
                var current = CategoryInfos.FirstOrDefault(c => c.Id == CategoryInfo.Id);
                CategoryInfos[CategoryInfos.IndexOf(current)] = CategoryInfo;
            }
            else
            {
                CategoryInfos.Add(CategoryInfo);

            }
        }


        private bool CanSubmit()
        {
            return this.CategoryInfos.Count > 0;

        }


        public RelayCommand GetDataCommand { get; set; }

        public RelayCommand SubmitCommand { get; set; }
        public RelayCommand CreateCommand { get; set; }
        public RelayCommand<CategoryInfo> EditCommand { get; set; }
        public RelayCommand<CategoryInfo> RemoveCommand { get; set; }

    }

}
