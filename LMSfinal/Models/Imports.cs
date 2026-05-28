using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.Imports
{
    public class QuizImportRow
    {
        public int RowIndex { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Range(1, 4)]
        public int Points { get; set; } = 1;

        public string AnswerA { get; set; } = string.Empty;
        public string AnswerB { get; set; } = string.Empty;
        public string AnswerC { get; set; } = string.Empty;
        public string AnswerD { get; set; } = string.Empty;

        public string CorrectLabel { get; set; } = "A";
    }

    public class QuizImportQuestion
    {
        public int RowIndex { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Points { get; set; }
        public int Order { get; set; }
        public List<QuizImportAnswer> Answers { get; set; } = new();
    }

    public class QuizImportAnswer
    {
        public string AnswerLabel { get; set; } = "A";
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Order { get; set; }
    }

    public class QuizImportError
    {
        public int RowIndex { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class QuizImportResult
    {
        public int TotalRows { get; set; }
        public int ValidQuestions { get; set; }
        public List<QuizImportQuestion> Questions { get; set; } = new();
        public List<QuizImportError> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}