using System;
using System.Globalization;
using System.Net;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace SXN.Azure.Extensions
{
	/// <summary>
	/// Provides a set of extension methods for <see cref="RoleInstanceEndpoint"/> class.
	/// </summary>
	public static class RoleInstanceEndpointEx
	{
		#region Methods

		/// <summary>
		/// Converts <see cref="RoleInstanceEndpoint"/> to <see cref="HttpListener"/> prefix.
		/// </summary>
		/// <param name="endpoint">An endpoint that is associated with a role instance.</param>
		/// <returns>Uniform Resource Identifier (URI) prefix for <see cref="HttpListener"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <c>null</c>.</exception>
		public static String ToHttpListenerPrefix(this RoleInstanceEndpoint endpoint)
		{
			if (endpoint == null)
			{
				throw new ArgumentNullException(nameof(endpoint));
			}

			return String.Format(CultureInfo.InvariantCulture, "{0}://+:{1}/", endpoint.Protocol, endpoint.IPEndpoint.Port);
		}

		#endregion
	}
}