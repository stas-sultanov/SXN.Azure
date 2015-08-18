using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.Storage
{
	/// <summary>
	/// Provides a set of extension methods for the <see cref="string"/> class.
	/// </summary>
	public static class StringEx
	{
		#region Methods

		/// <summary>
		/// Converts the current instance of <see cref="String"/> to the valid DNS name.
		/// </summary>
		/// <remarks>
		/// A valid DNS name conforms to the following naming rules::
		/// 1. Name must start with a letter or number, and can contain only letters, numbers, and the dash (-) character.
		/// 2. Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in DNS names.
		/// 3. All letters in a name must be lowercase.
		/// 4. Container names must be from 3 through 63 characters long.
		/// </remarks>
		/// <param name="value">An instance of the <see cref="String"/>.</param>
		/// <returns>A DNS valid name.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="value"/> contains less than 3 valid characters.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static String ToValidDnsName(this String value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			// Lowercase id
			var result = value.ToLowerInvariant();

			// Remove all invalid chars
			result = Regex.Replace(result, @"[^0-9a-z-]+", String.Empty);

			// Ensure that the name does not starts and does not ends on '-' dash
			result = Regex.Replace(result, @"(^-+)|(-+$)", String.Empty);

			// Replace multiple occurrence of '-' dash
			result = Regex.Replace(result, @"[-]{2,}", "-");

			// Check length
			if (result.Length > 63)
			{
				result = result.Substring(0, 63);
			}

			if (value.Length < 4)
			{
				throw new ArgumentException("length must be greater than 3 valid characters long", nameof(value));
			}

			return result;
		}

		#endregion
	}
}