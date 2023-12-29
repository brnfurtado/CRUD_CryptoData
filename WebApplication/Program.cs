using System;
using Microsoft.EntityFrameworkCore;
using Swashbuckle;
using WebApplication1;
using DomainDefinitions.Data;
using DomainDefinitions;
using DomainDefinitions.Interfaces;
using DomainDefinitions.CryptoDataSubscriber.Implementations;
using DomainDefinitions.Enums;
using LocalTester;
using Microsoft.Extensions.DependencyInjection;
using DomainDefinitions.AuxStaticCode;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.InMemory;

var builder = WebApplication.CreateBuilder(args);

//Default
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InMemoryDatabase"));


// This is commented for application simplicity, using InMemoryDatabase for all cases is easier since it's just for testing.
// If the user wants another DB, just uncomment both lines below
//if (builder.Environment.IsDevelopment())
//{
// Use an in-memory database for development
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseInMemoryDatabase("InMemoryDatabase"));
//}

//else
//{
//    // Use a real database in other environments
//    var connectionString = builder.Configuration.GetConnectionString("AppDbConnectionString");
//    builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), b => b.MigrationsAssembly("CoreApplication")));
//}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();



AuxStaticCode.cryptoDataSourceDBSubscriber = await CryptoDataSourceDBSubscriber.CreateCryptoDataSourceDBSubscriber(app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>());


//Subscribing the active subscriptions in the database, if there's any.
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    using (var dbContext = serviceProvider.GetRequiredService<AppDbContext>())
    {
        dbContext.Database.EnsureCreated();

        var subscriptions = await dbContext.subscriptions.ToListAsync();

        foreach (var subscription in subscriptions)
        {
            AuxStaticCode.RegisteredIDs.Add(subscription.ID);
            ICryptoDataSource cryptoDataSource = await AuxStaticCode.GetDataSourceByName(subscription.SourceName);
            await cryptoDataSource.SubscribeCryptoData(subscription.CryptoPair, (EFrequency)subscription.FrequencyMS, AuxStaticCode.cryptoDataSourceDBSubscriber);
        }
    }
}


app.Run();
