using DynamicTimeTableMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace DynamicTimeTableMVC.Controllers
{
    public class TimetableController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new TimetableViewModel { Input = new TimetableInput() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetSubjectHours(TimetableInput input)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new TimetableViewModel { Input = input });
            }

            var model = new TimetableViewModel
            {
                Input = input,
                SubjectHours = Enumerable.Range(0, input.TotalSubjects)
                    .Select(_ => new SubjectHour()).ToList()
            };

            return View("SetSubjectHours", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Generate(TimetableViewModel model)
        {
            // Validate SubjectHours
            if (model.SubjectHours == null || model.SubjectHours.Count != model.Input.TotalSubjects)
            {
                ModelState.AddModelError("", "Number of subjects must match total subjects specified.");
                return View("SetSubjectHours", model);
            }

            // Validate each subject entry
            for (int i = 0; i < model.SubjectHours.Count; i++)
            {
                var subjectHour = model.SubjectHours[i];
                if (string.IsNullOrWhiteSpace(subjectHour.SubjectName))
                    ModelState.AddModelError($"SubjectHours[{i}].SubjectName", "Subject name is required.");
                if (subjectHour.Hours <= 0)
                    ModelState.AddModelError($"SubjectHours[{i}].Hours", "Hours must be a positive number.");
            }

            // Validate total hours
            var totalSubjectHours = model.SubjectHours.Sum(sh => sh.Hours);
            if (totalSubjectHours != model.Input.TotalHours)
            {
                ModelState.AddModelError("", $"Total subject hours ({totalSubjectHours}) must equal total weekly hours ({model.Input.TotalHours}).");
                return View("SetSubjectHours", model);
            }

            try
            {
                model.GeneratedTimetable = GenerateTimetable(model);
                return View("Result", model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Error generating timetable: {ex.Message}";
                return View("SetSubjectHours", model);
            }
        }

        private string[,] GenerateTimetable(TimetableViewModel model)
        {
            string[,] timetable = new string[model.Input.SubjectsPerDay, model.Input.WorkingDays];
            var subjectHours = model.SubjectHours.ToDictionary(sh => sh.SubjectName, sh => sh.Hours);
            var totalSlots = model.Input.TotalHours;

            if (subjectHours.Values.Sum() != totalSlots)
                throw new InvalidOperationException("Total subject hours do not match timetable slots.");

            int row = 0, col = 0;
            while (subjectHours.Values.Sum() > 0)
            {
                var availableSubjects = subjectHours.Where(sh => sh.Value > 0).ToList();
                if (!availableSubjects.Any()) break;

                string subjectToPlace;
                if (col == 0 || col == model.Input.WorkingDays - 1)
                {
                    // Place subjects with fewer hours on edges
                    subjectToPlace = availableSubjects.OrderBy(s => s.Value).ThenBy(s => CountInColumn(timetable, col, s.Key, row)).First().Key;
                }
                else
                {
                    // Place subjects with more hours in the middle
                    subjectToPlace = availableSubjects.OrderByDescending(s => s.Value).ThenBy(s => CountInAdjacent(timetable, row, col, s.Key)).First().Key;
                }

                timetable[row, col] = subjectToPlace;
                subjectHours[subjectToPlace]--;

                col++;
                if (col >= model.Input.WorkingDays)
                {
                    col = 0;
                    row++;
                }
            }

            return timetable;
        }

        private int CountInColumn(string[,] timetable, int col, string subject, int maxRow)
        {
            int count = 0;
            for (int r = 0; r <= maxRow && r < timetable.GetLength(0); r++)
            {
                if (timetable[r, col] == subject) count++;
            }
            return count;
        }

        private int CountInAdjacent(string[,] timetable, int row, int col, string subject)
        {
            int count = 0;
            int rows = timetable.GetLength(0);
            int cols = timetable.GetLength(1);

            if (row > 0 && timetable[row - 1, col] == subject) count++;
            if (col > 0 && timetable[row, col - 1] == subject) count++;
            if (row < rows - 1 && timetable[row + 1, col] == subject) count++;
            if (col < cols - 1 && timetable[row, col + 1] == subject) count++;

            return count;
        }
    }
}