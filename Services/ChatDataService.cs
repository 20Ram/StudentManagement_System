namespace CrudOperation.Service
{
    public class ChatDataService : IChatDataService
    {
        public Task<string> GetStudentContextAsync(string userId)
        {
            return Task.FromResult(
                "Student profile data is not connected yet.");
        }

        public Task<string> GetCourseContextAsync(
            string userId,
            string role)
        {
            return Task.FromResult(
                "Course data is not connected yet.");
        }

        public Task<string> GetStudentStatisticsAsync(string role)
        {
            return Task.FromResult(
                "Student statistics are not connected yet.");
        }
    }
}