using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
//using Stargazer.API.Client.Extension;
using Stargazer.API.Client.Infrastructure;
using Stargazer.Common.Client.Utils;
using Stargazer.Web.UI.Utils;

namespace Stargazer.Web.UI.Components
{
    public abstract class StargazerGridBase<TItem> : ComponentBase, IDisposable  // CustomComponentBase
        where TItem : class, new()
    {
        protected bool _isLoading;
        protected int _count;
        protected string _query;
        protected RadzenDataGrid<TItem> _grid;
        protected Dictionary<int, CachedPage<TItem>> _cachedPages = new Dictionary<int, CachedPage<TItem>>();
        protected SortDescriptor _currentSortDescriptor;
        private LoadDataArgs _settings;
        private int _currentPageNum;

        protected List<TItem> _items;
        protected TItem _inserting;
        protected TItem _updating;

        protected CancellationTokenSource _cts = new();

        [Parameter]
        public string EntityName { get; set; }
        public IList<TItem> SelectedItems { get; set; }

        [Parameter]
        public EventCallback<IList<TItem>> OnSelected { get; set; }

        [Parameter]
        public string SearchPlaceholder { get; set; }

        [Parameter]
        public string Style { get; set; }

        [Parameter]
        public int? PageSize { get; set; }

        [Parameter]
        public bool AllowSearch { get; set; }

        [Parameter]
        public bool AllowAdd { get; set; }

        [Parameter]
        public bool AllowInline { get; set; }

        [Parameter]
        public bool AllowCaching { get; set; }

        [Parameter]
        public double? AutoRefreshPeriod { get; set; } = null;
        public static readonly double MinAutoRefreshPeriod = double.Parse(Environment.GetEnvironmentVariable("MinAutoRefreshPeriod") ?? "200");
        public double SumElapsedPercent { get; set; }
        private double _sumElapsed;
        protected RadzenProgressBar _autoUpdateBar;

        [Parameter]
        public Func<RowRenderEventArgs<TItem>, Task> RowRenderHandler { get; set; } = null;

        [Parameter]
        public EventCallback<TItem> OnRowDoubleClicked { get; set; }

        [Parameter]
        public Func<string, int, int, Task<PagedList<TItem>>> GetData { get; set; }

        [Parameter]
        public Func<string, int, int, string, Task<PagedList<TItem>>> GetSortableData { get; set; }

        [Parameter]
        public Func<string, int, int, bool, Task<PagedList<TItem>>> GetDeletableData { get; set; }

        [Parameter]
        public bool IncludeDeleted { get; set; }

        [Parameter]
        public Func<Task> AddItem { get; set; }

        [Parameter]
        public Func<TItem, Task> RowSelected { get; set; }

        [Parameter]
        public Func<TItem, Task> RowDeselected { get; set; }

        [Parameter]
        public Func<TItem, Task> CreateItem { get; set; }

        [Parameter]
        public Func<TItem, Task> UpdateItem { get; set; }

        [Parameter]
        public Func<TItem, Task> DeleteItem { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public EventCallback<DataGridCellMouseEventArgs<TItem>> CellContexMenu { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {          
            if (_settings != null)
            {
                _grid.CurrentPage = _currentPageNum;

            }
            base.BuildRenderTree(builder);
        }

        protected override Task OnInitializedAsync()
        {
            if (GetData != null && GetDeletableData != null && GetSortableData != null)
                throw new ArgumentException("Only one GetData should be used! If the data is deletable - use only GetDeletableData Parameter.");

            if (AutoRefreshPeriod != null)
            {
                if (AutoRefreshPeriod < MinAutoRefreshPeriod)
                    throw new ArgumentException($"AutoRefreshPeriod ({AutoRefreshPeriod}) should not be less than MinAutoRefreshPeriod ({MinAutoRefreshPeriod})");

                _ = RunAutoRefresh();
            }

            return Task.CompletedTask;
        }

        // It is extremely important not to use records in RadzenDataGrid!!!
        // https://forum.radzen.com/t/radzen-datagrid-inline-edit-jumps-out-of-edit-mode-when-bound-to-nuget-class/12060/6
        protected async Task LoadData(LoadDataArgs args)
        {
            _settings = args;
            _currentPageNum = _grid.CurrentPage;

            string sortDescriptor = SortDescription.ConvertToStringDescriptor(_currentSortDescriptor); // can be null                                                                                               
            
            _sumElapsed = 0;
            _isLoading = true;

            if (!AllowCaching || !string.IsNullOrEmpty(_query))
            {                
                var response = await GetItems(_query, _grid.CurrentPage + 1, args.Top.Value, sortDescriptor);
                _count = response?.TotalCount ?? 0;
                _items = response?.List ?? new List<TItem>();
            }
            else
            {
                if (!_cachedPages.TryGetValue(_grid.CurrentPage, out var page)
                    || page.ExpiryTime <= UnixTimeHelper.GetCurrentUnixTime())
                {
                    _cachedPages.Remove(_grid.CurrentPage);

                    var response = await GetItems(null, _grid.CurrentPage + 1, args.Top.Value, sortDescriptor);
                    if (response != null)
                    {
                        _count = response.TotalCount;

                        page = new CachedPage<TItem>
                        {
                            Items = response.List,
                            ExpiryTime = UnixTimeHelper.GetUnixTime(DateTime.Now.AddMinutes(5))
                        };

                        _cachedPages.Add(_grid.CurrentPage, page);
                    }
                }

                _items = page?.Items ?? new List<TItem>();
                _count = _count == 0 ? _items.Count : _count;
            }
            
            _isLoading = false;
        }

        protected void OnPage(PagerEventArgs args)
        {
            _grid.CurrentPage = args.PageIndex;
        }
        protected async Task OnSort(DataGridColumnSortEventArgs<TItem> args)
        {
            if (_currentSortDescriptor != null)
            {
                if (_currentSortDescriptor.Property == args.Column.Property)
                {
                    if (_currentSortDescriptor.SortOrder == null) _currentSortDescriptor.SortOrder = SortOrder.Ascending;
                    else if (_currentSortDescriptor.SortOrder == SortOrder.Ascending) _currentSortDescriptor.SortOrder = SortOrder.Descending;
                    else if (_currentSortDescriptor.SortOrder == SortOrder.Descending) _currentSortDescriptor.SortOrder = null;
                    return;
                }
            }
            _currentSortDescriptor = new SortDescriptor()
            {
                Property = args.Column.Property,
                SortOrder = args.SortOrder,
            };
        }

        private async Task<PagedList<TItem>> GetItems(string query, int page, int pageSize, string sortDescriptor = null)
        {
            if (GetSortableData != null)
                return await GetSortableData(query, page, pageSize, sortDescriptor);//.OnNotFound(new PagedList<TItem>());

            if (GetDeletableData != null)
                return await GetDeletableData(query, page, pageSize, IncludeDeleted);//.OnNotFound(new PagedList<TItem>());

            if (GetData != null)
                return await GetData(query, page, pageSize);//.OnNotFound(new PagedList<TItem>());

            return new PagedList<TItem>();
        }

        protected async Task InsertRow()
        {
            if (AddItem != null)
            {
                await AddItem();
            }

            if (!AllowInline) return;

            _inserting = new TItem();
            await _grid.InsertRow(_inserting);
            _count++;
        }

        private async Task RunAutoRefresh()
        {
            try
            {
                _sumElapsed = 0;
                while (!_cts.IsCancellationRequested)
                {
                    await Task.Delay(30);
                    _sumElapsed += 30;
                    SumElapsedPercent = _sumElapsed / AutoRefreshPeriod.Value * 100;

                    if (_sumElapsed > AutoRefreshPeriod.Value)
                    {
                        await Reload();
                        _sumElapsed = 0;
                        SumElapsedPercent = 0;
                    }

                    this.StateHasChanged();
                }
            }
            catch (Exception ex) //idk if this ever happens
            { }
        }

        protected async Task OnSearch()
        {
            await _grid.Reload();
        }

        protected async Task OnClearSearch()
        {
            _query = string.Empty;
            await _grid.Reload();
        }

        protected virtual async Task RenderRow(RowRenderEventArgs<TItem> args)
        {
            RowRenderHandler?.Invoke(args);
        }

        protected async Task OnRowSelected(TItem item)
        {
            if (RowSelected == null)
            {
                return;
            }

            await RowSelected(item);
        }

        protected async Task OnRowDeselected(TItem item)
        {
            if (RowDeselected == null)
            {
                return;
            }

            await RowDeselected(item);
        }

        protected async Task OnCreateRow(TItem item)
        {
            if (AllowInline && CreateItem == null)
            {
                throw new NullReferenceException($"CreateItem function must be set if AllowInline = true");
            }

            await CreateItem(item);
            _inserting = null;
        }

        protected async Task EditRow(TItem item)
        {
            if (!AllowInline) throw new InvalidOperationException();

            _updating = item;
            await _grid.EditRow(item);
        }

        protected async Task OnUpdateRow(TItem item)
        {
            if (AllowInline && UpdateItem == null)
            {
                throw new NullReferenceException($"UpdateItem function must be set if AllowInline = true");
            }

            await UpdateItem(item);

            if (item == _inserting)
            {
                _inserting = null;
            }

            _updating = null;
        }

        protected async Task SaveRow(TItem item)
        {
            await _grid.UpdateRow(item);
        }

        protected void CancelEdit(TItem client)
        {
            if (client == _inserting)
            {
                _inserting = null;
            }

            _updating = null;

            _grid.CancelEditRow(client);
        }

        protected async Task DeleteRow(TItem item)
        {
            if (AllowInline && DeleteItem == null)
            {
                throw new NullReferenceException($"DeleteItem function must be set if AllowInline = true");
            }

            if (item == _inserting)
            {
                _inserting = null;
            }

            if (item == _updating)
            {
                _updating = null;
            }

            if (_items.Contains(item))
            {
                await DeleteItem(item);
                _items.Remove(item);
            }
            else
            {
                _grid.CancelEditRow(item);
            }

            _count--;
            await _grid.Reload();
        }

        public async Task OnKeyPress(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await OnSearch();
            }
        }

        public Task Reload()
        {
            return _grid.Reload();
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
