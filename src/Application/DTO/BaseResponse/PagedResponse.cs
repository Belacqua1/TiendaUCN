namespace TiendaUCN.src.Application.DTO.BaseResponse
{
    /// <summary>
    /// Response wrapper for paginated data.
    /// Contains the list of items, pagination metadata, and total count.
    /// </summary>
    /// <typeparam name="T">Type of the items in the response.</typeparam>
    public class PagedResponse<T>
    {
        // List of items for the current page
        public List<T> Items { get; set; }

        // Current page number (1-based)
        public int CurrentPage { get; set; }

        // Number of items per page
        public int PageSize { get; set; }

        // Total number of items across all pages
        public int TotalCount { get; set; }

        // Total number of pages
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Indicates if there is a previous page
        public bool HasPreviousPage => CurrentPage > 1;

        // Indicates if there is a next page
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Creates a new paged response.
        /// </summary>
        /// <param name="items">List of items for the current page</param>
        /// <param name="currentPage">Current page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="totalCount">Total number of items</param>
        public PagedResponse(List<T> items, int currentPage, int pageSize, int totalCount)
        {
            Items = items;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
