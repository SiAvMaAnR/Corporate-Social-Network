﻿using CSN.Domain.Entities.Channels;
using CSN.Domain.Entities.Companies;
using CSN.Domain.Entities.Employees;
using CSN.Domain.Interfaces.UnitOfWork;
using CSN.Infrastructure.Interfaces.Services;
using CSN.Persistence.DBContext;
using CSN.Persistence.Repositories;
using CSN.Persistence.UnitOfWork;
using CSN.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace CSN.WebApi.Extensions.ServiceConfigurations
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddTransientDependencies(this IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }

        public static IServiceCollection AddScopedDependencies(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            serviceCollection.AddScoped<ICompanyRepository, CompanyRepository>();
            serviceCollection.AddScoped<IEmployeeRepository, EmployeeRepository>();
            serviceCollection.AddScoped<IChannelRepository, ChannelRepository>();
            serviceCollection.AddScoped<ICompanyService, CompanyService>();
            serviceCollection.AddScoped<IEmployeeService, EmployeeService>();
            serviceCollection.AddScoped<IChannelService, ChannelService>();
            return serviceCollection;
        }

        public static IServiceCollection AddSingletonDependencies(this IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }

        public static IServiceCollection AddCommonDependencies(this IServiceCollection serviceCollection, ConfigurationManager config)
        {
            string connection = config.GetConnectionString("DefaultConnection");

            serviceCollection.AddDbContext<EFContext>(options => options.UseSqlServer(connection));
            serviceCollection.AddControllers();
            serviceCollection.AddEndpointsApiExplorer();
            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddLogging();
            serviceCollection.AddCors();
            serviceCollection.AddAuthorization();
            serviceCollection.AddSwaggerGen(options => options.Config());
            serviceCollection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.Config(config));
            return serviceCollection;
        }
    }
}