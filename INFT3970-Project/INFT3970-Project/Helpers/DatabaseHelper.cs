﻿using Dapper;
using INFT3970Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace INFT3970Project.Helpers
{
    public class DatabaseHelper : IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly string _connectionString;
        public IDbConnection Connection { get; set; }
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        string format = "yyyy-MM-dd HH:mm:ss";

        public DatabaseHelper(IConfiguration configuration) 
        {
            this.configuration = configuration;
            _connectionString = configuration.GetConnectionString("AzureConnectionString").ToString();
            Connection = new SqlConnection(_connectionString);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
            }

            disposed = true;
        }

        /// <summary>
        /// Implementation to create a record async
        /// </summary>
        /// <param name="model"></param>
        public async void CreateRecord(object model)
        {
            var stringBuilder = new StringBuilder();

            if (model is TemperatureModel)
            {
                TemperatureModel castModel = (TemperatureModel)model;
                stringBuilder.Append($@"INSERT INTO dbo.Temperature
                                        ([SensorId],[Temp],[Date])
                                        VALUES ('{castModel.Id}',
                                               '{castModel.Temperature}',
                                               '{castModel.Timestamp.ToString(format)}');");
            }
            else if (model is HumidityModel)
            {
                HumidityModel castModel = (HumidityModel)model;
                stringBuilder.Append($@"INSERT INTO dbo.Humidity
                                        ([SensorId],[Humidity],[Date])
                                        VALUES ('{castModel.Id}',
                                               '{castModel.Humidity}',
                                               '{castModel.Timestamp.ToString(format)}');");
            }
            else if (model is MotionModel)
            {
                MotionModel castModel = (MotionModel)model;
                stringBuilder.Append($@"INSERT INTO dbo.Motion
                                        ([SensorId],[Motion],[Date])
                                        VALUES ('{castModel.Id}',
                                               '{Convert.ToInt32(castModel.Motion)}',
                                               '{castModel.Timestamp.ToString(format)}');");
            }

            var query = stringBuilder.ToString();

            if (!string.IsNullOrEmpty(query))
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var result = _databaseHelper.Connection.ExecuteAsync(query);

                    await result;
                }
            }

        }


        /// <summary>
        /// Implementation of method to query for all temperatures
        /// </summary>
        /// <typeparam name="TemperatureModel"></typeparam>
        /// <returns></returns>
        public IEnumerable<TemperatureModel> QueryAllTemperatures<TemperatureModel>()
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var stringBuilder = new StringBuilder();
                var query = stringBuilder.ToString();
                // DO SOME QUERIES
                var result = _databaseHelper.Connection.Query<TemperatureModel>(query);
                return result;
            }
        }

        /// <summary>
        /// Implementation of method to query for single temperature by Guid
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public TemperatureModel QuerySingleTemperatures(Guid? Id)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var stringBuilder = new StringBuilder();
                var query = stringBuilder.ToString();
                // DO SOME QUERIES
                var result = _databaseHelper.Connection.Query<TemperatureModel>(query).FirstOrDefault();
                return result;
            }
        }

        /// <summary>
        /// Implementation of method to query for temperatures between two datetime
        /// </summary>
        /// <typeparam name="TemperatureModel"></typeparam>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IEnumerable<TemperatureModel> QueryDateRangeTemperatures<TemperatureModel>(DateTime startDate, DateTime endDate)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var stringBuilder = new StringBuilder();
                var query = stringBuilder.ToString();
                // DO SOME QUERIES
                var results = _databaseHelper.Connection.Query<TemperatureModel>(query);
                return results;
            }
        }
    }
}
