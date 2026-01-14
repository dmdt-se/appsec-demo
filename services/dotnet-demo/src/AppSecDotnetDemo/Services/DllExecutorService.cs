using System.Diagnostics;
using System.Reflection;

namespace AppSecDotnetDemo.Services;

public class DllExecutorService
{
    public ExecutionResult ExecuteMethod(string assemblyPath, string typeName, string methodName, string? parameter = null)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // These reflection calls are what Dynatrace monitors at runtime
            var assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(typeName)
                ?? throw new TypeLoadException($"Type '{typeName}' not found in assembly");
            var instance = Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Could not create instance of '{typeName}'");

            // Find method - try with string parameter first, then no parameters
            var method = type.GetMethod(methodName, new[] { typeof(string) })
                      ?? type.GetMethod(methodName, Type.EmptyTypes)
                      ?? throw new MissingMethodException($"Method '{methodName}' not found on type '{typeName}'");

            // Invoke with or without parameter based on method signature
            object? result;
            if (method.GetParameters().Length > 0 && parameter != null)
            {
                result = method.Invoke(instance, new object[] { parameter });
            }
            else
            {
                result = method.Invoke(instance, null);
            }

            stopwatch.Stop();

            return new ExecutionResult
            {
                Success = true,
                Output = result?.ToString() ?? "(no output)",
                ExecutionTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ExecutionResult
            {
                Success = false,
                Error = ex.InnerException?.Message ?? ex.Message,
                ExecutionTime = stopwatch.Elapsed
            };
        }
    }
}
