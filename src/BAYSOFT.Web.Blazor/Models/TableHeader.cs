using MudBlazor;

namespace BAYSOFT.Web.Blazor.Models
{
	public class TableHeader<TEntity>
	{
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public SortDirection SortDirection { get; set; }
        public Func<TEntity, string> Value { get; set; }
        public TableHeader(string name, string? displayName = null, SortDirection sortDirection = SortDirection.None, Func<TEntity, string> value = null)
        {
            Name = name;
            DisplayName = !string.IsNullOrWhiteSpace(displayName) ? displayName : name;
            SortDirection = sortDirection;
            Value = value;
        }

        public object? GetValue(TEntity context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (Value != null)
            {
                return Value(context);
            }

            var returnValue = typeof(TEntity).GetProperty(Name)?.GetValue(context);

            return returnValue;
        }
    }
}
