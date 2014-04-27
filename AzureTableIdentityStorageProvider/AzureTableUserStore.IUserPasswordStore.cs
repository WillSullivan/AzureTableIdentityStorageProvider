using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace StateStreetGang.AspNet.Identity.AzureTable
{

    public partial class AzureTableUserStore<T> :
        IUserPasswordStore<T>
    {
        #region IUserPasswordStore<T> Members
        /// <summary>
        /// Gets the password hash for the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <returns>A <see cref="Task{T}"/> that returns the user's password hash.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        public Task<string> GetPasswordHashAsync(T user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
        /// Determines whether the given <paramref name="user"/> has a password hash set.
        /// </summary>
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <returns>A <see cref="Task{T}"/> that returns <c>true</c> if the user has a password hash set, or <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        public Task<bool> HasPasswordAsync(T user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        /// <summary>
        /// Sets the password hash for the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="passwordHash"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="passwordHash"/> is empty or consists solely of whitespace.</exception>
        /// <remarks>This method is called prior to creating a local user.  The given <paramref name="user"/> does not necessarily exist already within the system, nor should it be assumed that this method should update the user prior to returning.</remarks>
        public Task SetPasswordHashAsync(T user, string passwordHash)
        {
            return Task.Run(() =>
            {
                if (passwordHash == null)
                    throw new ArgumentNullException("passwordHash");
                if (string.IsNullOrWhiteSpace(passwordHash))
                    throw new ArgumentException("passwordHash cannot be null, empty, or consist of whitespace.");
                user.PasswordHash = passwordHash;
            });

            //user.EnsureETagSet();
            //await UpdateAsync(user);
        }

        #endregion
    }
}
