using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

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
        public virtual DbSet<CityPr> CityPrs { get; set; }
        public virtual DbSet<CityStateCountryWorld> CityStateCountryWorlds { get; set; }
        public virtual DbSet<CityStateU> CityStateUs { get; set; }
        public virtual DbSet<CommonAllergen> CommonAllergens { get; set; }
        public virtual DbSet<ContradictingIngredient> ContradictingIngredients { get; set; }
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
        public virtual DbSet<OrderProduct> OrderProducts { get; set; }
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
        public virtual DbSet<ProductSubCategory> ProductSubCategories { get; set; }
        public virtual DbSet<ProductSubUnderCategory> ProductSubUnderCategories { get; set; }
        public virtual DbSet<ProductType> ProductTypes { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Questionnaire> Questionnaires { get; set; }
        public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public virtual DbSet<ShoppingCartDiscountCode> ShoppingCartDiscountCodes { get; set; }
        public virtual DbSet<ShoppingCartDiscountCoupon> ShoppingCartDiscountCoupons { get; set; }
        public virtual DbSet<ShoppingCartHistory> ShoppingCartHistories { get; set; }
        public virtual DbSet<ShoppingCartProduct> ShoppingCartProducts { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<SubscriptionProduct> SubscriptionProducts { get; set; }
        public virtual DbSet<SubscriptionShipmentSchedule> SubscriptionShipmentSchedules { get; set; }
        public virtual DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public virtual DbSet<SynergeticIngredient> SynergeticIngredients { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAnswer> UserAnswers { get; set; }
        public virtual DbSet<UserCommonAllergen> UserCommonAllergens { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }
        public virtual DbSet<UserLedgerTransaction> UserLedgerTransactions { get; set; }
        public virtual DbSet<UserLedgerTransactionType> UserLedgerTransactionTypes { get; set; }
        public virtual DbSet<UserProductList> UserProductLists { get; set; }
        public virtual DbSet<UserProductListProduct> UserProductListProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("User ID=xoskinapp;Password=qu3rtY8085:;Initial Catalog=XOSkin;Server=SIR-GALAHAD");
                //optionsBuilder.UseSqlServer("Server=tcp:xoskin.database.windows.net,1433;Initial Catalog=XOSkinQATest;Persist Security Info=False;User ID=xoskin;Password=qu3rtY8085:;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
      }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Apartment)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CityPr).HasColumnName("CityPR");

                entity.Property(e => e.CityUs).HasColumnName("CityUS");

                entity.Property(e => e.CountryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ForeignPostalCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Line1)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Line2).IsUnicode(false);

                entity.Property(e => e.StateName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ZipCode4)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.ZipCode5)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.AddressTypeNavigation)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.AddressType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Address_AddressType");

                entity.HasOne(d => d.CityPrNavigation)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.CityPr)
                    .HasConstraintName("FK_Address_CityPR");

                entity.HasOne(d => d.CityUsNavigation)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.CityUs)
                    .HasConstraintName("FK_Address_CityStateUS");

                entity.HasOne(d => d.CityWorldNavigation)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.CityWorld)
                    .HasConstraintName("FK_Address_CityStateCountryWorld");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.User)
                    .HasConstraintName("FK_Address_User");
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

            modelBuilder.Entity<DiscountCode>(entity =>
            {
                entity.ToTable("DiscountCode");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.DiscountAsInNproductDollars).HasColumnName("DiscountAsInNProductDollars");

                entity.Property(e => e.DiscountAsInNproductPercentage).HasColumnName("DiscountAsInNProductPercentage");

                entity.Property(e => e.DiscountGlobalOrderDollars).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DiscountGlobalOrderPercentage).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DiscountInNproductDollars)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DiscountInNProductDollars");

                entity.Property(e => e.DiscountNproductPercentage)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DiscountNProductPercentage");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.ValidFrom).HasColumnType("datetime");

                entity.Property(e => e.ValidTo).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.DiscountCodeCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DiscountCode_User");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.DiscountCodeLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .HasConstraintName("FK_DiscountCode_User1");
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

                entity.Property(e => e.DiscountAsInNproductDollars).HasColumnName("DiscountAsInNProductDollars");

                entity.Property(e => e.DiscountAsInNproductPercentage).HasColumnName("DiscountAsInNProductPercentage");

                entity.Property(e => e.DiscountGlobalOrderDollars).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DiscountGlobalOrderPercentage).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DiscountInNproductDollars)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DiscountInNProductDollars");

                entity.Property(e => e.DiscountNproductPercentage)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("DiscountNProductPercentage");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.ValidFrom).HasColumnType("datetime");

                entity.Property(e => e.ValidTo).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.DiscountCouponCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DiscountCoupon_User");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.DiscountCouponLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .HasConstraintName("FK_DiscountCoupon_User1");
            });

            modelBuilder.Entity<DiscountCouponProduct>(entity =>
            {
                entity.ToTable("DiscountCouponProduct");

                entity.Property(e => e.Id).ValueGeneratedNever();

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
                    .WithMany(p => p.KitProducts)
                    .HasForeignKey(d => d.Kit)
                    .HasConstraintName("FK_KitProduct_Product");
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

            modelBuilder.Entity<OrderProduct>(entity =>
            {
                entity.ToTable("OrderProduct");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany(p => p.OrderProducts)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderProduct_Product");

                entity.HasOne(d => d.ProductOrderNavigation)
                    .WithMany(p => p.OrderProducts)
                    .HasForeignKey(d => d.ProductOrder)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderProduct_ProductOrder");
            });

            modelBuilder.Entity<OrderShipTo>(entity =>
            {
                entity.ToTable("OrderShipTo");

                entity.Property(e => e.Apartment)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CarrierName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CityPr).HasColumnName("CityPR");

                entity.Property(e => e.CityUs).HasColumnName("CityUS");

                entity.Property(e => e.CountryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ForeignPostalCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Line1)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Line2).IsUnicode(false);

                entity.Property(e => e.ShipDate).HasColumnType("datetime");

                entity.Property(e => e.StateName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.TrackingNumber).IsUnicode(false);

                entity.Property(e => e.ZipCode4)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.ZipCode5)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.HasOne(d => d.CityPrNavigation)
                    .WithMany(p => p.OrderShipTos)
                    .HasForeignKey(d => d.CityPr)
                    .HasConstraintName("FK_OrderShipTo_CityPR");

                entity.HasOne(d => d.CityUsNavigation)
                    .WithMany(p => p.OrderShipTos)
                    .HasForeignKey(d => d.CityUs)
                    .HasConstraintName("FK_OrderShipTo_CityStateUS");

                entity.HasOne(d => d.CityWorldNavigation)
                    .WithMany(p => p.OrderShipTos)
                    .HasForeignKey(d => d.CityWorld)
                    .HasConstraintName("FK_OrderShipTo_CityStateCountryWorld");

                entity.HasOne(d => d.OrderNavigation)
                    .WithMany(p => p.OrderShipTos)
                    .HasForeignKey(d => d.Order)
                    .HasConstraintName("FK_OrderShipTo_ProductOrder");
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

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PaymentPlanCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlan_User1");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.PaymentPlanUserNavigations)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlan_User");
            });

            modelBuilder.Entity<PaymentPlanProductOrder>(entity =>
            {
                entity.ToTable("PaymentPlanProductOrder");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PaymentPlanProductOrderCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanProductOrder_User");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.PaymentPlanProductOrderLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanProductOrder_User1");

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

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.ScheduleEnd).HasColumnType("datetime");

                entity.Property(e => e.ScheduleProjectedEnd).HasColumnType("datetime");

                entity.Property(e => e.ScheduleStart).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PaymentPlanScheduleCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedule_User3");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.PaymentPlanScheduleLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedule_User4");

                entity.HasOne(d => d.PaymentPlanNavigation)
                    .WithMany(p => p.PaymentPlanSchedules)
                    .HasForeignKey(d => d.PaymentPlan)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedule_PaymentPlan");
            });

            modelBuilder.Entity<PaymentPlanSchedulePayment>(entity =>
            {
                entity.ToTable("PaymentPlanSchedulePayment");

                entity.Property(e => e.ActualAmount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.ActualDate).HasColumnType("datetime");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.DueAmount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.PaymentPlanSchedulePaymentCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedulePayment_User4");

                entity.HasOne(d => d.LastUpdatedByNavigation)
                    .WithMany(p => p.PaymentPlanSchedulePaymentLastUpdatedByNavigations)
                    .HasForeignKey(d => d.LastUpdatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaymentPlanSchedulePayment_User5");

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

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.ValidFrom).HasColumnType("datetime");

                entity.Property(e => e.ValidTo).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Prices)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Price_User");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.ImagePathLarge).IsUnicode(false);

                entity.Property(e => e.ImagePathMedium).IsUnicode(false);

                entity.Property(e => e.ImagePathSmall).IsUnicode(false);

                entity.Property(e => e.LastUpdated).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Ph)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("PH");

                entity.Property(e => e.Sku)
                    .IsRequired()
                    .IsUnicode(false)
                    .HasColumnName("SKU");

                entity.Property(e => e.VolumeInFluidOunces).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.ProductCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_User");

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
                    .HasConstraintName("FK_Product_User1");

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

                entity.Property(e => e.ApplicableTaxes).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CodeDiscount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CouponDiscount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DatePlaced).HasColumnType("datetime");

                entity.Property(e => e.ShippingCost).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Total).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.ProductOrders)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOrder_User");
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
                    .HasConstraintName("FK_Questionnaire_User");
            });

            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.ToTable("ShoppingCart");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.ShoppingCarts)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCart_User");
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
                    .HasConstraintName("FK_ShoppingCartHistory_ShoppingCart2");
            });

            modelBuilder.Entity<ShoppingCartProduct>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("ShoppingCartProduct");

                entity.HasOne(d => d.ProductNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartProduct_Product");

                entity.HasOne(d => d.ShoppingCartNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.ShoppingCart)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartProduct_ShoppingCart");
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("Subscription");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Subscription_SubscriptionType");
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

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.EmailAddress, "IX_User")
                    .IsUnique();

                entity.Property(e => e.AdditionalPhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.HomePhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PasswordHash).IsUnicode(false);

                entity.Property(e => e.WorkPhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.PreferredLanguageNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.PreferredLanguage)
                    .HasConstraintName("FK_User_Language");

                entity.HasOne(d => d.UserGroupNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UserGroup)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_UserGroup");
            });

            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.ToTable("UserAnswer");

                entity.Property(e => e.Answer)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.AnswerDate).HasColumnType("datetime");

                entity.HasOne(d => d.QuestionNavigation)
                    .WithMany(p => p.UserAnswers)
                    .HasForeignKey(d => d.Question)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Answer_Question");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.UserAnswers)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Answer_User");
            });

            modelBuilder.Entity<UserCommonAllergen>(entity =>
            {
                entity.ToTable("UserCommonAllergen");

                entity.HasOne(d => d.CommonAllergenNavigation)
                    .WithMany(p => p.UserCommonAllergens)
                    .HasForeignKey(d => d.CommonAllergen)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserCommonAllergen_CommonAllergen");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.UserCommonAllergens)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserCommonAllergen_User");
            });

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.ToTable("UserGroup");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserLedgerTransaction>(entity =>
            {
                entity.ToTable("UserLedgerTransaction");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.AmountAfterTransaction).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.AmountBeforeTransaction).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.UserLedgerTransactionCreatedByNavigations)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLedgerTransaction_User2");

                entity.HasOne(d => d.TransactionTypeNavigation)
                    .WithMany(p => p.UserLedgerTransactions)
                    .HasForeignKey(d => d.TransactionType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLedgerTransaction_UserLedgerTransactionType");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.UserLedgerTransactionUserNavigations)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLedgerTransaction_User3");
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

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.UserProductLists)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserProductList_User");
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
