using BAYSOFT.Abstractions.Core.Domain.Entities;
using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using BAYSOFT.Core.Domain.Default.Samples.Resources;
using BAYSOFT.Core.Domain.Default.Shared.Resources;
using BAYSOFT.Core.Domain.Resources;
using System;
using System.Collections.Generic;

namespace BAYSOFT.Core.Domain.Default.Samples.Entity
{
    // <forge:entity context="Default" name="Sample" id-type="Guid" table="Samples" aggregateRoot="true" auditable="true">
    [InheritStringLocalizer(typeof(Messages), Priority = 2)]
    [InheritStringLocalizer(typeof(ContextDefault), Priority = 1)]
    [InheritStringLocalizer(typeof(EntitySample), Priority = 0)]
    public sealed class Sample : DomainEntity<Guid>
    {
        #region Attributes
        // <forge:property entity="Sample" name="Description" type="string" required="true" length="128" db-column="Description">
        public string Description { get; set; }
        #endregion

        #region Foreign keys
        // No foreign keys defined
        #endregion

        #region Navigation properties
        // No navigation properties defined
        #endregion

        #region Constructors
        public Sample()
        {
            InitializeCollections();
        }
        #endregion

        #region Methods
        public void InitializeCollections()
        {
            // Initialize collections here
        }
	    // Add domain methods here
        #endregion
    }
}