// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License
// See the LICENSE file in the project root for more information.
// Maintainer: Argo Zhang(argo@live.ca) Website: https://www.blazor.zone

using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Unicode;


namespace Microsoft.Extensions.DependencyInjection;

static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBootstrapBlazorServerService(this IServiceCollection services)
    {
        services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

        // Removed AddFileLogger - Longbow's BlockingCollection-based writer causes
        // "Cannot access disposed object" errors during background task logging

        services.AddRequestLocalization<IOptionsMonitor<BootstrapBlazorOptions>>((localizerOption, blazorOption) =>
        {
            blazorOption.OnChange(Invoke);
            Invoke(blazorOption.CurrentValue);
            return;

            void Invoke(BootstrapBlazorOptions option)
            {
                var supportedCultures = option.GetSupportedCultures();
                localizerOption.SupportedCultures = supportedCultures;
                localizerOption.SupportedUICultures = supportedCultures;

                // Force Vietnamese on first visit; cookie (CultureChooser) can override later.
                var defaultCulture = option.SupportedCultures?.FirstOrDefault()
                    ?? option.DefaultCultureInfo
                    ?? "vi-VN";
                localizerOption.DefaultRequestCulture = new RequestCulture(defaultCulture, defaultCulture);
            }
        });

        services.AddControllers();

        services.Configure<HubOptions>(option => option.MaximumReceiveMessageSize = null);

        services.AddHostedService<MockOnlineContributor>();
        // services.AddHostedService<MockReceiveSocketServerService>();
        // services.AddHostedService<MockSendReceiveSocketServerService>();
        // services.AddHostedService<MockCustomProtocolSocketServerService>();
        // services.AddHostedService<MockDisconnectServerService>();

        services.AddBootstrapBlazorServices();

        return services;
    }
}
