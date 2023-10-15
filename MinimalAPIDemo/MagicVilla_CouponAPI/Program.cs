using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTOs;
using MagicVilla_CouponAPI.Repository;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", async (ICouponRepository _couponRepo, ILogger<Program> _logger) =>
{
    APIResponse response = new();
    _logger.Log(LogLevel.Information, "Getting all coupons");

    response.Result = await _couponRepo.GetAllAsync();
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("GetCoupons")
.Produces<APIResponse>(StatusCodes.Status200OK);

app.MapGet("/api/coupon/{id:int}", async (ICouponRepository _couponRepo, int id) =>
{
    APIResponse response = new();
    response.Result = await _couponRepo.GetAsync(id);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("GetCoupon")
.Produces<APIResponse>(StatusCodes.Status200OK);

app.MapPost("/api/coupon", async (IMapper _mapper, IValidator<CouponCreateDTO> _validator,
    ICouponRepository _couponRepo, [FromBody] CouponCreateDTO couponCreateDto) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    var validationResult = await _validator.ValidateAsync(couponCreateDto);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.ToString());
        return Results.BadRequest(response);
    }

    if (await _couponRepo.GetAsync(couponCreateDto.Name) != null)
    {
        response.ErrorMessages.Add("Coupon Name already exists");
        return Results.BadRequest(response);
    }


    Coupon coupon = _mapper.Map<Coupon>(couponCreateDto);

    await _couponRepo.CreateAsync(coupon);
    await _couponRepo.SaveAsync();


    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

    response.Result = couponDTO;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.Created;
    return Results.Ok(response);
    //return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);
    //return Results.Created($"/api/coupon/{coupon.Id}",coupon);
}).WithName("CreateCoupons")
.Accepts<CouponCreateDTO>("application/json")
.Produces<APIResponse>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

app.MapPut("/api/coupon", async (IMapper _mapper, IValidator<CouponUpdateDTO> _validator,
    ICouponRepository _couponRepo, [FromBody] CouponUpdateDTO couponUpdateDto) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    var validationResult = await _validator.ValidateAsync(couponUpdateDto);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.ToString());
        return Results.BadRequest(response);
    }

    await _couponRepo.UpdateAsync(_mapper.Map<Coupon>(couponUpdateDto));
    await _couponRepo.SaveAsync();


    response.Result = _mapper.Map<CouponDTO>(await _couponRepo.GetAsync(couponUpdateDto.Id));
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("UpdateCoupon")
.Accepts<CouponUpdateDTO>("application/json")
.Produces<APIResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);


app.MapDelete("/api/coupon/{id:int}", async (ICouponRepository _couponRepo, int id) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    Coupon couponFromStore = await _couponRepo.GetAsync(id);

    if (couponFromStore != null)
    {
        await _couponRepo.RemoveAsync(couponFromStore);
        await _couponRepo.SaveAsync();
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.NoContent;
        return Results.Ok(response);
    }
    else
    {
        response.ErrorMessages.Add("Invalid Id");
        return Results.BadRequest(response);
    }

}).WithName("DeleteCoupon")
.Produces<APIResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.UseHttpsRedirection();

app.Run();
