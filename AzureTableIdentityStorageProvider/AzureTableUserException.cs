using System;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// An exception thrown by the <see cref="AzureTableUserStore"/> class.
    /// </summary>
    [Serializable]
    public class AzureTableUserException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUserException" /> class.
        /// </summary>
        public AzureTableUserException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUserException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public AzureTableUserException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUserException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public AzureTableUserException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUserException" /> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected AzureTableUserException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }
    }
}