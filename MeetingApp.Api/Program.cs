using Hangfire;
using MeetingApp.Data.Extensions;
using MeetingApp.Service.Extensions;
using MeetingApp.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddRepositoryExt(builder.Configuration).AddServiceExt(builder.Configuration);

var app = builder.Build();

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
    var serviceProvider = scope.ServiceProvider;
    var meetingCleanupService = serviceProvider.GetRequiredService<MeetingCleanupService>();

    var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate(
        "delete-cancelled-meetings",
        () => meetingCleanupService.DeleteCancelledMeetings(), Cron.Daily);
}

app.UseExceptionHandler(x => { });
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();