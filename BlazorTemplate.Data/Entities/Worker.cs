using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTemplate.Data.Entities;
public partial class Worker {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int WorkerId { get; set; }
    [ForeignKey(nameof(AssignedCompany))]
    public int? AssignedCompanyId { get; set; }
    [Required]
    public string FirstName { get; set; } = default!;
    [Required]
    public string LastName { get; set; } = default!;
    [Required]
    public string Email { get; set; } = default!;
    [Required]
    public string PhoneNumber { get; set; } = default!;
    public DateOnly BirthDate { get; set; }

    public virtual Company? AssignedCompany { get; set; }
}
