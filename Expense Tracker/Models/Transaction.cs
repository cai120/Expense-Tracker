using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    //Transaction Model
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage ="A Category Is Required")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int Amount { get; set; }

        [Column(TypeName ="nvarchar(25)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; }

        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return Category == null?"": Category.Icon + " " + Category.Title;
            } 
        }
        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense")? "-" : "+") + Amount.ToString("C0");
            }
        }
    }
}
