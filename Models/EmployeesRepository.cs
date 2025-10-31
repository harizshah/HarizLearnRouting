namespace HarizLearnRouting.Models

{
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
}