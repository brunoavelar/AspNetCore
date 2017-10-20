﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Api.Utils
{
    public static class LogMessages
    {
        public static string CityNotFoundMsg = "City with ID {0} not found";
        public static string PointOfInterestNotFoundMsg = "Point of Interest with ID {0} not found";
    }

    public interface ILoggerAdapter<T>
    {
        void LogError(string message);
        void LogInformation(string message);
    }

    public class LoggerAdapter<T> : ILoggerAdapter<T>
    {
        private ILogger<T> _logger;

        public LoggerAdapter(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }
    }
}
