using MediatR;
using System.Reflection;
using AutoMapper;
using CQRSWebApiProject.Business.MapProfiles;
using CQRSWebApiProject.DAL.Concrete.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using System;
using CQRSWebApiProject.Business.Validators;
using FluentValidation.AspNetCore;
using CQRSWebApiProject.Business.Validators.General;
using CQRSWebApiProject.Business.DTO.Request;
using FluentValidation;
using Kanbersky.Customer.Business.Extensions;
using CQRSWebApiProject.DAL.Concrete.EntityFramework.GenericRepository;
using Common.Messaging.Providers;
using Common.Messaging;
using Common.Messaging.Abstract;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());



builder.Services.AddMvc(options =>
{
    options.Filters.Add(new ResponseValidator());
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opt =>
    {
        opt.SuppressModelStateInvalidFilter = true;
    })
    .AddFluentValidation();

builder.Services.AddDbContext<EFContext>(opt => opt.UseInMemoryDatabase("InMem"));

//A�a��daki 2 �ekilde de ekleme yap�labilir. 
builder.Services.AddSingleton(
    MessageQueueFactory.CreateProvider());// 
//builder.Services.AddSingleton<IMessageQueueProvider>(sp =>
//    MessageQueueProviderFactory.CreateProvider(sp));// 


#region service aboneli�i yakla��m�
// clean code yakla��m� a�a��daki yap�y� Extensions klas�r� alt�na ald�m. bu sayede sadace a�a��daki iki sat�r kod ile baya i�lem halletmi� olduk...
//Fluentvalidation k�t�phanesi ile gelen istekleri kolay ve kod kalabal��� olmadan validation yapmam�za yarayan k�t�phanemiz var
//apimizi bu servislere de abone ediyoruz ve daha fazla servis i�ini de orada halledebiliyoruz.
//Core ve Customer servislerindeki startup.cs i�erisindeki kar���kl��� bu yap� ile par�alay�p daha temiz bir hale getirebiliriz....
//builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
//builder.Services.AddSingleton<IValidator<CreateCustomerRequest>, CreateCustomerRequestValidator>();
//builder.Services.AddSingleton<IValidator<UpdateCustomerRequest>, UpdateCustomerRequestValidator>();
builder.Services.RegisterHandlers();
builder.Services.RegisterValidators();
#endregion


var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMappingProfiels());
});
IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

var app = builder.Build();





// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
