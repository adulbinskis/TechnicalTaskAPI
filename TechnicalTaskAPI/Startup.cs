using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TechnicalTaskAPI.ORM.Entities;
using TechnicalTaskAPI.ORM.Services;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TechnicalTaskAPI.Application.Identity.Services;
using System.Text.Json.Serialization;
using TechnicalTaskAPI.Application.Services;
using TechnicalTaskAPI.Application.Services.Interfaces;
using TechnicalTaskAPI.Application.Products.Commands;
//using TechnicalTaskAPI.Application.Products.Services;
using TechnicalTaskAPI.Application.Common;
using Microsoft.Extensions.Options;
using TechnicalTaskAPI.Application.Products.Profiles;

namespace TechnicalTaskAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SiteSettings>(Configuration.GetSection("SiteSettings"));

            var siteSettings = Configuration.GetSection("SiteSettings").Get<SiteSettings>();
            ProductProfile.VatRate = siteSettings.VatRate;

            services.AddAutoMapper(typeof(Startup));

            services.AddControllers().AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IBaseEntityService, BaseEntityService>();
            services.AddScoped<IDateTimeService, DateTimeService>();
            services.AddScoped<IDbInitializationService, DbInitializationService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                };

                options.TokenHandlers.Add(new BearerTokenHandler());

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowSpecificOrigin",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });

            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });

                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                options.MapType<CreateProduct.Command>(() => new OpenApiSchema { Type = "object", Properties = new Dictionary<string, OpenApiSchema>() });
                options.MapType<UpdateProduct.Command>(() => new OpenApiSchema { Type = "object", Properties = new Dictionary<string, OpenApiSchema>() });
                options.MapType<DeleteProduct.Command>(() => new OpenApiSchema { Type = "object", Properties = new Dictionary<string, OpenApiSchema>() });

            });

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

}
