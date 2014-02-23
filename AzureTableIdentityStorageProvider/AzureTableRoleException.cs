using System;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// Exception used by the <see cref="AzureTableRoleStore"/>.
    /// </summary>
    [Serializable]
    public class AzureTableRoleException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRoleException" /> class.
        /// </summary>
        public AzureTableRoleException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRoleException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public AzureTableRoleException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRoleException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public AzureTableRoleException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRoleException" /> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected AzureTableRoleException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}