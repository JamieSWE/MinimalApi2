using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTOs;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.Endpoints;

public static class CouponEndpoints
{
    public static void ConfigureCouponEndpoints(this WebApplication app)
    {

        app.MapGet("/api/coupon", GetAllCoupons).WithName("GetCoupons")
        .Produces<APIResponse>(StatusCodes.Status200OK)
        .RequireAuthorization("AdminOnly");

        app.MapGet("/api/coupon/{id:int}", GetCoupon).WithName("GetCoupon")
        .Produces<APIResponse>(StatusCodes.Status200OK)
        .AddEndpointFilter(async (context, next) =>
        {
            var id = context.GetArgument<int>(2);
            if (id == 0)
            {
                return Results.BadRequest("Cannot have 0 in id");
            }
            return await next(context);
        });

        app.MapPost("/api/coupon", CreateCoupon).WithName("CreateCoupons")
        .Accepts<CouponCreateDTO>("application/json")
        .Produces<APIResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapPut("/api/coupon", UpdateCoupon).WithName("UpdateCoupon")
        .Accepts<CouponUpdateDTO>("application/json")
        .Produces<APIResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapDelete("/api/coupon/{id:int}", DeleteCoupon).WithName("DeleteCoupon")
        .Produces<APIResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

    }

    [Authorize]
    private async static Task<IResult> GetCoupon(ICouponRepository _couponRepo, int id)
    {
        APIResponse response = new();
        response.Result = await _couponRepo.GetAsync(id);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }
    [Authorize]
    private async static Task<IResult> CreateCoupon(IMapper _mapper, IValidator<CouponCreateDTO> _validator,
            ICouponRepository _couponRepo, [FromBody] CouponCreateDTO couponCreateDto)
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
    }
    [Authorize]
    private async static Task<IResult> UpdateCoupon(IMapper _mapper, IValidator<CouponUpdateDTO> _validator,
            ICouponRepository _couponRepo, [FromBody] CouponUpdateDTO couponUpdateDto)
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
    }
    [Authorize]
    private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
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
    }
    private async static Task<IResult> GetAllCoupons(ICouponRepository _couponRepo, ILogger<Program> _logger)
    {
        APIResponse response = new();
        _logger.Log(LogLevel.Information, "Getting all coupons");

        response.Result = await _couponRepo.GetAllAsync();
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }
}
