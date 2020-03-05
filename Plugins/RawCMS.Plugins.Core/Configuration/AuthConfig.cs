﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace RawCMS.Plugins.Core.Configuration
{
    public class AuthConfig
    {
        public AuthConfig()
        {
            RawCMSProvider.Authority = "http://localhost:50093";
            RawCMSProvider.ClientId = "raw.client";
            RawCMSProvider.ClientSecret = "raw.secret";
            RawCMSProvider.ApiResource = "rawcms";
        }

        public RawCMSProvider RawCMSProvider { get; set; } = new RawCMSProvider();
        public List<ExternalProvider> ExternalProvider { get; set; } = new List<ExternalProvider>();


        // scopes define the resources in your system
        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
                new IdentityResource("custom",new string[]{ ClaimTypes.Email,ClaimTypes.NameIdentifier, ClaimTypes.Name})
            };
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(RawCMSProvider.ApiResource, RawCMSProvider.ApiResource)
                {
                    ApiSecrets = new List<Secret>
                {
                    new Secret(RawCMSProvider.ClientSecret.Sha256())
                },
                Scopes=
                {
                    new Scope("openid"),
                },
                UserClaims= new string[]{ ClaimTypes.NameIdentifier, ClaimTypes.Email}
                }
            };
        }

        // clients want to access resources (aka scopes)
        public IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {
                new Client
                {
                    ClientId = RawCMSProvider.ClientId,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AlwaysSendClientClaims = true,

                    ClientSecrets =
                    {
                        new Secret(RawCMSProvider.ClientSecret.Sha256())
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                },
            };
        }
    }
}