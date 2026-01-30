using Exercise.Models;
using Exercise.MVVM;
using Exercise.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Exercise.ViewModels
{
    public class SelectProjectViewModel : BindableBase
    {
        private readonly ExcelImportService _excelService;

        public ObservableCollection<ProjectModel> Projects => ProjectDataManager.Instance.AvailableProjects;

        private ProjectModel _selectedProject;
        public ProjectModel SelectedProject
        {
            get => _selectedProject;
            set { _selectedProject = value; OnPropertyChanged(); }
        }

        public ICommand LoadExcelCommand { get; set; }
        public ICommand ConfirmCommand { get; set; }

        public Action<bool> CloseAction { get; set; }

        public SelectProjectViewModel()
        {
            _excelService = new ExcelImportService();

            SelectedProject = ProjectDataManager.Instance.CurrentProject;

            LoadExcelCommand = new RelayCommand(ExecuteLoadExcel);
            ConfirmCommand = new RelayCommand(ExecuteConfirm);
        }

        private void ExecuteLoadExcel(object obj)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var data = _excelService.ImportProjects(dlg.FileName);
                    ProjectDataManager.Instance.UpdateProjects(data);

                    MessageBox.Show($"Đã tải được {data.Count} dự án/tháp.", "Thành công");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }

        private void ExecuteConfirm(object obj)
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ Dự án và Tháp!", "Cảnh báo");
                return;
            }

            ProjectDataManager.Instance.CurrentProject = SelectedProject;

            MessageBox.Show($"Đã chọn: {SelectedProject.DisplayName}", "Thông báo");
            CloseAction?.Invoke(true);
        }
    }
}