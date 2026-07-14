using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Core.Domain.Resources;
using BAYSOFT.Web.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace BAYSOFT.Web.Blazor.Components.Shared
{
	public abstract class TableBase<TEntity, TKey> : ComponentBase
		where TEntity : DomainEntity<TKey>
		where TKey: IEquatable<TKey>
	{
		[Inject] protected NavigationManager Navigation { get; set; }
		[Inject] protected IStringLocalizer<Messages> Localizer { get; set; }
		public TableBase() { }
		[Parameter] public string RootEndPoint { get; set; }
		[Parameter] public int? VisibleRows { get; set; }
		[Parameter] public int Elevation { get; set; } = 1;

		[Parameter] public bool? AllowActions { get; set; } = true;
		[Parameter] public bool? AllowAdd { get; set; } = true; 
		[Parameter] public bool? AllowEdit { get; set; } = true;
		[Parameter] public bool? AllowDelete { get; set; } = true;

		protected string Search { get; set; }
		protected Table<TEntity, TKey> TableModel { get; set; }
		protected void InitializeConfigurations(string title, string collectionEndPoint)
		{
			TableModel = new() { Title = title, RootEndPoint = RootEndPoint, CollectionEndPoint = collectionEndPoint };
			InitializeConfigurations();
		}

		private void InitializeConfigurations()
		{
			TableModel.Elevation = Elevation;

			AddColumns();

			if (AllowActions.HasValue && AllowActions.Value)
				AddActions();
		}

		protected virtual void AddColumns() { }
		protected virtual void AddActions()
		{
			if (AllowEdit.HasValue && AllowEdit.Value)
			{
				TableModel.Actions.Add(new("Add", Localizer["Add"], Icons.Material.Filled.Add, Color.Surface, (items) => items == 0, () => Add()));
				TableModel.Actions.Add(new("Edit", Localizer["Edit"], Icons.Material.Filled.Edit, Color.Surface, (items) => items == 1, () => Edit()));
				TableModel.Actions.Add(new("Delete", Localizer["Delete"], Icons.Material.Filled.Delete, Color.Error, (items) => items > 0, async () => await Delete()));
			}
		}
		protected virtual void OnSearch(string text)
		{
			Search = text;
			TableModel.MudTable.ReloadServerData();
		}
		protected virtual void Add()
		{
			Navigation.NavigateTo(TableModel.CollectionEndPointCreate());
		}
		protected virtual void Edit()
		{
			var firstSelectedItem = TableModel.SelectedItems.ToList().FirstOrDefault();

			if (firstSelectedItem != null)
				Navigation.NavigateTo(TableModel.CollectionEndPointEdit(firstSelectedItem.Id));
		}
		protected abstract Task Delete();
	}
}
