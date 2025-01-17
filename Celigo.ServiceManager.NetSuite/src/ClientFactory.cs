﻿using SuiteTalk;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Celigo.ServiceManager.NetSuite
{
    public interface INetSuiteClientFactory
    {
        string ApplicationId { get; set; }

        INetSuiteClient CreateClient();

        INetSuiteClient CreateClient(Passport passport);

        INetSuiteClient CreateClient(Passport passport, IConfigurationProvider configurationProvider);

        INetSuiteClient CreateClient(IPassportProvider passportProvider);

        INetSuiteClient CreateClient(IPassportProvider passportProvider, IConfigurationProvider configProvider);

        INetSuiteClient CreateClient(ITokenPassportProvider passportProvider);

        INetSuiteClient CreateClient(ITokenPassportProvider passportProvider, IConfigurationProvider configProvider);
    }
    
    public class ClientFactory : ClientFactory<NetSuitePortTypeClient>
    {
        public ClientFactory(string appId): base(appId) { }
    }

    public class ClientFactory<T>: INetSuiteClientFactory where T: class, INetSuiteClient, new()
    {
        private List<IDynamicEndpointBehavior> _dynamicEndpointBehaviors;

        public string ApplicationId { get; set; }

        public Action<T> ClientInitializer { get; set; }

        string INetSuiteClientFactory.ApplicationId {
            get => this.ApplicationId;
            set => this.ApplicationId = value;
        }
        
        public ClientFactory(string appId)
        {
            this.ApplicationId = appId;
            _dynamicEndpointBehaviors = null;
        }

        public ClientFactory(string appId, List<IDynamicEndpointBehavior> dynamicEndpointBehaviours)
        {
            this.ApplicationId = appId;
            _dynamicEndpointBehaviors = dynamicEndpointBehaviours;
        }

        public T CreateClient()
        {
            var client = new T();
            return this.ConfigureClient(client);
        }

        public T CreateClient(Passport passport, IConfigurationProvider configurationProvider)
        {
            var client = new T { passport = passport };
            return this.ConfigureClient(client, configProvider: configurationProvider);
        }

        public T CreateClient(Passport passport) => this.CreateClient(passport, null);

        public T CreateClient(IPassportProvider passportProvider) => this.CreateClient(passportProvider, null);

        public T CreateClient(ITokenPassportProvider tokenPassportProvider) => this.CreateClient(tokenPassportProvider, null);

        public T CreateClient(ITokenPassportProvider tokenPassportProvider, IConfigurationProvider configProvider) => this.ConfigureClient(
                new T(), 
                tokenPassportProvider: tokenPassportProvider,
                configProvider: configProvider
            );

        public T CreateClient(IPassportProvider passportProvider, IConfigurationProvider configProvider) => this.ConfigureClient(
                new T(), 
                passportProvider: passportProvider, 
                configProvider: configProvider
            );

        private T AddDynamicEndpointBehaviours( T client) {
            if (_dynamicEndpointBehaviors == null)
            {
                return client;
            }

            foreach (IDynamicEndpointBehavior depb in _dynamicEndpointBehaviors)
            {
                if (depb.IsEnabled())
                {
                    client.Endpoint.EndpointBehaviors.Add(depb);
                }
            }

            return client;
        }

        private T ConfigureClient(
                T client,
                IPassportProvider passportProvider = null,
                ITokenPassportProvider tokenPassportProvider = null,
                IConfigurationProvider configProvider = null
            )
        {
            // Increase binding timeout.
            client.Endpoint.Binding.SendTimeout = new TimeSpan(0, 10, 0);

            SuiteTalkHeader[] headers;
            string account;

            if (client.tokenPassport != null)
            {
                account = client.tokenPassport.account;
                headers = new SuiteTalkHeader[] { new SearchPreferencesHeader(client) };
            }
            else if (client.passport != null)
            {
                account = client.passport.account;
                headers = new SuiteTalkHeader[] {
                    new ApplicationInfoHeader(this.ApplicationId),
                    new SearchPreferencesHeader(client)
                };
            }
            else if (tokenPassportProvider != null)
            {
                account = tokenPassportProvider.GetTokenPassport().account;
                headers = new SuiteTalkHeader[] {
                    new TokenPassportHeader(tokenPassportProvider),
                    new SearchPreferencesHeader(client)
                };
            }
            else
            {
                account = passportProvider.GetPassport().account;
                headers = new SuiteTalkHeader[] {
                    new ApplicationInfoHeader(this.ApplicationId),
                    new PassportHeader(passportProvider),
                    new SearchPreferencesHeader(client)
                };
            }
            var inspector = new SuiteTalkMessageInspector(headers);

            if (configProvider != null && configProvider.DataCenter != null)
            {
                client.Endpoint.Address = GetDataCenterEndpoint(configProvider.DataCenter.DataCenterDomain);
            }
            else
            {
                string subdomain = account.ToLowerInvariant().Replace("_", "-");
                client.Endpoint.Address = GetDataCenterEndpoint($"https://{subdomain}.suitetalk.api.netsuite.com");
            }

            var endpointBehavior = new SuiteTalkEndpointBehavior(inspector);
            client.Endpoint.EndpointBehaviors.Add(endpointBehavior);

            client = AddDynamicEndpointBehaviours(client);

            if (this.ClientInitializer != null)
            {
                this.ClientInitializer(client);
                return client;
            }
            else
            {
                return client;
            }
        }


        private EndpointAddress GetDataCenterEndpoint(string dataCenter)
        {
            var endpoint = NetSuitePortTypeClient.GetDefaultEndpoint();
            var relativeWsPath = endpoint.Uri.LocalPath;

            if (!dataCenter.EndsWith("/"))
            {
                return new EndpointAddress(dataCenter + relativeWsPath);
            }
            else
            {
                return new EndpointAddress(
                    string.Concat(dataCenter.Substring(0, dataCenter.Length - 1), relativeWsPath)
                );
            }
        }

        INetSuiteClient 
            INetSuiteClientFactory.CreateClient() => this.CreateClient();

        INetSuiteClient 
            INetSuiteClientFactory.CreateClient(Passport passport) => this.CreateClient(passport);

        INetSuiteClient 
            INetSuiteClientFactory.CreateClient(Passport passport, IConfigurationProvider configurationProvider) => 
                this.CreateClient(passport, configurationProvider);

        INetSuiteClient 
            INetSuiteClientFactory.CreateClient(IPassportProvider passportProvider) => this.CreateClient(passportProvider);

        INetSuiteClient 
            INetSuiteClientFactory.CreateClient(IPassportProvider passportProvider, IConfigurationProvider configProvider) => 
                this.CreateClient(passportProvider, configProvider);

        INetSuiteClient 
            INetSuiteClientFactory.CreateClient(ITokenPassportProvider passportProvider) => this.CreateClient(passportProvider);

        INetSuiteClient 
            INetSuiteClientFactory.CreateClient(ITokenPassportProvider passportProvider, IConfigurationProvider configProvider) => 
                this.CreateClient(passportProvider, configProvider);
    }
}
