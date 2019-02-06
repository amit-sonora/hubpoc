using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Okta.AspNetCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace DotNetCoreSqlDb
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
            // Add framework services.
            services.AddMvc();
            string clientId = string.Empty;
            string authority = string.Empty;
            string signInScheme = string.Empty;
            //services.AddDbContext<MyDatabaseContext>(options =>
            //options.UseSqlite("Data Source=localdatabase.db"));

            // Use SQL Database if in Azure, otherwise, use SQLite
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                services.AddDbContext<MyDatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("MyDbConnection")));
                clientId = Configuration["clientId"];
                authority = Configuration["authority"];
                signInScheme = Configuration["signInScheme"];
            }

            else
            {
                services.AddDbContext<MyDatabaseContext>(options =>
                options.UseSqlite("Data Source=localdatabase.db"));

                clientId = Configuration.GetSection("IS4").GetValue<string>("ClientId");
                authority = Configuration.GetSection("IS4").GetValue<string>("Authority");
                signInScheme = Configuration.GetSection("IS4").GetValue<string>("SignInScheme");

            }
            // Automatically perform database migration
            services.BuildServiceProvider().GetService<MyDatabaseContext>().Database.Migrate();
            services.AddAuthentication(options =>
        {
            options.DefaultScheme = "cookie";
            options.DefaultChallengeScheme = "oidc";
        })
            .AddCookie("cookie")
            .AddOpenIdConnect("oidc", options =>
       {
           options.Authority = authority;// Configuration.GetSection("IS4").GetValue<string>("Authority");
           options.ClientId = clientId;//Configuration.GetSection("IS4").GetValue<string>("ClientId");
           options.SignInScheme = signInScheme;// Configuration.GetSection("IS4").GetValue<string>("SignInScheme");


       });
            
            }


        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
           if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=About}/{id?}");
            });
        }
    }
}
