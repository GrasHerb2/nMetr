//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Metr
{
    using System;
    using System.Collections.Generic;
    
    public partial class Operation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Operation()
        {
            this.OperationDevice = new HashSet<OperationDevice>();
        }
    
        public int Operation_ID { get; set; }
        public System.DateTime OperationDate { get; set; }
        public string OperationText { get; set; }
        public int UserID { get; set; }
        public string ComputerName { get; set; }
        public Nullable<int> ID_Type { get; set; }
        public Nullable<int> ID_Status { get; set; }
    
        public virtual OperationStatus OperationStatus { get; set; }
        public virtual OperationType OperationType { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OperationDevice> OperationDevice { get; set; }
    }
}