using System;
using ArbitralSystem.Domain.MarketInfo;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArbitralSystem.PublicMarketInfoService.Persistence.Entities
{
    [UsedImplicitly]
    public class PairInfo : IEntityTypeConfiguration<PairInfo>
    {
        public int Id { get; set; }
        public string ExchangePairName { get; set; }  
        public string UnificatedPairName { get; set; }
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set;}
        public DateTime UtcCreatedAt { get; set; }
        public DateTime? UtcDelistedAt { get; set; }
        public Exchange Exchange { get; set; }

        public void Configure(EntityTypeBuilder<PairInfo> builder)
        {
            builder.ToTable("PairInfos")
                .HasKey(o=>o.Id);
            
            // TODO: add new index
            // builder.HasIndex(i => new {i.ExchangePairName, i.Exchange}).IsUnique();
            builder.HasIndex(i => new {i.ExchangePairName, i.Exchange, i.UtcDelistedAt}).IsUnique();

            builder.Property(o => o.ExchangePairName)
                .HasColumnType("varchar(32)")
                .IsRequired();
            
            builder.Property(o => o.UnificatedPairName)
                .HasColumnType("varchar(32)")
                .IsRequired();
            
            builder.Property(o => o.BaseCurrency)
                .HasColumnType("varchar(16)")
                .IsRequired();
            
            builder.Property(o => o.QuoteCurrency)
                .HasColumnType("varchar(16)")
                .IsRequired();
            
            builder.Property(o => o.UtcCreatedAt)
                .HasColumnType("smalldatetime")
                .IsRequired();

            builder.Property(o => o.UtcDelistedAt)
                .HasColumnType("smalldatetime");

            builder.Property(o => o.Exchange)
                .HasColumnType("tinyint")
                .IsRequired();
        }
    }
}
