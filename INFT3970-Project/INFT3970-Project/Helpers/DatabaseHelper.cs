using Dapper;
using INFT3970Project.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DB = INFT3970Project.Models.Database_Entities;

namespace INFT3970Project.Helpers
{
    public class DatabaseHelper : IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly string _connectionString;
        public IDbConnection Connection { get; set; }
        private bool disposed;
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

        public async Task<int> GetUserIdAsync(string username)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var result = _databaseHelper.Connection.Query<int>($"SELECT [UserID] FROM [Users] WHERE [Email] = '{username}';");
                return result.FirstOrDefault();
            }
        }

        public int GetUserId(string username)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var result = _databaseHelper.Connection.ExecuteScalar<int>($"SELECT [UserID] FROM [Users] WHERE [Email] = '{username}';");
                return result;
            }
        }

        public bool IsUserAdministrator(int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var result = _databaseHelper.Connection.ExecuteScalar<string>($"SELECT [IsAdmin] FROM [Users] WHERE [UserID] = '{userId}';");
                return (result == "N") ? false : true;
            }
        }

        public bool Authenticate(LoginModel model)
        {
            if (model.Username != null && model.Password != null)
            {
                try
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var command = new SqlCommand
                        {
                            Connection = (SqlConnection)_databaseHelper.Connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "dbo.UserLogin"
                        };

                        command.Parameters.AddWithValue("@Email", model.Username);
                        command.Parameters.AddWithValue("@Password", model.Password);

                        SqlParameter output = new SqlParameter("@responseMessage", SqlDbType.VarChar);
                        output.Direction = ParameterDirection.Output;
                        output.Size = 255;
                        command.Parameters.Add(output);

                        command.Connection.Open();
                        command.ExecuteNonQuery();

                        var response = command.Parameters["@responseMessage"].Value;

                        var valid = true;
                        if (response.ToString() == "0")
                        { valid = false; }
                        if (response.ToString() == "1")
                        { valid = true; }
                        return valid;
                    }
                }
                catch (Exception exception)
                {
                    // Need to add logger here.
                    // Need to reimplement
                    return false;
                }
            }
            return false;
        }

        

        public async Task<bool> Register(RegisterModel model)
        {
            if (model.fName != null && model.Password != null)
            {
                try
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var command = new SqlCommand
                        {
                            Connection = (SqlConnection)_databaseHelper.Connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "dbo.AddUser"
                        };

                        command.Parameters.AddWithValue("@fName", model.fName);
                        command.Parameters.AddWithValue("@lName", model.lName);
                        command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@StreetNum", model.StreetNum);
                        command.Parameters.AddWithValue("@StreetName", model.StreetName);
                        command.Parameters.AddWithValue("@Postcode", model.Postcode);
                        command.Parameters.AddWithValue("@City", model.City);
                        command.Parameters.AddWithValue("@State", model.State);
                        command.Parameters.AddWithValue("@Country", model.Country);
                        command.Parameters.AddWithValue("@HashedPassword", model.Password);

                        SqlParameter output = new SqlParameter("@responseMessage", SqlDbType.VarChar);
                        output.Direction = ParameterDirection.Output;
                        output.Size = 255;
                        command.Parameters.Add(output);

                        command.Connection.Open();
                        await command.ExecuteNonQueryAsync();

                        var response = command.Parameters["@responseMessage"].Value;

                        var valid = (response.ToString() == "Invalid login Details") ? false :
                            (response.ToString() == "Wrong Password") ? false : true;

                        return valid;
                    }
                }
                catch (Exception exception)
                {
                    // Need to add logger here.
                    // Need to reimplement
                    return false;
                }
            }
            return false;
        }

        public async Task<IEnumerable<SensorModel>> QueryUserSensors(UserModel model)
        {
            if (model != null)
            {
                try
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var command = new SqlCommand($"SELECT * FROM [Sensor] WHERE [Sensor].[UserID] = {model.UserId};");

                        return await _databaseHelper.Connection.QueryAsync<SensorModel>(command.ToString());
                    }
                }
                catch (Exception exception)
                {
                    // Need to add logger here.
                    // Need to reimplement
                    return null;
                }
            }
            return null;
        }

        public bool UpdatePassword(UserAndPasswordModel model)
        {
            if (model.User.Email != null && model.UserPassword.Password != null)
            {
                try
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var command = new SqlCommand
                        {
                            Connection = (SqlConnection)_databaseHelper.Connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "dbo.UpdatingUserPassword"
                        };

                        command.Parameters.AddWithValue("@Email", model.User.Email);
                        command.Parameters.AddWithValue("@Password", model.UserPassword.Password);


                        var output = new SqlParameter("@responseMessage", SqlDbType.VarChar)
                        {
                            Direction = ParameterDirection.Output,
                            Size = 255
                        };
                        command.Parameters.Add(output);

                        command.Connection.Open();
                        command.ExecuteNonQuery();

                        var response = command.Parameters["@responseMessage"].Value;

                        var valid = (response.ToString() == "Invalid email") ? false :
                            (response.ToString() == "Success") ? true : false;

                        return valid;
                    }
                }
                catch (Exception exception)
                {
                    // Need to add logger here.
                    // Need to reimplement
                    return false;
                }
            }
            return false;
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

        public async Task<IEnumerable<DB.TemperatureModel>> QueryUserTemperature(int userId)
        {
            try
            {
                if (userId != 0)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Connection.Open();
                        var query = new StringBuilder($@"SELECT * FROM [Temperature] t
                                                        INNER JOIN [Sensor] s ON t.[SensorID] = s.[SensorID]
                                                        WHERE s.[UserID] = {userId}");
                        var result = await _databaseHelper.Connection.QueryAsync<DB.TemperatureModel>(query.ToString());
                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                return null;
            }
            return null;
        }

        public async Task<IEnumerable<DB.HumidityModel>> QueryUserHumidity(int userId)
        {
            try
            {
                if (userId != 0)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Connection.Open();
                        var query = new StringBuilder($@"SELECT * FROM [Humidity] h
                                                        INNER JOIN [Sensor] s ON h.[SensorID] = s.[SensorID]
                                                        WHERE s.[UserID] = {userId}");
                        var result = await _databaseHelper.Connection.QueryAsync<DB.HumidityModel>(query.ToString());
                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                return null;
            }
            return null;
        }

        public async Task<IEnumerable<DB.MotionModel>> QueryUserMotion(int userId)
        {
            try
            {
                if (userId != 0)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Connection.Open();
                        var query = new StringBuilder($@"SELECT * FROM [Motion] m
                                                        INNER JOIN [Sensor] s ON m.[SensorID] = s.[SensorID]
                                                        WHERE s.[UserID] = {userId}");
                        var result = await _databaseHelper.Connection.QueryAsync<DB.MotionModel>(query.ToString());
                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Implementation of method to query for single temperature by Guid
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public TemperatureModel QuerySingleTemperature(Guid? Id)
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

        public IEnumerable<SensorModel> QueryAllSensors()
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = new StringBuilder("SELECT * FROM [dbo].[Sensor];");

                var results = _databaseHelper.Connection.Query<SensorModel>(command.ToString());
                return results;
            }
        }
    }
}
