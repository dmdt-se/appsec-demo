using System.Diagnostics;
using System.Text;
using Microsoft.Data.Sqlite;

namespace TaintedInputPlugin;

/// <summary>
    /// SECURITY DEMONSTRATION - Taint tracking demo
    ///
    /// These methods accept PARAMETERS to simulate user input flowing to sinks.
/// </summary>
public class Attacks
{
    private const string DbPath = "/tmp/demo_attacks.db";

    /// <summary>
    /// Initialize database for SQL injection testing.
    /// Call first with no parameters.
    /// </summary>
    public string InitializeDatabase()
    {
        var results = new StringBuilder();
        results.AppendLine("=== Database Initialization ===");

        try
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                DROP TABLE IF EXISTS Users;
                CREATE TABLE Users (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    Secret TEXT
                );
                INSERT INTO Users (Name, Email, Role, Secret) VALUES
                    ('Alice Admin', 'alice@company.com', 'admin', 'ADMIN_API_KEY_12345'),
                    ('Bob User', 'bob@company.com', 'user', NULL),
                    ('Charlie User', 'charlie@company.com', 'user', NULL),
                    ('Admin Root', 'root@company.com', 'superadmin', 'ROOT_SECRET_KEY_99999');
            ";
            cmd.ExecuteNonQuery();

            results.AppendLine($"Database created at: {DbPath}");
            results.AppendLine("Users table populated with 4 records");
            results.AppendLine("\nReady for SQL injection testing!");
            results.AppendLine("\nNext: Call SearchUsers with parameter:");
            results.AppendLine("  Safe: Alice");
            results.AppendLine("  Attack: ' OR '1'='1' --");
        }
        catch (Exception ex)
        {
            results.AppendLine($"Error: {ex.Message}");
        }

        return results.ToString();
    }

    /// <summary>
    /// SQL INJECTION - User input flows directly to SQL query
    ///
    /// Safe: "Alice"
    /// Attack: "' OR '1'='1' --"
    /// Attack: "' UNION SELECT Secret,Email,Role,Name,Id FROM Users WHERE Role='admin' --"
    /// </summary>
    public string SearchUsers(string searchTerm)
    {
        var results = new StringBuilder();
        results.AppendLine("=== SQL Injection Attack Vector ===");
        results.AppendLine($"Input: {searchTerm}");
        results.AppendLine();

        try
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();

            // VULNERABLE: Direct string concatenation
            var sql = $"SELECT Id, Name, Email, Role FROM Users WHERE Name LIKE '%{searchTerm}%'";
            results.AppendLine($"SQL: {sql}");
            results.AppendLine();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            var count = 0;
            while (reader.Read())
            {
                count++;
                results.AppendLine($"  [{reader["Id"]}] {reader["Name"]} - {reader["Email"]} ({reader["Role"]})");
            }
            results.AppendLine($"\nRecords returned: {count}");
        }
        catch (Exception ex)
        {
            results.AppendLine($"SQL Error: {ex.Message}");
        }

        return results.ToString();
    }

    /// <summary>
    /// COMMAND INJECTION - User input flows to shell command
    ///
    /// Safe: "echo hello"
    /// Attack: "cat /etc/passwd"
    /// Attack: "id && whoami"
    /// </summary>
    public string ExecuteCommand(string command)
    {
        var results = new StringBuilder();
        results.AppendLine("=== Command Injection Attack Vector ===");
        results.AppendLine($"Input: {command}");
        results.AppendLine();

        try
        {
            // VULNERABLE: User input flows directly to shell command
            var arguments = $"-c \"{command}\"";

            results.AppendLine($"Command: /bin/sh {arguments}");
            results.AppendLine();

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/bin/sh";
            startInfo.Arguments = arguments;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            results.AppendLine("Output:");
            results.AppendLine(string.IsNullOrEmpty(output) ? "(no output)" : output);
            if (!string.IsNullOrEmpty(error))
            {
                results.AppendLine("Stderr:");
                results.AppendLine(error);
            }
        }
        catch (Exception ex)
        {
            results.AppendLine($"Error: {ex.Message}");
        }

        return results.ToString();
    }


    /// <summary>
    /// Info method - shows available attacks and payloads
    /// </summary>
    public string ShowAttacks()
    {
        return @"
╔══════════════════════════════════════════════════════════════╗
║          TaintedInputPlugin - RAP Detection Demo             ║
╚══════════════════════════════════════════════════════════════╝

Available Methods (call with parameter):

[1] InitializeDatabase() - Setup DB first (no params)

[2] SearchUsers(string searchTerm) - SQL Injection
    Safe:   Alice
    Attack: ' OR '1'='1' --

[3] ExecuteCommand(string command) - Command Injection
    Safe:   echo hello
    Attack: cat /etc/passwd
    Attack: id && whoami

NOTE: This demo currently includes SQL Injection and Command Injection.
      Other categories are not wired into the .NET plugin yet.
";
    }
}
