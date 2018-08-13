using Dapper;
using INFT3970Project.Entities;
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

namespace INFT3970Project.Helpers
{
    public class DatabaseHelper : IDisposable
    {
        private readonly IOptions<ServiceSettings> _serviceSettings;
        public IDbConnection connection { get; set; }

        public DatabaseHelper()
        {
            connection = new SqlConnection(_serviceSettings.Value.AzureConnectionString);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async void CreateRecord(TemperatureModel temperatureModel)
        {
            using (var _databaseHelper = new DatabaseHelper())
            {
                _databaseHelper.connection.Open();
                var stringBuilder = new StringBuilder();
                var query = stringBuilder.ToString();
                // DO SOME QUERIES
                var result = _databaseHelper.connection.ExecuteAsync(query);

                await result;
            }
        }


        /// <summary>
        /// Implementation of method to query for all temperatures
        /// </summary>
        /// <typeparam name="TemperatureModel"></typeparam>
        /// <returns></returns>
        public IEnumerable<TemperatureModel> QueryAllTemperatures<TemperatureModel>()
        {
            using (var _databaseHelper = new DatabaseHelper())
            {
                _databaseHelper.connection.Open();
                var stringBuilder = new StringBuilder();
                var query = stringBuilder.ToString();
                // DO SOME QUERIES
                var result = _databaseHelper.connection.Query<TemperatureModel>(query);
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
            using (var _databaseHelper = new DatabaseHelper())
            {
                _databaseHelper.connection.Open();
                var stringBuilder = new StringBuilder();
                var query = stringBuilder.ToString();
                // DO SOME QUERIES
                var result = _databaseHelper.connection.Query<TemperatureModel>(query).FirstOrDefault();
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
            using (var _databaseHelper = new DatabaseHelper())
            {
                _databaseHelper.connection.Open();
                var stringBuilder = new StringBuilder();
                var query = stringBuilder.ToString();
                // DO SOME QUERIES
                var results = _databaseHelper.connection.Query<TemperatureModel>(query);
                return results;
            }
        }
    }
}
