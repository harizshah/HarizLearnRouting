// Import required libraries
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HarizLearnRouting.Models;

// 1️⃣ Create and build the ASP.NET Core web app
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 2️⃣ Enable routing so we can define endpoints (routes)
app.UseRouting();

// 3️⃣ Define endpoints for the app
app.UseEndpoints(endpoints =>
{
    // 🏠 GET / → Home endpoint
    endpoints.MapGet("/", async (HttpContext context) =>
    {
        await context.Response.WriteAsync("Welcome to the home page.");
    });

    // 👥 GET /employees → Get one employee using a value from HTTP HEADER
    // Example header:  "identity: 2"
    endpoints.MapGet("/employees", ([FromHeader(Name = "identity")] int id) =>
    {
        // ASP.NET Core MODEL BINDING automatically pulls 'identity' value
        // from the HTTP request header and stores it into variable 'id'
        var employee = EmployeesRepository.GetEmployeeById(id);

        return employee; // automatically serialized to JSON
    });

    // ➕ POST /employees → Add a new employee from JSON body
    endpoints.MapPost("/employees", async (HttpContext context) =>
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();

        try
        {
            // Deserialize JSON request body into Employee object
            var employee = JsonSerializer.Deserialize<Employee>(body);

            // Validate the input
            if (employee is null || employee.Id <= 0)
            {
                context.Response.StatusCode = 400; // Bad Request
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
            return;
        }
    });

    // ✏️ PUT /employees → Update an existing employee
    endpoints.MapPut("/employees", async (HttpContext context) =>
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var employee = JsonSerializer.Deserialize<Employee>(body);

        var result = EmployeesRepository.UpdateEmployee(employee);
        if (result)
        {
            context.Response.StatusCode = 204; // No Content (Success)
            await context.Response.WriteAsync("Employee updated successfully.");
        }
        else
        {
            await context.Response.WriteAsync("Employee not found.");
        }
    });

    // ❌ DELETE /employees/{id} → Delete employee by ID (authorized via header)
    endpoints.MapDelete("/employees/{id}", async (HttpContext context) =>
    {
        var id = context.Request.RouteValues["id"];
        var employeeId = int.Parse(id.ToString());

        // Header Authorization check: "Authorization: frank"
        if (context.Request.Headers["Authorization"] == "frank")
        {
            var result = EmployeesRepository.DeleteEmployee(employeeId);

            if (result)
            {
                await context.Response.WriteAsync("Employee is deleted successfully.");
            }
            else
            {
                context.Response.StatusCode = 404; // Not Found
                await context.Response.WriteAsync("Employee not found.");
            }
        }
        else
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("You are not authorized to delete.");
        }
    });
});

// 4️⃣ Run the web application
app.Run();

// ==========================
// 📦 Fake Employee Repository
// ==========================
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
    {
        return employees.FirstOrDefault(x => x.Id == id);
    }

    public static void AddEmployee(Employee? employee)
    {
        if (employee is not null)
        {
            employees.Add(employee);
        }
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

// ==========================
// 👤 Employee Model
// ==========================
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
