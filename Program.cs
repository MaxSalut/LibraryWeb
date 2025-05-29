using Microsoft.EntityFrameworkCore;
using LibraryWeb.Data; 

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
}
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Online Library API",
        Description = "An ASP.NET Core Web API for managing an online library"
       
    });
});


var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Library API V1");
    
});


app.UseHttpsRedirection();


app.UseAuthorization(); 

app.MapControllers();

app.Run();