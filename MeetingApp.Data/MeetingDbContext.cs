using MeetingApp.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public class MeetingDbContext : DbContext
{
    public MeetingDbContext(DbContextOptions<MeetingDbContext> options) : base(options)
    {
    }

    public DbSet<Meeting> Meetings { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = default!;
    public DbSet<DeletedMeetings> DeletedMeetings { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<DeletedMeetings>().ToTable("DeletedMeetings");
        modelBuilder.Entity<DeletedMeetings>().HasKey(e => e.Id);
        modelBuilder.Entity<DeletedMeetings>()
            .Property(e => e.DeletedAt)
            .HasDefaultValueSql("GETDATE()");
    }

    public void EnsureDatabaseSetup()
    {
        this.Database.ExecuteSqlRaw(CreateDeletedMeetingsTable());
        this.Database.ExecuteSqlRaw(CreateDeleteTrigger());
    }

    private static string CreateDeletedMeetingsTable()
    {
        return @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeletedMeetings')
            BEGIN
                CREATE TABLE DeletedMeetings (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    MeetingId INT NOT NULL,
                    UserId INT NOT NULL,
                    Title NVARCHAR(MAX) NOT NULL,
                    Status BIT NOT NULL,
                    Description NVARCHAR(MAX) NOT NULL,
                    DocumentPath NVARCHAR(MAX) NOT NULL,
                    StartDate DATETIME2(7) NOT NULL,
                    EndDate DATETIME2(7) NOT NULL,
                    DeletedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
                );
            END;";
    }
    public static string CreateDeleteTrigger()
    {
        return @"
        IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_AfterDeleteMeetings')
        BEGIN
            EXEC sp_executesql N'
            CREATE TRIGGER trg_AfterDeleteMeetings 
            ON Meetings
            AFTER DELETE
            AS
            BEGIN
                SET NOCOUNT ON;
                INSERT INTO DeletedMeetings (
                    MeetingId, UserId, Title, Status, Description, DocumentPath, StartDate, EndDate, DeletedAt
                ) 
                SELECT Id, UserId, Title, Status, Description, DocumentPath, StartDate, EndDate, GETDATE() 
                FROM DELETED;
            END';
        END";
    }
}
