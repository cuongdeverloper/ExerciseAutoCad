using System.Collections.Generic;
using System.Collections.ObjectModel;
using Exercise.Models;

namespace Exercise.Services
{
    public class ProjectDataManager
    {
        private static ProjectDataManager _instance;
        public static ProjectDataManager Instance => _instance ?? (_instance = new ProjectDataManager());

        public ObservableCollection<RouteItemModel> GlobalRoutes { get; set; }
        public ObservableCollection<ProjectModel> AvailableProjects { get; set; }

        private ProjectModel _currentProject;
        public ProjectModel CurrentProject
        {
            get => _currentProject;
            set
            {
                _currentProject = value;
            }
        }

        private ProjectDataManager()
        {
            GlobalRoutes = new ObservableCollection<RouteItemModel>();
            AvailableProjects = new ObservableCollection<ProjectModel>();
        }

        public void UpdateRoutes(List<RouteItemModel> newRoutes)
        {
            GlobalRoutes.Clear();
            foreach (var item in newRoutes) GlobalRoutes.Add(item);
        }

        public void UpdateProjects(List<ProjectModel> projects)
        {
            AvailableProjects.Clear();
            foreach (var item in projects) AvailableProjects.Add(item);
        }
    }
}