cd repo
echo "> Adding and committing"
git add --all
git commit --all -m "Updating ${ env.PUBLIC_NAME }"
echo "> Pushing to origin"
git push --force --set-upstream origin "${ env.PUBLIC_NAME }"
prRepo="goatcorp/DalamudPluginsD17"
prNumber=$(gh api repos/${prRepo}/pulls | jq ".[] | select(.head.ref == \"${ env.INTERNAL_NAME }\") | .number")
if [ ${ github.event.head_commit.message } == *[TEST]*]; then
    prTitle="[Testing] ${ env.PUBLIC_NAME } ${ steps.version.outputs.new_version }"
else
    prTitle="${ env.PUBLIC_NAME } ${ steps.version.outputs.new_version }"
fi
prBody="${ steps.version.clean.replaced }"
if [ "${prNumber}" ]; then
    echo "> Editing existing PR"
    gh api "repos/${prRepo}/pulls/${prNumber}" --silent --method PATCH -f "title=${prTitle}" -f "body=${prBody}" -f "state=open"
else
    echo "> Creating PR"
    gh pr create --repo "${prRepo}" --head "${ env.GITHUB_REPOSITORY_OWNER }:${ env.INTERNAL_NAME }" --base "main" --title "${prTitle}" --body "${prBody}"
fi