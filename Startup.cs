using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventAPI.CustomFilters;
using EventAPI.CustomFormatter;
using EventAPI.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace EventAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<EventDbContext>(options => {
                options.UseInMemoryDatabase(databaseName: "EventDb");
                //options.UseSqlServer(Configuration.GetConnectionString("EventSqlConnection"));
            });

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info {
                    Title = "Event API",
                    Version = "v1",
                    Contact = new Contact { Name= "Sujit D", Email="sujitd@email.com"}
                });
            });

            //services.AddCors(); //use this if the cors configuration is done in Configure method.
            services.AddCors(c => {
                c.AddPolicy("MSPolicy", builder => {
                    builder.WithOrigins("*.microsoft.com")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
                c.AddPolicy("SynPolicy", builder => {
                    builder.WithOrigins("*.Synergetics.com")
                        .WithMethods("GET")
                        .WithHeaders("Authorization", "Content-Type", "Accept");
                });
                c.AddPolicy("Others", builder => {
                    builder.AllowAnyOrigin()
                        .WithMethods("GET")
                        .AllowAnyHeader();
                });
                c.DefaultPolicyName = "Others";
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),
                        ValidAudience = Configuration.GetValue<string>("Jwt:Audience"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Secret")))
                    };
                });
                     
            services.AddMvc(c => {
                c.Filters.Add(typeof(CustomExceptionHandler));
                c.OutputFormatters.Add(new CSVCustomFormatter());
            }).AddXmlSerializerFormatters().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json","Event API");
                });
            }

            InitializeDatabase(app);
            app.UseCors(); //use this if the cors configuration is done in ConfigureServices method.
            //app.UseCors(c=> {
            //    c.WithOrigins("*.microsoft.com")
            //        .AllowAnyMethod()
            //        .AllowAnyHeader();
            //    c.WithOrigins("*.synergetics.com")
            //        .WithMethods("GET")
            //        .WithHeaders("Authorization", "Content-Type", "Accept");
            //});
            app.UseSwagger(); // add swagger as a middleware
            app.UseAuthentication();
            app.UseMvc();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<EventDbContext>();

                db.Events.Add(new Models.EventInfo
                {
                    Title = "Sample Event 1",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    StartTime = "9:00 AM",
                    EndTime = "5:30 PM",
                    Host = "Microsoft",
                    Speaker = "Sujit",
                    RegistrationUrl = "http://events.microsoft.com/3224"
                });
                db.Events.Add(new Models.EventInfo
                {
                    Title = "Sample Event 2",
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(4),
                    StartTime = "9:00 AM",
                    EndTime = "5:30 PM",
                    Host = "Google",
                    Speaker = "Nitish",
                    RegistrationUrl = "http://events.microsoft.com/3224"
                });
                db.Events.Add(new Models.EventInfo
                {
                    Title = "Sample Event 3",
                    StartDate = DateTime.Now.AddDays(4),
                    EndDate = DateTime.Now.AddDays(6),
                    StartTime = "9:00 AM",
                    EndTime = "5:30 PM",
                    Host = "Amazon",
                    Speaker = "Unaish",
                    RegistrationUrl = "http://events.microsoft.com/3224"
                });
                db.SaveChanges();
            }
        }
    }
}