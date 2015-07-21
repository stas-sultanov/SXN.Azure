using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAzure.Storage.Table;

namespace SXN.Azure.TimeDataStorage
{
	/// <summary>
	/// Provides an interface for types that implements methods required for <typeparamref name="T"/> to work with <see cref="TimeDataStorage{TTimeDataEntity}"/>.
	/// </summary>
	/// <typeparam name="T">The type of the time-data entities stored in the the Azure Table Storage.</typeparam>
	public interface ITimeDataEntityProvider<T>
	{
		#region Methods

		/// <summary>
		/// Gets the unique key of the time-data <paramref name="entity"/>.
		/// </summary>
		/// <param name="entity">The time-data entity.</param>
		/// <returns>The unique key.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		String GetKey(T entity);

		/// <summary>
		/// Gets the <see cref="IDictionary{TKey, TValue}"/> object that contains properties of the time-data <paramref name="entity"/>, indexed by property name.
		/// </summary>
		/// <param name="entity">The time-data entity.</param>
		/// <returns>The properties of the time-data entity.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IDictionary<String, EntityProperty> GetProperties(T entity);

		/// <summary>
		/// Gets the time of the time-data <paramref name="entity"/>.
		/// </summary>
		/// <param name="entity">The time-data entity.</param>
		/// <returns>The time.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		DateTime GetTime(T entity);

		/// <summary>
		/// Tries to resolve the time-data entity.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="time">The time.</param>
		/// <param name="properties">An <see cref="IDictionary{TKey,TValue}"/> object containing the entity's properties, indexed by property name.</param>
		/// <returns>
		/// An instance of <see cref="TryResult{TResult}"/> which encapsulates result of the operation.
		/// <see cref="TryResult{TResult}.Success"/> contains <c>true</c> if operation was successful, <c>false</c> otherwise.
		/// <see cref="TryResult{TResult}.Result"/> valid <typeparamref name="T"/> object if operation was successful, <c>default</c>(<typeparamref name="T"/>) otherwise.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		TryResult<T> TryResolve(String key, DateTime time, IDictionary<String, EntityProperty> properties);

		/// <summary>
		/// Tries to update <paramref name="currentEntity"/> with data of <paramref name="newEntity"/>.
		/// </summary>
		/// <param name="currentEntity">The current data.</param>
		/// <param name="newEntity">The new data.</param>
		/// <returns>
		/// An instance of <see cref="TryResult{TResult}"/> which encapsulates result of the operation.
		/// <see cref="TryResult{TResult}.Success"/> contains <c>true</c> if operation was successful, <c>false</c> otherwise.
		/// <see cref="TryResult{TResult}.Result"/> valid <typeparamref name="T"/> object if operation was successful, <c>default</c>(<typeparamref name="T"/>) otherwise.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		TryResult<T> TryUpdate(T currentEntity, T newEntity);

		#endregion
	}
}