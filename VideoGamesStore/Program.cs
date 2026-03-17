using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using VideoGamesStore.Models;
using VideoGamesStore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Поле обязательно для заполнения.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, fieldName) => $"Значение «{value}» недопустимо для поля «{fieldName}».");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"Значение «{value}» недопустимо.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(fieldName => $"Укажите корректное значение для поля «{fieldName}».");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(value => $"Значение «{value}» недопустимо.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(fieldName => $"Поле «{fieldName}» должно быть числом.");
    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() => "Значение должно быть числом.");
});
builder.Services.AddDbContext<VideoGamesStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VideoGamesStoreContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbInitializer.SeedAsync(context, hasher);
}

var supportedCultures = new[] { new CultureInfo("ru-RU") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ru-RU"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
