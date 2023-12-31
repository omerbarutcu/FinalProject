using Autofac;
using Autofac.Extensions.DependencyInjection;
using Business.DependencyResolvers.Autofac;
using Core.Utilities.IoC;
using Core.Utilities.Security.Encryption;
using Core.Utilities.Security.JWT;
using Core.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Core.DependencyResolvers;

namespace WepAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Autofac, Ninject, CastleWindsor, StructureMap, LightInject, DryInject --> IoC Container
            // AOP (springdeki annotationlar)
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //builder.Services.AddSingleton<IProductService, ProductManager>();
            //builder.Services.AddSingleton<IProductDal, EfProductDal>();


            //Farkl� bir IoC ortam� kullanmak istiyorsak <Autofac> bu syntax � kullan�r�z.
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            // Register services directly with Autofac here. Don't
            // call builder.Populate(), that happens in AutofacServiceProviderFactory.
            builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
            builder.RegisterModule(new AutofacBusinessModule()));



            var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = tokenOptions.Issuer,
                        ValidAudience = tokenOptions.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey)
                    };
                });

            // coreModel gibi mod�lleri eklemek i�in 
            builder.Services.AddDependencyResolvers(new ICoreModule[] { new CoreModule() });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); // eve girme yetkisi
            app.UseAuthorization();  // odaya girme yetkisi


            app.MapControllers();

            app.Run();
        }
    }
}
