using System.Linq;

namespace LMSfinal.Models.ViewModels.Student
{
    public class TuitionSemesterItemVM
    {
        public string SemesterLabel { get; set; } = string.Empty;
        public decimal BaseTuition { get; set; }
        public decimal Discount { get; set; }
        public decimal Paid { get; set; }

        public decimal Payable => BaseTuition - Discount;
        public decimal Balance => Payable - Paid;
    }

    public class StudentTuitionIndexVM
    {
        public List<TuitionSemesterItemVM> Items { get; set; } = new();
        public List<string> SemesterOptions { get; set; } = new();
        public string? SelectedSemester { get; set; }

        public decimal TotalPayable => Items.Sum(x => x.Payable);
        public decimal TotalPaid => Items.Sum(x => x.Paid);
        public decimal TotalBalance => Items.Sum(x => x.Balance);
    }
}