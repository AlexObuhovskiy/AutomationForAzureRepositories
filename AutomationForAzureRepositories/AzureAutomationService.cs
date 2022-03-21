using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace AutomationForAzureRepositories
{
    public class AzureAutomationService
    {
        private const string TaskPrefix = "Task ";

        private readonly IGitService _gitService;
        private readonly ReadOnlyCollection<string> _backEndRepositoriesForMergeDevToMaster;
        private readonly ReadOnlyCollection<string> _frontEndRepositoriesForMergeDevToMaster;

        public AzureAutomationService(IGitService gitService)
        {
            _gitService = gitService;
            _backEndRepositoriesForMergeDevToMaster = new ReadOnlyCollection<string>(new List<string>
            {
                RepositoriesConstants.BackEndConstants.AssetRepository,
                RepositoriesConstants.BackEndConstants.UserManagementService,
                RepositoriesConstants.BackEndConstants.AssetOrganization,
                RepositoriesConstants.BackEndConstants.MxProgramManagementService,
                RepositoriesConstants.BackEndConstants.InspectionService,
                RepositoriesConstants.BackEndConstants.CalcService
            });

            _frontEndRepositoriesForMergeDevToMaster = new ReadOnlyCollection<string>(new List<string>
            {
                RepositoriesConstants.FrontEndConstants.AssetRepositoryFrontend,
                RepositoriesConstants.FrontEndConstants.UserManagementFrontend,
                RepositoriesConstants.FrontEndConstants.AssetOrganizationFrontend,
                RepositoriesConstants.FrontEndConstants.MxProgramManagementFrontend,
                RepositoriesConstants.FrontEndConstants.InspectionFrontend,
                RepositoriesConstants.FrontEndConstants.ShellFrontend
            });
        }

        /// <summary>
        /// Create git branch for repository by full task name
        /// </summary>
        /// <param name="repositoriesFolderPath"></param>
        /// <param name="repositoryName"></param>
        /// <param name="fullTaskName">Task name copied from clipboard, E.G. Task 100200: test</param>
        /// <returns></returns>
        public string CreateBranchForTask(
            string repositoriesFolderPath,
            string repositoryName,
            string fullTaskName)
        {
            var branchName = GetBranchName(fullTaskName);
            var repositoryPath = Path.Combine(repositoriesFolderPath, repositoryName);
            _gitService.CreateBranch(repositoryPath, branchName);

            return branchName;
        }

        public void CommitForTask(
            string repositoriesFolderPath,
            string repositoryName,
            string fullTaskName)
        {
            var commitMessage = $"#{fullTaskName[TaskPrefix.Length..]}";
            var repositoryPath = Path.Combine(repositoriesFolderPath, repositoryName);
            _gitService.Commit(repositoryPath, commitMessage);
        }

        public void PushForTask(
            string repositoriesFolderPath,
            string repositoryName,
            string fullTaskName)
        {
            var repositoryPath = Path.Combine(repositoriesFolderPath, repositoryName);
            var branchName = GetBranchName(fullTaskName);
            _gitService.Push(repositoryPath, branchName);
        }

        public void CreatePullRequestForTask(
            string repositoryName,
            string fullTaskName,
            string targetBranch)
        {
            var branchName = GetBranchName(fullTaskName);
            var commitMessage = $"#{fullTaskName[TaskPrefix.Length..]}";
            var taskNumber = GetTaskNumber(fullTaskName);
            _gitService.CreatePullRequest(
                repositoryName,
                branchName,
                targetBranch,
                commitMessage,
                taskNumber,
                deleteSourceBranch: true,
                autoComplete: true,
                transitionWorkItems: true);
        }

        public void CreatePullRequestsFromDevToMaster(string commitMessage)
        {
            foreach (var backEndRepositoryName in _backEndRepositoriesForMergeDevToMaster)
            {
                _gitService.CreatePullRequest(
                    backEndRepositoryName,
                    TargetBranchConstants.Dev,
                    TargetBranchConstants.Master,
                    commitMessage,
                    autoComplete: true);
            }

            foreach (var frontEndRepositoryName in _frontEndRepositoriesForMergeDevToMaster)
            {
                _gitService.CreatePullRequest(
                    frontEndRepositoryName,
                    TargetBranchConstants.Dev,
                    TargetBranchConstants.Master,
                    commitMessage,
                    autoComplete: true);
            }
        }

        private string GetBranchName(string fullTaskName)
        {
            var taskNumber = GetTaskNumber(fullTaskName);
            if (string.IsNullOrEmpty(taskNumber) || taskNumber.Length != 6)
            {
                throw new ArgumentException(
                    "Invalid full task name, please, copy work item title from clipboard",
                    nameof(fullTaskName));
            }

            var branchName = $"feature/task-{taskNumber}";

            return branchName;
        }

        private string GetTaskNumber(string fullTaskName)
        {
            return Regex.Match(fullTaskName, @"\d+").Value;
        }
    }
}