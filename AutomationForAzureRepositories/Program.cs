namespace AutomationForAzureRepositories
{
    internal class Program
    {
        /// <summary>
        /// Change to folder that contain repositories
        /// </summary>
        //private const string RepositoriesFolderPath = "C:\\git";
        private const string RepositoriesFolderPath = "C:\\git\\UI";

        /// <summary>
        /// Change project for PR
        /// </summary>
        private const string ProjectForPr = RepositoriesConstants.FrontEndConstants.MxProgramManagementFrontend;
        
        /// <summary>
        /// Name of target branch for Pull Request
        /// </summary>
        private const string TargetBranch = TargetBranchConstants.Dev;

        /// <summary>
        /// Full task name copied from clipboard, E.G. 'Task 100200: test'
        /// </summary>
        private const string FullTaskTitle = "Task 352826: Fix: Cannot close all modals on MxOrders page";

        /// <summary>
        /// Commit message for merge 'dev' branch 'master' branch
        /// </summary>
        private const string CommitMessageForMergeDevToMaster = "Merge for sprint 27";

        private static void Main()
        {
            var azureService = new AzureAutomationService(new GitService());
            //CreatePrForTask(azureService);
            CreatePullRequestsFromDevToMaster(azureService);
        }

        private static void CreatePrForTask(AzureAutomationService azureService)
        {
            azureService.CreateBranchForTask(RepositoriesFolderPath, ProjectForPr, FullTaskTitle);
            azureService.CommitForTask(RepositoriesFolderPath, ProjectForPr, FullTaskTitle);
            azureService.PushForTask(RepositoriesFolderPath, ProjectForPr, FullTaskTitle);
            azureService.CreatePullRequestForTask(ProjectForPr, FullTaskTitle, TargetBranch);
        }

        private static void CreatePullRequestsFromDevToMaster(AzureAutomationService azureService)
        {
            azureService.CreatePullRequestsFromDevToMaster(CommitMessageForMergeDevToMaster);
        }
    }
}
