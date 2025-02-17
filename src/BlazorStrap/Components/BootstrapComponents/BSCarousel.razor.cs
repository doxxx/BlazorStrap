﻿using BlazorComponentUtilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Timers;
using BlazorStrap.Utilities;

namespace BlazorStrap
{
    public partial class BSCarousel : BlazorStrapBase, IDisposable
    {
        private DotNetObjectReference<BSCarousel> _objectRef;
        [Parameter] public bool HasControls { get; set; }
        [Parameter] public bool HasIndicators { get; set; }
        [Parameter] public bool IsDark { get; set; }
        [Parameter] public bool IsFade { get; set; }
        [Parameter] public bool IsSlide { get; set; } = true;

        private int _active;
        private InternalComponents.Indicators? _indicatorsRef;
        private int _last;
        private System.Timers.Timer? _transitionTimer;
        private bool ClickLocked { get; set; }
        
        private List<BSCarouselItem> Callback { get; set; } = new List<BSCarouselItem>();

        private List<BSCarouselItem> Children { get; set; } = new List<BSCarouselItem>();

        // private Func<Task>? Callback { get; set; }
        private string? ClassBuilder => new CssBuilder("carousel")
            .AddClass("slide", IsSlide)
            .AddClass("carousel-fade", IsFade)
            .AddClass("carousel-dark", IsDark)
            .AddClass(LayoutClass, !string.IsNullOrEmpty(LayoutClass))
            .AddClass(Class, !string.IsNullOrEmpty(Class))
            .Build().ToNullString();

        protected override void OnInitialized()
        {
            _objectRef = DotNetObjectReference.Create<BSCarousel>(this);
        }

        protected override bool ShouldRender()
        {
            return !ClickLocked;
        }

        public Task GotoSlideAsync(int slide)
        {
            if (slide >= Children.Count && slide < 0)
                return Task.CompletedTask;
            return GotoChildSlide(Children[slide]);
        }

        internal Task HideSlide(BSCarouselItem slide)
        {
            return GotoChildSlide(slide == Children.First() ? Children.Last() : Children.First());
        }
        
        [JSInvokable]
        public override async Task InteropEventCallback(string id, CallerName name, EventType type)
        {
            if (DataId == id && name.Equals(this) && type == EventType.TransitionEnd)
            {
                await TransitionEndAsync();
            }
        }
        private async Task TransitionEndAsync()
        {
            ClickLocked = false;
            await Children[_active].Refresh();
            await Children[_last].Refresh();
            
            await Children[_last].Refresh();
            if (Callback.Count > 0)
            {
                await GotoChildSlide(Callback.First());
                Callback.Remove(Callback.First());
            }
            if (Children[_active].OnShown.HasDelegate)
                _ = Task.Run(() => { _ = Children[_active].OnHidden.InvokeAsync(Children[_active]); });
            if (Children[_last].OnHidden.HasDelegate)
                _ = Task.Run(() => { _ = Children[_last].OnHidden.InvokeAsync(Children[_last]); });
        }

        internal async Task GotoChildSlide(BSCarouselItem item)
        {
            _transitionTimer?.Stop();
            var slide = Children.IndexOf(item);
            if (ClickLocked)
            {
                Callback.Add(item);
                return;
            }
            if (!Children.Contains(item)) return;

            var back = slide < _active;
            if (slide == _active) return;
            ClickLocked = true;
            _last = _active;
            _active = slide;
            await Children[_last].InternalHide();
            await Children[_active].InternalShow();
            await DoAnimations(back);
            
            await InvokeAsync(() => { _indicatorsRef?.Refresh(Children.Count, _active); });
            ResetTransitionTimer(Children[_active].Interval);
        }

        internal async Task BackAsync()
        {
            if (ClickLocked) return;
            ClickLocked = true;

            var last = _active;
            _active--;

            if (_active < 0)
                _active = Children.Count - 1;
            if (last == 0)
                _last = last;

            else
                _last = _active + 1;
            await Children[_last].InternalHide();
            await Children[_active].InternalShow();
            await DoAnimations(true);

            await InvokeAsync(() => { _indicatorsRef?.Refresh(Children.Count, _active); });
            ResetTransitionTimer(Children[_active].Interval);
        }

        internal async Task NextAsync()
        {
            if (ClickLocked) return;
            ClickLocked = true;
            _active++;
            if (_active > Children.Count - 1)
                _active = 0;

            if (_active == 0)
                _last = Children.Count - 1;

            else
                _last = _active - 1;
            await Children[_last].InternalHide();
            await Children[_active].InternalShow();

            await DoAnimations(false);


            await InvokeAsync(() => { _indicatorsRef?.Refresh(Children.Count, _active); });
            ResetTransitionTimer(Children[_active].Interval);
        }

        private async Task DoAnimations(bool back)
        {
            if(await BlazorStrap.Interop.AnimateCarouselAsync(_objectRef, DataId, Children[_active].MyRef, Children[_last].MyRef, back))
            {
                ClickLocked = false;
                await Children[_active].Refresh();
                await Children[_last].Refresh();
            }
        }
        internal void AddChild(BSCarouselItem item)
        {
            Children.Add(item);
            if (Children.First() == item)
            {
                item.First();
                if (item.Interval != 0)
                {
                    InitializeTransitionTimer(item.Interval);
                    _transitionTimer?.Start();
                }
            }

            _indicatorsRef?.Refresh(Children.Count, _active);
        }

        internal void RemoveChild(BSCarouselItem item)
        {
            Children.Remove(item);
            _indicatorsRef?.Refresh(Children.Count, _active);
        }

        private async Task PressEvent(KeyboardEventArgs e)
        {
            if (e.Code == "37")
            {
                await BackAsync();
            }
            else if (e.Code == "39")
            {
                await NextAsync();
            }
        }

        private void InitializeTransitionTimer(int interval)
        {
            _transitionTimer = new System.Timers.Timer(interval);
            _transitionTimer.Elapsed += OnTransitionTimerEvent;
            _transitionTimer.AutoReset = false;
        }

        private async void OnTransitionTimerEvent(object? sender, ElapsedEventArgs e)
        {
            await NextAsync();
        }

        private void ResetTransitionTimer(int interval)
        {
            _transitionTimer?.Stop();
            if (interval == 0) return;
            InitializeTransitionTimer(
                interval); // Avoid an System.ObjectDisposedException due to the timer being disposed. This occurs when the Enabled property of the timer is set to false by the call to Stop() above.
            _transitionTimer?.Start();
        }
        
        public void Dispose()
        {
            _objectRef.Dispose();
            _transitionTimer?.Stop();
            _transitionTimer?.Dispose();
        }
    }
}