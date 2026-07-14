using MudBlazor;

namespace BAYSOFT.Web.Blazor.Models
{
	public class TableAction
	{
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public Color Color { get; set; }
        public Func<int, bool> Show { get; set; }
        public Action Action { get; set; }
        public TableAction(string name, string displayName, string icon, Color color, Func<int, bool> show, Action action)
        {
            Name = name;
            DisplayName = displayName;
            Icon = icon;
            Color = color;
            Show = show;
            Action = action;
        }
    }
}
