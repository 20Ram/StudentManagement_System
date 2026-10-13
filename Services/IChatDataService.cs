namespace CrudOperation.Service
{
    public interface IChatDataService
    {
        Task<string> GetStudentContextAsync(string userId);

        Task<string> GetCourseContextAsync(
            string userId,
            string role);

        Task<string> GetStudentStatisticsAsync(string role);
    }
}