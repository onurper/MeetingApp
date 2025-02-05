using MeetingApp.Core;
using MeetingApp.Core.IRepositories;
using MeetingApp.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingApp.Data.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositoryExt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MeetingDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SqlServer"), sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("MeetingApp.Data");
            });
        });

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMeetingRepository, MeetingRepository>();

        return services;
    }
}