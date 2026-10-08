using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application;
using Pharmacy.Domain;
using Pharmacy.Infrastructure;
using Xunit;

namespace Pharmacy.IntegrationTests;

public sealed class PharmacyIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PharmacyDbContext _db;

    public PharmacyIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<PharmacyDbContext>().UseSqlite(_connection).Options;
        _db = new PharmacyDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public void AddMedicine_IsPersistedInSqlite()
    {
        var repository = new MedicineRepository(_db);
        var sales = new SaleRepository(_db);
        var service = new MedicineService(repository, sales);
        var created = service.Add("Парацетамол", "Обезболивающие", 120m, 10, DateTime.Today.AddYears(1));
        Assert.NotEqual(0, created.Id);
        Assert.Equal("Парацетамол", _db.Medicines.Single().Name);
    }

    [Fact]
    public void Search_ReturnsMatchingMedicine()
    {
        var repository = new MedicineRepository(_db);
        var sales = new SaleRepository(_db);
        var service = new MedicineService(repository, sales);
        service.Add("Ибупрофен", "Обезболивающие", 99m, 8, DateTime.Today.AddYears(1));
        service.Add("Аскорутин", "Витамины", 80m, 12, DateTime.Today.AddYears(1));
        var result = service.Search("ибу");
        Assert.Single(result);
        Assert.Equal("Ибупрофен", result[0].Name);
    }

    [Fact]
    public void Sale_UpdatesStockInDatabase()
    {
        var repository = new MedicineRepository(_db);
        var sales = new SaleRepository(_db);
        var service = new MedicineService(repository, sales);
        var medicine = service.Add("Но-шпа", "Спазмолитики", 200m, 7, DateTime.Today.AddYears(1));
        service.Sell(medicine.Id, 2, DateTime.Today);
        Assert.Equal(5, _db.Medicines.Single(x => x.Id == medicine.Id).Quantity);
    }

    [Fact]
    public void Sale_CreatesTransactionRecord()
    {
        var repository = new MedicineRepository(_db);
        var sales = new SaleRepository(_db);
        var service = new MedicineService(repository, sales);
        var medicine = service.Add("Магний B6", "Витамины", 400m, 5, DateTime.Today.AddYears(1));
        service.Sell(medicine.Id, 1, DateTime.Today);
        var sale = _db.Sales.Single();
        Assert.Equal(medicine.Id, sale.MedicineId);
        Assert.Equal(400m, sale.Total);
    }

    [Fact]
    public void DuplicateMedicineName_IsRejectedByService()
    {
        var repository = new MedicineRepository(_db);
        var sales = new SaleRepository(_db);
        var service = new MedicineService(repository, sales);
        service.Add("Цитрамон", "Обезболивающие", 70m, 5, DateTime.Today.AddYears(1));
        Assert.Throws<ArgumentException>(() => service.Add("Цитрамон", "Обезболивающие", 80m, 3, DateTime.Today.AddYears(1)));
        Assert.Single(_db.Medicines);
    }

    [Fact]
    public void LowStock_CanBeDetectedAfterSale()
    {
        var repository = new MedicineRepository(_db);
        var sales = new SaleRepository(_db);
        var service = new MedicineService(repository, sales);
        var medicine = service.Add("Валериана", "Успокоительные", 50m, 6, DateTime.Today.AddYears(1));
        service.Sell(medicine.Id, 2, DateTime.Today);
        var actual = _db.Medicines.Single(x => x.Id == medicine.Id);
        Assert.True(service.IsLowStock(actual));
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
