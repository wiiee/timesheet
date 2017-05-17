namespace Platform.Util
{
    public class Item
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Item() { }

        public Item(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
