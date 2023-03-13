using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
//using Stargazer.API.Client.Dto.External;
using Stargazer.Web.UI.Utils;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Stargazer.Web.UI.Components
{
    public class CustomComponentBase : ComponentBase, IHandleEvent, IHandleAfterRender
    {
        private bool hasCalledOnAfterRender;

        [Inject]
        private IServiceProvider Services { get; set; } = default!;

        [Inject]
        private BlazorServiceAccessor BlazorServiceAccessor { get; set; } = default!;

        public override Task SetParametersAsync(ParameterView parameters)
            => InvokeWithBlazorServiceContext(() => base.SetParametersAsync(parameters));

        Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
            => InvokeWithBlazorServiceContext(() =>
            {
                var task = callback.InvokeAsync(arg);
                var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion &&
                    task.Status != TaskStatus.Canceled;

                StateHasChanged();

                return shouldAwaitTask ?
                    CallStateHasChangedOnAsyncCompletion(task) :
                    Task.CompletedTask;
            });

        Task IHandleAfterRender.OnAfterRenderAsync()
            => InvokeWithBlazorServiceContext(() =>
            {
                var firstRender = !hasCalledOnAfterRender;
                hasCalledOnAfterRender |= true;

                OnAfterRender(firstRender);

                return OnAfterRenderAsync(firstRender);
            });

        private async Task CallStateHasChangedOnAsyncCompletion(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
                if (task.IsCanceled)
                {
                    return;
                }

                throw;
            }

            StateHasChanged();
        }

        private async Task InvokeWithBlazorServiceContext(Func<Task> func)
        {
            try
            {
                BlazorServiceAccessor.Services = Services;
                await func();
            }
            finally
            {
                BlazorServiceAccessor.Services = null;
            }
        }
    }
}