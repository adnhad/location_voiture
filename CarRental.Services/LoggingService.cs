using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;


namespace CarRental.Services
{
    public class LoggingService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _logDirectory;
        private bool _disposed = false;

        public LoggingService(string logDirectory = "logs")
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logDirectory);
            
            // Ensure log directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            // Configure Serilog
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Information
                )
                .WriteTo.File(
                    Path.Combine(_logDirectory, "carrental-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ThreadId}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Debug
                )
                .WriteTo.File(
                    Path.Combine(_logDirectory, "errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ThreadId}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Error
                )
                .CreateLogger();

            Log.Information("LoggingService initialized. Log directory: {LogDirectory}", _logDirectory);
            LogInfo("Application", "Logging service started");
        }

        #region Basic Logging Methods

        public void LogInfo(string message)
        {
            _logger.Information(message);
        }

        public void LogInfo(string category, string message)
        {
            _logger.Information("[{Category}] {Message}", category, message);
        }

        public void LogWarning(string message)
        {
            _logger.Warning(message);
        }

        public void LogWarning(string category, string message)
        {
            _logger.Warning("[{Category}] {Message}", category, message);
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }

        public void LogError(string category, string message)
        {
            _logger.Error("[{Category}] {Message}", category, message);
        }

        public void LogError(Exception exception, string message)
        {
            _logger.Error(exception, message);
        }

        public void LogError(string category, Exception exception, string message)
        {
            _logger.Error(exception, "[{Category}] {Message}", category, message);
        }

        public void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        public void LogDebug(string category, string message)
        {
            _logger.Debug("[{Category}] {Message}", category, message);
        }

        #endregion

        #region Application-Specific Logging Methods

        public void LogUserActivity(string username, string action, string details = null)
        {
            if (string.IsNullOrEmpty(details))
                _logger.Information("USER_ACTIVITY: User={Username}, Action={Action}", username, action);
            else
                _logger.Information("USER_ACTIVITY: User={Username}, Action={Action}, Details={Details}", 
                    username, action, details);
        }

        public void LogUserLogin(string username, bool success, string ipAddress = null)
        {
            if (success)
                _logger.Information("USER_LOGIN: User={Username}, Status=Success, IP={IpAddress}", 
                    username, ipAddress ?? "N/A");
            else
                _logger.Warning("USER_LOGIN: User={Username}, Status=Failed, IP={IpAddress}", 
                    username, ipAddress ?? "N/A");
        }

        public void LogRentalActivity(int rentalId, string action, string details = null)
        {
            if (string.IsNullOrEmpty(details))
                _logger.Information("RENTAL_ACTIVITY: RentalId={RentalId}, Action={Action}", rentalId, action);
            else
                _logger.Information("RENTAL_ACTIVITY: RentalId={RentalId}, Action={Action}, Details={Details}", 
                    rentalId, action, details);
        }

        public void LogPaymentActivity(int paymentId, string action, decimal amount, string status = null)
        {
            if (string.IsNullOrEmpty(status))
                _logger.Information("PAYMENT_ACTIVITY: PaymentId={PaymentId}, Action={Action}, Amount={Amount:C}", 
                    paymentId, action, amount);
            else
                _logger.Information("PAYMENT_ACTIVITY: PaymentId={PaymentId}, Action={Action}, Amount={Amount:C}, Status={Status}", 
                    paymentId, action, amount, status);
        }

        public void LogVehicleActivity(int vehicleId, string action, string details = null)
        {
            if (string.IsNullOrEmpty(details))
                _logger.Information("VEHICLE_ACTIVITY: VehicleId={VehicleId}, Action={Action}", vehicleId, action);
            else
                _logger.Information("VEHICLE_ACTIVITY: VehicleId={VehicleId}, Action={Action}, Details={Details}", 
                    vehicleId, action, details);
        }

        public void LogDatabaseOperation(string operation, string table, int? recordId = null, bool success = true)
        {
            if (recordId.HasValue)
                _logger.Information("DATABASE: Operation={Operation}, Table={Table}, RecordId={RecordId}, Success={Success}", 
                    operation, table, recordId.Value, success);
            else
                _logger.Information("DATABASE: Operation={Operation}, Table={Table}, Success={Success}", 
                    operation, table, success);
        }

        public void LogPerformance(string operation, long milliseconds)
        {
            if (milliseconds > 1000)
                _logger.Warning("PERFORMANCE: Operation={Operation}, Time={Milliseconds}ms (SLOW)", operation, milliseconds);
            else if (milliseconds > 500)
                _logger.Information("PERFORMANCE: Operation={Operation}, Time={Milliseconds}ms", operation, milliseconds);
            else
                _logger.Debug("PERFORMANCE: Operation={Operation}, Time={Milliseconds}ms", operation, milliseconds);
        }

        #endregion

        #region File and Audit Logging

        public void LogAudit(string userId, string action, string resource, string details = null)
        {
            var auditMessage = $"AUDIT: User={userId}, Action={action}, Resource={resource}";
            
            if (!string.IsNullOrEmpty(details))
                auditMessage += $", Details={details}";
            
            _logger.Information(auditMessage);
            
            // Also write to separate audit log file
            var auditLogPath = Path.Combine(_logDirectory, "audit.log");
            var auditEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | User: {userId} | Action: {action} | Resource: {resource} | Details: {details ?? "N/A"}{Environment.NewLine}";
            File.AppendAllText(auditLogPath, auditEntry);
        }

        public string GetLogFilePath(string logType = "main")
        {
            var fileName = logType.ToLower() switch
            {
                "error" => $"errors-{DateTime.Now:yyyyMMdd}.log",
                "audit" => "audit.log",
                _ => $"carrental-{DateTime.Now:yyyyMMdd}.log"
            };
            
            return Path.Combine(_logDirectory, fileName);
        }

        public List<string> GetRecentLogs(int lines = 100, string logType = "main")
        {
            var logFile = GetLogFilePath(logType);
            
            if (!File.Exists(logFile))
                return new List<string> { $"Log file not found: {logFile}" };

            var linesList = new List<string>();
            using (var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(fileStream))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null && linesList.Count < lines)
                {
                    linesList.Add(line);
                }
            }

            return linesList;
        }

        public void ClearOldLogs(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var logFiles = Directory.GetFiles(_logDirectory, "*.log");
                
                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.LastWriteTime < cutoffDate && !fileInfo.Name.Contains("audit.log"))
                    {
                        fileInfo.Delete();
                        _logger.Debug("Deleted old log file: {LogFile}", fileInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error clearing old logs");
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Flush and close the logger
                    (_logger as IDisposable)?.Dispose();
                }
                _disposed = true;
            }
        }

        ~LoggingService()
        {
            Dispose(false);
        }

        #endregion

        #region Static Helper Methods

        public static LoggingService CreateDefault()
        {
            return new LoggingService();
        }

        public static void LogToFile(string message, string fileName = "application.log")
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", fileName);
            var logDir = Path.GetDirectoryName(logPath);
            
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }

        #endregion
    }
}