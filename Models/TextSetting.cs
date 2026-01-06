namespace Exercise.Models
{
    public class TextSetting
    {
        public string FontName { get; set; } = "Standard";
        public double TextHeight { get; set; } = 2.5;
        public short ColorIndex { get; set; } = 4;
        public bool ShowPipeSize { get; set; } = true;
        public bool ShowCustomField { get; set; } = false;

        public string PreviewText => "1-11, 1n, 1e-2.5 / d20";
    }
}