echo "> Setting up $2/DalamudPluginsD17"
gh repo clone "$2/DalamudPluginsD17" repo
cd repo
git remote add pr_repo "https://github.com/goatcorp/DalamudPluginsD17.git"
git fetch pr_repo
git fetch origin
echo "> Adding token to origin push url"
originUrl=$(git config --get remote.origin.url | cut -d '/' -f 3-)
originUrl="https://$1@${originUrl}"
git config remote.origin.url "${originUrl}"
branch="$3"
if git show-ref --quiet "refs/heads/${branch}"; then
    echo "> Branch ${branch} already exists, reseting to master"
    git checkout "${branch}"
    git reset --hard "pr_repo/main"
else
    echo "> Creating new branch ${branch}"
    git reset --hard "pr_repo/main"
    git branch "${branch}"
    git checkout "${branch}"
    git push --set-upstream origin --force "${branch}"
fi
cd ..
