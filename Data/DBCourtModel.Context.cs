﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NoCompany.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CourtDBContext : DbContext
    {
        public CourtDBContext()
            : base("name=CourtDBContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CourtDistrict> CourtDistricts { get; set; }
        public virtual DbSet<CourtLocation> CourtLocations { get; set; }
        public virtual DbSet<CourtRegion> CourtRegions { get; set; }
    }
}
