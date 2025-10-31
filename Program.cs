// Import required namespaces
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HarizLearnRouting.Models;

// 1️⃣ Create and configure the web app
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 2️⃣ Enable routing middleware
app.UseRouting();

// 3️⃣ Define endpoints
app.UseEndpoints(endpoints =>
{
    // 🏠 Home endpoint
    endpoints.MapGet("/", async (HttpContext context) =>
    {
        await context.Response.WriteAsync("Welcome to the home page.");
    });

    // 👥 GET /employees/{id}?name=John with header: Position=Manager
    // Uses [AsParameters] to group inputs from multiple sources
    endpoints.MapGet("/employees/{id:int}", ([AsParameters] GetEmployeeParameter param) =>
    {
        // Access values that ASP.NET bound automatically
        // param.Id      → comes from Route {id}
        // param.Name    → comes from Query string ?name=
        // param.Position → comes from HTTP Header: Position:

        var employee = EmployeesRepository.GetEmployeeById(param.Id);

        // Modify data to show combined binding usage
        employee.Name = param.Name;
        employee.Position = param.Position;

        return employee; // returns JSON
    });

    // ➕ POST /employees → Add a new employee (JSON in body)
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
            context.Response.StatusCode = 201; // Created
            await context.Response.WriteAsync("Employee added successfully.");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(ex.ToString());
        }
    });

    // ✏️ PUT /employees → Update employee info
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
        }
        else
        {
            await context.Response.WriteAsync("Employee not found.");
        }
    });

    // ❌ DELETE /employees/{id} → Delete employee (with header check)
    endpoints.MapDelete("/employees/{id}", async (HttpContext context) =>
    {
        var id = context.Request.RouteValues["id"];
        var employeeId = int.Parse(id.ToString());

        if (context.Request.Headers["Authorization"] == "frank")
        {
            var result = EmployeesRepository.DeleteEmployee(employeeId);

            if (result)
                await context.Response.WriteAsync("Employee is deleted successfully.");
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

// 4️⃣ Run the app
app.Run();


// =============================
// 🧩 GROUPED PARAMETER STRUCT
// =============================

// [AsParameters] allows ASP.NET Core to bind multiple sources (route, query, header)
// into a single object so your endpoint stays clean and organized.
struct GetEmployeeParameter
{
    [FromRoute]  // Bind from URL route {id}
    public int Id { get; set; }

    [FromQuery]  // Bind from query string (?name=...)
    public string Name { get; set; }

    [FromHeader] // Bind from HTTP header (Position: ...)
    public string Position { get; set; }
}


// =============================
// 📦 EmployeesRepository (Fake DB)
// =============================
public static class EmployeesRepository
{
    private static List<Employee> employees = new List<Employee>
    {
        new Employee(1, "John Doe", "Engineer", 60000),
        new Employee(2, "Jane Smith", "Manager", 75000),
        new Employee(3, "Sam Brown", "Technician", 50000)
    };

    public static List<Employee> GetEmployees() => employees;

    public static Employee? GetEmployeeById(int id)
        => employees.FirstOrDefault(x => x.Id == id);

    public static void AddEmployee(Employee? employee)
    {
        if (employee is not null)
            employees.Add(employee);
    }

    public static bool UpdateEmployee(Employee? employee)
    {
        if (employee is not null)
        {
            var emp = employees.FirstOrDefault(x => x.Id == employee.Id);
            if (emp is not null)
            {
                emp.Name = employee.Name;
                emp.Position = employee.Position;
                emp.Salary = employee.Salary;
                return true;
            }
        }
        return false;
    }

    public static bool DeleteEmployee(int id)
    {
        var employee = employees.FirstOrDefault(x => x.Id == id);
        if (employee is not null)
        {
            employees.Remove(employee);
            return true;
        }
        return false;
    }
}


// =============================
// 👤 Employee Model
// =============================
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public double Salary { get; set; }

    public Employee(int id, string name, string position, double salary)
    {
        Id = id;
        Name = name;
        Position = position;
        Salary = salary;
    }
}
