﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PromoCodesEditor
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TCPromoCodesEntities : DbContext
    {
        public TCPromoCodesEntities()
            : base("name=TCPromoCodesEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C__EFMigrationsHistory> C__EFMigrationsHistory { get; set; }
        public virtual DbSet<PromoCode> PromoCodes { get; set; }
        public virtual DbSet<RedeemedCode> RedeemedCodes { get; set; }
    }
}
