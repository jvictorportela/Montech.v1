﻿using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Montech.Domain.Repositories;
using Montech.Domain.Repositories.Usuario;
using Montech.Domain.Security.Tokens;
using Montech.Domain.Services.LoggedUser;
using Montech.Infrastructure.Data;
using Montech.Infrastructure.Data.Repositories.Usuario;
using Montech.Infrastructure.Extensions;
using Montech.Infrastructure.Security.Tokens.Access.Generator;
using Montech.Infrastructure.Security.Tokens.Access.Validator;
using Montech.Infrastructure.Services.LoggedUser;
using System.Reflection;

namespace Montech.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext_SqlServer(services, configuration);
        AddFluentMigrator_SqlServer(services, configuration);
        AddTokens(services, configuration);
        AddRepositories(services, configuration);
        AddLoggedUser(services);
    }

    private static void AddDbContext_SqlServer(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MontechDbContext>(dbContextOptions =>
        {
            dbContextOptions.UseSqlServer(configuration.ConnectionString());
        });
    }

    private static void AddRepositories(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUsuarioReadOnlyRepository, UsuarioRepository>();
        services.AddScoped<IUsuarioWriteOnlyRepository, UsuarioRepository>();
        services.AddScoped<IUserUpdateOnlyRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddFluentMigrator_SqlServer(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.ConnectionString();

        services.AddFluentMigratorCore().ConfigureRunner(options =>
        {
            options
            .AddSqlServer()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(Assembly.Load("Montech.Infrastructure")).For.All();
        });
    }

    private static void AddTokens(IServiceCollection services, IConfiguration configuration)
    {
        var expirationTimeMinutes = configuration.GetValue<uint>("Settings:Jwt:ExpirationTimeMinutes");
        var signingKey = configuration.GetValue<string>("Settings:Jwt:SigningKey");

        services.AddScoped<IAccessTokenGenerator>(option => new JwtTokenGenerator(expirationTimeMinutes, signingKey!));
        services.AddScoped<IAccessTokenValidator>(option => new JwtTokenValidator(signingKey!));
    }

    private static void AddLoggedUser(IServiceCollection services)
    {
        services.AddScoped<ILoggedUser, LoggedUser>();
    }
}
