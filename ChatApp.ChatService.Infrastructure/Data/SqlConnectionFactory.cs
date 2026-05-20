using ChatApp.ChatService.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Infrastructure.Data
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChatDb")
                ?? throw new InvalidOperationException("Connection string 'ChatDb' not found");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
