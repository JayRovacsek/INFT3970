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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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


        ///   REGISTER

        public bool Authenticate(RegisterModel model)
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
                        command.ExecuteNonQuery();

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
                var command = new SqlCommand("SELECT * FROM [Sensor];");
                var results = _databaseHelper.Connection.Query<SensorModel>(command.ToString());
                return results;
            }
        }

        public bool RegisterAccount(string username, string password)
        {
            return false;
        }

        public UserSensorAndPasswordViewModel GetUserSensorAndPasswordViewModel(int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = new SqlCommand($"SELECT * FROM [Sensor] WHERE [UserID]={userId};");
                var sensors = _databaseHelper.Connection.Query<SensorModel>(command.ToString()).ToList();

                command = new SqlCommand($"SELECT * FROM [Users] WHERE [UserID]={userId};");
                var user = _databaseHelper.Connection.Query<UserModel>(command.ToString()).FirstOrDefault();

                command = new SqlCommand($"SELECT * FROM [UsersPassword] WHERE [UserID]={userId};");
                var password = _databaseHelper.Connection.Query<UserPasswordModel>(command.ToString()).FirstOrDefault();
                return new UserSensorAndPasswordViewModel()
                {
                    User = user,
                    Password = password,
                    Sensors = sensors
                };
            }
        }
    }
}
