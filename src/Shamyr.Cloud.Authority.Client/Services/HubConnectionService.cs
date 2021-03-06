﻿using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Shamyr.Logging;
using Shamyr.Operations;

namespace Shamyr.Cloud.Authority.Client.Services
{
  internal class HubConnectionService: IHubConnectionService
  {
    private readonly ILogger fLogger;

    public HubConnectionService(ILogger logger)
    {
      fLogger = logger;
    }

    public HubConnection CreateConnection(Uri signalUrl)
    {
      return new HubConnectionBuilder()
        .WithUrl(signalUrl, SetupUrl)
        .AddJsonProtocol(SetupJsonProtocol)
        .WithAutomaticReconnect(new SignalR.RetryPolicy(TimeSpan.FromSeconds(30), 2))
        .Build();
    }

    public async Task InitialConnectAsync(HubConnection hubConnection, Uri signalUrl, ILoggingContext context, CancellationToken cancellationToken)
    {
      var config = new RetryOperationConfig
      {
        RetryCount = null,
        RetryPolicy = new RetryPolicy(TimeSpan.FromSeconds(60), 2),
        SameExceptions = SameExceptions
      };

      await new RetryOperation((_, cancellationToken) => hubConnection.StartAsync(cancellationToken), config)
        .Catch<Unit, WebSocketException>(fLogger)
        .Catch<Unit, Exception>(fLogger, true)
        .OnRetryInformation(fLogger, $"Retrying to connect to '{signalUrl}' ...")
        .OnSuccess(fLogger, $"Successfuly connected to '{signalUrl}'.")
        .OnFail(fLogger, $"Unable to connect to '{signalUrl}'.")
        .OnFailRethrow()
        .ExecuteAsync(context, cancellationToken);
    }

    private bool SameExceptions(Exception ex1, Exception ex2)
    {
      if (ex1 is WebSocketException we1 && ex2 is WebSocketException we2)
        return we1.ErrorCode == we2.ErrorCode;

      return ex1.GetType() == ex2.GetType();
    }

    private void SetupUrl(HttpConnectionOptions options)
    {
      options.Transports = HttpTransportType.WebSockets;
      options.SkipNegotiation = true;
    }

    private void SetupJsonProtocol(JsonHubProtocolOptions options)
    {
      options.PayloadSerializerOptions = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };
    }
  }
}
