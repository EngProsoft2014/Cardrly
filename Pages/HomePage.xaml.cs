﻿using Cardrly.Controls;
using Cardrly.Helpers;
#if ANDROID
using Cardrly.Platforms.Android;
#elif IOS
using Cardrly.Platforms.iOS;
#endif
using Cardrly.Resources.Lan;
using Cardrly.ViewModels;
using Cardrly.ViewModels.Leads;
using CommunityToolkit.Mvvm.Input;
using Controls.UserDialogs.Maui;
using Mopups.Services;
using System;
using static Cardrly.Models.Calendar.CalendlyResponseModel;
using static Cardrly.Models.Calendar.GmailResponseModel;

namespace Cardrly.Pages;

public partial class HomePage : Controls.CustomControl
{
    #region Prop
    CardsViewModel cardsViewModel;
    HomeViewModel homeViewModel;
    LeadViewModel LeadViewModel;
    CalendarViewModel CalendarViewModel;
    #endregion

    #region Service
    readonly IGenericRepository Rep;
    readonly Services.Data.ServicesService _service;
    #endregion

    public HomePage(HomeViewModel model, IGenericRepository GenericRep, Services.Data.ServicesService service)
    {
        InitializeComponent();
        homeViewModel = model;
        //CardPicker.SelectedIndex = 0;
        Rep = GenericRep;
        _service = service;
        HomeView.BindingContext = model;
        // Add Flow Direction For Content View 
        HomeView.FlowDirection = this.FlowDirection;
        CardsView.FlowDirection = this.FlowDirection;
        LeadView.FlowDirection = this.FlowDirection;
        CalendarView.FlowDirection = this.FlowDirection;
        MoreView.FlowDirection = this.FlowDirection;

        //#if ANDROID || IOS
        //        MessagingCenter.Subscribe<NotificationManagerService, bool>(this, "NoifcationClicked", async (sender, message) =>
        //        {
        //            if (true)
        //            {
        //                tabHome.SelectedIndex = 2;
        //            }
        //        });
        //#endif

        //tabHome.SelectedIndex = StaticMember.TabIndex;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //string num = Preferences.Default.Get("NotificationLead", "0");
        //if(int.Parse(num) == 2)
        //{
        //    StaticMember.TabIndex = 2;
        //}

        //Preferences.Default.Remove("NotificationTabIndex"); // Clear the stored value

        //if (StaticMember.TabIndex != tabHome.SelectedIndex)
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        tabHome.SelectedIndex = StaticMember.TabIndex; // Move to the correct tab
        //        if (StaticMember.TabIndex == 2)
        //        {
        //            LeadView.BindingContext = LeadViewModel = new LeadViewModel(Rep, _service, homeViewModel._audioManager);
        //        }
        //    });
        //}
    }

    //private bool isHandlingSelectionChange = false;

    #region Methods
    private void SfTabView_SelectionChanged(object sender, Syncfusion.Maui.TabView.TabSelectionChangedEventArgs e)
    {
        //if (isHandlingSelectionChange)
        //    return; // 🔹 Prevent re-entering the method when changing index manually

        //isHandlingSelectionChange = true; // 🔹 Start handling selection

        if (e.NewIndex == 0)
        {
            HomeView.BindingContext = homeViewModel;
        }
        else if (e.NewIndex == 1)
        {
            if (homeViewModel.IsEnable == true)
            {
                CardsView.BindingContext = cardsViewModel = new CardsViewModel(Rep, _service);
            }
            else
            {
                tabHome.SelectedIndex = 0;
            }
        }
        else if (e.NewIndex == 2)
        {
            if (homeViewModel.IsEnable == true)
            {
                LeadView.BindingContext = LeadViewModel = new LeadViewModel(Rep, _service, homeViewModel._audioManager);
            }
            else
            {
                tabHome.SelectedIndex = 0;
            }
        }
        else if (e.NewIndex == 3)
        {
            if (homeViewModel.IsEnable == true)
            {
                CalendarView.BindingContext = CalendarViewModel = new CalendarViewModel(Rep, _service);
            }
            else
            {
                tabHome.SelectedIndex = 0;
            }
        }
        else if (e.NewIndex == 4)
        {
            if (homeViewModel.IsEnable == true)
            {
                MoreView.BindingContext = new MoreViewModel(Rep, _service, homeViewModel._audioManager);
            }
            else
            {
                tabHome.SelectedIndex = 0;
            }
        }

    }

