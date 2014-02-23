using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// A substitute of <see cref="System.Security.Claims.Claim"/> that can be stored in an Azure Table.
    /// </summary>
    /// <remarks>Other parts of ASP.NET Identity rely upon interfaces to define their contract with users.  Unfortunately,
    /// claims are stored using the <see cref="System.Security.Claims.Claim"/> class, which does not inherit from the necessary
    /// <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity"/> base class.  Therefore, in order to store user claims within
    /// Azure Tables, we must map back and forth between these models.</remarks>
    public class AzureTableClaim : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        /// <summary>
        /// The claim issuer (see <see cref="Claim.Issuer"/>)
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// The original issuer of the claim (see <see cref="Claim.OriginalIssuer"/>)  
        /// </summary>
        public string OriginalIssuer { get; set; }

        /// <summary>
        /// The type of claim (see <see cref="Claim.Type"/>) 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The value of the claim (see <see cref="Claim.Value"/>) 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The value type of the claim (see <see cref="Claim.ValueType"/>) 
        /// </summary>
        public string ValueType { get; set; }
    }
}
