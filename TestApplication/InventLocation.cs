namespace TestApplication
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("InventLocation")]
    public partial class InventLocation
    {
        [StringLength(10)]
        public string InventLocationId { get; set; }

        [StringLength(10)]
        public string InventSiteId { get; set; }

        [StringLength(140)]
        public string Name { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecordId { get; set; }

        [Column(TypeName = "timestamp")]
        [MaxLength(8)]
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime UpdateDateTime { get; set; }

        [StringLength(5)]
        public string CreatedBy { get; set; }

        [StringLength(5)]
        public string ModifiedBy { get; set; }
    }
}
