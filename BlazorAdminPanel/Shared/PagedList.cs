namespace Stargazer.API.Client.Infrastructure
{
    public class PagedList<T>
    {
        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public List<T> List { get; init; } = new List<T>();

        //still required for deserialization. Fix is WIP in netcore
        public PagedList() { }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            List.AddRange(items);
        }

        public PagedList<TTarget> CastTo<TTarget>(Func<T, TTarget> castFunc)
        {
            return new PagedList<TTarget>
            {
                CurrentPage = this.CurrentPage,
                TotalPages = this.TotalPages,
                PageSize = this.PageSize,
                TotalCount = this.TotalCount,
                List = this.List.Select(castFunc).ToList()
            };
        }
    }
}

