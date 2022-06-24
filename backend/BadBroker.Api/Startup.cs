using System.Text.Json.Serialization;
using BadBroker.Application;
using BadBroker.Core;
using BadBroker.Core.Repositories;
using BadBroker.Core.Services;
using BadBroker.DataAccess.Psql.Repositories;
using BadBroker.ExchangeratesIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace BadBroker.Api
{
	public class Startup
	{
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				});
			
			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo{Title = "BadBroker", Version = "v1"});
			});

			var exchangeratesSection = _configuration.GetSection("ExchangeratesApi");
			services.AddTransient<IRateProvider>(x =>
				new ExchangeratesRatesProvider((string)exchangeratesSection.GetValue(typeof(string), "apiKey")));
			
			services.AddTransient<IRateRepository, RateRepository>();
			services.AddTransient<IRateService, RateService>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
					options.RoutePrefix = string.Empty;
				});
			}

			app.UseRouting();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}