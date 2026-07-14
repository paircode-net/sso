using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using BAYSOFT.Abstractions.Core.Domain.Entities;

namespace BAYSOFT.Web.Blazor.Models
{
	public class Table<TEntity, TKey> 
		where TEntity : DomainEntity<TKey>
		where TKey : IEquatable<TKey>
	{
        public string Title { get; set; }
		public string RootEndPoint { get; set; }
		public string CollectionEndPoint { get; set; }
		public MudTable<TEntity> MudTable { get; set; }
		public HashSet<TEntity> SelectedItems { get; set; }
		public List<TableHeader<TEntity>> Headers { get; set; }
		public List<TableAction> Actions { get; set; }
		public bool FixedHeader { get; set; } = true;
		public bool FixedFooter { get; set; } = true;
		public bool MultiSelection { get; set; } = true;
		public bool SelectOnRowClick { get; set; } = true;
		public bool Dense { get; set; } = false;
		public bool Hover { get; set; } = true;
		public bool Striped { get; set; } = false;
		public bool Bordered { get; set; } = false;
		public int Elevation { get; set; } = 1;
		public int[] PageSizeOptions { get; set; } = new int[] { 10, 25, 50, 100, int.MaxValue };
		public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Right;
		public bool HidePagination { get; set; }
		public bool HideRowsPerPage { get; set; }
		public bool HidePageNumber { get; set; }
		public string RowsPerPageString { get; set; } = "Rows per page:";
		public string InfoFormat { get; set; } = "{first_item}-{last_item} of {all_items}";
		public string AllItemsText { get; set; } = "All";
        public string CollectionEndPointCreate() => $"{CollectionEndPoint}/Create";
		public string CollectionEndPointEdit(TKey id) => $"{CollectionEndPoint}/{id}";
		public Table()
        {
			Headers = new List<TableHeader<TEntity>>();
			SelectedItems = new HashSet<TEntity>();
			Actions = new List<TableAction>();
		}
    }
}
