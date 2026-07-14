using BAYSOFT.Core.Application.Default.Samples.Queries;
using ModelWrapper.Extensions.Filter;
using ModelWrapper.Extensions.Ordination;
using ModelWrapper.Extensions.Pagination;
using ModelWrapper.Extensions.Search;

namespace BAYSOFT.Web.Blazor.Components.Pages.Default.Samples.Filters
{
	public static class SamplesFilterHelper
	{
		public static void ApplyOrdination(this GetSamplesByFilterQuery query, string order, string? orderBy)
		{
			if (!string.IsNullOrWhiteSpace(orderBy))
				query.SetOrdination(order, orderBy);
		}
		public static void ApplyPagination(this GetSamplesByFilterQuery query, int pageSize, int pageNumber)
		{
			query.SetPagination(pageSize, pageNumber);
		}
		public static void ApplySearch(this GetSamplesByFilterQuery query, string search)
		{
			query.ClearSearch();
			if (!string.IsNullOrWhiteSpace(search))
			{
				query.SetSearch(search);
			}
		}
		public static void ApplyFilters(this GetSamplesByFilterQuery query)
		{
			query.ClearFilters();
		}
	}
}