using offer_transaction_sync.Models;
using System.Data.SqlClient;
using offer_transaction_sync.Services;
using offer_transaction_sync.Attributes;
using System.Reflection;

namespace offer_transaction_sync.Utilities
{
    public class DbHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DbHandler> _logger;

        private readonly string _entityTableName = "offer_transaction";

        public DbHandler(IConfiguration configuration, ILogger<DbHandler> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Job GetJob(string job_name)
        {
            Job job = null;
            string query = @"select id,method,definition,status,period,last_dt,next_dt,log_path from jobs where status=0 and method='" + job_name + "'";
            try
            {
                using SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DWH"));
                using SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                using SqlDataReader reader = sqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    job = new Job(
                        reader.GetByte(reader.GetOrdinal("id")),
                        reader.GetString(reader.GetOrdinal("method")),
                        reader.GetString(reader.GetOrdinal("definition")),
                        reader.GetByte(reader.GetOrdinal("status")),
                        reader.GetInt32(reader.GetOrdinal("period")),
                        reader.GetDateTime(reader.GetOrdinal("last_dt")),
                        reader.GetDateTime(reader.GetOrdinal("next_dt")),
                        reader.GetString(reader.GetOrdinal("log_path"))
                    );
                }
            }
            catch (Exception ex)
            {
                var appLog = new AppLog(
                    _AppName: "offer_transaction_sync",
                    _LogLevel: "Error",
                    _Logger: "DbHandler.GetJob",
                    _Message: $"An error occurred while getting the job '{job_name}'",
                    _Exception: ex.Message ?? string.Empty,
                    _StackTrace: ex.StackTrace ?? string.Empty,
                    _MachineName: Environment.MachineName,
                    _RequestId: 0,
                    _CreatedAt: DateTime.Now
                );


                InsertAppLog(appLog);

                throw ex;
            }
            return job;
        }

        public void UpdateJobStatus(Job job)
        {
            try
            {
                using SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DWH"));
                using SqlCommand sqlCommand = new SqlCommand(
                    @"UPDATE jobs SET next_dt = DATEADD(MINUTE, period, CURRENT_TIMESTAMP), last_dt = CURRENT_TIMESTAMP WHERE id = @Id",
                    sqlConnection
                );
                sqlCommand.Parameters.AddWithValue("@Id", job.Id);

                sqlConnection.Open();
                if (sqlCommand.ExecuteNonQuery() != 1)
                {
                    throw new Exception("Nothing was updated, affected rowcount = 0");
                }

            }
            catch (Exception ex)
            {
                var appLog = new AppLog(
                    _AppName: "offer_transaction_sync",
                    _LogLevel: "Error",
                    _Logger: "DbHandler.GetJob",
                    _Message: $"An error occurred while updating the job '{job.Method}'",
                    _Exception: ex.Message ?? string.Empty,
                    _StackTrace: ex.StackTrace ?? string.Empty,
                    _MachineName: Environment.MachineName,
                    _RequestId: 0,
                    _CreatedAt: DateTime.Now
                );

                InsertAppLog(appLog);

                throw ex;
            }
        }

        public void InsertAppLog(AppLog log)
        {
            string insertQuery = @"
            INSERT INTO AppLog (AppName, LogLevel, Logger, Message, Exception, StackTrace, MachineName, RequestId, CreatedAt)
            VALUES (@AppName, @LogLevel, @Logger, @Message, @Exception, @StackTrace, @MachineName, @RequestId, @CreatedAt)";

            try
            {
                using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DEV"));
                using SqlCommand command = new SqlCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@AppName", log.AppName);
                command.Parameters.AddWithValue("@LogLevel", log.LogLevel);
                command.Parameters.AddWithValue("@Logger", log.Logger);
                command.Parameters.AddWithValue("@Message", log.Message);
                command.Parameters.AddWithValue("@Exception", log.Exception);
                command.Parameters.AddWithValue("@StackTrace", log.StackTrace);
                command.Parameters.AddWithValue("@MachineName", log.MachineName);
                command.Parameters.AddWithValue("@RequestId", log.RequestId);
                command.Parameters.AddWithValue("@CreatedAt", log.CreatedAt);

                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting the log");
            }
        }

