//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IconicsArena.Context
{
    using System;
    using System.Collections.Generic;
    
    public partial class Booking
    {
        public int BookingId { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> SlotId { get; set; }
        public Nullable<bool> IsBooked { get; set; }
    
        public virtual Slot Slot { get; set; }
        public virtual User User { get; set; }
    }
}
