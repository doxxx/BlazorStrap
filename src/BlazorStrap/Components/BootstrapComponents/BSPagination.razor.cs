using BlazorComponentUtilities;
using BlazorStrap.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStrap
{
    public partial class BSPagination : BlazorStrapBase
    {
        private DotNetObjectReference<BSPagination> _objectRef;
        private string MyId { get; set; }= Guid.NewGuid().ToString();
        [Parameter] public Align Align { get; set; } = Align.Default;
        [Parameter] public EventCallback<int> ValueChanged { get; set; }
        protected int? CurrentValue
        {
            get => Value;
            set
            {
                var hasChanged = !EqualityComparer<int?>.Default.Equals(value, Value);
                if (hasChanged)
                {
                    Value = value;
                    _ = ValueChanged.InvokeAsync(Value ?? 0);
                }
            }
        }

        private int Page => Value ?? 0;
        [Parameter] public BSColor Color { get; set; } = BSColor.Default;
        [Parameter] public Size Size { get; set; } = Size.None;
        [Parameter] public int Pages { get; set; } = -1;
        [Parameter] public int? Value { get; set; } = null;
        private int _pageWidth = 50;
        private int MaxItems { get; set; }= 0;
        private bool _resized;
        private bool _hasRendered;
        private ElementReference NavReference { get; set; }
        private string? ClassBuilder => new CssBuilder("pagination")
          .AddClass($"bg-{Color.NameToLower()}", Color != BSColor.Default)
          .AddClass($"pagination-{Size.ToDescriptionString()}", Size != Size.None)
          .AddClass($"justify-content-{Align.NameToLower()}", Align != Align.Default)
          .AddClass(LayoutClass, !string.IsNullOrEmpty(LayoutClass))
          .AddClass(Class, !string.IsNullOrEmpty(Class))
          .Build().ToNullString();

        protected override void OnInitialized()
        {
            Console.WriteLine(Pages);
            if(Pages > 999)
                _pageWidth = 66;
            if(Pages > 9999)
                _pageWidth = 82;
            if(Pages > 99999)
                _pageWidth = 98;
            
            _objectRef = DotNetObjectReference.Create<BSPagination>(this);
            BlazorStrap.OnEventForward += InteropEventCallback;
        }

        private async Task GetMaxItems()
        {
            var navWidth = await BlazorStrap.Interop.GetWidthAsync(NavReference) - 140;
            var max = ((navWidth / _pageWidth) / 2) ;
            if (MaxItems != max)
            {
                MaxItems = max;    
             
            }
        }

        private void ChangePage(int page)
        {
            Console.WriteLine(Pages);
            if (page < 1) page = 1;
            if (page > Pages) page = Pages;
            CurrentValue = page;
        }
        private int GetPreviousPages()
        {
            var value = Page - MaxItems - GetExtraBefore();
            return value < 1 ? 1 : value;
        }
   
        private int GetMaxAfter()
        {
            // MaxItems + the remaining spaces             
            return MaxItems + (MaxItems - (Page - GetPreviousPages()));
        }

        private int GetExtraBefore()
        {
            var value = (Pages - (Page + MaxItems)) * -1 ;
            return value < 0 ? 0 : value;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Pages != -1)
            {
                var _objectRef = DotNetObjectReference.Create<BSPagination>(this);
                await BlazorStrap.Interop.AddEventAsync(_objectRef, MyId, EventType.Resize);
               // await BlazorStrap.Interop.AddEventAsync(_objectRef, MyId, EventType.Resize);
                await GetMaxItems();
                await InvokeAsync(StateHasChanged);
                _hasRendered = true;
            }
            else if (_resized)
            {
                _resized = false;
                await GetMaxItems();
                await InvokeAsync(StateHasChanged);
            }
        }
        
        [JSInvokable]
        public override async Task InteropEventCallback(string id, CallerName name, EventType type, Dictionary<string, string>? classList, JavascriptEvent? e)
        {
            if (id == MyId && name.Equals(this) && type == EventType.Resize)
            {
                _resized = true;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}