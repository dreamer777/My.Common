#region usings
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

using JetBrains.Annotations;

#endregion



namespace My.Common
{
    /// <summary>
    /// Adds mexBehavior if it is not added in config
    /// </summary>
    public class AutoMexServiceHost : ServiceHost
    {
        public AutoMexServiceHost() { }


        public AutoMexServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses) { }


        public AutoMexServiceHost(object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses) { }


        protected override void ApplyConfiguration()
        {
            base.ApplyConfiguration();

            ServiceMetadataBehavior mexBehavior = this.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (mexBehavior != null)
                return;

            mexBehavior = new ServiceMetadataBehavior();
            this.Description.Behaviors.Add(mexBehavior);

            foreach (Uri baseAddress in this.BaseAddresses)
                if (baseAddress.Scheme == Uri.UriSchemeHttp)
                {
                    mexBehavior.HttpGetEnabled = true;
                    this.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                        MetadataExchangeBindings.CreateMexHttpBinding(),
                        "mex");
                }
                else if (baseAddress.Scheme == Uri.UriSchemeHttps)
                {
                    mexBehavior.HttpsGetEnabled = true;
                    this.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                        MetadataExchangeBindings.CreateMexHttpsBinding(),
                        "mex");
                }
                else if (baseAddress.Scheme == Uri.UriSchemeNetPipe)
                    this.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                        MetadataExchangeBindings.CreateMexNamedPipeBinding(),
                        "mex");
                else if (baseAddress.Scheme == Uri.UriSchemeNetTcp)
                    this.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                        MetadataExchangeBindings.CreateMexTcpBinding(),
                        "mex");
        }
    }
}