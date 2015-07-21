using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace SXN.Azure
{
	/// <summary>
	/// Provides an interface for <see cref="ITableEntity"/> that represents data.
	/// </summary>
	/// <typeparam name="T">The type of the data that is managed by the table entity.</typeparam>
	public interface IDataTableEntity<T> : ITableEntity
	{
		#region Methods

		/// <summary>
		/// Puts the instance of <typeparamref name="T"/> into the table entity.
		/// </summary>
		/// <param name="data">The data to put into the table entity.</param>
		void Put(T data);

		/// <summary>
		/// Tries to get instance of <typeparamref name="T"/> from the table entity.
		/// </summary>
		/// <returns>
		/// An instance of <see cref="TryResult{TResult}"/> which encapsulates result of the operation.
		/// <see cref="TryResult{TResult}.Success"/> contains <c>true</c> if operation was successful, <c>false</c> otherwise.
		/// <see cref="TryResult{TResult}.Result"/> valid <typeparamref name="T"/> object if operation was successful, <c>default</c>(<typeparamref name="T"/>) otherwise.
		/// </returns>
		TryResult<T> TryGet();

		#endregion
	}
}