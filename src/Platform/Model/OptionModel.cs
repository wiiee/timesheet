namespace Platform.Model
{
    public class OptionModel
    {
        public string value;
        public string text;
        public bool isSelected;

        public OptionModel(string text, string value, bool isSelected)
        {
            this.text = text;
            this.value = value;
            this.isSelected = isSelected;
        }
    }
}
