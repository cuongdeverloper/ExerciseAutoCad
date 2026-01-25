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

        private ProjectDataManager()
        {
            GlobalRoutes = new ObservableCollection<RouteItemModel>();
        }

        public void UpdateRoutes(List<RouteItemModel> newRoutes)
        {
            GlobalRoutes.Clear();
            foreach (var item in newRoutes)
            {
                GlobalRoutes.Add(item);
            }
        }
    }
}