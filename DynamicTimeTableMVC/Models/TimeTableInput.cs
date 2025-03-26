using System.ComponentModel.DataAnnotations;

namespace DynamicTimeTableMVC.Models
{
    public class TimetableInput
    {
        [Required(ErrorMessage = "Number of working days is required")]
        [Range(1, 7, ErrorMessage = "Working days must be between 1 and 7")]
        public int WorkingDays { get; set; }

        [Required(ErrorMessage = "Number of subjects per day is required")]
        [Range(1, 8, ErrorMessage = "Subjects per day must be between 1 and 8")]
        public int SubjectsPerDay { get; set; }

        [Required(ErrorMessage = "Total subjects are required")]
        [Range(1, int.MaxValue, ErrorMessage = "Total subjects must be a positive number")]
        public int TotalSubjects { get; set; }

        public int TotalHours => WorkingDays * SubjectsPerDay;
    }

    public class SubjectHour
    {
        [Required(ErrorMessage = "Subject name is required")]
        public string SubjectName { get; set; }

        [Required(ErrorMessage = "Hours are required")]
        [Range(1, int.MaxValue, ErrorMessage = "Hours must be a positive number")]
        public int Hours { get; set; }
    }

    public class TimetableViewModel
    {
        public TimetableInput Input { get; set; }
        public List<SubjectHour> SubjectHours { get; set; }
        public string[,] GeneratedTimetable { get; set; }
        public string ErrorMessage { get; set; }
    }

}
