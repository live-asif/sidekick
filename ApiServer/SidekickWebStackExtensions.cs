using System;
using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using Api.Common.Configurations;
using Microsoft.Owin.Security.Jwt;
using NLog;
using Owin;

namespace ApiServer
{
    public static class SidekickWebStackExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void AttachAuthenticationModules(this IAppBuilder app)
        {
            var config = ConfigurationManager.GetSection("authenticationMiddleware") as AuthenticationModulesConfigurationSection;

            if (config != null)
            {
                Logger.Info("Loaded {0} authentication modules", config.Modules.Count);
                foreach (ProviderSettings moduleElement in config.Modules)
                {
                    if (moduleElement.Type != null)
                    {
                        JwtBearerAuthenticationOptions jwtBearerAuthenticationOptions = null;

                        try
                        {
                            jwtBearerAuthenticationOptions =
                                Activator.CreateInstance(Type.GetType(moduleElement.Type)) as JwtBearerAuthenticationOptions;

                        }
                        catch (Exception exception)
                        {

                        }

                        if (jwtBearerAuthenticationOptions != null)
                        {

                            app.UseJwtBearerAuthentication(jwtBearerAuthenticationOptions);
                        }
                        else
                        {
                            app.Use(Type.GetType(moduleElement.Type));
                        }


                        Logger.Debug("added {0}", moduleElement.Name);
                    }

                }
            }
            else
            {
                throw new Exception("no authentication modules found");
            }
        }

        public static void AttachDelegatingHandlers(this HttpConfiguration httpConfiguration)
        {
            var config = ConfigurationManager.GetSection("sidekickDelegatingHandlers") as DelegatingHandlersConfigurationSection;

            if (config != null)
            {
                Logger.Info("Loaded {0} delegating handlers", config.DelegatingHandlers.Count);
                foreach (ProviderSettings element in config.DelegatingHandlers)
                {
                    if (element.Type != null)
                    {
                        DelegatingHandler delegatingHandler =
                            Activator.CreateInstance(Type.GetType(element.Type)) as DelegatingHandler;


                        httpConfiguration.MessageHandlers.Add(delegatingHandler);
                        Logger.Debug("added {0}", element.Name);
                    }

                }
            }
            else
            {
                Logger.Warn("DelegatingHandlers handler not found");
            }
        }
    }
}