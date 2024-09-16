using offer_transaction_sync.Models;
using offer_transaction_sync.Services;
using offer_transaction_sync.Utilities;

namespace offer_transaction_sync
{
    public class Worker : BackgroundService
    {
        private readonly DbHandler _dbHandler;
        private readonly EntityService _entityService;

        public readonly string _ApplicationName = "offer_transaction_sync";

        List<Entity> entities;
        List<Entity> existingEntities;

        public Worker(DbHandler dbHandler, EntityService entityService)
        {
            _dbHandler = dbHandler;
            _entityService = entityService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var job = _dbHandler.GetJob(_ApplicationName);
                    if (job.Next_dt < DateTime.Now)
                    {
                        entities = _dbHandler.GetEntities();
                        existingEntities = _dbHandler.GetExistingEntities();

                        foreach (var entity in entities)
                        {
                            var existingEntity = _entityService.FindExistingEntity(entity, existingEntities.AsQueryable());
                            if (existingEntity == null)
                            {
                                _dbHandler.InsertEntity(entity);
                            }
                            else if (_entityService.EntitiesAreDifferent(entity, existingEntity))
                            {
                                _dbHandler.UpdateEntity(entity);
                            }
                        }

                        foreach (var existingEntity in existingEntities)
                        {
                            var currentEntity = _entityService.FindExistingEntity(existingEntity, entities.AsQueryable());
                            if (currentEntity == null)
                            {
                                _dbHandler.DeleteEntity(existingEntity);
                            }
                        }

                        _dbHandler.UpdateJobStatus(job);
                    }
                }
                catch (Exception ex)
                {
                    var appLog = new AppLog(
                    _AppName: _ApplicationName,
                    _LogLevel: "Error",
                    _Logger: "Worker",
                    _Message: $"An error occurred while running the service",
                    _Exception: ex.Message ?? string.Empty,
                    _StackTrace: ex.StackTrace ?? string.Empty,
                    _MachineName: Environment.MachineName,
                    _RequestId: 0,
                    _CreatedAt: DateTime.Now
                );

                    _dbHandler.InsertAppLog(appLog);

                    throw ex;
                }
                await Task.Delay(10000, stoppingToken);
            }
        }


        public static async Task RunInteractiveAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Worker>>();
            var dbHandler = scope.ServiceProvider.GetRequiredService<DbHandler>();
            var entityService = scope.ServiceProvider.GetRequiredService<EntityService>();

            var worker = new Worker(dbHandler, entityService);
            await worker.ExecuteAsync(cancellationToken);
        }
    }
}
