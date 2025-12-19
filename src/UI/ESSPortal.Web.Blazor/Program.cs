using EssPortal.Web.Blazor.Middleware;
using EssPortal.Web.Blazor.Utilities;

using ESSPortal.Web.Blazor.Components;

using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddApplicationConfiguration(builder.Configuration);
    builder.Services.AddAuthenticationServices(builder.Configuration);
    builder.Services.AddClientServices();
    builder.Services.AddLoggingServices(builder.Configuration);
    builder.Services.AddBlazorClientServices(builder.Configuration);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error during service registration: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandler = async context =>
        {
            var exceptionHandler = context.RequestServices.GetRequiredService<ExceptionHandler>();
            await exceptionHandler.TryHandleAsync(
                context,
                context.Features.Get<IExceptionHandlerFeature>()?.Error!,
                CancellationToken.None);
        }
    });

    app.UseHsts();
}

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error", createScopeForErrors: true);
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
