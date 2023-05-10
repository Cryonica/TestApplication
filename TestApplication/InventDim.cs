namespace TestApplication
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("InventDim")]
    public partial class InventDim
    {
        [StringLength(20)]
        public string InventDimId { get; set; }

        [StringLength(10)]
        public string InventSiteId { get; set; }

        [StringLength(10)]
        public string InventLocationId { get; set; }

        [StringLength(10)]
        public string WMSLocationId { get; set; }

        [StringLength(20)]
        public string InventBatchId { get; set; }

        [StringLength(18)]
        public string WMSPalletId { get; set; }

        [StringLength(10)]
        public string InventColorId { get; set; }

        [StringLength(20)]
        public string InventSerialId { get; set; }

        [StringLength(10)]
        public string InventSizeId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecordId { get; set; }

        [Column(TypeName = "timestamp")]
        [MaxLength(8)]
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [StringLength(5)]
        public string CreatedBy { get; set; }

        [StringLength(5)]
        public string ModifiedBy { get; set; }
    }
}
