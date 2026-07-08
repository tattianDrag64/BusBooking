using System;
using System.ComponentModel.DataAnnotations;

namespace BusBookingDemo.Entity
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

    }
}
