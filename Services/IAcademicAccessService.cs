namespace CrudOperation.Service;

public interface IAcademicAccessService
{ 
    Task<bool> TeacherHasClassAsync(int teacherUserId, int classId, CancellationToken ct = default);
 
    Task<bool> TeacherHasSubjectAsync(int teacherUserId, int subjectId, CancellationToken ct = default);

    
    Task<bool> TeacherHasStudentAsync(int teacherUserId, int studentId, CancellationToken ct = default);
}