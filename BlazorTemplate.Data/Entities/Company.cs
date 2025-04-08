using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTemplate.Data.Entities;
public partial class Company
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CompanyId { get; set; }
    [Required]
    public string Name { get; set; } = default!;
    public virtual ICollection<Worker> Workers { get; set; } = [];
    //added v1
    public int WorkerCount { get; set; }
    //added v2
    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

}
