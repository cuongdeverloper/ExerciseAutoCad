using Exercise.MVVM;
using System;
using System.Windows.Input;

namespace Exercise.ViewModels
{
    public class CreateRouteViewModel
    {
        public double Width { get; set; } = 200; 
        public double Height { get; set; } = 100; 
        public double Elevation { get; set; } = 2800; 

        public Action<bool> CloseAction { get; set; }

        public ICommand DrawCommand { get; set; }

        public CreateRouteViewModel()
        {
            DrawCommand = new RelayCommand(ExecuteDraw);
        }

        private void ExecuteDraw(object obj)
        {
            CloseAction?.Invoke(true);
        }
    }
}