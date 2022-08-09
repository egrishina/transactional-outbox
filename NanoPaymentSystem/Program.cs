using NanoPaymentSystem.Application;
using NanoPaymentSystem.Database;
using NanoPaymentSystem.MessageBroker;
using NanoPaymentSystem.Options;
using NanoPaymentSystem.PaymentProviders;
using NanoPaymentSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFakePaymentProvider();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddMessageBroker();
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection(nameof(OutboxOptions)));
builder.Services.AddHostedService<KafkaHostedService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
