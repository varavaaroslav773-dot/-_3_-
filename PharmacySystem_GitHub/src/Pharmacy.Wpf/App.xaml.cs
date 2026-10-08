using System.Windows;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Domain;
using Pharmacy.Infrastructure;

namespace Pharmacy.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var options = new DbContextOptionsBuilder<PharmacyDbContext>()
            .UseSqlite("Data Source=pharmacy.db")
            .Options;

        var db = new PharmacyDbContext(options);
        db.Database.EnsureCreated();
        Seed(db);

        MainWindow = new MainWindow(db);
        MainWindow.Show();
    }

    private static void Seed(PharmacyDbContext db)
    {
        if (db.Medicines.Any()) return;

        db.Medicines.AddRange(
            new Medicine { Name = "Парацетамол", Category = "Обезболивающие", Price = 120m, Quantity = 10, ExpiryDate = DateTime.Today.AddYears(1), IsActive = true },
            new Medicine { Name = "Ибупрофен", Category = "Обезболивающие", Price = 99m, Quantity = 8, ExpiryDate = DateTime.Today.AddYears(1), IsActive = true },
            new Medicine { Name = "Но-шпа", Category = "Спазмолитики", Price = 200m, Quantity = 7, ExpiryDate = DateTime.Today.AddYears(1), IsActive = true },
            new Medicine { Name = "Витамин C", Category = "Витамины", Price = 180m, Quantity = 5, ExpiryDate = DateTime.Today.AddYears(1), IsActive = true });
        db.SaveChanges();
    }
}
