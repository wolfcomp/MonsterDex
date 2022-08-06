cd repo
echo "> Adding and committing"
git add --all
git commit --all -m "Updating $1"
echo "> Pushing to origin"
git push --force --set-upstream origin "$1"
prRepo="goatcorp/DalamudPluginsD17"
prNumber=$(gh api repos/${prRepo}/pulls | jq ".[] | select(.head.ref == \"$2\") | .number")
if [ $4 == *[TEST]*]; then
    prTitle="[Testing] $1 $3"
else
    prTitle="$1 $3"
fi
prBody="$6"
if [ "${prNumber}" ]; then
    echo "> Editing existing PR"
    gh api "repos/${prRepo}/pulls/${prNumber}" --silent --method PATCH -f "title=${prTitle}" -f "body=${prBody}" -f "state=open"
else
    echo "> Creating PR"
    gh pr create --repo "${prRepo}" --head "$5:$2" --base "main" --title "${prTitle}" --body "${prBody}"
fi
