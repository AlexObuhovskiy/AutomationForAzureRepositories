using System;
using System.IO;
using System.Management.Automation;
using System.Text;

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
            
            var azurePrCommandBuilder = new StringBuilder();
            azurePrCommandBuilder.Append("az repos pr create ");
            azurePrCommandBuilder.Append($"--org '{AzureDevOpsOrganizationUrl}' ");
            azurePrCommandBuilder.Append($"--project '{AzureDevOpsProjectName}' ");
            azurePrCommandBuilder.Append($"--repository '{repositoryName}' ");
            azurePrCommandBuilder.Append($"--source-branch '{branchName}' ");
            azurePrCommandBuilder.Append($"--target-branch '{targetBranch}' ");
            azurePrCommandBuilder.Append($"--title '{commitMessage}' ");
            azurePrCommandBuilder.Append($"--description '{commitMessage}'");
            azurePrCommandBuilder.Append($"--work-items {taskNumber} ");
            azurePrCommandBuilder.Append($"--auto-complete {autoComplete} ");
            azurePrCommandBuilder.Append($"--delete-source-branch {deleteSourceBranch} ");
            azurePrCommandBuilder.Append($"--merge-commit-message {commitMessage} ");
            azurePrCommandBuilder.Append($"--transition-work-items {transitionWorkItems}");
            
            ps.AddScript(azurePrCommandBuilder.ToString()).Invoke();

            var logBuilder = new StringBuilder();
            logBuilder.Append($"Created PR for repository '{repositoryName}' from ");
            logBuilder.Append($"source branch '{branchName}' to target branch '{targetBranch}' ");
            logBuilder.Append($"with title and description: '{commitMessage}', ");
            logBuilder.Append($"for work items: '{taskNumber}', auto-complete = '{autoComplete}', ");
            logBuilder.Append($"delete source branch ='{deleteSourceBranch}', ");
            logBuilder.Append($"transition work items = '{transitionWorkItems}'");

            Console.WriteLine(logBuilder.ToString());
        }

        private static void ChangeDirectory(
            PowerShell ps,
            string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new ArgumentException(
                    $"Can't change directory. There is no folder with following path: '${directoryPath}'");
            }

            ps.AddScript($"cd {directoryPath}").Invoke();
        }
    }
}
