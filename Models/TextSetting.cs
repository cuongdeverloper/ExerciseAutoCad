namespace Exercise.Models
{
    public class TextSetting
    {
        public string FontName { get; set; } = "Standard (Simplex)";
        public string TextHeight { get; set; } = "2,5";
        public bool AlwaysShowSize { get; set; } = true;
        public bool ShowCustomField { get; set; } = false;
    }
}