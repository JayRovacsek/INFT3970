using Dapper;
using INFT3970Project.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
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

                        var valid = (response.ToString() == "0") ? false : true;

                        if (valid)
                        {
                            _databaseHelper.Log("Infomation", $"User Login: {model.Username}", null, GetCurrentMethod());
                            return valid;
                        }
                        else
                        {
                            _databaseHelper.Log("Warning", $"User Login Failure: {model.Username}", null, GetCurrentMethod());
                            return valid;
                        }
                    }
                }
                catch (Exception exception)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Log("Fatal", exception.Message, null, exception.TargetSite.Name);
                    }
                        return false;
                }
            }
            return false;
        }

        public async Task<bool> Register(RegisterModel model)
        {
            if (model.fName != null && model.Password != null)
            {
                var success = false;
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

                        success = true;

                        return valid;
                    }
                }
                catch (Exception exception)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                    }
                    success = false;
                    return false;
                }
                finally
                {
                    if (success)
                    {
                        using (var _databaseHelper = new DatabaseHelper(configuration))
                        {
                            _databaseHelper.Log("Infomation", $"User Registered: {model.Email}", null, GetCurrentMethod());
                        }
                    }
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
                    return null;
                }
            }
            return null;
        }

        public bool UpdatePassword(UserAndPasswordModel model)
        {
            if (model.User.Email != null && model.UserPassword.Password != null)
            {
                var success = false;
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

                        success = true;

                        return valid;
                    }
                }
                catch (Exception exception)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                    }
                    success = false;
                    return false;
                }
                finally
                {
                    if (success)
                    {
                        using (var _databaseHelper = new DatabaseHelper(configuration))
                        {
                            _databaseHelper.Log("Infomation", $"User Updated Password: {model.User.Email}", model.User.UserId, GetCurrentMethod());
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Implementation of method to query for all temperatures
        /// </summary>
        /// <typeparam name="TemperatureModel"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<DB.TemperatureModel>> QueryAllTemperature()
        {
            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();
                    var query = new StringBuilder($@"SELECT TOP (250) * FROM [Temperature] t
                                                    INNER JOIN [Sensor] s ON t.[SensorID] = s.[SensorID]
                                                    ORDER BY t.Date desc;");
                    var result = await _databaseHelper.Connection.QueryAsync<DB.TemperatureModel>(query.ToString());
                    return result;
                }
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public string QuerySensorLocation(int sensorId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var query = new StringBuilder($@"  SELECT [Description] FROM [Sensor] WHERE [SensorID]={sensorId};");
                var result = _databaseHelper.Connection.Query<string>(query.ToString());
                return result.FirstOrDefault();
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
                        var query = new StringBuilder($@"SELECT TOP (250) * FROM [Temperature] t
                                                        INNER JOIN [Sensor] s ON t.[SensorID] = s.[SensorID]
                                                        WHERE s.[UserID] = {userId}
                                                        ORDER BY t.Date desc;");
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

        public async Task<IEnumerable<DB.HumidityModel>> QueryAllHumidity()
        {
            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();
                    var query = new StringBuilder($@"SELECT TOP (250) * FROM [Humidity] h
                                                        INNER JOIN [Sensor] s ON h.[SensorID] = s.[SensorID]
                                                        ORDER BY h.Date desc;");
                    var result = await _databaseHelper.Connection.QueryAsync<DB.HumidityModel>(query.ToString());
                    return result;
                }
            }
            catch (Exception exception)
            {
                return null;
            }
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
                        var query = new StringBuilder($@"SELECT TOP (250) * FROM [Humidity] h
                                                        INNER JOIN [Sensor] s ON h.[SensorID] = s.[SensorID]
                                                        WHERE s.[UserID] = {userId}
                                                        ORDER BY h.Date desc;");
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

        public async Task<IEnumerable<DB.MotionModel>> QueryAllMotion()
        {
            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();
                    var query = new StringBuilder($@"SELECT TOP (250) * FROM [Motion] m
                                                        INNER JOIN [Sensor] s ON m.[SensorID] = s.[SensorID]
                                                        ORDER BY m.Date desc;");
                    var result = await _databaseHelper.Connection.QueryAsync<DB.MotionModel>(query.ToString());
                    return result;
                }
            }
            catch (Exception exception)
            {
                return null;
            }
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
                        var query = new StringBuilder($@"SELECT TOP (250) * FROM [Motion] m
                                                        INNER JOIN [Sensor] s ON m.[SensorID] = s.[SensorID]
                                                        WHERE s.[UserID] = {userId}
                                                        ORDER BY m.Date desc;");
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

        public void Log(string severity, string message, int? userId, string location)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var command = new SqlCommand
                {
                    Connection = (SqlConnection)_databaseHelper.Connection,
                    CommandText = $"INSERT INTO [Logs] VALUES(@Severity,@Message,@UserId,@Location,@Datetime);"
                };

                command.Parameters.AddWithValue("@Severity", severity);
                command.Parameters.AddWithValue("@Message", message);
                command.Parameters.AddWithValue("@UserId", userId ?? 0);
                command.Parameters.AddWithValue("@Location", location);
                command.Parameters.AddWithValue("@Datetime", DateTime.Now);

                command.Connection.Open();
                command.ExecuteNonQuery();
            }

        }

        public async Task<string> QueryUserEmail (int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var query = $"SELECT [Email] FROM [Users] WHERE [UserID] = {userId}";

                var result = await _databaseHelper.Connection.QueryAsync<string>(query);

                return result.FirstOrDefault();
            }
        }

        /*public async Task<string> QueryUserDetails(int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var query = $"SELECT [fName,lName,ContactNumber,Email, StreetNum, StreetName,City,State,Postcode,Country] FROM [Users u, UserAddress ua] WHERE [u.UserID = ua.UserID AND u.UserID ] = {userId}";

                var result = await _databaseHelper.Connection.QueryAsync<string>(query);

                return result.FirstOrDefault();
            }
        }
        */
        public async Task<IEnumerable<UpdateUserDetailsModel>> QueryUserDetails(int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var query = new StringBuilder($@"SELECT u.fName, u.lName, u.ContactNumber,
                                                u.Email, ua.StreetNum, ua.StreetName,
                                                ua.City, ua.State, ua.Postcode, ua.Country
                                                FROM Users u 
                                                INNER JOIN UsersAddress ua ON u.UserID = ua.UserID 
                                                WHERE u.UserID = {userId};");
                var results = await _databaseHelper.Connection.QueryAsync<UpdateUserDetailsModel>(query.ToString());

                return results;
            }
        }


        // Admin Section
        public bool AddSensor(SensorModel model)
        {
            if (model.UserId != null && model.RoomId != null)
            {
                var success = false;
                try
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var command = new SqlCommand
                        {
                            Connection = (SqlConnection)_databaseHelper.Connection,
                            CommandType = CommandType.StoredProcedure,
                            CommandText = "dbo.AddSensor"
                        };
                        command.Parameters.AddWithValue("@UserID", model.UserId);
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);
                        command.Parameters.AddWithValue("@RoomID", model.RoomId);

                        SqlParameter output = new SqlParameter("@responseMessage", SqlDbType.VarChar);
                        output.Direction = ParameterDirection.Output;
                        output.Size = 255;
                        command.Parameters.Add(output);
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                        var response = command.Parameters["@responseMessage"].Value;
                        var valid = (response.ToString() == "Invalid email") ? false :
                            (response.ToString() == "Success") ? true : false;
                        success = true;
                        return valid;
                    }
                }
                catch (Exception exception)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                    }
                    success = false;
                    return false;
                }
                finally
                {
                    if (success)
                    {
                        using (var _databaseHelper = new DatabaseHelper(configuration))
                        {
                            _databaseHelper.Log("Infomation", $"User Registered: {model.Name}", null, GetCurrentMethod());
                        }
                    }
                }
            }
            return false;
        }

    }
}
