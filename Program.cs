using System.Data;
using MassMailApp.Data;
using MassMailApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<MassMailDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=massmail.db"));
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MassMailDbContext>();
    db.Database.EnsureCreated();

    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""EmailTemplates"" (
            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_EmailTemplates"" PRIMARY KEY AUTOINCREMENT,
            ""Name"" TEXT NOT NULL,
            ""Subject"" TEXT NOT NULL,
            ""Body"" TEXT NOT NULL,
            ""Description"" TEXT NOT NULL,
            ""CreatedAt"" TEXT NOT NULL
        );
    ");

    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""ContactGroups"" (
            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ContactGroups"" PRIMARY KEY AUTOINCREMENT,
            ""Name"" TEXT NOT NULL,
            ""Description"" TEXT NOT NULL,
            ""CreatedAt"" TEXT NOT NULL
        );
    ");

    db.Database.ExecuteSqlRaw(@"
        PRAGMA table_info('Contacts');
    ");

    var tableInfo = db.Database.SqlQueryRaw<string>(@"
        SELECT name FROM pragma_table_info('Contacts');
    ").ToList();

    if (!tableInfo.Contains("ContactGroupId"))
    {
        db.Database.ExecuteSqlRaw(@"
            ALTER TABLE ""Contacts"" ADD COLUMN ""ContactGroupId"" INTEGER NULL;
        ");
    }
}

app.Run();
