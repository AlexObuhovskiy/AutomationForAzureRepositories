using System;
using System.IO;
using System.Management.Automation;

namespace AutomationForAzureRepositories
{
    public interface IGitService
    {
        string CreateBranch(
            string directoryPath,
            string branchName);

        void Commit(
            string directoryPath,
            string commitMessage);

        void Push(
            string directoryPath,
            string branchName);

        void CreatePullRequest(
            string repositoryName,
            string branchName,
            string targetBranch,
            string commitMessage,
            string taskNumber = "",
            bool autoComplete = false,
            bool deleteSourceBranch = false,
            bool transitionWorkItems = false);
    }

    public class GitService : IGitService
    {
        private const string AzureDevOpsOrganizationUrl = "https://dev.azure.com/dnvgl-one/";
        private const string AzureDevOpsProjectName = "Electric Grid Ecosystem";

        public string CreateBranch(
            string directoryPath,
            string branchName)
        {
            using var ps = PowerShell.Create();
            ChangeDirectory(ps, directoryPath);
            ps.AddScript($"git checkout -b {branchName}").Invoke();

            Console.WriteLine($"Created branch '{branchName}' and made checkout on it.");

            return branchName;
        }

        public void Commit(
            string directoryPath,
            string commitMessage)
        {
            using var ps = PowerShell.Create();
            ChangeDirectory(ps, directoryPath);

            ps.AddScript($"git commit -am \"{commitMessage}\"").Invoke();
            Console.WriteLine($"Made successful commit with message: '{commitMessage}'");
        }

        public void Push(
            string directoryPath,
            string branchName)
        {
            using var ps = PowerShell.Create();
            ChangeDirectory(ps, directoryPath);
            ps.AddScript($"git push --progress \"origin\" {branchName}").Invoke();

            Console.WriteLine($"Branch '{branchName}' pushed to origin");
        }

        public void CreatePullRequest(
            string repositoryName,
            string branchName,
            string targetBranch,
            string commitMessage,
            string taskNumber = "",
            bool autoComplete = false,
            bool deleteSourceBranch = false,
            bool transitionWorkItems = false)
        {
            using var ps = PowerShell.Create();

            ps.AddScript("az login").Invoke();
            ps.AddScript("az repos pr create " +
                         $"--org '{AzureDevOpsOrganizationUrl}' " +
                         $"--project '{AzureDevOpsProjectName}' " +
                         $"--repository '{repositoryName}' " +
                         $"--source-branch '{branchName}' " +
                         $"--target-branch '{targetBranch}' " +
                         $"--title '{commitMessage}' " +
                         $"--description '{commitMessage}'" +
                         $"--work-items {taskNumber} " +
                         $"--auto-complete {autoComplete} " +
                         $"--delete-source-branch {deleteSourceBranch} " +
                         $"--merge-commit-message {commitMessage} " +
                         $"--transition-work-items {transitionWorkItems}").Invoke();

            Console.WriteLine($"Created PR for repository '{repositoryName}' from " +
                              $"source branch '{branchName}' to target branch '{targetBranch}' " +
                              $"with title and description: '{commitMessage}', " +
                              $"for work item '{taskNumber}', auto-complete = '{autoComplete}', " +
                              $"delete source branch ='{deleteSourceBranch}', " +
                              $"transition work items = '{transitionWorkItems}'");
        }

        private static void ChangeDirectory(
            PowerShell ps,
            string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new ArgumentException(
                    $"Can't change directory. There is no folder by following path: '${directoryPath}'");
            }

            ps.AddScript($"cd {directoryPath}").Invoke();
        }
    }
}
