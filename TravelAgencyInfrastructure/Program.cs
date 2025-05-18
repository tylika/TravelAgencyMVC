// Файл: TravelAgencyInfrastructure/Program.cs
using TravelAgencyInfrastructure; // Namespace, де знаходиться ваш TravelAgencyDbContext
using TravelAgencyDomain.Model;   // Namespace, де знаходяться ваші моделі
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Для MVC

// Реєстрація DbContext
builder.Services.AddDbContext<TravelAgencyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")

));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) // У режимі розробки краще бачити детальну помилку
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // Показує детальну сторінку помилки для розробника
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Для CSS, JS, зображень у wwwroot

app.UseRouting();

app.UseAuthorization(); // Якщо будете додавати автентифікацію/авторизацію

// Маршрут за замовчуванням для MVC контролерів
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// app.MapStaticAssets(); // Це може бути специфічне для вашого шаблону, якщо використовуєте Blazor або щось подібне. Для чистого MVC зазвичай не потрібно.
// .WithStaticAssets(); // Аналогічно

app.Run();