    protected override bool OnBackButtonPressed()
    {
        // Run the async code on the UI thread
        Dispatcher.Dispatch(() =>
        {
            Action action = () => Application.Current!.Quit();
            Controls.StaticMember.ShowSnackBar($"{AppResources.msgDoYouWantToLogout}", Controls.StaticMember.SnackBarColor, Controls.StaticMember.SnackBarTextColor, action);
        });

        // Return true to prevent the default behavior
        return true;
    }

    private void Cards_Refreshing(object sender, EventArgs e)
    {
        RefView.IsRefreshing = true;
        cardsViewModel.Init();
        RefView.IsRefreshing = false;
    }

    private async void Lead_Refreshing(object sender, EventArgs e)
    {
        LeadRef.IsRefreshing = true;
        LeadViewModel.FilterRequest.PageNumber = 1;
        await LeadViewModel.SearchLeads();
        LeadRef.IsRefreshing = false;
    }

    private async void SearchBar_Lead(object sender, TextChangedEventArgs e)
    {
        LeadViewModel.FilterRequest.SearchLead = e.NewTextValue;
        LeadViewModel.FilterRequest.PageNumber = 1;
        await LeadViewModel.SearchLeads();
    }

    private async void HomeRef_Refreshing(object sender, EventArgs e)
    {
        HomeRef.IsRefreshing = true;
        await homeViewModel.GetAccData();
        await homeViewModel.GetAllCards();
        HomeRef.IsRefreshing = false;
    }

    private void CalFromData_DateSelected(object sender, DateChangedEventArgs e)
    {
        CalendarViewModel.CalendlyResponses = new System.Collections.ObjectModel.ObservableCollection<CalendarCalendlyResponse>();
        CalendarViewModel.CalendarEventGmails = new System.Collections.ObjectModel.ObservableCollection<CalendarEventGmail>();
        CalendarViewModel.CalendarOutlookEvents = new System.Collections.ObjectModel.ObservableCollection<Models.Calendar.OutLookResponseModel.CalendarOutlookEvent>();
    }

    private void CalenderCardPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        CalendarViewModel.CalendlyResponses = new System.Collections.ObjectModel.ObservableCollection<CalendarCalendlyResponse>();
        CalendarViewModel.CalendarEventGmails = new System.Collections.ObjectModel.ObservableCollection<CalendarEventGmail>();
        CalendarViewModel.CalendarOutlookEvents = new System.Collections.ObjectModel.ObservableCollection<Models.Calendar.OutLookResponseModel.CalendarOutlookEvent>();
    }
    #endregion

    private void DatePicker_Home(object sender, DateChangedEventArgs e)
    {
        homeViewModel.IsCheckOrGo = 2;

        StartButtonAnimation();
    }

    private void CardPicker_Home(object sender, EventArgs e)
    {
        homeViewModel.IsCheckOrGo = 2;

        StartButtonAnimation();
    }

    private async void StartButtonAnimation()
    {
        homeViewModel.IsAnimating = true;
        await Task.Run(async () =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                LoadIcon.TranslationY = 100;
                LoadIcon.Opacity = 0;

                await Task.WhenAll(
                    LoadIcon.TranslateTo(0, 0, 700, Easing.BounceOut), // Move up with bounce effect
                    LoadIcon.FadeTo(1, 700, Easing.CubicOut)           // Fade in smoothly
                );

                await Task.Delay(100);

                LoadIcon.TranslationY = -100;
                LoadIcon.Opacity = 0;

                await Task.WhenAll(
                    LoadIcon.TranslateTo(0, 0, 700, Easing.BounceOut), // Move up with bounce effect
                    LoadIcon.FadeTo(1, 700, Easing.CubicOut)           // Fade in smoothly
                );

            });
        });
    }
}