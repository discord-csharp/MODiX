$ErrorActionPreference = 'Stop';

$tag = $env:APPVEYOR_REPO_BRANCH
if(-not [System.String]::IsNullOrWhitespace($env:APPVEYOR_PULL_REQUEST_NUMBER)) {
	$tag = "$tag-pr-${$env:APPVEYOR_PULL_REQUEST_NUMBER}"
}

if([System.String]::IsNullOrWhitespace($tag)) {
    $tag = "untagged"
}
if (Enter-OncePerDeployment "install_docker_image")
{
	docker stop Modix
	docker rm Modix
	docker pull "cisien/onibot:$tag"
	docker run -d -e "Token=$env:ModixToken" -e "ReplToken=$env:ReplServiceToken" -e "StackoverflowToken=$env:SOToken" -e "PostgreConnectionString=$env:ConnectionString" --name Modix -v modix:c:\app\config "cisien/modix:$tag"
}