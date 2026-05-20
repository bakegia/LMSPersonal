using LMSfinal.Models.Enums;

namespace LMSfinal.Models.Utilities
{
    /// <summary>
    /// Lớp tiện ích để tính toán điểm và hạng điểm
    /// </summary>
    public static class GradeCalculator
    {
        /// <summary>
        /// Tính hạng điểm dựa trên điểm tổng
        /// </summary>
        /// <param name="finalScore">Điểm tổng (0-10)</param>
        /// <returns>Hạng điểm (A, B, C, D, F)</returns>
        public static GradeLetterEnum CalculateGradeLetter(decimal finalScore)
        {
            return finalScore switch
            {
                >= 8.5m => GradeLetterEnum.A,
                >= 7.0m => GradeLetterEnum.B,
                >= 5.5m => GradeLetterEnum.C,
                >= 4.0m => GradeLetterEnum.D,
                _ => GradeLetterEnum.F
            };
        }

        /// <summary>
        /// Tính GPA từ điểm tổng (thang điểm 0-4)
        /// </summary>
        /// <param name="finalScore">Điểm tổng (0-10)</param>
        /// <returns>GPA (0-4)</returns>
        public static decimal CalculateGPA(decimal finalScore)
        {
            if (finalScore < 0 || finalScore > 10)
                return 0;

            return Math.Round((finalScore / 10) * 4, 2);
        }

        /// <summary>
        /// Tính điểm tổng từ 3 thành phần
        /// Công thức: (ProcessScore * 0.3) + (MidtermScore * 0.2) + (FinalExamScore * 0.5)
        /// </summary>
        /// <param name="processScore">Điểm quá trình (trọng số 30%)</param>
        /// <param name="midtermScore">Điểm giữa kỳ (trọng số 20%)</param>
        /// <param name="finalExamScore">Điểm thi (trọng số 50%)</param>
        /// <returns>Điểm tổng (0-10)</returns>
        public static decimal CalculateFinalScore(decimal? processScore, decimal? midtermScore, decimal? finalExamScore)
        {
            // Nếu bất kỳ điểm nào chưa nhập (null), trả về 0
            if (processScore == null || midtermScore == null || finalExamScore == null)
                return 0;

            var finalScore = ((decimal)processScore * 0.3m) + ((decimal)midtermScore * 0.2m) + ((decimal)finalExamScore * 0.5m);

            // Làm tròn đến 2 chữ số thập phân
            return Math.Round(finalScore, 2);
        }

        /// <summary>
        /// Lấy mô tả của hạng điểm
        /// </summary>
        /// <param name="gradeLetter">Hạng điểm</param>
        /// <returns>Mô tả hạng điểm (ví dụ: "Xuất sắc", "Giỏi")</returns>
        public static string GetGradeDescription(GradeLetterEnum gradeLetter)
        {
            return gradeLetter switch
            {
                GradeLetterEnum.A => "Xuất sắc",
                GradeLetterEnum.B => "Giỏi",
                GradeLetterEnum.C => "Khá",
                GradeLetterEnum.D => "Trung bình",
                GradeLetterEnum.F => "Yếu",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Lấy khoảng điểm của hạng điểm
        /// </summary>
        /// <param name="gradeLetter">Hạng điểm</param>
        /// <returns>Chuỗi mô tả khoảng điểm</returns>
        public static string GetGradeRange(GradeLetterEnum gradeLetter)
        {
            return gradeLetter switch
            {
                GradeLetterEnum.A => "8.5 - 10.0",
                GradeLetterEnum.B => "7.0 - 8.4",
                GradeLetterEnum.C => "5.5 - 6.9",
                GradeLetterEnum.D => "4.0 - 5.4",
                GradeLetterEnum.F => "< 4.0",
                _ => "Không xác định"
            };
        }
    }
}