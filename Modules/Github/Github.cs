using System;
using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Combot.Modules.Plugins
{
    public class Github : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));

            switch (foundCommand.Name)
            {
                case "Github Information":
                    GetGithubInformation(command);
                    break;
                case "Repository Information":
                    GetRepositoryInformation(command);
                    break;
            }
        }

        private async void GetGithubInformation(CommandMessage command)
        {
            try
            {
                string username = (command.Arguments.ContainsKey("Username")) ? command.Arguments["Username"] : command.Nick.Nickname;
                GitHubClient github = new GitHubClient(new ProductHeaderValue("Combot-IRC-Bot"));
                if (GetOptionValue("Token").ToString() != string.Empty)
                {
                    string token = GetOptionValue("Token").ToString();
                    Credentials creds = new Credentials(token);
                    github.Credentials = creds;
                }
                SearchUsersResult foundUser = await github.Search.SearchUsers(new SearchUsersRequest(username));
                if (foundUser.TotalCount > 0)
                {
                    User user = await github.User.Get(foundUser.Items.First().Login);
                    if (command.Arguments.ContainsKey("Repository"))
                    {
                        string repo = command.Arguments["Repository"];
                        try
                        {
                            Repository foundRepo = await github.Repository.Get(user.Login, repo);
                            if (foundRepo != null)
                            {
                                string repoMessage = string.Format("\u0002{0}\u0002 | Created On \u0002{1}\u0002 | \u0002{2}\u0002 Open Issues | \u0002{3}\u0002 Forks | \u0002{4}\u0002 Stargazers | {5}", foundRepo.FullName, foundRepo.CreatedAt.ToString("d"), foundRepo.OpenIssuesCount, foundRepo.ForksCount, foundRepo.StargazersCount, foundRepo.HtmlUrl);
                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, repoMessage);
                                if (foundRepo.Description != string.Empty)
                                {
                                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, foundRepo.Description);
                                }
                            }
                            else
                            {
                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "Invalid Repository Name");
                            }
                        }
                        catch (Octokit.NotFoundException ex)
                        {
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "Invalid Repository Name");
                        }
                    }
                    else
                    {
                        string userMessage = string.Format("\u0002{0}\u0002 - \u0002{1}\u0002 Followers - Following \u0002{2}\u0002 Users - \u0002{3}\u0002 Repositories | {4}", user.Login, user.Followers, user.Following, user.PublicRepos, user.HtmlUrl);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, userMessage);
                    }
                }
                else
                {
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("Github user \u0002{0}\u0002 does not exist.", username));
                }
            }
            catch (Octokit.RateLimitExceededException ex)
            {
                ThrowError(ex.Message);
            }
            catch (Octokit.ApiValidationException ex)
            {
                ThrowError(ex.Message);
            }
        }

        private async void GetRepositoryInformation(CommandMessage command)
        {
            try
            {
                GitHubClient github = new GitHubClient(new ProductHeaderValue("CombotIRCBot"));
                if (GetOptionValue("Token").ToString() != string.Empty)
                {
                    string token = GetOptionValue("Token").ToString();
                    github.Credentials = new Credentials(token);
                }
                SearchRepositoryResult foundRepo = await github.Search.SearchRepo(new SearchRepositoriesRequest(command.Arguments["Repository"]));
                if (foundRepo.TotalCount > 0)
                {
                    Repository repo = foundRepo.Items.First();
                    string repoMessage = string.Format("\u0002{0}\u0002 | Created On \u0002{1}\u0002 | \u0002{2}\u0002 Open Issues | \u0002{3}\u0002 Forks | \u0002{4}\u0002 Stargazers | {5}", repo.FullName, repo.CreatedAt.ToString("d"), repo.OpenIssuesCount, repo.ForksCount, repo.StargazersCount, repo.HtmlUrl);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, repoMessage);
                    if (repo.Description != string.Empty)
                    {
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, repo.Description);
                    }
                }
                else
                {
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("Repository \u0002{0}\u0002 does not exist.", command.Arguments["Repository"]));
                }
            }
            catch (Octokit.RateLimitExceededException ex)
            {
                ThrowError(ex.Message);
            }
            catch (Octokit.ApiValidationException ex)
            {
                ThrowError(ex.Message);
                return;
            }
        }
    }
}
