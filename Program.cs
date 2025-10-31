using System.Text.Json;
using HarizLearnRouting.Models;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async (HttpContext context) =>
    {
        await context.Response.WriteAsync("Welcome to the home page.");
    });

    endpoints.MapGet("/employees", async (HttpContext context) =>
    {
        // Get all of the employees' information
        var employees = EmployeesRepository.GetEmployees();

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync("<h2>Employees</h2>");
        await context.Response.WriteAsync("<ul>");
        foreach (var employee in employees)
        {
            await context.Response.WriteAsync($"<li><b>{employee.Name}</b>: {employee.Position}</li>");
        }
        await context.Response.WriteAsync("</ul>");        

    });

    endpoints.MapGet("/employees/{id:int}", async (HttpContext context) =>
    {
        var id = context.Request.RouteValues["id"];
        var employeeId = int.Parse(id.ToString());
        
        // Get a particular employee's information
        var employee = EmployeesRepository.GetEmployeeById(employeeId);

        context.Response.ContentType = "text/html";

        await context.Response.WriteAsync("<h2>Employee</h2>");
        if (employee is not null)
        {
            await context.Response.WriteAsync($"Name: {employee.Name}<br/>");
            await context.Response.WriteAsync($"Position: {employee.Position}<br/>");
            await context.Response.WriteAsync($"Salary: {employee.Salary}<br/>");
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Employee not found.");
        }
        

    });

    endpoints.MapPost("/employees", async (HttpContext context) =>
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        try
        {
            var employee = JsonSerializer.Deserialize<Employee>(body);

            if (employee is null || employee.Id <= 0)
            {
                context.Response.StatusCode = 400;
                return;
            }

            EmployeesRepository.AddEmployee(employee);

            context.Response.StatusCode = 201;
            await context.Response.WriteAsync("Employee added successfully.");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(ex.ToString());
            return;
        }

    });

    endpoints.MapPut("/employees", async (HttpContext context) =>
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var employee = JsonSerializer.Deserialize<Employee>(body);

        var result = EmployeesRepository.UpdateEmployee(employee);
        if (result)
        {
            context.Response.StatusCode = 204;
            await context.Response.WriteAsync("Employee updated successfully.");
            return;
        }
        else
        {
            await context.Response.WriteAsync("Employee not found.");
        }
    });

    endpoints.MapDelete("/employees/{id}", async (HttpContext context) =>
    {

        var id = context.Request.RouteValues["id"];
        var employeeId = int.Parse(id.ToString());        
        
        if (context.Request.Headers["Authorization"] == "frank")
        {
            var result = EmployeesRepository.DeleteEmployee(employeeId);

            if (result)
            {
                await context.Response.WriteAsync("Employee is deleted successfully.");
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Employee not found.");
            }
        }
        else
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("You are not authorized to delete.");
        }

    });        
    
});

app.Run();






