using System.Windows;
using Pharmacy.Application;
using Pharmacy.Domain;
using Pharmacy.Infrastructure;

namespace Pharmacy.Wpf;

public partial class MainWindow : Window
{
    private readonly PharmacyDbContext _db;
    private readonly MedicineRepository _medicines;
    private readonly MedicineService _service;
    private readonly SaleRepository _sales;

    public MainWindow(PharmacyDbContext db)
    {
        InitializeComponent();
        _db = db;
        _medicines = new MedicineRepository(_db);
        _sales = new SaleRepository(_db);
        _service = new MedicineService(_medicines, _sales);
        Loaded += (_, _) => LoadAll();
        Closed += (_, _) => _db.Dispose();
    }

    private void LoadAll()
    {
        MedicinesGrid.ItemsSource = _service.GetAll();
        StatusText.Text = $"Записей: {_service.GetAll().Count}";
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        var query = SearchBox.Text.Trim();
        var result = _service.Search(query);
        MedicinesGrid.ItemsSource = result;
        StatusText.Text = string.IsNullOrWhiteSpace(query)
            ? $"Записей: {result.Count}"
            : $"Найдено: {result.Count}";
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Clear();
        LoadAll();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new MedicineFormWindow(null) { Owner = this };
        if (dialog.ShowDialog() != true) return;

        try
        {
            _service.Add(dialog.MedicineName, dialog.Category, dialog.Price, dialog.Quantity, dialog.ExpiryDate);
            LoadAll();
            StatusText.Text = "Лекарство добавлено.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (MedicinesGrid.SelectedItem is not Medicine medicine)
        {
            MessageBox.Show("Выберите лекарство.", "Изменение", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new MedicineFormWindow(medicine) { Owner = this };
        if (dialog.ShowDialog() != true) return;

        try
        {
            medicine.Name = dialog.MedicineName;
            medicine.Category = dialog.Category;
            medicine.Price = dialog.Price;
            medicine.Quantity = dialog.Quantity;
            medicine.ExpiryDate = dialog.ExpiryDate;
            medicine.IsActive = dialog.IsActive;
            _service.Update(medicine);
            LoadAll();
            StatusText.Text = "Данные изменены.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Sell_Click(object sender, RoutedEventArgs e)
    {
        if (MedicinesGrid.SelectedItem is not Medicine medicine)
        {
            MessageBox.Show("Выберите лекарство.", "Продажа", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaleWindow(medicine) { Owner = this };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var total = _service.Sell(medicine.Id, dialog.Quantity, DateTime.Today);
            LoadAll();
            MessageBox.Show($"Продажа оформлена. Сумма: {total:F2} руб.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusText.Text = "Продажа сохранена.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка продажи", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}

internal sealed class MedicineFormWindow : Window
{
    private readonly TextBox _name = new() { Width = 280 };
    private readonly TextBox _category = new() { Width = 280 };
    private readonly TextBox _price = new() { Width = 280 };
    private readonly TextBox _quantity = new() { Width = 280 };
    private readonly DatePicker _expiry = new() { Width = 280 };
    private readonly CheckBox _active = new() { Content = "Активен", IsChecked = true };

    public string MedicineName => _name.Text.Trim();
    public string Category => _category.Text.Trim();
    public decimal Price => decimal.Parse(_price.Text.Trim());
    public int Quantity => int.Parse(_quantity.Text.Trim());
    public DateTime ExpiryDate => _expiry.SelectedDate ?? DateTime.Today;
    public bool IsActive => _active.IsChecked == true;

    public MedicineFormWindow(Medicine? medicine)
    {
        Title = medicine is null ? "Добавление лекарства" : "Редактирование лекарства";
        Width = 360;
        Height = 360;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        _name.Text = medicine?.Name ?? string.Empty;
        _category.Text = medicine?.Category ?? string.Empty;
        _price.Text = medicine?.Price.ToString("F2") ?? "";
        _quantity.Text = medicine?.Quantity.ToString() ?? "";
        _expiry.SelectedDate = medicine?.ExpiryDate ?? DateTime.Today.AddYears(1);
        _active.IsChecked = medicine?.IsActive ?? true;

        var save = new Button { Content = "Сохранить", Width = 110, Margin = new Thickness(0, 15, 10, 0) };
        var cancel = new Button { Content = "Отмена", Width = 110, Margin = new Thickness(0, 15, 0, 0) };
        save.Click += (_, _) =>
        {
            if (!decimal.TryParse(_price.Text, out _) || !int.TryParse(_quantity.Text, out _))
            {
                MessageBox.Show("Цена и остаток должны быть числами.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        };
        cancel.Click += (_, _) => DialogResult = false;

        var panel = new StackPanel { Margin = new Thickness(20) };
        AddField(panel, "Название", _name);
        AddField(panel, "Категория", _category);
        AddField(panel, "Цена", _price);
        AddField(panel, "Остаток", _quantity);
        AddField(panel, "Срок годности", _expiry);
        panel.Children.Add(_active);
        panel.Children.Add(new StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            Children = { save, cancel }
        });
        Content = panel;
    }

    private static void AddField(StackPanel panel, string title, UIElement input)
    {
        panel.Children.Add(new TextBlock { Text = title, Margin = new Thickness(0, 5, 0, 3) });
        panel.Children.Add(input);
    }
}

internal sealed class SaleWindow : Window
{
    private readonly TextBox _quantity = new() { Width = 180 };
    public int Quantity => int.Parse(_quantity.Text.Trim());

    public SaleWindow(Medicine medicine)
    {
        Title = "Продажа лекарства";
        Width = 300;
        Height = 200;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var save = new Button { Content = "Продать", Width = 100, Margin = new Thickness(0, 15, 10, 0) };
        var cancel = new Button { Content = "Отмена", Width = 100, Margin = new Thickness(0, 15, 0, 0) };
        save.Click += (_, _) =>
        {
            if (!int.TryParse(_quantity.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Количество продажи должно быть положительным целым числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (quantity > medicine.Quantity)
            {
                MessageBox.Show("Недостаточно товара на складе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        };
        cancel.Click += (_, _) => DialogResult = false;

        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(new TextBlock { Text = $"Препарат: {medicine.Name}" });
        panel.Children.Add(new TextBlock { Text = $"На складе: {medicine.Quantity}", Margin = new Thickness(0, 5, 0, 0) });
        panel.Children.Add(new TextBlock { Text = "Количество", Margin = new Thickness(0, 10, 0, 3) });
        panel.Children.Add(_quantity);
        panel.Children.Add(new StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            Children = { save, cancel }
        });
        Content = panel;
    }
}
