echo "> Logging into GitHub"
echo "${ secrets.PAT }" | gh auth login --with-token
echo "> Configuring git user"
authorName=$(jq -r '.commits[0].author.name' "${GITHUB_EVENT_PATH}")
authorEmail=$(jq -r '.commits[0].author.email' "${GITHUB_EVENT_PATH}")
git config --global user.name "${authorName}"
git config --global user.email "${authorEmail}"