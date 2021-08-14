﻿using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using BeatSaberModManager.ViewModels;

using Splat;


namespace BeatSaberModManager.Views
{
    public class ModsView : ReactiveUserControl<ModsViewModel>
    {
        public ModsView()
        {
            AvaloniaXamlLoader.Load(this);
            ViewModel = Locator.Current.GetService<ModsViewModel>();
        }

        protected override async void OnInitialized()
        {
            DataGridCollectionView dataGridCollection = new(ViewModel!.GridItems);
            dataGridCollection.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ModGridItemViewModel.AvailableMod) + "." + nameof(ModGridItemViewModel.AvailableMod.Category)));
            await ViewModel.RefreshDataGridAsync();
            this.FindControl<DataGrid>("ModsDataGrid").Items = dataGridCollection;
        }
    }
}