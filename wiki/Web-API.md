# Web API

The Web API is an HTTP-based interface by which the Web Frontend can instruct the MODiX server application to perform [Business Logic](Business-Logic) operations.

## Purpose

1. Allow the Web Frontend to access the Business Logic layer.

## Methodology

### Controllers

The Web API utilizes [MVC Controllers](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-3.1) to implement API endpoints. In a nutshell, a Controller is a `class` that contains one or more endpoints, generally those which have the same relative path, in the form of methods that are executed when those endpoints are targeted by an HTTP request. Controllers and endpoint methods are configured through `[Attribute]`-based annotations, that define things like the endpoint path and endpoint HTTP Method, and support auto-loading and modeling of endpoints at runtime, through Reflection.

Controller actions should generally be extremely thin wrappers for code within the [Business Logic](Business-Logic) layer, executing only code that is specific to the HTTP Request pipeline.

E.G.
```cs
[Route("~/api/infractions")]
public class InfractionController
    : ModixController
{
    ...
    [HttpPost("{id}/rescind")]
    public async Task<IActionResult> RescindInfractionAsync(long id)
    {
        try
        {
            await ModerationService.RescindInfractionAsync(id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok();
    }
    ...
}
```

### Authentication

Authentication is the task of determining, in a secure manner, what user is performing a particular user operation. Authentication within the Web Frontend and Web API is performed by the ASP.NET Core Authentication Middleware, using implementations of the OAuth protocol and JWT encoding techniques.

See [`Modix/Auth/`](../../tree/master/Modix/Auth) for implementation details.

### Testing

As the Web API is a very thin layer around Business Logic operations, it does not currently have any requirements for testing.
