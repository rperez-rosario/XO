﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace XOSkinWebApp.ORM
{
    public partial class XOSkinContext : DbContext
    {
        public XOSkinContext()
        {
        }

        public XOSkinContext(DbContextOptions<XOSkinContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<AddressType> AddressTypes { get; set; }
        public virtual DbSet<AnswerIngredient> AnswerIngredients { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
        public virtual DbSet<CityPr> CityPrs { get; set; }
        public virtual DbSet<CityStateCountryWorld> CityStateCountryWorlds { get; set; }
        public virtual DbSet<CityStateU> CityStateUs { get; set; }
        public virtual DbSet<CommonAllergen> CommonAllergens { get; set; }
        public virtual DbSet<ContradictingIngredient> ContradictingIngredients { get; set; }
        public virtual DbSet<Cost> Costs { get; set; }
        public virtual DbSet<DiscountCode> DiscountCodes { get; set; }
        public virtual DbSet<DiscountCodeProduct> DiscountCodeProducts { get; set; }
        public virtual DbSet<DiscountCoupon> DiscountCoupons { get; set; }
        public virtual DbSet<DiscountCouponProduct> DiscountCouponProducts { get; set; }
        public virtual DbSet<Ingredient> Ingredients { get; set; }
        public virtual DbSet<KitProduct> KitProducts { get; set; }
        public virtual DbSet<KitType> KitTypes { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<LocalizedImage> LocalizedImages { get; set; }
        public virtual DbSet<LocalizedText> LocalizedTexts { get; set; }
        public virtual DbSet<OrderBillTo> OrderBillTos { get; set; }
        public virtual DbSet<OrderShipTo> OrderShipTos { get; set; }
        public virtual DbSet<Page> Pages { get; set; }
        public virtual DbSet<PaymentPlan> PaymentPlans { get; set; }
        public virtual DbSet<PaymentPlanProductOrder> PaymentPlanProductOrders { get; set; }
        public virtual DbSet<PaymentPlanSchedule> PaymentPlanSchedules { get; set; }
        public virtual DbSet<PaymentPlanSchedulePayment> PaymentPlanSchedulePayments { get; set; }
        public virtual DbSet<PossibleAnswer> PossibleAnswers { get; set; }
        public virtual DbSet<Price> Prices { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<ProductIngredient> ProductIngredients { get; set; }
        public virtual DbSet<ProductOrder> ProductOrders { get; set; }
        public virtual DbSet<ProductOrderDiscountCode> ProductOrderDiscountCodes { get; set; }
        public virtual DbSet<ProductOrderDiscountCoupon> ProductOrderDiscountCoupons { get; set; }
        public virtual DbSet<ProductOrderLineItem> ProductOrderLineItems { get; set; }
        public virtual DbSet<ProductSubCategory> ProductSubCategories { get; set; }
        public virtual DbSet<ProductSubUnderCategory> ProductSubUnderCategories { get; set; }
        public virtual DbSet<ProductType> ProductTypes { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Questionnaire> Questionnaires { get; set; }
        public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public virtual DbSet<ShoppingCartDiscountCode> ShoppingCartDiscountCodes { get; set; }
        public virtual DbSet<ShoppingCartDiscountCoupon> ShoppingCartDiscountCoupons { get; set; }
        public virtual DbSet<ShoppingCartHistory> ShoppingCartHistories { get; set; }
        public virtual DbSet<ShoppingCartLineItem> ShoppingCartLineItems { get; set; }
        public virtual DbSet<StateU> StateUs { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<SubscriptionCustomer> SubscriptionCustomers { get; set; }
        public virtual DbSet<SubscriptionProduct> SubscriptionProducts { get; set; }
        public virtual DbSet<SubscriptionShipmentSchedule> SubscriptionShipmentSchedules { get; set; }
        public virtual DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public virtual DbSet<SynergeticIngredient> SynergeticIngredients { get; set; }
        public virtual DbSet<TransactionConcept> TransactionConcepts { get; set; }
        public virtual DbSet<UserAnswer> UserAnswers { get; set; }
        public virtual DbSet<UserCommonAllergen> UserCommonAllergens { get; set; }
        public virtual DbSet<UserLedgerTransaction> UserLedgerTransactions { get; set; }
        public virtual DbSet<UserLedgerTransactionType> UserLedgerTransactionTypes { get; set; }
        public virtual DbSet<UserProductList> UserProductLists { get; set; }
        public virtual DbSet<UserProductListProduct> UserProductListProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");

                entity.Property(e => e.CityName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Line1)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Line2).IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PostalCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StateName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.User).HasMaxLength(450);

                entity.HasOne(d => d.AddressTypeNavigation)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.AddressType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Address_AddressType");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.User)
                    .HasConstraintName("FK_Address_AspNetUsers");
            });

            modelBuilder.Entity<AddressType>(entity =>
            {
                entity.ToTable("AddressType");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AnswerIngredient>(entity =>
            {
                entity.ToTable("AnswerIngredient");

                entity.HasOne(d => d.AnswerNavigation)
                    .WithMany(p => p.AnswerIngredients)
                    .HasForeignKey(d => d.Answer)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AnswerIngredient_PossibleAnswer");
            });

            modelBuilder.Entity<AspNetRole>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetRoleClaim>(entity =>
            {
                entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetUser>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasDefaultValueSql("(N'')");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasDefaultValueSql("(N'')");

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.StripeCustomerId)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.UserName).HasMaxLength(256);

                entity.HasMany(d => d.Roles)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "AspNetUserRole",
                        l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                        r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                        j =>
                        {
                            j.HasKey("UserId", "RoleId");

                            j.ToTable("AspNetUserRoles");

                            j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                        });
            });

            modelBuilder.Entity<AspNetUserClaim>(entity =>
            {
                entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserToken>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<CityPr>(entity =>
            {
                entity.ToTable("CityPR");

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CityStateCountryWorld>(entity =>
            {
                entity.ToTable("CityStateCountryWorld");

                entity.HasIndex(e => new { e.CountryName, e.StateName }, "NonClusteredIndex-20210727-185919");

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StateName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CityStateU>(entity =>
            {
                entity.ToTable("CityStateUS");

                entity.HasIndex(e => new { e.CityName, e.StateName }, "NonClusteredIndex-20210727-190050");

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StateAbbreviation)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CommonAllergen>(entity =>
            {
                entity.ToTable("CommonAllergen");

                entity.Property(e => e.AlergenName)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ContradictingIngredient>(entity =>
            {
                entity.ToTable("ContradictingIngredient");

                entity.HasOne(d => d.IngredientANavigation)
                    .WithMany(p => p.ContradictingIngredientIngredientANavigations)
                    .HasForeignKey(d => d.IngredientA)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ContradictingIngredient_Ingredient");

                entity.HasOne(d => d.IngredientBNavigation)
                    .WithMany(p => p.ContradictingIngredientIngredientBNavigations)
                    .HasForeignKey(d => d.IngredientB)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ContradictingIngredient_Ingredient1");
            });

            modelBuilder.Entity<Cost>(entity =>
            {
                entity.ToTable("Cost");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Costs)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Cost_AspNetUsers");
            });

            modelBuilder.Entity<DiscountCode>(entity =>
            {
                entity.ToTable("DiscountCode");

                entity.Property(e => e.Code)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.DiscountAsInNproductDollars).HasColumnName("DiscountAsInNProductDollars");

                entity.Property(e => e.DiscountAsInNproductPercentage).HasColumnName("DiscountAsInNProductPercentage");

                entity.Property(e => e.DiscountGlobalOrderDollars).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiscountGlobalOrderPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiscountInNproductDollars)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("DiscountInNProductDollars");

                entity.Property(e => e.DiscountNproductPercentage)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("DiscountNProductPercentage");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(450);

                entity.Property(e => e.MinimumPurchase).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ValidFrom).HasColumnType("datetime");

                entity.Property(e => e.ValidTo).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.DiscountCodeCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DiscountCode_AspNetUsers");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.DiscountCodeLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .HasConstraintName("FK_DiscountCode_AspNetUsers1");
            });

            modelBuilder.Entity<DiscountCodeProduct>(entity =>
            {
                entity.ToTable("DiscountCodeProduct");

                entity.HasOne(d => d.CodeNavigation)
                    .WithMany(p => p.DiscountCodeProducts)
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DiscountCodeProduct_DiscountCode");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.DiscountCodeProducts)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DiscountCodeProduct_Product");
            });

            modelBuilder.Entity<DiscountCoupon>(entity =>
            {
                entity.ToTable("DiscountCoupon");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.DiscountAsInNproductDollars).HasColumnName("DiscountAsInNProductDollars");

                entity.Property(e => e.DiscountAsInNproductPercentage).HasColumnName("DiscountAsInNProductPercentage");

                entity.Property(e => e.DiscountGlobalOrderDollars).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiscountGlobalOrderPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiscountInNproductDollars)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("DiscountInNProductDollars");

                entity.Property(e => e.DiscountNproductPercentage)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("DiscountNProductPercentage");

                entity.Property(e => e.ImagePathLarge).IsUnicode(false);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(450);

                entity.Property(e => e.MinimumPurchase).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Name)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.ValidFrom).HasColumnType("datetime");

                entity.Property(e => e.ValidTo).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.DiscountCouponCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DiscountCoupon_AspNetUsers");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.DiscountCouponLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .HasConstraintName("FK_DiscountCoupon_AspNetUsers1");
            });

            modelBuilder.Entity<DiscountCouponProduct>(entity =>
            {
                entity.ToTable("DiscountCouponProduct");

                entity.HasOne(d => d.CouponNavigation)
                    .WithMany(p => p.DiscountCouponProducts)
                    .HasForeignKey(d => d.Coupon)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DiscountCouponProduct_DiscountCoupon");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.DiscountCouponProducts)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DiscountCouponProduct_Product");
            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.ToTable("Ingredient");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<KitProduct>(entity =>
            {
                entity.ToTable("KitProduct");

                entity.HasOne(d => d.KitNavigation)
                    .WithMany(p => p.KitProductKitNavigations)
                    .HasForeignKey(d => d.Kit)
                    .HasConstraintName("FK_KitProduct_Product");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.KitProductProductNavigations)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_KitProduct_Product1");
            });

            modelBuilder.Entity<KitType>(entity =>
            {
                entity.ToTable("KitType");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.ToTable("Language");

                entity.HasIndex(e => e.LanguageName, "IX_Language")
                    .IsUnique();

                entity.Property(e => e.LanguageName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LocalizedImage>(entity =>
            {
                entity.ToTable("LocalizedImage");

                entity.HasIndex(e => new { e.PlacementPointCode, e.Language }, "IX_LocalizedImage");

                entity.Property(e => e.Path)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.PlacementPointCode)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.LanguageNavigation)
                    .WithMany(p => p.LocalizedImages)
                    .HasForeignKey(d => d.Language)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LocalizedImage_Language");

                entity.HasOne(d => d.PageNavigation)
                    .WithMany(p => p.LocalizedImages)
                    .HasForeignKey(d => d.Page)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LocalizedImage_Page");
            });

            modelBuilder.Entity<LocalizedText>(entity =>
            {
                entity.ToTable("LocalizedText");

                entity.HasIndex(e => new { e.PlacementPointCode, e.Language }, "IX_LocalizedText");

                entity.Property(e => e.PlacementPointCode)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.LanguageNavigation)
                    .WithMany(p => p.LocalizedTexts)
                    .HasForeignKey(d => d.Language)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Text_Language");

                entity.HasOne(d => d.PageNavigation)
                    .WithMany(p => p.LocalizedTexts)
                    .HasForeignKey(d => d.Page)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LocalizedText_Page");
            });

            modelBuilder.Entity<OrderBillTo>(entity =>
            {
                entity.ToTable("OrderBillTo");

                entity.Property(e => e.AddressLine1)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.AddressLine2).IsUnicode(false);

                entity.Property(e => e.BillingDate).HasColumnType("datetime");

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.NameOnCreditCard)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PostalCode)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.RefundAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.RefundReason).IsUnicode(false);

                entity.Property(e => e.RefundedBy)
                    .HasMaxLength(450)
                    .IsUnicode(false);

                entity.Property(e => e.RefundedOn).HasColumnType("datetime");

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.OrderNavigation)
                    .WithMany(p => p.OrderBillTos)
                    .HasForeignKey(d => d.Order)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderBillTo_ProductOrder");
            });

            modelBuilder.Entity<OrderShipTo>(entity =>
            {
                entity.ToTable("OrderShipTo");

                entity.Property(e => e.ActualArrives).HasColumnType("datetime");

                entity.Property(e => e.ActualShipDate).HasColumnType("datetime");

                entity.Property(e => e.AddressLine1)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.AddressLine2).IsUnicode(false);

                entity.Property(e => e.Arrives).HasColumnType("datetime");

                entity.Property(e => e.CarrierName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PostalCode)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.RecipientName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ShipDate).HasColumnType("datetime");

                entity.Property(e => e.ShipEngineId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ShipEngineRateId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ShippedBy).HasMaxLength(450);

                entity.Property(e => e.ShippingLabelUrl)
                    .HasMaxLength(256)
                    .IsUnicode(false)
                    .HasColumnName("ShippingLabelURL");

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.TrackingNumber).IsUnicode(false);

                entity.HasOne(d => d.OrderNavigation)
                    .WithMany(p => p.OrderShipTos)
                    .HasForeignKey(d => d.Order)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderShipTo_ProductOrder");

                entity.HasOne(d => d.ShippedByNavigation)
                    .WithMany(p => p.OrderShipTos)
                    .HasForeignKey(d => d.ShippedBy)
                    .HasConstraintName("FK_OrderShipTo_AspNetUsers");
            });

            modelBuilder.Entity<Page>(entity =>
            {
                entity.ToTable("Page");

                entity.HasIndex(e => e.Name, "IX_Page")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PaymentPlan>(entity =>
            {
                entity.ToTable("PaymentPlan");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PaymentPlanCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlan_AspNetUsers1");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.PaymentPlanLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlan_AspNetUsers2");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.PaymentPlanUserNavigations)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlan_AspNetUsers");
            });

            modelBuilder.Entity<PaymentPlanProductOrder>(entity =>
            {
                entity.ToTable("PaymentPlanProductOrder");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PaymentPlanProductOrderCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanProductOrder_AspNetUsers");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.PaymentPlanProductOrderLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanProductOrder_AspNetUsers1");

                entity.HasOne(d => d.PaymentPlanNavigation)
                    .WithMany(p => p.PaymentPlanProductOrders)
                    .HasForeignKey(d => d.PaymentPlan)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanProductOrder_PaymentPlan");

                entity.HasOne(d => d.ProductOrderNavigation)
                    .WithMany(p => p.PaymentPlanProductOrders)
                    .HasForeignKey(d => d.ProductOrder)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanProductOrder_ProductOrder");
            });

            modelBuilder.Entity<PaymentPlanSchedule>(entity =>
            {
                entity.ToTable("PaymentPlanSchedule");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.ScheduleEnd).HasColumnType("datetime");

                entity.Property(e => e.ScheduleProjectedEnd).HasColumnType("datetime");

                entity.Property(e => e.ScheduleStart).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PaymentPlanScheduleCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedule_AspNetUsers");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.PaymentPlanScheduleLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedule_AspNetUsers1");

                entity.HasOne(d => d.PaymentPlanNavigation)
                    .WithMany(p => p.PaymentPlanSchedules)
                    .HasForeignKey(d => d.PaymentPlan)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedule_PaymentPlan");
            });

            modelBuilder.Entity<PaymentPlanSchedulePayment>(entity =>
            {
                entity.ToTable("PaymentPlanSchedulePayment");

                entity.Property(e => e.ActualAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ActualDate).HasColumnType("datetime");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.DueAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PaymentPlanSchedulePaymentCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedulePayment_AspNetUsers");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.PaymentPlanSchedulePaymentLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedulePayment_AspNetUsers1");

                entity.HasOne(d => d.LedgerTransactionNavigation)
                    .WithMany(p => p.PaymentPlanSchedulePayments)
                    .HasForeignKey(d => d.LedgerTransaction)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedulePayment_UserLedgerTransaction");

                entity.HasOne(d => d.PlanScheduleNavigation)
                    .WithMany(p => p.PaymentPlanSchedulePayments)
                    .HasForeignKey(d => d.PlanSchedule)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedulePayment_PaymentPlanSchedule");
            });

            modelBuilder.Entity<PossibleAnswer>(entity =>
            {
                entity.ToTable("PossibleAnswer");

                entity.Property(e => e.Answer)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.QuestionNavigation)
                    .WithMany(p => p.PossibleAnswers)
                    .HasForeignKey(d => d.Question)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Answer_Question1");
            });

            modelBuilder.Entity<Price>(entity =>
            {
                entity.ToTable("Price");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Prices)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Price_AspNetUsers");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.ImagePathLarge).IsUnicode(false);

                entity.Property(e => e.ImagePathMedium).IsUnicode(false);

                entity.Property(e => e.ImagePathSmall).IsUnicode(false);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(450);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Ph)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("PH");

                entity.Property(e => e.ShippingWeightLb).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Sku)
                    .IsRequired()
                    .IsUnicode(false)
                    .HasColumnName("SKU");

                entity.Property(e => e.VolumeInFluidOunces).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.CostNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.Cost)
                    .HasConstraintName("FK_Product_Cost1");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.ProductCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_AspNetUsers");

                entity.HasOne(d => d.CurrentPriceNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CurrentPrice)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_Price");

                entity.HasOne(d => d.KitTypeNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.KitType)
                    .HasConstraintName("FK_Product_KitType");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.ProductLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .HasConstraintName("FK_Product_AspNetUsers1");

                entity.HasOne(d => d.ProductCategoryNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductCategory)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_ProductCategory");

                entity.HasOne(d => d.ProductTypeNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_ProductType");

                entity.HasOne(d => d.SubscriptionTypeNavigation)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.SubscriptionType)
                    .HasConstraintName("FK_Product_SubscriptionType");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProductIngredient>(entity =>
            {
                entity.ToTable("ProductIngredient");

                entity.HasOne(d => d.IngredientNavigation)
                    .WithMany(p => p.ProductIngredients)
                    .HasForeignKey(d => d.Ingredient)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductIngredient_Ingredient");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.ProductIngredients)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductIngredient_Product");
            });

            modelBuilder.Entity<ProductOrder>(entity =>
            {
                entity.ToTable("ProductOrder");

                entity.Property(e => e.ApplicableTaxes).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CancelReason).IsUnicode(false);

                entity.Property(e => e.CancelledBy)
                    .HasMaxLength(450)
                    .IsUnicode(false);

                entity.Property(e => e.CancelledOn).HasColumnType("datetime");

                entity.Property(e => e.ClientIpAddress)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.CodeDiscount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CouponDiscount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DatePlaced).HasColumnType("datetime");

                entity.Property(e => e.ShippingCost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StripeChargeId)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.StripeChargeStatus)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StripeRefundId)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.ProductOrders)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrder_AspNetUsers");
            });

            modelBuilder.Entity<ProductOrderDiscountCode>(entity =>
            {
                entity.ToTable("ProductOrderDiscountCode");

                entity.HasOne(d => d.CodeNavigation)
                    .WithMany(p => p.ProductOrderDiscountCodes)
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrderDiscountCode_DiscountCode");

                entity.HasOne(d => d.ProductOrderNavigation)
                    .WithMany(p => p.ProductOrderDiscountCodes)
                    .HasForeignKey(d => d.ProductOrder)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrderDiscountCode_ProductOrder");
            });

            modelBuilder.Entity<ProductOrderDiscountCoupon>(entity =>
            {
                entity.ToTable("ProductOrderDiscountCoupon");

                entity.HasOne(d => d.CouponNavigation)
                    .WithMany(p => p.ProductOrderDiscountCoupons)
                    .HasForeignKey(d => d.Coupon)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrderDiscountCoupon_DiscountCoupon");

                entity.HasOne(d => d.ProductOrderNavigation)
                    .WithMany(p => p.ProductOrderDiscountCoupons)
                    .HasForeignKey(d => d.ProductOrder)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrderDiscountCoupon_ProductOrder");
            });

            modelBuilder.Entity<ProductOrderLineItem>(entity =>
            {
                entity.ToTable("ProductOrderLineItem");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.ImageSource).IsUnicode(false);

                entity.Property(e => e.Name).IsUnicode(false);

                entity.Property(e => e.PhBalance).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShippingWeightLb).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Sku)
                    .IsUnicode(false)
                    .HasColumnName("SKU");

                entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.VolumeInFluidOunces).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.ProductOrderLineItems)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderProduct_Product");

                entity.HasOne(d => d.ProductOrderNavigation)
                    .WithMany(p => p.ProductOrderLineItems)
                    .HasForeignKey(d => d.ProductOrder)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderProduct_ProductOrder");
            });

            modelBuilder.Entity<ProductSubCategory>(entity =>
            {
                entity.ToTable("ProductSubCategory");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.CategoryNavigation)
                    .WithMany(p => p.ProductSubCategories)
                    .HasForeignKey(d => d.Category)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductSubCategory_ProductCategory");
            });

            modelBuilder.Entity<ProductSubUnderCategory>(entity =>
            {
                entity.ToTable("ProductSubUnderCategory");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.SubCategoryNavigation)
                    .WithMany(p => p.ProductSubUnderCategories)
                    .HasForeignKey(d => d.SubCategory)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductSubUnderCategory_ProductSubCategory");
            });

            modelBuilder.Entity<ProductType>(entity =>
            {
                entity.ToTable("ProductType");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Question");

                entity.Property(e => e.QuestionText)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.QuestionnaireNavigation)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.Questionnaire)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Question_Questionnaire");
            });

            modelBuilder.Entity<Questionnaire>(entity =>
            {
                entity.ToTable("Questionnaire");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.QuestionnaireName)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Questionnaires)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Questionnaire_AspNetUsers");
            });

            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.ToTable("ShoppingCart");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.ShoppingCarts)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCart_AspNetUsers");
            });

            modelBuilder.Entity<ShoppingCartDiscountCode>(entity =>
            {
                entity.ToTable("ShoppingCartDiscountCode");

                entity.HasOne(d => d.CodeNavigation)
                    .WithMany(p => p.ShoppingCartDiscountCodes)
                    .HasForeignKey(d => d.Code)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartDiscountCode_DiscountCode");

                entity.HasOne(d => d.ShoppingCartNavigation)
                    .WithMany(p => p.ShoppingCartDiscountCodes)
                    .HasForeignKey(d => d.ShoppingCart)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartDiscountCode_ShoppingCart");
            });

            modelBuilder.Entity<ShoppingCartDiscountCoupon>(entity =>
            {
                entity.ToTable("ShoppingCartDiscountCoupon");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.CouponNavigation)
                    .WithMany(p => p.ShoppingCartDiscountCoupons)
                    .HasForeignKey(d => d.Coupon)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartDiscountCoupon_DiscountCoupon");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.ShoppingCartDiscountCoupon)
                    .HasForeignKey<ShoppingCartDiscountCoupon>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartDiscountCoupon_ShoppingCart");
            });

            modelBuilder.Entity<ShoppingCartHistory>(entity =>
            {
                entity.ToTable("ShoppingCartHistory");

                entity.Property(e => e.DateAddedToCart).HasColumnType("datetime");

                entity.Property(e => e.DateRemovedFromCart).HasColumnType("datetime");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.ShoppingCartHistories)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartHistory_Product1");

                entity.HasOne(d => d.ShoppingCartNavigation)
                    .WithMany(p => p.ShoppingCartHistories)
                    .HasForeignKey(d => d.ShoppingCart)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartHistory_ShoppingCart2");
            });

            modelBuilder.Entity<ShoppingCartLineItem>(entity =>
            {
                entity.ToTable("ShoppingCartLineItem");

                entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.ShoppingCartLineItems)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartLineItem_Product");

                entity.HasOne(d => d.ShoppingCartNavigation)
                    .WithMany(p => p.ShoppingCartLineItems)
                    .HasForeignKey(d => d.ShoppingCart)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartLineItem_ShoppingCart");
            });

            modelBuilder.Entity<StateU>(entity =>
            {
                entity.ToTable("StateUS");

                entity.Property(e => e.StateAbbreviation)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("Subscription");

                entity.Property(e => e.ImagePathLarge).IsUnicode(false);

                entity.Property(e => e.Subscription1).HasColumnName("Subscription");

                entity.HasOne(d => d.Subscription1Navigation)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(d => d.Subscription1)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subscription_Product");

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subscription_SubscriptionType");
            });

            modelBuilder.Entity<SubscriptionCustomer>(entity =>
            {
                entity.ToTable("SubscriptionCustomer");

                entity.Property(e => e.Customer)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.CustomerNavigation)
                    .WithMany(p => p.SubscriptionCustomers)
                    .HasForeignKey(d => d.Customer)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubscriptionCustomer_AspNetUsers");

                entity.HasOne(d => d.SubscriptionNavigation)
                    .WithMany(p => p.SubscriptionCustomers)
                    .HasForeignKey(d => d.Subscription)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubscriptionCustomer_Subscription");
            });

            modelBuilder.Entity<SubscriptionProduct>(entity =>
            {
                entity.ToTable("SubscriptionProduct");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.SubscriptionProducts)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubscriptionProduct_Product");

                entity.HasOne(d => d.SubscriptionNavigation)
                    .WithMany(p => p.SubscriptionProducts)
                    .HasForeignKey(d => d.Subscription)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubscriptionProduct_Subscription");
            });

            modelBuilder.Entity<SubscriptionShipmentSchedule>(entity =>
            {
                entity.ToTable("SubscriptionShipmentSchedule");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ShipOn).HasColumnType("datetime");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.SubscriptionShipmentSchedule)
                    .HasForeignKey<SubscriptionShipmentSchedule>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubscriptionShipmentSchedule_Subscription");
            });

            modelBuilder.Entity<SubscriptionType>(entity =>
            {
                entity.ToTable("SubscriptionType");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SynergeticIngredient>(entity =>
            {
                entity.ToTable("SynergeticIngredient");

                entity.HasOne(d => d.IngredientANavigation)
                    .WithMany(p => p.SynergeticIngredientIngredientANavigations)
                    .HasForeignKey(d => d.IngredientA)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SynergeticIngredient_Ingredient");

                entity.HasOne(d => d.IngredientBNavigation)
                    .WithMany(p => p.SynergeticIngredientIngredientBNavigations)
                    .HasForeignKey(d => d.IngredientB)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SynergeticIngredient_Ingredient1");
            });

            modelBuilder.Entity<TransactionConcept>(entity =>
            {
                entity.ToTable("TransactionConcept");

                entity.HasIndex(e => e.Name, "IX_TransactionConcept")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.ToTable("UserAnswer");

                entity.Property(e => e.Answer)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.AnswerDate).HasColumnType("datetime");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.QuestionNavigation)
                    .WithMany(p => p.UserAnswers)
                    .HasForeignKey(d => d.Question)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Answer_Question");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.UserAnswers)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserAnswer_AspNetUsers");
            });

            modelBuilder.Entity<UserCommonAllergen>(entity =>
            {
                entity.ToTable("UserCommonAllergen");

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.CommonAllergenNavigation)
                    .WithMany(p => p.UserCommonAllergens)
                    .HasForeignKey(d => d.CommonAllergen)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserCommonAllergen_CommonAllergen");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.UserCommonAllergens)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserCommonAllergen_AspNetUsers");
            });

            modelBuilder.Entity<UserLedgerTransaction>(entity =>
            {
                entity.ToTable("UserLedgerTransaction");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.BalanceAfterTransaction).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.BalanceBeforeTransaction).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.ConceptNavigation)
                    .WithMany(p => p.UserLedgerTransactions)
                    .HasForeignKey(d => d.Concept)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLedgerTransaction_TransactionConcept");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.UserLedgerTransactionCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLedgerTransaction_AspNetUsers1");

                entity.HasOne(d => d.ProductOrderNavigation)
                    .WithMany(p => p.UserLedgerTransactions)
                    .HasForeignKey(d => d.ProductOrder)
                    .HasConstraintName("FK_UserLedgerTransaction_ProductOrder");

                entity.HasOne(d => d.TransactionTypeNavigation)
                    .WithMany(p => p.UserLedgerTransactions)
                    .HasForeignKey(d => d.TransactionType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLedgerTransaction_UserLedgerTransactionType");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.UserLedgerTransactionUserNavigations)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLedgerTransaction_AspNetUsers");
            });

            modelBuilder.Entity<UserLedgerTransactionType>(entity =>
            {
                entity.ToTable("UserLedgerTransactionType");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserProductList>(entity =>
            {
                entity.ToTable("UserProductList");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.UserProductLists)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserProductList_AspNetUsers");
            });

            modelBuilder.Entity<UserProductListProduct>(entity =>
            {
                entity.ToTable("UserProductListProduct");

                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.DateRemoved).HasColumnType("datetime");

                entity.HasOne(d => d.UserProductListNavigation)
                    .WithMany(p => p.UserProductListProducts)
                    .HasForeignKey(d => d.UserProductList)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserProductListProduct_Product");

                entity.HasOne(d => d.UserProductList1)
                    .WithMany(p => p.UserProductListProducts)
                    .HasForeignKey(d => d.UserProductList)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserProductListProduct_UserProductList");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
