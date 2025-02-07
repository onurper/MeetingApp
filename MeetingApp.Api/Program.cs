using Hangfire;
using MeetingApp.Data.Extensions;
using MeetingApp.Service.Extensions;
using MeetingApp.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddRepositoryExt(builder.Configuration).AddServiceExt(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MeetingDbContext>();
        context.Database.Migrate();
        context.EnsureDatabaseSetup();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration hatası: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        options.RoutePrefix = string.Empty;
    });
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MeetingDbContext>();
    dbContext.Database.Migrate();
    dbContext.EnsureDatabaseSetup();
}

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var meetingCleanupService = serviceProvider.GetRequiredService<MeetingCleanupService>();

    var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate(
        "delete-cancelled-meetings",
        () => meetingCleanupService.DeleteCancelledMeetings(), Cron.Minutely);
}

app.UseExceptionHandler(x => { });
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();