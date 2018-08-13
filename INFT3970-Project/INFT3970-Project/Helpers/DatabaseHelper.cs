using INFT3970Project.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Helpers
{
    public class DatabaseHelper:IDisposable
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
    }
}
