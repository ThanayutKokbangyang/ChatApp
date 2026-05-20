using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ChatApp.AuthService.Infrastructure.Data
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AuthDb") ?? throw new InvalidOperationException("Connection string 'AuthDb' not found.");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
