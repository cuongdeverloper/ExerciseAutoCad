using Exercise.Models;

namespace Exercise.Models
{
    public class ProjectModel : BindableBase
    {
        public string ProjectName { get; set; }
        public string TowerName { get; set; }

        public string DisplayName => $"{ProjectName} - {TowerName}";
    }
}