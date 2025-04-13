using MediatR;
using Microsoft.Data.Sqlite;
using Questao5.Infrastructure.Repositories;
using Questao5.Infrastructure.Repositories.Interfaces;
using Questao5.Infrastructure.Services;
using Questao5.Infrastructure.Sqlite;

using System.Data;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
  .AddJsonOptions(options =>
   {
       options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
   });

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

// sqlite
builder.Services.AddSingleton(new DatabaseConfig { Name = builder.Configuration.GetValue<string>("DatabaseName", "Data Source=database.sqlite") });
builder.Services.AddSingleton<IDatabaseBootstrap, DatabaseBootstrap>();
builder.Services.AddSingleton<IContaCorrenteService, ContaCorrenteService>();
builder.Services.AddSingleton<IContaCorrenteDapperRepository, ContaCorrenteDapperRepository>();
builder.Services.AddSingleton<IMovimentoDapperRepository, MovimentoDapperRepository>();
builder.Services.AddSingleton<IIdempotenciaRepository, IdempotenciaRepository>();

builder.Services.AddSingleton<IDbConnection>(provider =>
{
    var connection = new SqliteConnection(builder.Configuration.GetValue<string>("DatabaseName", "Data Source=database.sqlite"));
    connection.Open();
    return connection;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// sqlite
#pragma warning disable CS8602 // Dereference of a possibly null reference.
app.Services.GetService<IDatabaseBootstrap>().Setup();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

app.Run();

// Informações úteis:
// Tipos do Sqlite - https://www.sqlite.org/datatype3.html


