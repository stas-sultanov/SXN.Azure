using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.ServiceBus;
using SXN.Azure.Extensions;

namespace SXN.Azure
{
	/// <summary>
	/// Provides a configuration for the Azure Service Bus Relay.
	/// </summary>
	[DataContract]
	public sealed class RelaySettings
	{
		#region Fields

		[DataMember(Name = @"serviceNamespace")]
		private String serviceNamespace;

		[DataMember(Name = @"Token")]
		private SharedAccessSignatureTokenSettings token;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="RelaySettings"/> class.
		/// </summary>
		/// <param name="serviceNamespace">The service namespace used by the application.</param>
		/// <param name="token">The configuration of the token.</param>
		public RelaySettings(String serviceNamespace, SharedAccessSignatureTokenSettings token)
		{
			this.serviceNamespace = serviceNamespace;

			this.token = token;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The root of the name of the service namespace used by the application.
		/// </summary>
		public String ServiceNamespace
		{
			get
			{
				return serviceNamespace;
			}
		}

		/// <summary>
		/// The configuration of the token.
		/// </summary>
		public SharedAccessSignatureTokenSettings Token
		{
			get
			{
				return token;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Configures the instance of <see cref="ServiceHost"/> using current configuration.
		/// </summary>
		/// <param name="host">A host of the services.</param>
		/// <param name="contractType">A <see cref="Type"/> of contract for the endpoint added.</param>
		/// <param name="servicePath">A service path that follows the host name section of the URI.</param>
		/// <param name="incomingPort">An incoming port of the service.</param>
		/// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="host"/> is not in <see cref="CommunicationState.Created"/> state.</exception>
		/// <exception cref="InvalidOperationException">Configuration is invalid.</exception>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void ConfigureServiceHost(ServiceHost host, Type contractType, String servicePath, Int32 incomingPort)
		{
			host.CheckArgument(@"host");

			servicePath.CheckArgument(@"servicePath");

			contractType.CheckArgument(@"contractType");

			if (host.State != CommunicationState.Created)
			{
				throw new ArgumentException("is not in Created state", "host");
			}

			// Construct an Azure Service Bus address
			var address = CreateServiceUri(servicePath);

			// Add a relay service endpoint to the hosted service with a specified contract, binding, and URI that contains the endpoint address
			var relayServiceEndpoint = host.AddServiceEndpoint(contractType, new NetTcpRelayBinding(), address);

			// Create a shared secret token provider
			var relayTokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(token.KeyName, token.SharedAccessKey);

			// Initialize a new instance of the TransportClientEndpointBehavior class
			var relayEndpointBehavior = new TransportClientEndpointBehavior(relayTokenProvider);

			// Add endpoint behavior
			relayServiceEndpoint.Behaviors.Add(relayEndpointBehavior);

			// Add incoming service endpoint
			host.AddServiceEndpoint
				(
					contractType,
					new NetTcpBinding(),
					new UriBuilder(@"net.tcp", @"localhost", incomingPort, servicePath).Uri
				);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChannelFactory{TChannel}"/> class.
		/// </summary>
		/// <typeparam name="TChannel">A type of channel produced by the channel factory. This type must be either <see cref="IOutputChannel"/> or <see cref="IRequestChannel"/>.</typeparam>
		/// <returns>A new instance of <see cref="ChannelFactory{TChannel}"/> class.</returns>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public ChannelFactory<TChannel> CreateChannelFactory<TChannel>()
		{
			// Create channel factory
			var result = new ChannelFactory<TChannel>(new NetTcpRelayBinding());

			// Create behavior
			var behavior = new TransportClientEndpointBehavior
			{
				TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(Token.KeyName, Token.SharedAccessKey)
			};

			// Add endpoint behavior
			result.Endpoint.Behaviors.Add(behavior);

			return result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceManager"/> class using <see cref="ServiceNamespace"/> and <see cref="Token"/>.
		/// </summary>
		/// <returns>The full URI address of the service namespace.</returns>
		public NamespaceManager CreateNamespaceManager()
		{
			// Create address
			var address = CreateServiceUri(String.Empty);

			// Create token
			var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(Token.KeyName, Token.SharedAccessKey);

			return new NamespaceManager(address, tokenProvider);
		}

		/// <summary>
		/// Creates the Windows Azure Service Bus URI for an application, using <see cref="ServiceNamespace"/> and <paramref name="servicePath"/>.
		/// </summary>
		/// <param name="servicePath">A service path that follows the host name section of the URI.</param>
		/// <returns>Returns a Windows Azure Service Bus URI.</returns>
		public Uri CreateServiceUri(String servicePath)
		{
			// Check arguments
			return ServiceBusEnvironment.CreateServiceUri(@"sb", ServiceNamespace, String.IsNullOrWhiteSpace(servicePath) ? String.Empty : servicePath.ToValidDnsName());
		}

		#endregion
	}
}