using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using restful_api_building.Services;
using restful_api_building.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using restful_api_building.Helpers;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace restful_api_building
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
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, LibraryContext libraryContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Error być");
                    });
                });
                app.UseHsts();
            }
            libraryContext.EnsureSeedDataForContext();
            //app.UseHttpsRedirection();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.Author, Models.AuthorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                    src.DateOfBirth.GetCurrentAge()));
                cfg.CreateMap<Book, Models.BookDto>();
            });
            app.UseMvc();
        }
    }
}