        public List<Entity> GetEntities()
        {
            List<Entity> entities = null;
            string query = @" SELECT

                                T.BtcERPId AS document_type,
                                O.Number AS document_number,
                                BtcValidFromDate AS valid_from,
                                BtcValidToDate AS valid_until,
                                ISNULL(B.Name,'') AS business_area,
                                A.Code AS customer,
                                ISNULL(BtcTotalWithVAT,0) AS total_amount,
                                ISNULL(PL.Name,'') AS price_list,
                                ISNULL(BtcNotesNew,'') AS note,
                                ISNULL(R.Name,'') AS reject_reason,
                                ISNULL(CS.Name,'') AS completion_status,
                                ISNULL(LTRIM(OS.Name),'') AS order_status,
                                ISNULL(PC.Name,'') AS payment_condition,
                                ISNULL(MC.Name,'') AS manager_customer,
                                ISNULL(MD.Name,'') AS manager_document,
                                CR.Name AS created_by,
                                O.Createdon AS created_at,
                                CH.Name AS changed_by,
                                O.ModifiedOn AS changed_at

                                FROM [Order] O
                                LEFT JOIN Account A ON
                                (O.AccountId = A.Id)
                                LEFT JOIN BtcFieldOfActivity B ON
                                (O.BtcFieldOfActivityId = B.Id)
                                LEFT JOIN Contact CR ON
                                (O.CreatedById = CR.Id)
                                LEFT JOIN Contact CH ON
                                (O.ModifiedById = CH.Id)
                                LEFT JOIN Contact MC ON
                                (O.OwnerId = MC.Id)
                                LEFT JOIN Contact MD ON
                                (O.BtcBackManagerId = MD.Id)
                                LEFT JOIN BtcLookupZakazERP T ON
                                (O.BtcZakazERPId = T.Id)
                                LEFT JOIN BtcRejectReason R ON
                                (O.BtcRejectReasonId = R.Id)
                                LEFT JOIN Pricelist PL ON
                                (O.BtcPriceListId = PL.Id)
                                LEFT JOIN BtcPaymentCondition PC ON
                                (O.BtcPaymentConditionId = PC.Id)
                                LEFT JOIN BtcOrderCompletionStatus CS ON
                                (O.BtcCompletionStatusId = CS.Id)
                                LEFT JOIN SysOrderStatusLcz OS ON
                                (O.StatusId = OS.RecordId
                                AND OS.SysCultureId = '1A778E3F-0A8E-E111-84A3-00155D054C03')

                                WHERE O.BtcTypeId = '91e1e208-f1b7-4743-8c7a-5819cddcecfd'
                                AND O.BtcIsDeleted = 0
                                AND LEN(A.CODE) = 6 ";
            try
            {
                using SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("CRM"));
                using SqlCommand command = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                entities = EntityService.ReadEntitiesDynamic(command);
            }
            catch (Exception ex)
            {
                var appLog = new AppLog(
                    _AppName: "offer_transaction_sync",
                    _LogLevel: "Error",
                    _Logger: "DbHandler.GetEntities",
                    _Message: $"An error occurred while getting transaction entities",
                    _Exception: ex.Message ?? string.Empty,
                    _StackTrace: ex.StackTrace ?? string.Empty,
                    _MachineName: Environment.MachineName,
                    _RequestId: 0,
                    _CreatedAt: DateTime.Now
                );


                InsertAppLog(appLog);

                throw ex;
            }
            return entities;
        }

        public List<Entity> GetExistingEntities()
        {
            List<Entity> entities = null;
            string query = $"SELECT * FROM {_entityTableName}";
            try
            {
                using SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DWH"));
                using SqlCommand command = new SqlCommand(query, sqlConnection);
                sqlConnection.Open();
                entities = EntityService.ReadEntitiesDynamic(command);
            }
            catch (Exception ex)
            {
                var appLog = new AppLog(
                    _AppName: "offer_transaction_sync",
                    _LogLevel: "Error",
                    _Logger: "DbHandler.GetExistingEntities",
                    _Message: $"An error occurred while getting existing transaction entities",
                    _Exception: ex.Message ?? string.Empty,
                    _StackTrace: ex.StackTrace ?? string.Empty,
                    _MachineName: Environment.MachineName,
                    _RequestId: 0,
                    _CreatedAt: DateTime.Now
                );


                InsertAppLog(appLog);

                throw ex;
            }
            return entities;
        }

        public void InsertEntity(Entity entity)
        {
            var properties = ReflectionHelper.GetProperties<Entity>().ToList();
            var columns = string.Join(", ", properties.Select(p => p.GetCustomAttribute<DataSourceAttribute>().SourceName));
            var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            var query = $"INSERT INTO {_entityTableName} ({columns}) VALUES ({parameters})";

            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DWH"));
            using SqlCommand command = new SqlCommand(query, connection);

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                command.Parameters.AddWithValue($"@{property.Name}", value ?? DBNull.Value);
            }

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void UpdateEntity(Entity entity)
        {
            var properties = ReflectionHelper.GetProperties<Entity>().ToList();
            var keyProperties = properties.Where(p => p.GetCustomAttribute<DataSourceAttribute>().IsKey).ToList();
            var updateProperties = properties.Except(keyProperties).ToList();

            var setClause = string.Join(", ", updateProperties.Select(p =>
                $"{p.GetCustomAttribute<DataSourceAttribute>().SourceName} = @{p.Name}"));
            var whereClause = string.Join(" AND ", keyProperties.Select(p =>
                $"{p.GetCustomAttribute<DataSourceAttribute>().SourceName} = @{p.Name}"));

            var query = $"UPDATE {_entityTableName} SET {setClause} WHERE {whereClause}";

            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DWH"));
            using SqlCommand command = new SqlCommand(query, connection);

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                command.Parameters.AddWithValue($"@{property.Name}", value ?? DBNull.Value);
            }

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void DeleteEntity(Entity entity)
        {
            var properties = ReflectionHelper.GetProperties<Entity>().ToList();
            var keyProperties = properties.Where(p => p.GetCustomAttribute<DataSourceAttribute>().IsKey).ToList();

            var whereClause = string.Join(" AND ", keyProperties.Select(p =>
                $"{p.GetCustomAttribute<DataSourceAttribute>().SourceName} = @{p.Name}"));

            var query = $"DELETE FROM {_entityTableName} WHERE {whereClause}";

            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DWH"));
            using SqlCommand command = new SqlCommand(query, connection);

            foreach (var keyProperty in keyProperties)
            {
                var value = keyProperty.GetValue(entity);
                command.Parameters.AddWithValue($"@{keyProperty.Name}", value ?? DBNull.Value);
            }

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
