using api.Data;
using api.Interfaces;
using api.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    //option.AddSecurityDefinition();
});

builder.Services.AddDbContext<ApplicationDBContext>(options => {
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<ICacheService, CacheService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI(options => 
   {
      options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
      options.RoutePrefix = string.Empty;
   });
}

//app.UseHttpsRedirection();

app.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials() 
        .SetIsOriginAllowed(origin => true)
);


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();