﻿using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JokesWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(
            IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.Use(
                (
                    context,
                    func) =>
                {
                    var isKnown = context.Request.Cookies.TryGetValue("user-cookie", out var track);
                    if (!isKnown)
                    {
                        track = Guid.NewGuid().ToString("N");
                        context.Response.Cookies.Append("user-cookie", track);
                    }

                    context.Items["user-cookie"] = track;

                    return func();
                });

            app.UseMvc(
                routes =>
                {
                    routes.MapRoute(
                        "default",
                        "{controller}/{action}",
                        new { controller = "Home", action = "Index" });
                });
        }
    }
}