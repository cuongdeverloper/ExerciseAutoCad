using Exercise.Models;
using Exercise.MVVM;
using Exercise.Actions; 
using System.Windows.Input;

namespace Exercise.ViewModels
{
    public class TextSetupViewModel
    {
        public TextSetting Settings { get; set; }
        public ICommand SaveCommand { get; set; }

        public TextSetupViewModel()
        {
            Settings = new TextSetting();
            SaveCommand = new RelayCommand(o => DrawNow());
        }

        private void DrawNow()
        {
            DrawActions.CreateMLeader(this.Settings);
        }
    }
}