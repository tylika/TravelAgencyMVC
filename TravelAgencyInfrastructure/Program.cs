// ����: TravelAgencyInfrastructure/Program.cs
using TravelAgencyInfrastructure; // Namespace, �� ����������� ��� TravelAgencyDbContext
using TravelAgencyDomain.Model;   // Namespace, �� ����������� ���� �����
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // ��� MVC

// ��������� DbContext
builder.Services.AddDbContext<TravelAgencyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")

));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) // � ����� �������� ����� ������ �������� �������
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // ������ �������� ������� ������� ��� ����������
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ��� CSS, JS, ��������� � wwwroot

app.UseRouting();

app.UseAuthorization(); // ���� ������ �������� ��������������/�����������

// ������� �� ������������� ��� MVC ����������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// app.MapStaticAssets(); // �� ���� ���� ���������� ��� ������ �������, ���� ������������� Blazor ��� ���� ������. ��� ������� MVC �������� �� �������.
// .WithStaticAssets(); // ���������

app.Run();