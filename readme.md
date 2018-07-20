<p align="center"><img src="https://imagr.eu/up/oGOcm_eJwFwdsNwyAMAMBdGACrNs8sU1GCSCQSI-yqH1V3793XvNcwmzlUp2wA-ymV125FeZXebGfuo5V5iq18QVEt9bjarQJI2VHCQC48yEXCDJgooHc5pJh9wOgJBnd-vj523t38_ggIIs0.png" /></p>

<a href="https://cisien.visualstudio.com/MODiX/_build/latest?definitionId=7"><img src="https://cisien.visualstudio.com/MODiX/_apis/build/status/MODiX-Docker container-CI?branchName=master"></a>

# MODiX - A moderation and analysis bot for discord

MODiX is a moderation and analysis bot for discord. As for now, it just deals with moderation, but development should be driven towards analytics to reach our goal in the near future.

## Roadmap & Contributions

Issues are the center of MODiX´s development. You can see what features are being worked on in the issues and you can see which features are planned. You might also create issues to share problems/ideas. 

## Getting Started

1. Download and install the latest version of the [.NET Core SDK](https://www.microsoft.com/net/download).
2. Download and install [Docker](https://www.docker.com/get-docker).
3. Create a new [Discord application](https://discordapp.com/developers/applications/me) with a bot user.
4. Create the following environment variables for your user account:
    - **DiscordClientId**: the client ID for your Discord app.
    - **DiscordClientSecret**: the client secret for your Discord app.
    - **DiscordToken**: the token for your Discord bot user.
5. Open a browser and navigate to https://discordapp.com/oauth2/authorize?scope=bot&permissions=0&client_id=<Client ID>, replacing `<Client ID>` with your Discord app's client ID from the previous step.
6. Select a Discord server from the dropdown list to add your bot to.
