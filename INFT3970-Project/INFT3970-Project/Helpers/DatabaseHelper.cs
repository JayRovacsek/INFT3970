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
            if(Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }

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
        public async void CreateRecordAsync(object model)
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
                var command = new SqlCommand
                {
                    CommandText = $"SELECT TOP(1) [UserID] FROM [Users] WHERE [Email] = @Username;",
                    Connection = (SqlConnection)_databaseHelper.Connection,
                };

                command.Parameters.AddWithValue("@Username", username);

                command.Connection.Open();
                var result = await command.ExecuteScalarAsync();

                return (int)result;

                //var result = _databaseHelper.Connection.Query<int>($"SELECT [UserID] FROM [Users] WHERE [Email] = '{username}';");
                //return result.FirstOrDefault();
            }
        }

        public int GetUserId(string username)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var command = new SqlCommand
                {
                    CommandText = $"SELECT TOP(1) [UserID] FROM [Users] WHERE [Email] = @Username;",
                    Connection = (SqlConnection)_databaseHelper.Connection
                };

                command.Parameters.AddWithValue("@Username", username);

                command.Connection.Open();
                var result = command.ExecuteScalar();

                return (int)result;
            }
        }

        public bool IsUserAdministrator(int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var command = new SqlCommand
                {
                    CommandText = $"SELECT TOP(1) [IsAdmin] FROM [Users] WHERE [UserID] = @UserId;",
                    Connection = (SqlConnection)_databaseHelper.Connection
                };

                command.Parameters.AddWithValue("@UserId", userId);

                command.Connection.Open();
                var result = command.ExecuteScalar();

                return ((string)result == "N") ? false : true;
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

        public async Task<bool> RegisterAsync(RegisterModel model)
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

        public async Task<IEnumerable<SensorModel>> QueryUserSensorsAsync(UserModel model)
        {
            if (model != null)
            {
                try
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        var command = $"SELECT * FROM [Sensor] WHERE [Sensor].[UserID] = {model.UserId};";

                        return await _databaseHelper.Connection.QueryAsync<SensorModel>(command);
                    }
                }
                catch (Exception exception)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                    }

                    return null;
                }
            }
            return null;
        }

        public async Task<IEnumerable<SensorModel>> QueryUserSensorsAsync(int userId)
        {
            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    var command = $"SELECT * FROM [Sensor] WHERE [Sensor].[UserID] = {userId};";

                    return await _databaseHelper.Connection.QueryAsync<SensorModel>(command);
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
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
        public async Task<IEnumerable<AverageTemperatureModelWithId>> QueryAllTemperatureAsync(DateTime? startTime, DateTime? endTime)
        {
            startTime = startTime.Equals(null) ? DateTime.Now.Subtract(TimeSpan.FromHours(24)) : startTime;
            endTime = endTime.Equals(null) ? DateTime.Now : endTime;

            var sensors = QueryAllSensors();

            var results = new List<AverageTemperatureModelWithId>();

            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();

                    foreach (var sensor in sensors)
                    {
                        var command = $@"SELECT AVG(Temp) AS 'Temperature', StartTime, EndTime
	                                    FROM (
		                                    SELECT TempID, StartTime, Temp, StartTime + '00:59:59' AS EndTime
		                                        FROM (
				                                        SELECT TempID, DATEADD(hh,DATEDIFF(hh,0,t.[Date]),0) AS StartTime, Temp, s.SensorID
				                                        FROM Temperature t
				                                        INNER JOIN Sensor s ON  s.SensorID = t.SensorID
				                                        WHERE s.SensorID = {sensor.SensorId} AND t.[Date] 
                                                        BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' 
                                                        AND '{endTime.Value.ToUniversalTime().ToString(format)}'
				                                        GROUP BY t.TempID, t.[Date], t.Temp, s.SensorID	  
				                                    ) 
				                                    Temperature
				                                    INNER JOIN Sensor s ON s.SensorID = Temperature.SensorId 
		                                        GROUP BY TempID, StartTime, Temp 
		                                    )
		                                    Temperature
	                                    WHERE StartTime BETWEEN StartTime AND EndTime
	                                    GROUP BY StartTime, EndTime
	                                    ORDER BY EndTime";

                        var dbResult = _databaseHelper.Connection.Query<AverageTemperatureModel>(command);

                        foreach (var result in dbResult)
                        {
                            results.Add(new AverageTemperatureModelWithId
                            {
                                SensorId = sensor.SensorId,
                                Temperature = result.Temperature,
                                StartTime = result.StartTime,
                                EndTime = result.EndTime
                            });
                        }
                    }
                    return results;
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
        }

        public IEnumerable<RoomModel> QuerySensorLocation(int sensorId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var query = $@"SELECT * FROM [Sensor] WHERE [SensorID]={sensorId};";
                var result = _databaseHelper.Connection.Query<RoomModel>(query.ToString());
                return result;
            }
        }

        public async Task<IEnumerable<AverageTemperatureModelWithId>> QueryUserTemperatureAsync(int userId, bool demo, DateTime? startTime, DateTime? endTime)
        {
            startTime = startTime.Equals(null) ? DateTime.Now.Subtract(TimeSpan.FromHours(24)) : startTime;
            endTime = endTime.Equals(null) ? DateTime.Now : endTime;

            var sensors = await QueryUserSensorsAsync(userId);

            var results = new List<AverageTemperatureModelWithId>();

            try
            {
                if (userId != 0)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Connection.Open();

                        foreach (var sensor in sensors)
                        {
                            var command = string.Empty;

                            if (demo)
                            {
                                command = $@"SELECT AVG(Temp) AS 'Temperature', StartTime, EndTime
                                            FROM (
	                                            SELECT TempID, StartTime, Temp, StartTime + '00:01:00' AS EndTime
		                                            FROM (
				                                            SELECT TempID, DATEADD(MINUTE,DATEDIFF(MINUTE,0,t.[Date]),0) AS StartTime, Temp, s.SensorID
				                                            FROM Temperature t
				                                            INNER JOIN Sensor s ON  s.SensorID = t.SensorID
				                                            WHERE s.SensorID = {sensor.SensorId} AND t.[Date] 
                                                            BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' 
                                                            AND '{endTime.Value.ToUniversalTime().ToString(format)}'
				                                            GROUP BY t.TempID, t.[Date], t.Temp, s.SensorID	  
			                                            ) 
			                                            Temperature
			                                            INNER JOIN Sensor s ON s.SensorID = Temperature.SensorId 
		                                            GROUP BY TempID, StartTime, Temp 
	                                            )
	                                            Temperature
                                            WHERE StartTime BETWEEN StartTime AND EndTime
                                            GROUP BY StartTime, EndTime
                                            ORDER BY EndTime";
                            }
                            else
                            {
                                command = $@"SELECT AVG(Temp) AS 'Temperature', StartTime, EndTime
	                                        FROM (
		                                        SELECT TempID, StartTime, Temp, StartTime + '00:59:59' AS EndTime
		                                            FROM (
				                                            SELECT TempID, DATEADD(hh,DATEDIFF(hh,0,t.[Date]),0) AS StartTime, Temp, s.SensorID
				                                            FROM Temperature t
				                                            INNER JOIN Sensor s ON  s.SensorID = t.SensorID
				                                            WHERE s.SensorID = {sensor.SensorId} AND t.[Date] 
                                                            BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' 
                                                            AND '{endTime.Value.ToUniversalTime().ToString(format)}'
				                                            GROUP BY t.TempID, t.[Date], t.Temp, s.SensorID	  
				                                        ) 
				                                        Temperature
				                                        INNER JOIN Sensor s ON s.SensorID = Temperature.SensorId 
		                                            GROUP BY TempID, StartTime, Temp 
		                                        )
		                                        Temperature
	                                        WHERE StartTime BETWEEN StartTime AND EndTime
	                                        GROUP BY StartTime, EndTime
	                                        ORDER BY EndTime";
                            }

                            var dbResult = _databaseHelper.Connection.Query<AverageTemperatureModel>(command);

                            foreach (var result in dbResult)
                            {
                                results.Add(new AverageTemperatureModelWithId
                                {
                                    SensorId = sensor.SensorId,
                                    Temperature = result.Temperature,
                                    StartTime = result.StartTime,
                                    EndTime = result.EndTime
                                });
                            }
                        }
                        return results;
                    }
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
            return null;
        }

        public async Task<IEnumerable<AverageHumidityModelWithId>> QueryAllHumidityAsync(DateTime? startTime, DateTime? endTime)
        {
            startTime = startTime.Equals(null) ? DateTime.Now.Subtract(TimeSpan.FromHours(24)) : startTime;
            endTime = endTime.Equals(null) ? DateTime.Now : endTime;

            var sensors = QueryAllSensors();

            var results = new List<AverageHumidityModelWithId>();

            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();

                    foreach (var sensor in sensors)
                    {
                        var command = $@"SELECT AVG(Humidity) AS 'Humidity', StartTime, EndTime	
                                        FROM (
		                                    SELECT HumidityID, StartTime, Humidity, StartTime + '00:59:59' AS EndTime		   
                                            FROM (
				                                SELECT HumidityID, DATEADD(hh,DATEDIFF(hh,0,t.[Date]),0) AS StartTime, Humidity, s.SensorID, s.UserID
				                               FROM Humidity t
				                               INNER JOIN Sensor s ON  s.SensorID = t.SensorID
				                               WHERE s.SensorID = {sensor.SensorId} AND t.[Date] 
                                                BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' 
                                                AND '{endTime.Value.ToUniversalTime().ToString(format)}'
				                               GROUP BY t.HumidityID, t.[Date], t.Humidity, s.SensorID, s.UserID) Humidity				
                                            INNER JOIN Sensor s ON s.SensorID = Humidity.SensorId 
		                                    GROUP BY HumidityID, StartTime, Humidity) Humidity
	                                    WHERE StartTime BETWEEN StartTime AND EndTime
	                                    GROUP BY StartTime, EndTime
                                    ORDER BY EndTime";

                        var dbResult = _databaseHelper.Connection.Query<AverageHumidityModel>(command);

                        foreach (var result in dbResult)
                        {
                            results.Add(new AverageHumidityModelWithId
                            {
                                SensorId = sensor.SensorId,
                                Humidity = result.Humidity,
                                StartTime = result.StartTime,
                                EndTime = result.EndTime
                            });
                        }
                    }
                    return results;
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
        }

        public async Task<IEnumerable<AverageHumidityModelWithId>> QueryUserHumidityAsync(int userId, bool demo, DateTime? startTime, DateTime? endTime)
        {
            startTime = startTime.Equals(null) ? DateTime.Now.Subtract(TimeSpan.FromHours(24)) : startTime;
            endTime = endTime.Equals(null) ? DateTime.Now : endTime;

            var userSensors = QueryUserSensorsAsync(userId);

            var results = new List<AverageHumidityModelWithId>();

            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();
                    var sensors = await userSensors;
                    foreach (var sensor in sensors)
                    {
                        var command = string.Empty;

                        if (demo)
                        {
                            command = $@"SELECT AVG(Humidity) AS 'Humidity', StartTime, EndTime	
                                        FROM (
		                                    SELECT HumidityID, StartTime, Humidity, StartTime + '00:01:00' AS EndTime		   
                                            FROM (
				                                SELECT HumidityID, DATEADD(MINUTE,DATEDIFF(MINUTE,0,t.[Date]),0) AS StartTime, Humidity, s.SensorID, s.UserID
				                               FROM Humidity t
				                               INNER JOIN Sensor s ON  s.SensorID = t.SensorID
				                               WHERE s.SensorID = {sensor.SensorId} AND t.[Date] 
                                                BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' 
                                                AND '{endTime.Value.ToUniversalTime().ToString(format)}'
				                               GROUP BY t.HumidityID, t.[Date], t.Humidity, s.SensorID, s.UserID) Humidity				
                                            INNER JOIN Sensor s ON s.SensorID = Humidity.SensorId 
		                                    GROUP BY HumidityID, StartTime, Humidity) Humidity
	                                    WHERE StartTime BETWEEN StartTime AND EndTime
	                                    GROUP BY StartTime, EndTime
                                    ORDER BY EndTime";
                        }
                        else
                        {
                            command = $@"SELECT AVG(Humidity) AS 'Humidity', StartTime, EndTime	
                                        FROM (
		                                    SELECT HumidityID, StartTime, Humidity, StartTime + '00:59:59' AS EndTime		   
                                            FROM (
				                                SELECT HumidityID, DATEADD(hh,DATEDIFF(hh,0,t.[Date]),0) AS StartTime, Humidity, s.SensorID, s.UserID
				                               FROM Humidity t
				                               INNER JOIN Sensor s ON  s.SensorID = t.SensorID
				                               WHERE s.SensorID = {sensor.SensorId} AND t.[Date] 
                                                BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' 
                                                AND '{endTime.Value.ToUniversalTime().ToString(format)}'
				                               GROUP BY t.HumidityID, t.[Date], t.Humidity, s.SensorID, s.UserID) Humidity				
                                            INNER JOIN Sensor s ON s.SensorID = Humidity.SensorId 
		                                    GROUP BY HumidityID, StartTime, Humidity) Humidity
	                                    WHERE StartTime BETWEEN StartTime AND EndTime
	                                    GROUP BY StartTime, EndTime
                                    ORDER BY EndTime";
                        }

                        var dbResult = _databaseHelper.Connection.Query<AverageHumidityModel>(command);

                        foreach (var result in dbResult)
                        {
                            results.Add(new AverageHumidityModelWithId
                            {
                                SensorId = sensor.SensorId,
                                Humidity = result.Humidity,
                                StartTime = result.StartTime,
                                EndTime = result.EndTime
                            });
                        }
                    }
                    return results;
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
        }

        public async Task<IEnumerable<DB.HumidityModel>> QueryUserHumidityAsync(int userId, int? count = 250)
        {
            try
            {
                if (userId != 0)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Connection.Open();

                        var query = (count != 0)
                            ? $@"SELECT TOP ({count}) * FROM [Humidity] h
                                INNER JOIN [Sensor] s ON h.[SensorID] = s.[SensorID]
                                WHERE s.[UserID] = {userId}
                                ORDER BY h.Date DESC;"

                            : $@"SELECT * FROM [Humidity] h
                                INNER JOIN [Sensor] s ON h.[SensorID] = s.[SensorID]
                                WHERE s.[UserID] = {userId}
                                ORDER BY h.Date DESC;";

                        var result = await _databaseHelper.Connection.QueryAsync<DB.HumidityModel>(query.ToString());
                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
            return null;
        }

        public async Task<IEnumerable<AverageHumidityModelWithId>> QueryUserHumidityAsync(int userId, DateTime? startTime, DateTime? endTime)
        {
            startTime = startTime.Equals(null) ? DateTime.Now.Subtract(TimeSpan.FromHours(24)) : startTime;
            endTime = endTime.Equals(null) ? DateTime.Now : endTime;

            var userSensors = QueryUserSensorsAsync(userId);

            var results = new List<AverageHumidityModelWithId>();

            try
            {
                if (userId != 0)
                {
                    using (var _databaseHelper = new DatabaseHelper(configuration))
                    {
                        _databaseHelper.Connection.Open();

                        var sensors = await userSensors;

                        foreach (var sensor in sensors)
                        {
                            var command = $@"SELECT AVG(Humidity) AS 'Humidity', StartTime, EndTime	
                                    FROM (
		                                SELECT HumidityID, StartTime, Humidity, StartTime + '00:59:59' AS EndTime		   
                                        FROM (
				                            SELECT HumidityID, DATEADD(hh,DATEDIFF(hh,0,t.[Date]),0) AS StartTime, Humidity, s.SensorID, s.UserID
				                            FROM Humidity t
				                            INNER JOIN Sensor s ON  s.SensorID = t.SensorID
				                            WHERE s.SensorID = {sensor.SensorId} AND t.[Date] 
                                            BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' AND '{endTime.Value.ToUniversalTime().ToString(format)}'
				                            GROUP BY t.HumidityID, t.[Date], t.Humidity, s.SensorID, s.UserID) Humidity				
                                        INNER JOIN Sensor s ON s.SensorID = Humidity.SensorId 
		                                GROUP BY HumidityID, StartTime, Humidity) Humidity
	                                WHERE StartTime BETWEEN StartTime AND EndTime
	                                GROUP BY StartTime, EndTime
                                ORDER BY EndTime";

                            var dbResult = _databaseHelper.Connection.Query<AverageHumidityModel>(command);

                            foreach (var result in dbResult)
                            {
                                results.Add(new AverageHumidityModelWithId
                                {
                                    SensorId = sensor.SensorId,
                                    Humidity = result.Humidity,
                                    StartTime = result.StartTime,
                                    EndTime = result.EndTime
                                });
                            }
                        }
                        return results;
                    }
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
            return null;
        }

        public async Task<IEnumerable<MotionCountModelWithId>> QueryAllMotionAsync(DateTime? startTime, DateTime? endTime)
        {
            startTime = startTime.Equals(null) ? DateTime.Now.Subtract(TimeSpan.FromHours(24)) : startTime;
            endTime = endTime.Equals(null) ? DateTime.Now : endTime;

            var sensors = QueryAllSensors();

            var results = new List<MotionCountModelWithId>();

            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();

                    foreach (var sensor in sensors)
                    {
                        var command = $@"SELECT Count(Motion) AS MotionCount, StartTime, EndTime
	                                    FROM (
		                                    SELECT MotionID, StartTime, Motion, StartTime + '00:59:59' AS EndTime
		                                        FROM (
				                                    SELECT MotionID, DATEADD(hh,DATEDIFF(hh,0,m.[Date]),0) AS StartTime, Motion, s.SensorID
                                                        FROM Motion m
                                                        INNER JOIN Sensor s ON  s.SensorID = m.SensorID
                                                        WHERE s.SensorID = {sensor.SensorId} AND m.[Date] 
                                                        BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' 
                                                        AND '{endTime.Value.ToUniversalTime().ToString(format)}'
                                                        GROUP BY m.MotionID, m.[Date], m.Motion, s.SensorID) Motion
				                                    INNER JOIN Sensor s ON s.SensorID = Motion.SensorId 
		                                      GROUP BY MotionID, StartTime, Motion) Motion
	                                    WHERE StartTime between StartTime and EndTime
	                                    GROUP BY StartTime, EndTime
	                                    ORDER BY EndTime";

                        var dbResult = _databaseHelper.Connection.Query<MotionCountModel>(command);

                        foreach (var result in dbResult)
                        {
                            results.Add(new MotionCountModelWithId
                            {
                                SensorId = sensor.SensorId,
                                MotionCount = result.MotionCount,
                                StartTime = result.StartTime,
                                EndTime = result.EndTime
                            });
                        }
                    }
                    return results;
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
        }

        public async Task<IEnumerable<MotionCountModelWithId>> QueryUserMotionAsync(int userId, DateTime? startTime, DateTime? endTime)
        {
            startTime = startTime.Equals(null) ? DateTime.Now.Subtract(TimeSpan.FromHours(24)) : startTime;
            endTime = endTime.Equals(null) ? DateTime.Now : endTime;

            var userSensors = QueryUserSensorsAsync(userId);

            var results = new List<MotionCountModelWithId>();

            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();

                    var sensors = await userSensors;

                    foreach (var sensor in sensors)
                    {
                        var command = $@"SELECT Count(Motion) AS MotionCount, StartTime, EndTime
	                                    FROM (
		                                    SELECT MotionID, StartTime, Motion, StartTime + '00:59:59' AS EndTime
		                                        FROM (
				                                    SELECT MotionID, DATEADD(hh,DATEDIFF(hh,0,m.[Date]),0) AS StartTime, Motion, s.SensorID
                                                        FROM Motion m
                                                        INNER JOIN Sensor s ON  s.SensorID = m.SensorID
                                                        WHERE s.SensorID = {sensor.SensorId} AND m.[Date] 
                                                        BETWEEN '{startTime.Value.ToUniversalTime().ToString(format)}' 
                                                        AND '{endTime.Value.ToUniversalTime().ToString(format)}'
                                                        GROUP BY m.MotionID, m.[Date], m.Motion, s.SensorID) Motion
				                                    INNER JOIN Sensor s ON s.SensorID = Motion.SensorId 
		                                      GROUP BY MotionID, StartTime, Motion) Motion
	                                    WHERE StartTime between StartTime and EndTime
	                                    GROUP BY StartTime, EndTime
	                                    ORDER BY EndTime";

                        var dbResult = _databaseHelper.Connection.Query<MotionCountModel>(command);

                        foreach (var result in dbResult)
                        {
                            results.Add(new MotionCountModelWithId
                            {
                                SensorId = sensor.SensorId,
                                MotionCount = result.MotionCount,
                                StartTime = result.StartTime,
                                EndTime = result.EndTime
                            });
                        }
                    }
                    return results;
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
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

        public async Task<string> QueryUserEmailAsync(int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var query = $"SELECT [Email] FROM [Users] WHERE [UserID] = {userId}";

                var result = await _databaseHelper.Connection.QueryAsync<string>(query);

                return result.FirstOrDefault();
            }
        }

        public bool UpdateUserDetails(UpdateUserDetailsModel model)
        {
            if (model.fName != null && model.Country != null)
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
                            CommandText = "dbo.UpdateUser"
                        };
                        command.Parameters.AddWithValue("@UserID", model.UserID);
                        command.Parameters.AddWithValue("@fName", model.fName);
                        command.Parameters.AddWithValue("@lName", model.lName);
                        command.Parameters.AddWithValue("@ContactNumber", model.ContactNumber); ;
                        command.Parameters.AddWithValue("@StreetNum", model.StreetNum);
                        command.Parameters.AddWithValue("@StreetName", model.StreetName);
                        command.Parameters.AddWithValue("@Postcode", model.Postcode);
                        command.Parameters.AddWithValue("@City", model.City);
                        command.Parameters.AddWithValue("@State", model.State);
                        command.Parameters.AddWithValue("@Country", model.Country);

                        SqlParameter output = new SqlParameter("@responseMessage", SqlDbType.VarChar);
                        output.Direction = ParameterDirection.Output;
                        output.Size = 255;
                        command.Parameters.Add(output);

                        command.Connection.Open();
                        command.ExecuteNonQuery();

                        var response = command.Parameters["@responseMessage"].Value;

                        var valid = (response.ToString() == "Invalid login Details") ? false :
                            (response.ToString() == "Wrong Details") ? false : true;

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
                            _databaseHelper.Log("Infomation", $"User Details Updated: {model.Email}", null, GetCurrentMethod());
                        }
                    }
                }
            }
            return false;
        }


        public async Task<IEnumerable<UpdateUserDetailsModel>> QueryUserDetailsAsync(int userId)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var query = new StringBuilder($@"SELECT u.UserID, u.fName, u.lName, u.ContactNumber,
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
            if (model != null)
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
                            _databaseHelper.Log("Infomation", $"Sensor Modified: {model.SensorId}", null, GetCurrentMethod());
                        }
                    }
                }
            }
            return false;
        }


        public bool AddRoom(RoomModel model)
        {
            if (model.Name != null && model.Description != null)
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
                            CommandText = "dbo.AddRoom"
                        };
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Description", model.Description);


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
                            _databaseHelper.Log("Infomation", $"Sensor Modified: {model.Name}", null, GetCurrentMethod());
                        }
                    }
                }
            }
            return false;
        }


        public IEnumerable<UserModel> QueryAllUsers()
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = new StringBuilder("SELECT UserID, fName FROM [dbo].[Users];");

                var results = _databaseHelper.Connection.Query<UserModel>(command.ToString());
                return results;
            }
        }

        public IEnumerable<RoomModel> QueryAllRooms()
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = new StringBuilder("SELECT RoomID, Name FROM [dbo].[Room];");

                var results = _databaseHelper.Connection.Query<RoomModel>(command.ToString());
                return results;
            }
        }

        public IEnumerable<RoomModel> DeleteRoom(RoomModel model)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = new StringBuilder($"DELETE FROM Room WHERE RoomId = {model.RoomID};");


                var results = _databaseHelper.Connection.Query<RoomModel>(command.ToString());
                return results;
            }
        }

        public IEnumerable<SensorModel> DeleteSensor(SensorModel model)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = new StringBuilder($"DELETE FROM Sensor WHERE SensorID = {model.SensorId};");

                var results = _databaseHelper.Connection.Query<SensorModel>(command.ToString());
                return results;
            }
        }

        public IEnumerable<LogModel> QueryAllLogs(int? count = 250)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = $"SELECT TOP({count}) * FROM Logs ORDER BY Id DESC;";

                var results = _databaseHelper.Connection.Query<LogModel>(command);

                return results;
            }
        }

        public async Task<IEnumerable<DB.TemperatureModel>> QuerySensorTemperature(int sensorId, int? count = 250)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = $@"SELECT TOP({count}) * 
                                FROM [Temperature] 
                                WHERE [SensorID] = {sensorId}
                                ORDER BY [TempID] DESC;";

                var results = await _databaseHelper.Connection.QueryAsync<DB.TemperatureModel>(command.ToString());

                return results;
            }
        }

        public async Task<IEnumerable<DB.HumidityModel>> QuerySensorHumidity(int sensorId, int? count = 250)
        {
            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                _databaseHelper.Connection.Open();
                var command = $@"SELECT TOP({count}) * 
                                FROM [Humidity] 
                                WHERE [SensorID] = {sensorId}
                                ORDER BY [HumidityID] DESC;";

                var results = await _databaseHelper.Connection.QueryAsync<DB.HumidityModel>(command.ToString());

                return results;
            }
        }

        public async Task<IEnumerable<DB.MotionModel>> QuerySensorMotion(int sensorId, DateTime? startDate, DateTime? endDate, int? count = 250)
        {
            startDate = startDate.Equals(null) ? DateTime.Now.Subtract(TimeSpan.FromHours(24)) : startDate;
            endDate = startDate.Equals(null) ? DateTime.Now : endDate;

            using (var _databaseHelper = new DatabaseHelper(configuration))
            {
                var userID = 2;
                var sensorID = 9;
                var command = new SqlCommand
                {
                    Connection = (SqlConnection)_databaseHelper.Connection,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "dbo.MotionCount"
                };
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@SensorID", sensorID);
                command.Parameters.AddWithValue("@SearchStartTime", startDate);
                command.Parameters.AddWithValue("@SearchEndTime", endDate);

                // The return of the proc needs to either being a datareader which you parse, or you should define a model to use.
                //var results = await _databaseHelper.Connection.QueryAsync<DB.MotionModel>(command.ToString());

                return null;
            }
        }

        //Home Page Top Temp and Huminity 
        public async Task<IEnumerable<CurrentTempModel>> QueryCurrentAsync(int userId)
        {
            var sensors = await QueryUserSensorsAsync(userId);
            var results = new List<CurrentTempModel>();

            foreach (var sensor in sensors)
            {
                var humidityResults = await QuerySensorHumidity(sensor.SensorId, 1);
                var temperatureResults = await QuerySensorTemperature(sensor.SensorId, 1);
                var locationResults = QuerySensorLocation(sensor.SensorId);

                if (humidityResults.Any() && temperatureResults.Any() && locationResults.Any())

                    results.Add(new CurrentTempModel
                    {
                        Humidity = humidityResults.FirstOrDefault().Humidity,
                        Temperature = temperatureResults.FirstOrDefault().Temp,
                        RoomName = locationResults.FirstOrDefault().Name,
                        SensorName = sensor.Description
                    });
            }

            return results;
        }

        public async Task<IEnumerable<AverageTemperatureModelWithId>> QueryPredicitiveTemperatureAsync(int userId)
        {
            var userSensors = QueryUserSensorsAsync(userId);
            var results = new List<AverageTemperatureModelWithId>();

            try
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Connection.Open();

                    var sensors = await userSensors;

                    foreach (var sensor in sensors)
                    {
                        var command = $@"DECLARE @SearchEndTime datetime;
                                        SET @SearchEndTime = CURRENT_TIMESTAMP 
                                        DECLARE @SearchStartTime datetime;
                                        SET @SearchStartTime = DATEADD(week,-1,@SearchEndTime)

		                                SELECT StartTime, LAG(PredictedValue, 1, PredictedValue) OVER (ORDER BY StartTime) + (LAG(PredictedValue, 1, 0) OVER (ORDER BY StartTime) * (PercentChange)) AS 'Temperature'
		                                FROM (
		                                    SELECT HourlyAverage, StartTime,  PercentChange, ID, HourlyAverage as PredictedValue
		                                    FROM (
						                        SELECT HourlyAverage, StartTime,- 1 * (1 - Lag(HourlyAverage, 1, 0) OVER (Order by StartTime) / HourlyAverage) AS PercentChange, ID, EndTime
						                        FROM (
								                    SELECT AVG(Temp) AS HourlyAverage, StartTime, ROW_NUMBER() OVER (ORDER BY StartTime) AS ID, EndTime
								                    FROM (
										                SELECT TempID, StartTime, Temp, StartTime + '00:59:59' AS EndTime
										                FROM (
										                    SELECT TempID, DATEADD(hh,DATEDIFF(hh,0,t.[Date]),0) AS StartTime, Temp, s.SensorID
										                    FROM Temperature t
										                    INNER JOIN Sensor s ON  s.SensorID = t.SensorID
										                    WHERE t.SensorID = {sensor.SensorId} and  t.[Date] BETWEEN @SearchStartTime AND @SearchEndTime 
										                    GROUP BY t.TempID, t.[Date], t.Temp, s.SensorID ) Temperature
										                INNER JOIN Sensor s ON s.SensorID = Temperature.SensorId 
									                    GROUP BY TempID, StartTime, Temp ) Temperature
								                        WHERE StartTime BETWEEN StartTime AND EndTime
								                    GROUP BY StartTime, EndTime ) Temperature
							                        WHERE StartTime BETWEEN StartTime AND EndTime
							                    GROUP BY HourlyAverage, StartTime, ID, EndTime )			
					                        Temperature
					                        GROUP BY HourlyAverage, StartTime, PercentChange, ID ) Temperature
                                            ORDER BY StartTime";

                        var dbResult = _databaseHelper.Connection.Query<AverageTemperatureModel>(command);

                        foreach (var result in dbResult)
                        {
                            results.Add(new AverageTemperatureModelWithId
                            {
                                SensorId = sensor.SensorId,
                                Temperature = result.Temperature,
                                StartTime = result.StartTime,
                                EndTime = result.EndTime
                            });
                        }
                    }
                    return results;
                }
            }
            catch (Exception exception)
            {
                using (var _databaseHelper = new DatabaseHelper(configuration))
                {
                    _databaseHelper.Log("Fatal", exception.Message, null, GetCurrentMethod());
                }

                return null;
            }
        }
    }
}
