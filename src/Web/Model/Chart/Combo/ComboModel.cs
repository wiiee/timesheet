namespace Web.Model.Chart.Combo
{
    using System.Collections.Generic;

    public class ComboModel
    {
        public List<ComboItem> Combos { get; set; }

        public ComboModel(List<ComboItem> combos)
        {
            this.Combos = combos;
        }
    }
}
