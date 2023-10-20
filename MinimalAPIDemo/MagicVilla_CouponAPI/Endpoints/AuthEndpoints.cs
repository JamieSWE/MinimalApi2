using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTOs;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.Endpoints;

public static class AuthEndpoints
{
    public static void ConfigureAuthEndpoints(this WebApplication app)
    {


        app.MapPost("/api/login", Login).WithName("Login")
        .Accepts<LoginRequestDTO>("application/json")
        .Produces<APIResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
        app.MapPost("/api/register", Register).WithName("Register")
       .Accepts<RegistrationRequestDTO>("application/json")
       .Produces<APIResponse>(StatusCodes.Status201Created)
       .Produces(StatusCodes.Status400BadRequest);



    }


    private async static Task<IResult> Login(IAuthRepository _authRepo, [FromBody] LoginRequestDTO model)
    {
        APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

        var loginResponse = await _authRepo.Login(model);

        if (loginResponse == null)
        {
            response.ErrorMessages.Add("Username and/or password is incorrect.");
            return Results.BadRequest(response);
        }

        response.Result = loginResponse;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }

    private async static Task<IResult> Register(IAuthRepository _authRepo, [FromBody] RegistrationRequestDTO model)
    {
        APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

        bool isUserUnique = _authRepo.IsUniqueUser(model.UserName);

        if (!isUserUnique)
        {
            response.ErrorMessages.Add("Username already exists.");
            return Results.BadRequest(response);
        }

        var registerResponse = await _authRepo.Register(model);
        if (registerResponse == null || string.IsNullOrWhiteSpace(registerResponse.UserName))
            return Results.BadRequest(response);

        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }
}
