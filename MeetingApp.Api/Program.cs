using MeetingApp.Data.Extensions;
using MeetingApp.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddRepositoryExt(builder.Configuration).AddServiceExt(builder.Configuration);

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var dbContext = services.GetRequiredService<MeetingDbContext>();

//    //if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
//    //{
//        Console.WriteLine("Migration'lar uygulanıyor...");
//        await dbContext.Database.MigrateAsync();
//        Console.WriteLine("Migration'lar başarıyla uygulandı.");
//    //}
//}

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

app.UseExceptionHandler(x => { });

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();