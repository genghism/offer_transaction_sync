using offer_transaction_sync;
using offer_transaction_sync.Services;
using offer_transaction_sync.Utilities;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Metak DWH 14 offer_transaction_sync";
});


builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<DbHandler>();
builder.Services.AddSingleton<EntityService>();

var host = builder.Build();

if (Environment.UserInteractive)
{
    Console.WriteLine("Running in interactive mode. Press Ctrl+C to stop.");
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    await Worker.RunInteractiveAsync(host.Services, cts.Token);
}
else
{
    host.Run();
}
