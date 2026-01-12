namespace Tasco.ProjectService.Service.Constant
{
    public static class MenberRoleConstants
    {
        public const string Member = "MEMBER";
        public const string Owner = "OWNER";
        public static readonly List<string> AllRoles = new List<string>
           {
               Member,
               Owner
           };

        public const string ApprovedStatus = "APPROVED";
        public const string PendingStatus = "PENDING";
        public const string RejectedStatus = "REJECTED";
        public const string RemovedStatus = "REMOVED";
        public static readonly List<string> AllApprovedStatuses = new List<string>
           {
               ApprovedStatus,
               PendingStatus,
               RejectedStatus,
               RemovedStatus
           };
    }
}
